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

        // Process bones and skinned meshes - this will create SceneObjects for each bone
        SceneObject rootBoneObject = ProcessSkinnedModel(modelInstance, skeleton, parent ?? GetTree().CurrentScene, baseName);
        
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

    private SceneObject ProcessSkinnedModel(Node3D modelInstance, Skeleton3D skeleton, Node parent, string baseName)
    {
        // Find all skinned meshes in the model
        var skinnedMeshes = FindSkinnedMeshes(modelInstance);
        
        // Also find bone attachments for models that use per-bone meshes (like Steve)
        var boneAttachments = FindBoneAttachments(modelInstance);
        
        // Dictionary to store bone ID to SceneObject mapping
        var boneObjects = new Dictionary<int, SceneObject>();
        SceneObject rootBoneObject = null;

        // Create SceneObjects for each bone
        for (int boneId = 0; boneId < skeleton.GetBoneCount(); boneId++)
        {
            var boneName = skeleton.GetBoneName(boneId);

            // Get bone rest transform (bind pose) - this is the default/neutral position
            var boneRestTransform = skeleton.GetBoneRest(boneId);

            // Get parent bone ID
            int parentBoneId = skeleton.GetBoneParent(boneId);
            Node parentNode = parent;

            // If bone has a parent, use the parent's SceneObject
            if (parentBoneId >= 0 && boneObjects.TryGetValue(parentBoneId, out var parentBoneObject))
            {
                parentNode = parentBoneObject;
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
            
            // If this bone has an attachment, attach the mesh to this bone
            if (boneAttachments.TryGetValue(boneName, out var attachment))
            {
                var mesh = FindMeshInAttachment(attachment);
                if (mesh != null)
                {
                    AttachMeshToBoneObject(mesh, boneObject, mesh.Rotation, Vector3.Zero);
                }
            }
        }

        // Use the first bone object as root if no "root" bone found
        rootBoneObject ??= boneObjects.Values.FirstOrDefault();

        // Create a shared runtime skeleton for all skinned meshes
        if (rootBoneObject != null && skinnedMeshes.Count > 0)
        {
            // Create the runtime skeleton and add it to the root bone object
            var runtimeSkeleton = new Skeleton3D();
            runtimeSkeleton.Name = "RuntimeSkeleton";
            rootBoneObject.AddChild(runtimeSkeleton);

            // Set up the skeleton with the same bone structure as the original
            SetupRuntimeSkeleton(runtimeSkeleton, skeleton);

            // Attach all skinned meshes to the root bone object with the shared skeleton
            foreach (var meshInstance in skinnedMeshes)
            {
                AttachSkinnedMeshToBoneObject(meshInstance, rootBoneObject, runtimeSkeleton);
            }

            // Create a single SkinnedMeshController for the entire model
            var skinController = new SkinnedMeshController();
            skinController.SkeletonBones = boneObjects;
            skinController.RuntimeSkeleton = runtimeSkeleton;
            
            // Store bone names for reference
            for (int boneId = 0; boneId < skeleton.GetBoneCount(); boneId++)
            {
                skinController.BoneNames[boneId] = skeleton.GetBoneName(boneId);
            }
            
            rootBoneObject.AddChild(skinController);
            
            GD.Print($"Created SkinnedMeshController for {baseName} with {skinnedMeshes.Count} skinned meshes");
        }
        else if (rootBoneObject != null && boneAttachments.Count > 0)
        {
            GD.Print($"Model {baseName} uses bone attachments ({boneAttachments.Count} attachments)");
        }

        // Free the original model instance since we've extracted what we need
        modelInstance.QueueFree();
        
        Main.GetInstance().MainViewport.UpdatePicking();

        return rootBoneObject;
    }

    private void SetupRuntimeSkeleton(Skeleton3D runtimeSkeleton, Skeleton3D originalSkeleton)
    {
        if (originalSkeleton == null || runtimeSkeleton == null)
            return;

        // Clear any existing bones
        runtimeSkeleton.ClearBones();

        // Add bones to match the original skeleton structure
        for (int boneId = 0; boneId < originalSkeleton.GetBoneCount(); boneId++)
        {
            var boneName = originalSkeleton.GetBoneName(boneId);
            var parentBoneId = originalSkeleton.GetBoneParent(boneId);
            var restTransform = originalSkeleton.GetBoneRest(boneId);

            // Add the bone to the runtime skeleton
            runtimeSkeleton.AddBone(boneName);
            runtimeSkeleton.SetBoneRest(boneId, restTransform);

            if (parentBoneId >= 0)
            {
                runtimeSkeleton.SetBoneParent(boneId, parentBoneId);
            }
        }
    }

    private List<MeshInstance3D> FindSkinnedMeshes(Node node)
    {
        var skinnedMeshes = new List<MeshInstance3D>();
        
        // Check if this node is a MeshInstance3D with a skin
        if (node is MeshInstance3D meshInstance)
        {
            // Check if this mesh has skinning data (either through Skeleton3D or Skin resource)
            if (meshInstance.Skin != null || HasBoneWeights(meshInstance))
            {
                skinnedMeshes.Add(meshInstance);
            }
        }

        // Recursively search children
        foreach (Node child in node.GetChildren())
        {
            skinnedMeshes.AddRange(FindSkinnedMeshes(child));
        }

        return skinnedMeshes;
    }

    private bool HasBoneWeights(MeshInstance3D meshInstance)
    {
        // Check if the mesh has bone weights by examining the mesh surfaces
        if (meshInstance.Mesh == null)
            return false;

        for (int surfaceIndex = 0; surfaceIndex < meshInstance.Mesh.GetSurfaceCount(); surfaceIndex++)
        {
            var arrays = meshInstance.Mesh.SurfaceGetArrays(surfaceIndex);
            if (arrays == null || arrays.Count <= (int)Mesh.ArrayType.Bones)
                continue;

            var boneWeights = arrays[(int)Mesh.ArrayType.Bones];
            var boneIndices = arrays[(int)Mesh.ArrayType.Bones + 1]; // Bone indices are typically in the next array

            if (boneWeights.VariantType != Variant.Type.Nil && boneWeights.AsGodotArray().Count > 0)
                return true;
        }

        return false;
    }

    private void AttachSkinnedMeshToBoneObject(MeshInstance3D mesh, SceneObject rootBoneObject, Skeleton3D runtimeSkeleton)
    {
        // Create a new MeshInstance3D with a duplicated mesh to preserve skinning data
        var newMeshInstance = new MeshInstance3D
        {
            Name = mesh.Name,
            Mesh = mesh.Mesh.Duplicate() as Mesh,
            Position = Vector3.Zero,
            Rotation = Vector3.Zero,
            Skeleton = runtimeSkeleton.GetPath() // Use the shared runtime skeleton
        };

        // Copy the skin resource if it exists (this contains bone weights and influences)
        if (mesh.Skin != null)
        {
            newMeshInstance.Skin = mesh.Skin.Duplicate() as Skin;
        }

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

        // Add the mesh to the root bone object
        rootBoneObject.AddVisuals(newMeshInstance);
        
        GD.Print($"Attached skinned mesh '{mesh.Name}' to bone object '{rootBoneObject.Name}'");
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
        
        GD.Print($"Attached mesh '{mesh.Name}' to bone '{boneObject.Name}'");
    }
}