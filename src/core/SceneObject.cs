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
        // Generate unique GUID for this object if not already set
        if (ObjectGuid == Guid.Empty)
        {
            ObjectGuid = Guid.NewGuid();
        }
        
        SelectionManager.TransformGizmo.TransformEnd += TransformGizmoOnTransformEnd;
        InitializeControlProperties();
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

    public SceneObject DeepDuplicateSceneObject(int newId = -1)
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
            
            // Update the ID if requested
            if (newId > 0)
            {
                duplicatedObject.ID = newId;
            }
            else if (duplicatedObject.ID == 0)
            {
                // If ID is 0, assign a new unique ID
                duplicatedObject.ID = GetUniqueSceneObjectId();
            }
            
            // CRITICAL: Preserve essential properties that affect visual positioning and behavior
            duplicatedObject.TargetPosition = this.TargetPosition;
            duplicatedObject.ObjectOriginOffset = this.ObjectOriginOffset;
            duplicatedObject.OriginalOriginOffset = this.OriginalOriginOffset;
            duplicatedObject.Alpha = this.Alpha;
            
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
            
            // Recursively update IDs for all child SceneObjects to ensure uniqueness
            UpdateChildSceneObjectIds(duplicatedObject);
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
                    // The key is to preserve the TARGET POSITION which drives the local positioning
                    // Don't copy the absolute Position, as that's the world position
                    duplicatedChild.TargetPosition = originalChild.TargetPosition;
                    
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
    
    private int GetUniqueSceneObjectId()
    {
        // Get all existing SceneObjects to find the maximum ID
        int maxId = 0;
        var allSceneObjects = GetAllSceneObjects();
        foreach (var obj in allSceneObjects)
        {
            maxId = Math.Max(maxId, obj.ID);
        }
        return maxId + 1;
    }
    
    private static List<SceneObject> GetAllSceneObjects()
    {
        var sceneObjects = new List<SceneObject>();
        if (Main.GetInstance()?.UI?.SceneTreePanel?.SceneObjects != null)
        {
            sceneObjects.AddRange(Main.GetInstance().UI.SceneTreePanel.SceneObjects.Values);
        }
        return sceneObjects;
    }
    
    private void UpdateChildSceneObjectIds(SceneObject rootObject)
    {
        int baseId = rootObject.ID;
        int childIndex = 1;
        
        // Recursively update all child SceneObjects
        foreach (var child in rootObject.GetChildren())
        {
            if (child is SceneObject childSceneObject)
            {
                // Use a strategy that ensures no conflicts across different root objects:
                // - Parent gets ID N
                // - Children get IDs starting at N * 50000 + 1
                // - This ensures child IDs are far enough apart to avoid conflicts
                // - For example:
                //   - Root ID 1: children get 50001, 50002, etc.
                //   - Root ID 2: children get 100001, 100002, etc.
                //   - Root ID 3: children get 150001, 150002, etc.
                int childId = baseId * 50000 + childIndex;
                
                // Ensure this ID doesn't conflict with existing objects
                while (IsIdConflict(childId))
                {
                    childId++;
                }
                
                childSceneObject.ID = childId;
                childIndex++;
                
                // Recursively update grandchildren with smaller offsets
                UpdateChildSceneObjectIds(childSceneObject);
            }
        }
    }
    
    private bool IsIdConflict(int testId)
    {
        var allSceneObjects = GetAllSceneObjects();
        return allSceneObjects.Any(obj => obj.ID == testId);
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