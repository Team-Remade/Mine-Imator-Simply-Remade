using System.Collections.Generic;
using System.Linq;
using Godot;
using SimplyRemadeMI.core;

namespace SimplyRemadeMI.core;

public partial class ModelLoader : Node
{
    public SceneObject LoadModel(string glbPath, Node3D parent = null)
    {
        // Load the GLB file using ResourceLoader
        var packedScene = ResourceLoader.Load<PackedScene>(glbPath);
        if (packedScene == null)
        {
            GD.PrintErr($"Failed to load GLB file: {glbPath}");
            return null;
        }

        // Instantiate the scene
        var modelInstance = packedScene.Instantiate<Node3D>();
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

    private Skeleton3D FindSkeleton(Node node)
    {
        if (node is Skeleton3D skeleton)
            return skeleton;

        foreach (Node child in node.GetChildren())
        {
            var foundSkeleton = FindSkeleton(child);
            if (foundSkeleton != null)
                return foundSkeleton;
        }

        return null;
    }

    private SceneObject ProcessBones(Skeleton3D skeleton, Node parent, string baseName)
    {
        var boneAttachments = FindBoneAttachments(skeleton);
        int totalMeshesAttached = 0;
        
        // Dictionary to store bone ID to SceneObject mapping
        var boneObjects = new Dictionary<int, SceneObject>();
        SceneObject rootBoneObject = null;

        for (int boneId = 0; boneId < skeleton.GetBoneCount(); boneId++)
        {
            if (boneAttachments.TryGetValue(skeleton.GetBoneName(boneId), out var bone))
            {
                continue;
            }
            
            var boneName = skeleton.GetBoneName(boneId);

            // Get bone transform
            var boneTransform = skeleton.GetBonePose(boneId);
            var boneGlobalTransform = skeleton.GetBonePose(boneId);

            // Get parent bone ID
            int parentBoneId = skeleton.GetBoneParent(boneId);
            Node parentNode = parent;

            // If bone has a parent, use the parent's SceneObject
            if (parentBoneId >= 0 && boneObjects.ContainsKey(parentBoneId))
            {
                parentNode = boneObjects[parentBoneId];
            }

            // Create SceneObject for this bone
            var boneObject = CreateSceneObject(boneName, parentNode);
            
            // Set target position
            boneObject.TargetPosition = boneTransform.Origin;
            
            // Set local scale from bone's local transform
            boneObject.Scale = boneTransform.Basis.Scale;
            
            boneObject.ObjectOriginOffset = Vector3.Zero;
            boneObject.OriginalOriginOffset = Vector3.Zero;

            // Handle rotation based on BoneAttachment3D node if exists
            if (boneAttachments.TryGetValue(boneName, out var attachment))
            {
                // Calculate the global transform of the BoneAttachment3D node using bone global transform and attachment local transform
                var attachmentGlobalTransform = boneGlobalTransform * attachment.Transform;
                // Get the rotation as Euler angles from the calculated global transform
                var attachmentEuler = attachmentGlobalTransform.Basis.GetEuler();
                boneObject.GlobalRotation = attachmentEuler;
            }
            else
            {
                // Set rotation to zero for bones without attachments
                boneObject.Rotation = Vector3.Zero;
            }

            // If bone is named "root", rename the object after the GLB file
            if (boneName.ToLower() == "root")
            {
                boneObject.Name = baseName;
                rootBoneObject = boneObject;
            }

            // Store the bone object for parenting
            boneObjects[boneId] = boneObject;

            // For root bone, skip attaching meshes
            if (boneId == 0 || boneName.ToLower() == "root")
            {
                continue;
            }

            // Find and attach mesh controlled by this bone
            int meshesAttached = AttachBoneMesh(skeleton, boneId, boneObject);
            totalMeshesAttached += meshesAttached;
        }
        
        // If no meshes were attached to any bone, attach all meshes to appropriate bones as fallback
        if (totalMeshesAttached == 0)
        {
            AttachMeshesToBones(skeleton, boneObjects);
        }

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

    private SceneObject CreateSceneObject(string name, Node parent)
    {
        var main = Main.GetInstance();
        if (main?.ObjectScene == null)
        {
            GD.PrintErr("Main instance or ObjectScene is null");
            return null;
        }

        var sceneObject = main.ObjectScene.Instantiate<SceneObject>();
        if (sceneObject == null)
        {
            GD.PrintErr("Failed to instantiate SceneObject");
            return null;
        }

        sceneObject.Name = name;
        parent.AddChild(sceneObject);
        return sceneObject;
    }

    private int AttachBoneMesh(Skeleton3D skeleton, int boneId, SceneObject boneObject)
    {
        // Find meshes that use this bone
        var meshes = FindMeshesForBone(skeleton, boneId);
        int meshesAttached = 0;
        
        foreach (var mesh in meshes)
        {
            // Create a new MeshInstance3D with the same mesh
            var newMeshInstance = new MeshInstance3D
            {
                Mesh = mesh.Mesh,
                Name = mesh.Name
            };


            for (int i = 0; i < mesh.Mesh.GetSurfaceCount(); i++)
            {
                var mat = (StandardMaterial3D)mesh.Mesh.SurfaceGetMaterial(i);
                mat.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
                mat.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
            }

            // Add to the bone object using AddVisuals
            boneObject.AddVisuals(newMeshInstance);
            meshesAttached++;
        }

        return meshesAttached;
    }

    private List<MeshInstance3D> FindMeshesForBone(Skeleton3D skeleton, int boneId)
    {
        var meshes = new List<MeshInstance3D>();
        var boneName = skeleton.GetBoneName(boneId);

        // Search through all nodes in the model instance to find meshes
        var modelInstance = skeleton.GetParent<Node3D>();
        if (modelInstance == null)
            return meshes;

        // Recursively find all MeshInstance3D nodes in the model
        FindAllMeshes(modelInstance, meshes);

        // First try: filter meshes that are influenced by this bone using skin data
        var influencedMeshes = new List<MeshInstance3D>();
        foreach (var mesh in meshes)
        {
            if (IsMeshInfluencedByBone(mesh, skeleton, boneId))
            {
                influencedMeshes.Add(mesh);
            }
        }

        // Second try: if no skin-based matches, try name-based matching as fallback
        if (influencedMeshes.Count == 0)
        {
            foreach (var mesh in meshes)
            {
                if (DoesMeshNameMatchBone(mesh.Name, boneName))
                {
                    influencedMeshes.Add(mesh);
                }
            }
        }

        // Third try: if still no matches, attach all meshes to root bone
        if (influencedMeshes.Count == 0 && (boneName.ToLower() == "root" || boneId == 0))
        {
            return meshes;
        }
        
        return influencedMeshes;
    }

    private bool IsMeshInfluencedByBone(MeshInstance3D mesh, Skeleton3D skeleton, int boneId)
    {
        var boneName = skeleton.GetBoneName(boneId);
        
        // Check if the mesh has a skin
        if (mesh.Skin == null)
        {
            return false;
        }

        // Check if the mesh is using the correct skeleton
        var meshSkeletonPath = mesh.Skeleton;
        if (meshSkeletonPath.IsEmpty)
        {
            return false;
        }

        var meshSkeleton = mesh.GetNodeOrNull<Skeleton3D>(meshSkeletonPath);
        if (meshSkeleton == null)
        {
            return false;
        }

        if (meshSkeleton != skeleton)
        {
            return false;
        }

        // Get the skin resource
        var skin = mesh.Skin;
        
        // Iterate through all bones in the skin to see if our bone ID is used
        for (int i = 0; i < skin.GetBindCount(); i++)
        {
            int skinBoneId = skin.GetBindBone(i);
            string skinBoneName = skeleton.GetBoneName(skinBoneId);
            
            if (skinBoneId == boneId)
            {
                return true;
            }
        }
        
        return false;
    }

    private void FindAllMeshes(Node node, List<MeshInstance3D> meshes)
    {
        if (node is MeshInstance3D meshInstance)
        {
            meshes.Add(meshInstance);
        }

        foreach (Node child in node.GetChildren())
        {
            FindAllMeshes(child, meshes);
        }
    }

    private bool DoesMeshNameMatchBone(string meshName, string boneName)
    {
        // Common Minecraft model naming conventions
        var mapping = new Dictionary<string, string[]>
        {
            ["Body"] = new[] { "B", "Body" },
            ["Right Arm"] = new[] { "RA", "RightArm", "Right_Arm" },
            ["Left Arm"] = new[] { "LA", "LeftArm", "Left_Arm" },
            ["Head"] = new[] { "H", "Head" },
            ["Right Leg"] = new[] { "RL", "RightLeg", "Right_Leg" },
            ["Left Leg"] = new[] { "LL", "LeftLeg", "Left_Leg" }
        };

        // Check if the bone name is in our mapping
        if (mapping.ContainsKey(boneName))
        {
            return mapping[boneName].Contains(meshName);
        }

        // Fallback: exact match
        return meshName == boneName;
    }

    private void AttachMeshesToBones(Skeleton3D skeleton, Dictionary<int, SceneObject> boneObjects)
    {
        var meshes = new List<MeshInstance3D>();
        var modelInstance = skeleton.GetParent<Node3D>();
        if (modelInstance == null)
            return;

        FindAllMeshes(modelInstance, meshes);

        foreach (var mesh in meshes)
        {
            // Try to find a bone with a matching name
            bool attached = false;
            for (int boneId = 0; boneId < skeleton.GetBoneCount(); boneId++)
            {
                var boneName = skeleton.GetBoneName(boneId);
                if (DoesMeshNameMatchBone(mesh.Name, boneName) && boneObjects.ContainsKey(boneId))
                {
                    var boneObject = boneObjects[boneId];
                    var newMeshInstance = new MeshInstance3D
                    {
                        Mesh = mesh.Mesh,
                        Name = mesh.Name
                    };

                    if (mesh.MaterialOverride != null)
                    {
                        newMeshInstance.MaterialOverride = mesh.MaterialOverride;
                    }

                    boneObject.AddVisuals(newMeshInstance);
                    attached = true;
                    break;
                }
            }
        }
    }
}