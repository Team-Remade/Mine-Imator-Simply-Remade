using System.Collections.Generic;
using Godot;

namespace SimplyRemadeMI.core;

public partial class SkinnedMeshController : Node
{
    public Dictionary<int, SceneObject> SkeletonBones { get; set; }
    [Export] public Skeleton3D RuntimeSkeleton { get; set; }
    
    public Dictionary<int, string> BoneNames { get; set; } = new();

    private bool _initialized = false;

    public override void _Ready()
    {
        InitializeSkinnedMesh();
    }

    private void InitializeSkinnedMesh()
    {
        if (_initialized || RuntimeSkeleton == null)
            return;

        _initialized = true;
    }

    public override void _Process(double delta)
    {
        if (!_initialized || RuntimeSkeleton == null || SkeletonBones == null)
            return;

        // Update bone transforms based on SceneObject positions
        UpdateBoneTransforms();
    }

    private void UpdateBoneTransforms()
    {
        if (RuntimeSkeleton == null) return;
        
        // First reset all bone poses to their rest position
        RuntimeSkeleton.ResetBonePoses();
        
        for (int boneId = 0; boneId < RuntimeSkeleton.GetBoneCount(); boneId++)
        {
            if (SkeletonBones.TryGetValue(boneId, out var boneObject))
            {
                // Calculate the bone transform relative to the runtime skeleton
                var bonePose = CalculateBoneTransform(boneObject);
                
                // Set the bone pose in the skeleton
                RuntimeSkeleton.SetBonePosePosition(boneId, bonePose.Origin);
                RuntimeSkeleton.SetBonePoseRotation(boneId, bonePose.Basis.GetRotationQuaternion());
                RuntimeSkeleton.SetBonePoseScale(boneId, bonePose.Basis.Scale);
            }
        }
    }

    private Transform3D CalculateBoneTransform(SceneObject boneObject)
    {
        if (RuntimeSkeleton == null) return Transform3D.Identity;
        
        // Get the bone ID for this bone object
        int boneId = -1;
        foreach (var kvp in SkeletonBones)
        {
            if (kvp.Value == boneObject)
            {
                boneId = kvp.Key;
                break;
            }
        }
        
        if (boneId == -1) return Transform3D.Identity;
        
        // Get the rest pose as the base
        Transform3D restPose = RuntimeSkeleton.GetBoneRest(boneId);
        
        // Get parent bone ID
        int parentBoneId = RuntimeSkeleton.GetBoneParent(boneId);
        
        // Calculate the local transform relative to the parent
        Transform3D localTransform;
        
        if (parentBoneId != -1 && SkeletonBones.TryGetValue(parentBoneId, out var parentBoneObject))
        {
            // Calculate local transform relative to parent bone
            Transform3D parentGlobalTransform = parentBoneObject.GlobalTransform;
            Transform3D boneGlobalTransform = boneObject.GlobalTransform;
            localTransform = parentGlobalTransform.AffineInverse() * boneGlobalTransform;
        }
        else
        {
            // Root bone - calculate relative to skeleton
            Transform3D skeletonGlobalTransform = RuntimeSkeleton.GlobalTransform;
            Transform3D boneGlobalTransform = boneObject.GlobalTransform;
            localTransform = skeletonGlobalTransform.AffineInverse() * boneGlobalTransform;
        }
        
        return localTransform;
    }

    // Public method to reset bones to their rest pose
    public void ResetToRestPose()
    {
        if (RuntimeSkeleton == null || SkeletonBones == null)
            return;

        for (int boneId = 0; boneId < RuntimeSkeleton.GetBoneCount(); boneId++)
        {
            if (SkeletonBones.TryGetValue(boneId, out var boneObject))
            {
                var restTransform = RuntimeSkeleton.GetBoneRest(boneId);
                boneObject.TargetPosition = restTransform.Origin;
                boneObject.Rotation = restTransform.Basis.GetEuler();
                boneObject.Scale = restTransform.Basis.Scale;
            }
        }
    }

    // Public method to get bone by name
    public SceneObject GetBoneByName(string boneName)
    {
        if (SkeletonBones == null || BoneNames == null)
            return null;

        foreach (var kvp in BoneNames)
        {
            if (kvp.Value == boneName && SkeletonBones.TryGetValue(kvp.Key, out var boneObject))
            {
                return boneObject;
            }
        }

        return null;
    }

    // Public method to get all bone names
    public string[] GetBoneNames()
    {
        if (BoneNames == null)
            return new string[0];

        var names = new string[BoneNames.Count];
        int i = 0;
        foreach (var boneName in BoneNames.Values)
        {
            names[i++] = boneName;
        }
        return names;
    }
}