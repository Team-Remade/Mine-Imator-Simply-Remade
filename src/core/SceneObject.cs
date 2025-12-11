using System;
using System.Collections.Generic;
using System.Linq;
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
        Camera,
        PointLight,
    }

    public int ID;
    public Guid ObjectGuid { get; set; }
    
    public Type ObjectType;
    
    // Control properties for what can be manipulated for this object
    public bool Rotatable = true;
    public bool Scalable = true;
    public bool AlphaControl = true;
    
    public void InitializeControlProperties()
    {
        // Set control properties based on object type
        switch (ObjectType)
        {
            case Type.Camera:
                Rotatable = true;    // Cameras can be rotated
                Scalable = false;    // Cameras cannot be scaled
                AlphaControl = false; // Cameras cannot have alpha control
                break;
            case Type.PointLight:
                Rotatable = false;
                Scalable = false;
                AlphaControl = false;
                break;
            default:
                Rotatable = true;
                Scalable = true;
                AlphaControl = true;
                break;
        }
    }
    
    public Dictionary<int, Keyframe> PosXKeyframes { get; set; } = new Dictionary<int, Keyframe>();
    public Dictionary<int, Keyframe> PosYKeyframes { get; set; } = new Dictionary<int, Keyframe>();
    public Dictionary<int, Keyframe> PosZKeyframes { get; set; } = new Dictionary<int, Keyframe>();
    public Dictionary<int, Keyframe> RotXKeyframes { get; set; } = new Dictionary<int, Keyframe>();
    public Dictionary<int, Keyframe> RotYKeyframes { get; set; } = new Dictionary<int, Keyframe>();
    public Dictionary<int, Keyframe> RotZKeyframes { get; set; } = new Dictionary<int, Keyframe>();
    public Dictionary<int, Keyframe> ScaleXKeyframes { get; set; } = new Dictionary<int, Keyframe>();
    public Dictionary<int, Keyframe> ScaleYKeyframes { get; set; } = new Dictionary<int, Keyframe>();
    public Dictionary<int, Keyframe> ScaleZKeyframes { get; set; } = new Dictionary<int, Keyframe>();
    public Dictionary<int, Keyframe> AlphaKeyframes { get; set; } = new Dictionary<int, Keyframe>();
    
    public float Alpha = 1.0f;

    public Vector3 ObjectOriginOffset = new Vector3(0, 0.5f, 0);
    public Vector3 OriginalOriginOffset = new Vector3(0, 0.5f, 0);
    public Vector3 TargetPosition = Vector3.Zero;

    public override void _Ready()
    {
        // Generate unique GUID for this object if not already set
        if (ObjectGuid == Guid.Empty)
        {
            ObjectGuid = Guid.NewGuid();
        }
        
        SelectionManager.TransformGizmo.TransformEnd += TransformGizmoOnTransformEnd;
        InitializeControlProperties();
    }
    
    public override void _ExitTree()
    {
        // Unsubscribe from events when the object is removed from the tree
        SelectionManager.TransformGizmo.TransformEnd -= TransformGizmoOnTransformEnd;
        base._ExitTree();
    }

    public void AddVisuals(Node3D node)
    {
        Visuals.AddChild(node);
    }

    private void TransformGizmoOnTransformEnd(int mode)
    {
        // Validate that this object is still valid before accessing its properties
        if (!GodotObject.IsInstanceValid(this))
            return;
            
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
                sceneObject.ObjectOriginOffset = ObjectOriginOffset;
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

    public Camera3D GetCamera()
    {
        // Traverse children to find a Camera3D node
        foreach (var child in Visuals.GetChildren())
        {
            if (child is MeshInstance3D meshInstance)
            {
                foreach (var meshChild in meshInstance.GetChildren())
                {
                    
                    if (meshChild is Camera3D camera)
                    {
                        return camera;
                    }
                }
            }
        }
        return null;
    }

    public SceneObject DeepDuplicateSceneObject()
    {
        // Use Godot's built-in Duplicate method to create a deep copy
        SceneObject duplicatedObject = null;
        
        try
        {
            // Store original local position relative to parent BEFORE duplication
            Vector3 originalLocalPosition = Position;
            SceneObject originalParent = GetParent() as SceneObject;
            Vector3 originalParentWorldPosition = originalParent != null ? originalParent.GlobalPosition : Vector3.Zero;
            
            // Duplicate the entire node tree including children and resources
            duplicatedObject = Duplicate() as SceneObject;
            
            if (duplicatedObject == null)
            {
                GD.PrintErr("Failed to duplicate SceneObject");
                return null;
            }
            
            // IDs will be assigned based on index in SceneObjects dictionary by UpdateAllObjectIDs()
            
            // CRITICAL: Preserve essential properties that affect visual positioning and behavior
            duplicatedObject.ObjectType = this.ObjectType;
            duplicatedObject.TargetPosition = this.TargetPosition;
            duplicatedObject.ObjectOriginOffset = this.ObjectOriginOffset;
            duplicatedObject.OriginalOriginOffset = this.OriginalOriginOffset;
            duplicatedObject.Alpha = this.Alpha;
            
            // Copy all keyframe dictionaries for animations
            duplicatedObject.PosXKeyframes = CloneKeyframeDictionary(this.PosXKeyframes);
            duplicatedObject.PosYKeyframes = CloneKeyframeDictionary(this.PosYKeyframes);
            duplicatedObject.PosZKeyframes = CloneKeyframeDictionary(this.PosZKeyframes);
            duplicatedObject.RotXKeyframes = CloneKeyframeDictionary(this.RotXKeyframes);
            duplicatedObject.RotYKeyframes = CloneKeyframeDictionary(this.RotYKeyframes);
            duplicatedObject.RotZKeyframes = CloneKeyframeDictionary(this.RotZKeyframes);
            duplicatedObject.ScaleXKeyframes = CloneKeyframeDictionary(this.ScaleXKeyframes);
            duplicatedObject.ScaleYKeyframes = CloneKeyframeDictionary(this.ScaleYKeyframes);
            duplicatedObject.ScaleZKeyframes = CloneKeyframeDictionary(this.ScaleZKeyframes);
            duplicatedObject.AlphaKeyframes = CloneKeyframeDictionary(this.AlphaKeyframes);
            
            // For hierarchical objects, we need to ensure children maintain their local positions
            // by properly setting their TargetPosition relative to the parent
            PreserveChildLocalPositions(this, duplicatedObject);
            
            // Ensure Visuals hierarchy is properly duplicated
            if (duplicatedObject.Visuals == null)
            {
                duplicatedObject.Visuals = new Marker3D();
                duplicatedObject.Visuals.Name = "Visuals";
            }
            
            // Initialize control properties for the new object
            duplicatedObject.InitializeControlProperties();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to duplicate SceneObject: {ex.Message}");
            return null;
        }
        
        return duplicatedObject;
    }
    
    private void PreserveChildLocalPositions(SceneObject originalParent, SceneObject duplicatedParent)
    {
        // This method ensures that child objects maintain their relative positioning
        // to the parent after duplication
        
        foreach (var child in originalParent.GetChildren())
        {
            if (child is SceneObject originalChild)
            {
                // Find the corresponding duplicated child
                SceneObject duplicatedChild = FindDuplicatedChild(originalChild, duplicatedParent);
                if (duplicatedChild != null)
                {
                    // Preserve essential properties for child objects
                    duplicatedChild.ObjectType = originalChild.ObjectType;
                    duplicatedChild.TargetPosition = originalChild.TargetPosition;
                    duplicatedChild.ObjectOriginOffset = originalChild.ObjectOriginOffset;
                    duplicatedChild.OriginalOriginOffset = originalChild.OriginalOriginOffset;
                    duplicatedChild.Alpha = originalChild.Alpha;
                    
                    // Copy keyframes
                    duplicatedChild.PosXKeyframes = CloneKeyframeDictionary(originalChild.PosXKeyframes);
                    duplicatedChild.PosYKeyframes = CloneKeyframeDictionary(originalChild.PosYKeyframes);
                    duplicatedChild.PosZKeyframes = CloneKeyframeDictionary(originalChild.PosZKeyframes);
                    duplicatedChild.RotXKeyframes = CloneKeyframeDictionary(originalChild.RotXKeyframes);
                    duplicatedChild.RotYKeyframes = CloneKeyframeDictionary(originalChild.RotYKeyframes);
                    duplicatedChild.RotZKeyframes = CloneKeyframeDictionary(originalChild.RotZKeyframes);
                    duplicatedChild.ScaleXKeyframes = CloneKeyframeDictionary(originalChild.ScaleXKeyframes);
                    duplicatedChild.ScaleYKeyframes = CloneKeyframeDictionary(originalChild.ScaleYKeyframes);
                    duplicatedChild.ScaleZKeyframes = CloneKeyframeDictionary(originalChild.ScaleZKeyframes);
                    duplicatedChild.AlphaKeyframes = CloneKeyframeDictionary(originalChild.AlphaKeyframes);
                    
                    // Recursively handle grandchildren
                    PreserveChildLocalPositions(originalChild, duplicatedChild);
                }
            }
        }
    }
    
    private SceneObject FindDuplicatedChild(SceneObject originalChild, SceneObject duplicatedParent)
    {
        // Find the corresponding duplicated child by matching names and hierarchy
        foreach (var child in duplicatedParent.GetChildren())
        {
            if (child is SceneObject sceneObject)
            {
                // Try to match by name first, then by hierarchy position
                if (child.Name == originalChild.Name ||
                    (child.GetIndex() == originalChild.GetIndex() &&
                     child.GetType() == originalChild.GetType()))
                {
                    return sceneObject;
                }
            }
        }
        
        // If we can't find by name or index, this might be a Godot-specific issue
        // Return the first SceneObject child as fallback
        foreach (var child in duplicatedParent.GetChildren())
        {
            if (child is SceneObject)
            {
                return child as SceneObject;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Helper method to deep clone a keyframe dictionary
    /// </summary>
    private static Dictionary<int, Keyframe> CloneKeyframeDictionary(Dictionary<int, Keyframe> source)
    {
        var cloned = new Dictionary<int, Keyframe>();
        foreach (var kvp in source)
        {
            cloned[kvp.Key] = kvp.Value.Clone();
        }
        return cloned;
    }
    
    private void CopyVisualsHierarchy(Node source, Node destination)
    {
        if (source == null || destination == null) return;
        
        var children = source.GetChildren().ToList();
        foreach (var child in children)
        {
            Node duplicatedChild = null;
            
            try
            {
                if (child is MeshInstance3D meshInstance)
                {
                    duplicatedChild = new MeshInstance3D();
                    duplicatedChild.Name = meshInstance.Name ?? "DuplicatedMesh";
                    
                    // Copy mesh if exists
                    if (meshInstance.Mesh != null)
                    {
                        var duplicatedMesh = meshInstance.Mesh.Duplicate() as Mesh;
                        if (duplicatedMesh != null)
                        {
                            ((MeshInstance3D)duplicatedChild).Mesh = duplicatedMesh;
                        }
                    }
                    
                    // Copy material override
                    if (meshInstance.MaterialOverride != null)
                    {
                        var duplicatedMaterial = meshInstance.MaterialOverride.Duplicate() as Material;
                        if (duplicatedMaterial != null)
                        {
                            ((MeshInstance3D)duplicatedChild).MaterialOverride = duplicatedMaterial;
                        }
                    }
                    
                    // Copy transform
                    ((Node3D)duplicatedChild).Transform = meshInstance.Transform;
                    
                    // Copy mesh instance specific properties
                    var originalMeshInstance = (MeshInstance3D)child;
                    var duplicatedMeshInstance = (MeshInstance3D)duplicatedChild;
                    duplicatedMeshInstance.SortingOffset = originalMeshInstance.SortingOffset;
                    duplicatedMeshInstance.Visible = originalMeshInstance.Visible;
                    duplicatedMeshInstance.CastShadow = originalMeshInstance.CastShadow;
                }
                else if (child is Node3D node3D)
                {
                    duplicatedChild = new Node3D();
                    duplicatedChild.Name = node3D.Name ?? "DuplicatedNode3D";
                    ((Node3D)duplicatedChild).Transform = node3D.Transform;
                }
                else if (child is Node regularNode)
                {
                    duplicatedChild = new Node();
                    duplicatedChild.Name = regularNode.Name ?? "DuplicatedNode";
                }
                
                if (duplicatedChild != null)
                {
                    destination.AddChild(duplicatedChild);
                    
                    // Recursively copy children
                    if (child.GetChildCount() > 0)
                    {
                        CopyVisualsHierarchy(child, duplicatedChild);
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error copying visual hierarchy for child {child.Name}: {ex.Message}");
            }
        }
    }
}