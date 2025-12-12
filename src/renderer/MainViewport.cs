using System;
using System.Collections.Generic;
using System.Linq;
using Gizmo3DPlugin;
using Godot;
using ImGuiGodot;
using ImGuiNET;
using SimplyRemadeMI.core;

namespace SimplyRemadeMI.renderer;

public partial class MainViewport : SubViewport
{
    [Export] public SceneWorld World { get; private set; }
    [Export] private Camera3D _camera;
    public Camera3D Camera => _camera;
    [Export] private ShaderMaterial PickingMaterial;
    [Export] public Background BackgroundObject { get; private set; }

    private bool Initialized;
    
    private Dictionary<int, SceneObject> Objects =  new();
    
    public SubViewport ObjectPicking = new();
    private Camera3D ObjectPickingCam = new();
    private Node3D ObjectPickingObject = new();

    public bool Controlled;
    private bool _debugView;
    
    public bool ShouldRenderToFile = false;
    public int RenderWidth = 0;
    public int RenderHeight = 0;
    public string RenderFilePath = "";

    public override void _Ready()
    {
        // Make this viewport handle input
        ObjectPicking.AnisotropicFilteringLevel = AnisotropicFiltering.Disabled;
        ObjectPicking.SetUpdateMode(UpdateMode.Disabled);
        SelectionManager.TransformGizmo.Visible = false;
        SelectionManager.TransformGizmo.Layers = 2;
        SelectionManager.TransformGizmo.Mode = Gizmo3D.ToolMode.Move;
        SelectionManager.TransformGizmo.TransformEnd += TransformGizmoOnTransformEnd;
        World.AddChild(SelectionManager.TransformGizmo);
        
        HandleInputLocally = true;
    }

    private void Init()
    {
        if (Initialized) return;
        
        var main = GetNode<Main>("/root/Main");
        
        if (main == null) return;
        
        ObjectPicking.OwnWorld3D = true;
        ObjectPicking.World3D = new World3D();
        main.Second.AddChild(ObjectPicking);
        ObjectPicking.RenderTargetUpdateMode = RenderTargetUpdateMode;
        
        ObjectPicking.AddChild(ObjectPickingCam);
        ObjectPicking.AddChild(ObjectPickingObject);
        
        Initialized = true;
    }

    private void UpdatePickingView()
    {
        ObjectPicking.SetUpdateMode(UpdateMode.Once);
    }

    private static IEnumerable<SceneObject> GetAllSceneObjects(Node node)
    {
        var sceneObjects = new List<SceneObject>();
        
        if (node is SceneObject sceneObject)
        {
            sceneObjects.Add(sceneObject);
        }
        
        foreach (var child in node.GetChildren())
        {
            sceneObjects.AddRange(GetAllSceneObjects(child));
        }
        
        return sceneObjects;
    }

