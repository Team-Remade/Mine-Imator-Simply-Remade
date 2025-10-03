using System;
using System.Collections.Generic;
using Godot;

namespace SimplyRemadeMI.core;

public partial class SceneObject : Node3D
{
    [Export] public Marker3D Visuals;
    
    public enum Type
    {
        Empty,
        Cube,
        Block,
        Item,
        ModelPart,
        TestInvalidType,
    }

    public int ID;
    
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

    public Vector3 ObjectOriginOffset = new Vector3(0, 0.5f, 0);
    public Vector3 OriginalOriginOffset = new Vector3(0, 0.5f, 0);
    public Vector3 TargetPosition = Vector3.Zero;

    public override void _Ready()
    {
        SelectionManager.TransformGizmo.TransformEnd += TransformGizmoOnTransformEnd;
    }

    public void AddVisuals(Node3D node)
    {
        Visuals.AddChild(node);
    }

    private void TransformGizmoOnTransformEnd(int mode)
    {
        if (mode == 2)
        {
            TargetPosition = Position;
        }
    }

    private MeshInstance3D[] GetMeshes()
    {
        List<MeshInstance3D> meshes = new List<MeshInstance3D>();
        
        foreach (var child in Visuals.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                meshes.Add(mesh);
            }
        }
        
        return meshes.ToArray();
    }

    public float GetEffectiveAlpha()
    {
        float effectiveAlpha = Alpha;
        Node current = GetParent();
        
        while (current != null)
        {
            if (current is SceneObject sceneObject)
            {
                effectiveAlpha *= sceneObject.Alpha;
            }
            current = current.GetParent();
        }
        
        return effectiveAlpha;
    }

    public override void _Process(double delta)
    {
        float effectiveAlpha = GetEffectiveAlpha();
        
        if (GetMeshes().Length > 0)
        {
            foreach (MeshInstance3D mesh in GetMeshes())
            {
                // Update material override if it exists
                for (int i = 0; i < mesh.GetSurfaceOverrideMaterialCount(); i++)
                {
                    var mat = (StandardMaterial3D)mesh.GetSurfaceOverrideMaterial(i);
                    if (mat != null)
                    {
                        mat.AlbedoColor = new Color(mat.AlbedoColor.R, mat.AlbedoColor.G, mat.AlbedoColor.B, effectiveAlpha);
                    }
                }
                
                // Update surface materials
                if (mesh.Mesh != null)
                {
                    for (int i = 0; i < mesh.Mesh.GetSurfaceCount(); i++)
                    {
                        var mat = (StandardMaterial3D)mesh.Mesh.SurfaceGetMaterial(i);
                        if (mat != null)
                        {
                            mat.AlbedoColor = new Color(mat.AlbedoColor.R, mat.AlbedoColor.G, mat.AlbedoColor.B, effectiveAlpha);
                        }
                    }
                }
            }
        }

        if (SelectionManager.TransformGizmo.Editing) return;
        
        Visuals.Position = ObjectOriginOffset;

        foreach (var obj in GetChildren())
        {
            if (obj is SceneObject sceneObject)
            {
                sceneObject.Position = new Vector3(0, -0.5f, 0) + ObjectOriginOffset + sceneObject.TargetPosition;
            }
        }
        
        Position = TargetPosition;
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