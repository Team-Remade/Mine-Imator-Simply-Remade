using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SimplyRemadeMI.core;

public partial class ModelLoader : Node
{
    public SceneObject LoadModel(string glbPath, Node3D parent = null)
    {
        // Load the GLB file using ResourceLoader
        var packedScene = ResourceLoader.Load<PackedScene>(glbPath);
        
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
        {
            return skeleton;
        }

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
            if (boneAttachments.TryGetValue(skeleton.GetBoneName(boneId), out var boneAttachment))
            {
                // For bones with attachments, don't create SceneObject, but find mesh and attach to parent bone
                var attachmentBoneName = skeleton.GetBoneName(boneId);

                // Find mesh in the attachment that matches the bone name
                var mesh = FindMeshInAttachment(boneAttachment, attachmentBoneName);
                if (mesh != null)
                {
                    // Material duplication is handled in AttachMeshToBoneObject to avoid sharing
                    
                    // Get parent bone ID
                    int attachmentParentBoneId = skeleton.GetBoneParent(boneId);
                    if (attachmentParentBoneId >= 0 && boneObjects.ContainsKey(attachmentParentBoneId))
                    {
                        var parentBoneObject = boneObjects[attachmentParentBoneId];
                        
                        var rotation = skeleton.GetBoneGlobalPose(boneId).Basis.GetEuler();
                        AttachMeshToBoneObject(mesh, parentBoneObject, rotation);
                    }
                    else
                    {
                        // If no parent bone, skip or handle differently
                        GD.PrintErr($"No parent bone found for bone with attachment '{attachmentBoneName}'");
                    }
                }
                continue; // Skip creating SceneObject for this bone
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

            // Find meshes for this bone
            var meshes = FindMeshesForBone(skeleton, boneId);
            MeshInstance3D firstMesh = null;
            if (meshes.Count > 0)
            {
                firstMesh = meshes[0];
            }

            // Create SceneObject for this bone using MainViewport's method
            var mainViewport = Main.GetInstance()?.MainViewport;
            if (mainViewport == null)
            {
                GD.PrintErr("MainViewport is null");
                continue;
            }
            
            var boneObject = mainViewport.CreateSceneObject(SceneObject.Type.ModelPart, null, boneName, parentNode);
            
            // Set target position
            boneObject.TargetPosition = boneTransform.Origin;
            
            // Set local scale from bone's local transform
            boneObject.Scale = boneTransform.Basis.Scale;
            
            boneObject.ObjectOriginOffset = Vector3.Zero;
            boneObject.OriginalOriginOffset = Vector3.Zero;

            // Handle rotation based on BoneAttachment3D node if exists
            if (boneAttachments.TryGetValue(boneName, out var attachmentForRotation))
            {
                // Calculate the global transform of the BoneAttachment3D node using bone global transform and attachment local transform
                var attachmentGlobalTransform = boneGlobalTransform * attachmentForRotation.Transform;
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


    private int AttachBoneMesh(Skeleton3D skeleton, int boneId, SceneObject boneObject)
    {
        // Find meshes that use this bone
        var meshes = FindMeshesForBone(skeleton, boneId);
        int meshesAttached = 0;
        
        foreach (var mesh in meshes)
        {
            // Create a new MeshInstance3D with a duplicated mesh to avoid sharing
            var newMeshInstance = new MeshInstance3D
            {
                Mesh = mesh.Mesh.Duplicate() as Mesh,
                Name = mesh.Name
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
                //duplicatedMat.Uv1Scale = new Vector3(1.01f, 1.01f, 1.01f);
                newMeshInstance.Mesh.SurfaceSetMaterial(i, duplicatedMat);
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

    private MeshInstance3D FindMeshInAttachment(Node attachment, string boneName)
    {
        // Recursively search for MeshInstance3D in the attachment
        if (attachment is MeshInstance3D meshInstance && DoesMeshNameMatchBone(meshInstance.Name, boneName))
        {
            return meshInstance;
        }

        foreach (Node child in attachment.GetChildren())
        {
            var foundMesh = FindMeshInAttachment(child, boneName);
            if (foundMesh != null)
            {
                return foundMesh;
            }
        }

        return null;
    }

    private void AttachMeshToBoneObject(MeshInstance3D mesh, SceneObject boneObject, Vector3 rotation)
    {
        // Create a new MeshInstance3D with a duplicated mesh to avoid sharing
        var newMeshInstance = new MeshInstance3D
        {
            Name = mesh.Name,
            //TODO: Fix UVs
            Mesh = mesh.Mesh.Duplicate() as Mesh,
            Rotation = rotation // Set the rotation from attachment
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
            newMeshInstance.Mesh.SurfaceSetMaterial(i, duplicatedMat);
        }

        // Add to the bone object
        boneObject.AddVisuals(newMeshInstance);
    }

    private bool DoesMeshNameMatchBone(string meshName, string boneName)
    {
        // Replace periods with underscores in both names for comparison
        string processedMeshName = meshName.Replace(".", "_");
        string processedBoneName = boneName.Replace(".", "_");
        return processedMeshName.Equals(processedBoneName, System.StringComparison.OrdinalIgnoreCase);
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
                        Mesh = mesh.Mesh.Duplicate() as Mesh,
                        Name = mesh.Name
                    };

                    if (mesh.MaterialOverride != null)
                    {
                        // Create a duplicate of the material to avoid modifying the original
                        var overrideMat = (StandardMaterial3D)mesh.MaterialOverride.Duplicate();
                        overrideMat.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
                        overrideMat.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
                        newMeshInstance.MaterialOverride = overrideMat;
                    }

                    // Use zero rotation for meshes attached without specific attachment rotation
                    AttachMeshToBoneObject(mesh, boneObject, Vector3.Zero);
                    attached = true;
                    break;
                }
            }
        }
    }
}