    public void UpdatePicking()
    {
        Objects.Clear();
        
        foreach (var obj in ObjectPickingObject.GetChildren())
        {
            obj.QueueFree();
        }

        // Get all SceneObjects recursively from the World, filtering out disposed objects
        var allSceneObjects = GetAllSceneObjects(World)
            .Where(so => GodotObject.IsInstanceValid(so))
            .ToList();
        
        foreach (var so in allSceneObjects)
        {
            // Safety check for duplicate IDs
            if (Objects.ContainsKey(so.ID))
            {
                GD.PrintErr($"Duplicate object ID detected: {so.ID} for object {so.Name}. Regenerating ID.");
                // Find a new unique ID
                int newId = so.ID;
                while (Objects.ContainsKey(newId))
                {
                    newId++;
                }
                so.ID = newId;
            }
            
            // Create a new node for the picking system instead of duplicating the entire SceneObject
            var pickingNode = new Node3D();
            pickingNode.Name = so.Name + "_Picking";
            // Store the original object ID as metadata
            pickingNode.SetMeta("object_id", so.ID);
            ObjectPickingObject.AddChild(pickingNode);
            
            // Copy the transform from the original object
            pickingNode.Transform = so.Transform;
            
            // Add meshes from the original object's Visuals
            if (so.Visuals != null)
            {
                foreach (var child in so.Visuals.GetChildren())
                {
                    if (child is MeshInstance3D originalMesh)
                    {
                        var meshCopy = new MeshInstance3D();
                        meshCopy.Mesh = originalMesh.Mesh;
                        // Combine Visuals transform with the mesh's local transform
                        meshCopy.Transform = so.Visuals.Transform * originalMesh.Transform;
                        
                        // Apply picking material
                        meshCopy.MaterialOverride = PickingMaterial.Duplicate() as Material;
                        
                        // Encode ID across RGB channels to support up to 16,777,216 objects
                        // Red: ID % 256 (lowest 8 bits)
                        // Green: (ID / 256) % 256 (middle 8 bits)
                        // Blue: (ID / 65536) % 256 (highest 8 bits)
                        int id = so.ID;
                        float r = (id % 256) / 255f;
                        float g = ((id / 256) % 256) / 255f;
                        float b = ((id / 65536) % 256) / 255f;
                        var colorId = new Color(r, g, b, 0);
                        
                        var mat = (ShaderMaterial)meshCopy.MaterialOverride;
                        mat?.SetShaderParameter("object_id", colorId);
                        
                        pickingNode.AddChild(meshCopy);
                    }
                }
            }
                
            Objects.Add(so.ID, so);
        }
    }

    public void RemovePickingNode(int objectId)
    {
        // Remove from Objects dictionary
        Objects.Remove(objectId);
        
        // Find and remove the picking node with the matching object_id meta
        foreach (var child in ObjectPickingObject.GetChildren())
        {
            if (child is not Node3D pickingNode || !pickingNode.HasMeta("object_id")) continue;
            int id = (int)pickingNode.GetMeta("object_id");
            if (id != objectId) continue;
            pickingNode.QueueFree();
            break;
        }
    }

    public SceneObject CreateSceneObject(SceneObject.Type objectType = SceneObject.Type.Cube, MeshInstance3D meshInstance = null, string name = null, Node parent = null)
    {
        var sceneObject = Main.GetInstance().ObjectScene.Instantiate<SceneObject>();
        
        if (meshInstance == null && objectType == SceneObject.Type.Cube)
        {
            var cube = new MeshInstance3D();
            var cubeMesh = new BoxMesh();
            cube.Mesh = cubeMesh;
            cube.SortingOffset = 1;
            var material = new StandardMaterial3D();
            material.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
            material.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
            cube.Mesh.SurfaceSetMaterial(0, material);
            sceneObject.AddVisuals(cube);
        }
        else if (meshInstance != null)
        {
            sceneObject.AddVisuals(meshInstance);
        }
        else if (objectType == SceneObject.Type.Camera)
        {
            sceneObject = Main.GetInstance().CameraObjectScene.Instantiate<SceneObject>();
            sceneObject.OriginalOriginOffset = Vector3.Zero;
            sceneObject.ObjectOriginOffset = Vector3.Zero;
        }
        else if (objectType == SceneObject.Type.PointLight)
        {
            sceneObject = Main.GetInstance().SceneObjects["pointLight"].Instantiate<SceneObject>();
            sceneObject.OriginalOriginOffset = Vector3.Zero;
            sceneObject.ObjectOriginOffset = Vector3.Zero;
        }
        
        sceneObject.ObjectType = objectType;
        
        // Add to the provided parent or default to World
        if (parent != null)
        {
            parent.AddChild(sceneObject);
        }
        else
        {
            World.AddChild(sceneObject);
        }
        
        // Ensure the object has a GUID
        if (sceneObject.ObjectGuid == Guid.Empty)
        {
            sceneObject.ObjectGuid = Guid.NewGuid();
        }
        
        // Add to SceneObjects dictionary using GUID as key
        Main.GetInstance().UI.SceneTreePanel.SceneObjects.Add(sceneObject.ObjectGuid, sceneObject);
        
        sceneObject.Name = string.IsNullOrEmpty(name) ? $"{objectType}{Main.GetInstance().UI.SceneTreePanel.SceneObjects.Count}" : name;
        
        // Update all object IDs based on their indices in the SceneObjects dictionary
        Main.GetInstance().UI.SceneTreePanel.UpdateAllObjectIDs();
        
        return sceneObject;
    }

