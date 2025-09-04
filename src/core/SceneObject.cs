using System.Collections.Generic;
using Godot;

namespace SimplyRemadeMI.core;

public partial class SceneObject : Node3D
{
    public enum Type
    {
        Empty,
        Cube,
        Block,
        Item,
        ModelPart,
        TestInvalidType,
    }
    
    public Type ObjectType;
    
    public Dictionary<int, float> PosXKeyframes { get; set; } = new Dictionary<int, float>();
    public Dictionary<int, float> PosYKeyframes { get; set; } = new Dictionary<int, float>();
    public Dictionary<int, float> PosZKeyframes { get; set; } = new Dictionary<int, float>();
    public Dictionary<int, float> RotXKeyframes { get; set; } = new Dictionary<int, float>();
    public Dictionary<int, float> RotYKeyframes { get; set; } = new Dictionary<int, float>();
    public Dictionary<int, float> RotZKeyframes { get; set; } = new Dictionary<int, float>();
    public Dictionary<int, float> ScaleXKeyframes { get; set; } = new Dictionary<int, float>();
    public Dictionary<int, float> ScaleYKeyframes { get; set; } = new Dictionary<int, float>();
    public Dictionary<int, float> ScaleZKeyframes { get; set; } = new Dictionary<int, float>();
    public Dictionary<int, float> AlphaKeyframes { get; set; } = new Dictionary<int, float>();
    
    public float Alpha = 1.0f;

    public Vector3 ObjectOriginOffset = Vector3.Zero;
    public Vector3 OriginalOriginOffset = Vector3.Zero;

    private MeshInstance3D[] GetMeshes()
    {
        List<MeshInstance3D> meshes = new List<MeshInstance3D>();
        
        foreach (var child in GetChildren())
        {
            if (child is MeshInstance3D)
            {
                meshes.Add((MeshInstance3D)child);
            }
        }
        
        return meshes.ToArray();
    }

    public override void _Process(double delta)
    {
        if (GetMeshes().Length > 0)
        {
            foreach (MeshInstance3D mesh in GetMeshes())
            {
                var mat = (StandardMaterial3D)mesh.MaterialOverride;
                mat.AlbedoColor = new Color(mat.AlbedoColor.R, mat.AlbedoColor.G, mat.AlbedoColor.B, Alpha);
            }
        }
    }

    public void SetParent(SceneObject parent)
    {
        GetParent().RemoveChild(this);
        parent.AddChild(this);
    }
    
    public bool IsDescendantOf(SceneObject ancestor)
    {
        var current = GetParent();
        while (current != null)
        {
            if (current == ancestor)
                return true;
            current = current.GetParent();
        }

        return false;
    }
}