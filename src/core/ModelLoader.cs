using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SimplyRemadeMI.core;

public partial class ModelLoader : Node
{
    public SceneObject LoadModel(string glbPath, Node3D parent = null, bool isExternalFile = false)
    {
        // Load the GLB file using ResourceLoader
        Node3D modelInstance = null;

        if (!isExternalFile)
        {
            var packedScene = ResourceLoader.Load<PackedScene>(glbPath);
            modelInstance = packedScene.Instantiate<Node3D>();
        }
        else
        {
            var doc = new GltfDocument();
            var state = new GltfState();
            
            var error = doc.AppendFromFile(glbPath, state);
            if (error == Error.Ok)
            {
                modelInstance = doc.GenerateScene(state) as Node3D;
            }
            else
            {
                GD.PrintErr(error);
            }
        }
        
        if (modelInstance == null)
        {
            GD.PrintErr($"Failed to instantiate GLB scene: {glbPath}");
            return null;
        }

        // Find Skeleton3D node
        var skeleton = FindSkeleton(modelInstance);
        if (skeleton == null)
        {
            GD.PrintErr($"No Skeleton3D found in model: {glbPath}");
            modelInstance.QueueFree();
            return null;
        }

        // Get the base name from the GLB file path
        var baseName = System.IO.Path.GetFileNameWithoutExtension(glbPath);

        // Process bones - this will create SceneObjects for each bone
        SceneObject rootBoneObject = ProcessBones(skeleton, parent ?? GetTree().CurrentScene, baseName);

        // Free the model instance
        modelInstance.QueueFree();
        
        return rootBoneObject;
    }

    private static Skeleton3D FindSkeleton(Node node)
    {
        if (node is Skeleton3D skeleton)
        {
            return skeleton;
        }

        return node.GetChildren().Select(FindSkeleton).FirstOrDefault(foundSkeleton => foundSkeleton != null);
    }

    private SceneObject ProcessBones(Skeleton3D skeleton, Node parent, string baseName)
    {
        var boneAttachments = FindBoneAttachments(skeleton);
        
        // Dictionary to store bone ID to SceneObject mapping
        var boneObjects = new Dictionary<int, SceneObject>();
        SceneObject rootBoneObject = null;

        for (int boneId = 0; boneId < skeleton.GetBoneCount(); boneId++)
        {
            var boneName = skeleton.GetBoneName(boneId);

            // Get bone rest transform (bind pose) - this is the default/neutral position
            var boneRestTransform = skeleton.GetBoneRest(boneId);

            // Get parent bone ID
            int parentBoneId = skeleton.GetBoneParent(boneId);
            Node parentNode = parent;

            // If bone has a parent, use the parent's SceneObject
            if (parentBoneId >= 0 && boneObjects.TryGetValue(parentBoneId, out var o))
            {
                parentNode = o;
            }

            // Create SceneObject for this bone using MainViewport's method
            var mainViewport = Main.GetInstance()?.MainViewport;
            if (mainViewport == null)
            {
                GD.PrintErr("MainViewport is null");
                continue;
            }
            
            var boneObject = mainViewport.CreateSceneObject(SceneObject.Type.ModelPart, null, boneName, parentNode);
            
            // Set local position and rotation from bone's rest transform (bind pose)
            boneObject.TargetPosition = boneRestTransform.Origin;
            boneObject.Rotation = boneRestTransform.Basis.GetEuler();
            
            // Set local scale from bone's rest transform
            boneObject.Scale = boneRestTransform.Basis.Scale;
            
            boneObject.ObjectOriginOffset = Vector3.Zero;
            boneObject.OriginalOriginOffset = Vector3.Zero;

            // If bone is named "root", rename the object after the GLB file
            if (boneName.ToLower() == "root")
            {
                boneObject.Name = baseName;
                rootBoneObject = boneObject;
            }

            // Store the bone object for parenting
            boneObjects[boneId] = boneObject;

            // If this bone has an attachment, find and attach the mesh directly to this bone's SceneObject
            if (boneAttachments.TryGetValue(boneName, out var attachment))
            {
                var mesh = FindMeshInAttachment(attachment);
                if (mesh != null)
                {
                    // Use only the BoneAttachment3D's rotation for orientation
                    // Position is zero since bone position is already set correctly
                    var rotation = mesh.Rotation;
                    AttachMeshToBoneObject(mesh, boneObject, rotation, Vector3.Zero);
                }
            }
        }
        
        Main.GetInstance().MainViewport.UpdatePicking();

        return rootBoneObject ?? boneObjects.Values.FirstOrDefault();
    }

    private Dictionary<string, BoneAttachment3D> FindBoneAttachments(Node node)
    {
        var attachments = new Dictionary<string, BoneAttachment3D>();
        
        if (node is BoneAttachment3D boneAttachment)
        {
            attachments[boneAttachment.BoneName] = boneAttachment;
        }

        foreach (Node child in node.GetChildren())
        {
            var childAttachments = FindBoneAttachments(child);
            foreach (var kvp in childAttachments)
            {
                attachments[kvp.Key] = kvp.Value;
            }
        }

        return attachments;
    }

    private MeshInstance3D FindMeshInAttachment(Node attachment)
    {
        // Recursively search for the first MeshInstance3D in the attachment
        if (attachment is MeshInstance3D meshInstance)
        {
            return meshInstance;
        }

        foreach (Node child in attachment.GetChildren())
        {
            var foundMesh = FindMeshInAttachment(child);
            if (foundMesh != null)
            {
                return foundMesh;
            }
        }

        return null;
    }

    private void AttachMeshToBoneObject(MeshInstance3D mesh, SceneObject boneObject, Vector3 rotation, Vector3 position = default)
    {
        // Create a new MeshInstance3D with a duplicated mesh to avoid sharing
        var newMeshInstance = new MeshInstance3D
        {
            Name = mesh.Name,
            //TODO: Fix UVs
            Mesh = mesh.Mesh.Duplicate() as Mesh,
            Rotation = rotation, // Set the rotation from attachment
            Position = position
        };

        // Copy material override if exists
        if (mesh.MaterialOverride != null)
        {
            // Create a duplicate of the material to avoid modifying the original
            var overrideMat = (StandardMaterial3D)mesh.MaterialOverride.Duplicate();
            overrideMat.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
            overrideMat.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
            newMeshInstance.MaterialOverride = overrideMat;
        }

        // Configure surface materials for alpha transparency - duplicate materials to avoid sharing
        for (int i = 0; i < mesh.Mesh.GetSurfaceCount(); i++)
        {
            var originalMat = (StandardMaterial3D)mesh.Mesh.SurfaceGetMaterial(i);
            var duplicatedMat = (StandardMaterial3D)originalMat.Duplicate();
            duplicatedMat.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
            duplicatedMat.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
            newMeshInstance.Mesh?.SurfaceSetMaterial(i, duplicatedMat);
        }

        // Add to the bone object
        boneObject.AddVisuals(newMeshInstance);
    }
}