    private void TransformGizmoOnTransformEnd(int mode)
    {
        var selectedObject = Main.GetInstance().UI.SceneTreePanel.SelectedObject;
        if (selectedObject == null) return;

        switch (mode)
        {
            case 2:
                // For all objects, calculate TargetPosition from Position - InternalBoneOffset
                // For ModelPart: Position - InternalBoneOffset = TargetPosition
                // For others: Position - Vector3.Zero = TargetPosition
                Vector3 positionToSave = selectedObject.Position - selectedObject.InternalBoneOffset;
                
                Main.GetInstance().UI.Timeline.AddKeyframe("position.x", Main.GetInstance().UI.Timeline.CurrentFrame, positionToSave.X);
                Main.GetInstance().UI.Timeline.AddKeyframe("position.y", Main.GetInstance().UI.Timeline.CurrentFrame, positionToSave.Y);
                Main.GetInstance().UI.Timeline.AddKeyframe("position.z", Main.GetInstance().UI.Timeline.CurrentFrame, positionToSave.Z);
                break;
            case (int)Gizmo3D.ToolMode.Rotate - 1:
                Main.GetInstance().UI.Timeline.AddKeyframe("rotation.x", Main.GetInstance().UI.Timeline.CurrentFrame, selectedObject.RotationDegrees.X);
                Main.GetInstance().UI.Timeline.AddKeyframe("rotation.y", Main.GetInstance().UI.Timeline.CurrentFrame, selectedObject.RotationDegrees.Y);
                Main.GetInstance().UI.Timeline.AddKeyframe("rotation.z", Main.GetInstance().UI.Timeline.CurrentFrame, selectedObject.RotationDegrees.Z);
                break;
            case (int)Gizmo3D.ToolMode.Scale - 1:
                Main.GetInstance().UI.Timeline.AddKeyframe("scale.x", Main.GetInstance().UI.Timeline.CurrentFrame, selectedObject.Scale.X);
                Main.GetInstance().UI.Timeline.AddKeyframe("scale.y", Main.GetInstance().UI.Timeline.CurrentFrame, selectedObject.Scale.Y);
                Main.GetInstance().UI.Timeline.AddKeyframe("scale.z", Main.GetInstance().UI.Timeline.CurrentFrame, selectedObject.Scale.Z);
                break;
        }
    }

    public override void _Process(double delta)
    {
        Init();
        
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(Size.X, Size.Y));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(0, 0));

        if (_debugView)
        {
            var flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize |
                        ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse;

            if (Controlled) flags |=  ImGuiWindowFlags.NoInputs;
            
            if (ImGui.Begin("Debug Viewport", flags))
            {
                ImGuiGD.SubViewport(ObjectPicking);
                ImGui.End();
            }
        }

        BackgroundObject.BackgroundColor.Color = new Color(Main.GetInstance().UI.PropertiesPanel.Project.ClearColor.X, Main.GetInstance().UI.PropertiesPanel.Project.ClearColor.Y, Main.GetInstance().UI.PropertiesPanel.Project.ClearColor.Z);
        Main.GetInstance().Output.BackgroundObject.BackgroundColor.Color = new Color(Main.GetInstance().UI.PropertiesPanel.Project.ClearColor.X, Main.GetInstance().UI.PropertiesPanel.Project.ClearColor.Y, Main.GetInstance().UI.PropertiesPanel.Project.ClearColor.Z);

        if (Main.GetInstance().UI.PropertiesPanel.Project.StretchBackgroundToFit)
        {
            BackgroundObject.BackgroundTexture.StretchMode = TextureRect.StretchModeEnum.Scale;
            Main.GetInstance().Output.BackgroundObject.BackgroundTexture.StretchMode = TextureRect.StretchModeEnum.Scale;
        }
        else
        {
            BackgroundObject.BackgroundTexture.StretchMode = TextureRect.StretchModeEnum.Keep;
            Main.GetInstance().Output.BackgroundObject.BackgroundTexture.StretchMode = TextureRect.StretchModeEnum.Keep;
        }
        
        World.Floor.Visible = Main.GetInstance().UI.PropertiesPanel.Project.FloorVisible;
        
        ImGui.PopStyleVar();
        
        ObjectPicking.Size = Size;
        ObjectPickingCam.Transform = _camera.Transform;
        
        if (Input.IsActionJustPressed("SpawnDebugCube"))
        {
            CreateSceneObject(SceneObject.Type.Empty);
            UpdatePicking();
        }
        
        // Debug: Spawn 300 stone blocks to test RGB ID encoding
        if (Input.IsKeyPressed(Key.F9))
        {
            return;
            SpawnTestBlocks(300);
        }

        if (Input.IsActionJustPressed("Duplicate"))
        {
            DuplicateSelectedObject();
        }

        if (Input.IsActionJustPressed("ToggleDebugView"))
        {
            _debugView = !_debugView;
        }

        foreach (var child in ObjectPickingObject.GetChildren())
        {
            if (child is not Node3D pickingNode) continue;
            
            // Get the original object ID from metadata
            var objectId = (int)pickingNode.GetMeta("object_id");
            if (Objects.TryGetValue(objectId, out var parentObj) && GodotObject.IsInstanceValid(parentObj))
            {
                // Don't apply ObjectOriginOffset here - it's already baked into the mesh transforms from Visuals
                // Just follow the SceneObject's global transform
                pickingNode.GlobalTransform = parentObj.GlobalTransform;
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseButton mouseEvent:
                switch (mouseEvent.ButtonIndex)
                {
                    case MouseButton.Left when mouseEvent.Pressed:
                        PerformDepthTest(mouseEvent.Position);
                        break;
                    case MouseButton.Right when mouseEvent.Pressed:
                        Controlled = true;
                        break;
                    case MouseButton.Right when !mouseEvent.Pressed:
                        Controlled = false;
                        break;
                }

                break;
            case InputEventKey { Pressed: true } eventKey:
            {
                if (Controlled) return;
                switch (eventKey.Keycode)
                {
                    case Key.R:
                        // Check if selected object can be rotated
                        if (SelectionManager.Selection.Count > 0 && SelectionManager.Selection[0] is SceneObject rotatableObj && rotatableObj.Rotatable)
                        {
                            SelectionManager.TransformGizmo.Mode = Gizmo3D.ToolMode.Rotate;
                        }
                        break;
                    case Key.T:
                        SelectionManager.TransformGizmo.Mode = Gizmo3D.ToolMode.Move;
                        break;
                    case Key.S:
                        // Only allow scale mode if selected object is scalable (not a camera)
                        if (SelectionManager.Selection.Count > 0 && SelectionManager.Selection[0] is SceneObject scalableObj && scalableObj.Scalable)
                        {
                            SelectionManager.TransformGizmo.Mode = Gizmo3D.ToolMode.Scale;
                        }
                        break;
                    case Key.G:
                        SelectionManager.TransformGizmo.UseLocalSpace = !SelectionManager.TransformGizmo.UseLocalSpace;
                        break;
                    case Key.X:
                    {
                        // Delete selected object with X key
                        if (SelectionManager.Selection.Count > 0)
                        {
                            Main.GetInstance().UI.ShowDeleteConfirmationDialog();
                        }

                        break;
                    }
                }

                break;
            }
        }
    }

    private void PerformDepthTest(Vector2 mousePosition)
    {
        if (SelectionManager.TransformGizmo.Hovering) return;

        // Get the image from the object picking viewport
        var image = ObjectPicking.GetTexture().GetImage();
        if (image == null)
        {
            GD.Print("No image available from object picking viewport");
            return;
        }

        // Use mouse position directly since ObjectPicking.Size == Size (no scaling needed)
        var viewportPos = new Vector2I(
            (int)mousePosition.X,
            (int)mousePosition.Y
        );

        // Ensure coordinates are within bounds
        if (viewportPos.X < 0 || viewportPos.X >= ObjectPicking.Size.X ||
            viewportPos.Y < 0 || viewportPos.Y >= ObjectPicking.Size.Y)
        {
            return;
        }

        // Read the pixel color at the mouse position
        var pixelColor = image.GetPixel(viewportPos.X, viewportPos.Y);
        
        // Decode ID from RGB channels to support up to 16,777,216 objects
        // Reconstruct: ID = R + (G * 256) + (B * 65536)
        if (pixelColor.R > 0 || pixelColor.G > 0 || pixelColor.B > 0)
        {
            int r = Mathf.RoundToInt(pixelColor.R * 255f);
            int g = Mathf.RoundToInt(pixelColor.G * 255f);
            int b = Mathf.RoundToInt(pixelColor.B * 255f);
            int objectId = r + (g * 256) + (b * 65536);

            if (!Objects.TryGetValue(objectId, out var sceneObject)) return;
            SceneObject objectToSelect;
                
            // If no other scene objects are selected, follow the parent chain and select the first scene object
            objectToSelect = SelectionManager.Selection.Count == 0 ? FindFirstSceneObjectInChain(sceneObject) :
                // If other objects are selected, select the clicked object directly
                sceneObject;
            
            // Update SceneTreePanel selection using GUID
            Main.GetInstance().UI.SceneTreePanel.SelectedObjectGuid = objectToSelect.ObjectGuid;
                
            // Update SelectionManager
            SelectionManager.ClearSelection();
            SelectionManager.Selection.Add(objectToSelect);
            SelectionManager.TransformGizmo.Visible = true;
            SelectionManager.TransformGizmo.Position = objectToSelect.Position;
            SelectionManager.TransformGizmo.Select(objectToSelect);
        }
        else
        {
            // No object selected (clicked on background)
            Main.GetInstance().UI.SceneTreePanel.SelectedObjectGuid = null;
            SelectionManager.ClearSelection();
        }
    }

    private static SceneObject FindSceneObject(Node3D node)
    {
        // Traverse up the hierarchy to find a SceneObject
        var current = node;
        while (current != null)
        {
            if (current is SceneObject sceneObject)
            {
                return sceneObject;
            }
            current = current.GetParent() as Node3D;
        }
        return null;
    }

    private static SceneObject FindFirstSceneObjectInChain(SceneObject sceneObject)
    {
        // Follow the parent chain upwards to find the first SceneObject (root of the hierarchy)
        var current = sceneObject;
        SceneObject rootSceneObject = sceneObject;
        
        while (current != null)
        {
            var parent = current.GetParent();
            if (parent is SceneObject parentSceneObject)
            {
                rootSceneObject = parentSceneObject;
                current = parentSceneObject;
            }
            else
            {
                break;
            }
        }
        
        return rootSceneObject;
    }

    public void LoadBackgroundImage(string imagePath)
    {
        var img = new Image();
        img.Load(imagePath);
        
        var imgTex = new ImageTexture();
        imgTex.SetImage(img);
        
        BackgroundObject.BackgroundTexture.Texture = imgTex;
        Main.GetInstance().Output.BackgroundObject.BackgroundTexture.Texture = imgTex;
    }

    public void ClearBackgroundImage()
    {
        BackgroundObject.BackgroundTexture.Texture = null;
        Main.GetInstance().Output.BackgroundObject.BackgroundTexture.Texture = null;
    }

    private string GenerateUniqueName(string baseName)
    {
        // Extract the base name and any existing number suffix
        string nameBase = baseName;
        int startCounter = 1;
        
        // Check if the name ends with a number
        int digitStartIndex = baseName.Length;
        while (digitStartIndex > 0 && char.IsDigit(baseName[digitStartIndex - 1]))
        {
            digitStartIndex--;
        }
        
        // If we found digits at the end, extract them
        if (digitStartIndex < baseName.Length)
        {
            nameBase = baseName.Substring(0, digitStartIndex);
            if (int.TryParse(baseName.Substring(digitStartIndex), out int existingNumber))
            {
                startCounter = existingNumber + 1;
            }
        }
        
        // Start from the extracted/calculated counter and find the next available name
        int counter = startCounter;
        string newName;
        bool nameExists;
        
        do
        {
            newName = $"{nameBase}{counter}";
            nameExists = Main.GetInstance().UI.SceneTreePanel.SceneObjects.Values
                .Any(obj => obj.Name == newName);
            counter++;
        } while (nameExists);
        
        return newName;
    }

    private void DuplicateSelectedObject()
    {
        // Check if we have selected objects to duplicate
        if (SelectionManager.Selection.Count == 0) return;
        
        var duplicatedObjects = new List<SceneObject>();
        
        // Duplicate each selected object
        for (int i = 0; i < SelectionManager.Selection.Count; i++)
        {
            var selectedObject = SelectionManager.Selection[i];
            if (selectedObject is not SceneObject sceneObject) continue;
            
            // Create a deep duplicate of the scene object with all its children and keyframes
            // ID will be assigned based on index after adding to SceneObjects dictionary
            var duplicatedObject = sceneObject.DeepDuplicateSceneObject();
            
            if (duplicatedObject == null)
            {
                GD.PrintErr($"Failed to duplicate SceneObject {sceneObject.Name}");
                continue;
            }
            
            // Ensure the duplicated object has a unique GUID
            if (duplicatedObject.ObjectGuid == Guid.Empty || duplicatedObject.ObjectGuid == sceneObject.ObjectGuid)
            {
                duplicatedObject.ObjectGuid = Guid.NewGuid();
            }
            
            // Position the duplicate slightly offset from the original and stagger multiple duplicates
            Vector3 offset = new Vector3((i + 1) * 0.5f, 0, (i + 1) * 0.5f);
            duplicatedObject.Position = sceneObject.Position + offset;
            
            // Add to the scene tree panel using GUID as key
            Main.GetInstance().UI.SceneTreePanel.SceneObjects.Add(duplicatedObject.ObjectGuid, duplicatedObject);
            
            // Add to the world
            World.AddChild(duplicatedObject);
            
            // Update the object name - preserve original name and add a suffix
            duplicatedObject.Name = GenerateUniqueName(sceneObject.Name);
            
            duplicatedObjects.Add(duplicatedObject);
        }
        
        // Update all object IDs based on their indices in the SceneObjects dictionary
        Main.GetInstance().UI.SceneTreePanel.UpdateAllObjectIDs();
        
        // Select all newly duplicated objects
        SelectionManager.ClearSelection();
        foreach (var duplicatedObject in duplicatedObjects)
        {
            SelectionManager.Selection.Add(duplicatedObject);
        }
        
        if (duplicatedObjects.Count > 0)
        {
            SelectionManager.TransformGizmo.Visible = true;
            // Position gizmo at the first duplicated object for reference
            SelectionManager.TransformGizmo.Position = duplicatedObjects[0].Position;
            SelectionManager.TransformGizmo.Select(duplicatedObjects[0]);
            
            // Update selected GUID to the last duplicated object
            Main.GetInstance().UI.SceneTreePanel.SelectedObjectGuid = duplicatedObjects[duplicatedObjects.Count - 1].ObjectGuid;
        }
    }
    
    private void SpawnTestBlocks(int count)
    {
        GD.Print($"Spawning {count} test blocks to verify RGB ID encoding...");
        
        // Calculate grid dimensions (roughly square)
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(count));
        float spacing = 1.5f;
        
        for (int i = 0; i < count; i++)
        {
            int x = i % gridSize;
            int z = i / gridSize;
            
            // Use Type.Cube to create blocks with visible BoxMesh
            var block = CreateSceneObject(SceneObject.Type.Cube);
            block.Name = $"TestBlock_{i}";
            block.Position = new Vector3(x * spacing, 0, z * spacing);
        }
        
        UpdatePicking();
        GD.Print($"Spawned {count} blocks with meshes. Check debug view (toggle with assigned key) to see ID color encoding.");
        GD.Print("IDs 0-255 should be red gradient, 256-511 should start showing green, etc.");
    }
}