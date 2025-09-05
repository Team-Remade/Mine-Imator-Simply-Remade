using System;
using System.Collections.Generic;
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
    [Export] private ShaderMaterial PickingMaterial;
    [Export] public Background BackgroundObject { get; private set; }

    private bool Initialized;
    
    private Dictionary<int, SceneObject> Objects =  new Dictionary<int, SceneObject>();
    
    public SubViewport ObjectPicking = new SubViewport();
    private Camera3D ObjectPickingCam = new Camera3D();
    private Node3D ObjectPickingObject = new();

    public bool Controlled;
    private bool _debugView;
    
    public bool _shouldRenderToFile = false;
    public int _renderWidth = 0;
    public int _renderHeight = 0;
    public string _renderFilePath = "";

    public override void _Ready()
    {
        // Make this viewport handle input
        SelectionManager.TransformGizmo.Visible = false;
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

    public void UpdatePicking()
    {
        Objects.Clear();
        
        foreach (var obj in ObjectPickingObject.GetChildren())
        {
            obj.QueueFree();
        }

        foreach (var obj in World.GetChildren())
        {
            if (obj is SceneObject so)
            {
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
                            meshCopy.Transform = originalMesh.Transform;
                            
                            // Apply picking material
                            meshCopy.MaterialOverride = PickingMaterial.Duplicate() as Material;
                            
                            var id = (float)so.ID;
                            var colorId = new Color(id / 255f, 1.0f, 1.0f, 0);
                            
                            var mat = (ShaderMaterial)meshCopy.MaterialOverride;
                            mat.SetShaderParameter("object_id", colorId);
                            
                            pickingNode.AddChild(meshCopy);
                        }
                    }
                }
                    
                Objects.Add(so.ID, so);
            }
        }
    }

    private void CreateSceneObject()
    {
        var sceneObject = Main.GetInstance().ObjectScene.Instantiate<SceneObject>();
        Main.GetInstance().UI.sceneTreePanel.SceneObjects.Add(sceneObject);
        var cube = new MeshInstance3D();
        var cubeMesh = new BoxMesh();
        cube.Mesh = cubeMesh;
        sceneObject.ObjectType = SceneObject.Type.Cube;
        var material = new StandardMaterial3D();
        material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        cube.MaterialOverride = material;
        sceneObject.AddVisuals(cube);
        World.AddChild(sceneObject);
        
        sceneObject.Name = $"Cube{Main.GetInstance().UI.sceneTreePanel.SceneObjects.IndexOf(sceneObject)}";
        sceneObject.ID = Main.GetInstance().UI.sceneTreePanel.SceneObjects.IndexOf(sceneObject) + 1;
    }

    private void TransformGizmoOnTransformEnd(int mode)
    {
        switch (mode)
        {
            case 2:
                Main.GetInstance().UI.timeline.AddKeyframe("position.x", Main.GetInstance().UI.timeline.CurrentFrame, Main.GetInstance().UI.sceneTreePanel.SceneObjects[Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex].Position.X);
                Main.GetInstance().UI.timeline.AddKeyframe("position.y", Main.GetInstance().UI.timeline.CurrentFrame, Main.GetInstance().UI.sceneTreePanel.SceneObjects[Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex].Position.Y);
                Main.GetInstance().UI.timeline.AddKeyframe("position.z", Main.GetInstance().UI.timeline.CurrentFrame, Main.GetInstance().UI.sceneTreePanel.SceneObjects[Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex].Position.Z);
                break;
            case (int)Gizmo3D.ToolMode.Rotate - 1:
                Main.GetInstance().UI.timeline.AddKeyframe("rotation.x", Main.GetInstance().UI.timeline.CurrentFrame, Main.GetInstance().UI.sceneTreePanel.SceneObjects[Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex].RotationDegrees.X);
                Main.GetInstance().UI.timeline.AddKeyframe("rotation.y", Main.GetInstance().UI.timeline.CurrentFrame, Main.GetInstance().UI.sceneTreePanel.SceneObjects[Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex].RotationDegrees.Y);
                Main.GetInstance().UI.timeline.AddKeyframe("rotation.z", Main.GetInstance().UI.timeline.CurrentFrame, Main.GetInstance().UI.sceneTreePanel.SceneObjects[Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex].RotationDegrees.Z);
                break;
            case (int)Gizmo3D.ToolMode.Scale - 1:
                Main.GetInstance().UI.timeline.AddKeyframe("scale.x", Main.GetInstance().UI.timeline.CurrentFrame, Main.GetInstance().UI.sceneTreePanel.SceneObjects[Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex].Scale.X);
                Main.GetInstance().UI.timeline.AddKeyframe("scale.y", Main.GetInstance().UI.timeline.CurrentFrame, Main.GetInstance().UI.sceneTreePanel.SceneObjects[Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex].Scale.Y);
                Main.GetInstance().UI.timeline.AddKeyframe("scale.z", Main.GetInstance().UI.timeline.CurrentFrame, Main.GetInstance().UI.sceneTreePanel.SceneObjects[Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex].Scale.Z);
                break;
        }
    }

    public override void _Process(double delta)
    {
        Init();
        
        Main.GetInstance().Output.MainCamera.GlobalTransform = _camera.GlobalTransform;
        
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(Size.X, Size.Y));
        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(0, 0));

        if (_debugView)
        {
            if (ImGui.Begin("Debug Viewport", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse))
            {
                ImGuiGD.SubViewport(ObjectPicking);
                ImGui.End();
            }
        }

        BackgroundObject.BackgroundColor.Color = new Color(Main.GetInstance().UI.propertiesPanel.project._clearColor.X, Main.GetInstance().UI.propertiesPanel.project._clearColor.Y, Main.GetInstance().UI.propertiesPanel.project._clearColor.Z);
        Main.GetInstance().Output.BackgroundObject.BackgroundColor.Color = new Color(Main.GetInstance().UI.propertiesPanel.project._clearColor.X, Main.GetInstance().UI.propertiesPanel.project._clearColor.Y, Main.GetInstance().UI.propertiesPanel.project._clearColor.Z);

        if (Main.GetInstance().UI.propertiesPanel.project._stretchBackgroundToFit)
        {
            BackgroundObject.BackgroundTexture.StretchMode = TextureRect.StretchModeEnum.Scale;
            Main.GetInstance().Output.BackgroundObject.BackgroundTexture.StretchMode = TextureRect.StretchModeEnum.Scale;
        }
        else
        {
            BackgroundObject.BackgroundTexture.StretchMode = TextureRect.StretchModeEnum.Keep;
            Main.GetInstance().Output.BackgroundObject.BackgroundTexture.StretchMode = TextureRect.StretchModeEnum.Keep;
        }
        
        World.Floor.Visible = Main.GetInstance().UI.propertiesPanel.project._floorVisible;
        
        ImGui.PopStyleVar();
        
        ObjectPicking.Size = Size;
        ObjectPickingCam.Transform = _camera.Transform;
        
        if (Input.IsActionJustPressed("SpawnDebugCube"))
        {
            CreateSceneObject();
            UpdatePicking();
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
            if (Objects.TryGetValue(objectId, out var parentObj))
            {
                pickingNode.GlobalTransform = parentObj.GlobalTransform * new Transform3D(Basis.Identity, parentObj.ObjectOriginOffset);
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                PerformDepthTest(mouseEvent.Position);
            }

            if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
            {
                Controlled = true;
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Right && !mouseEvent.Pressed)
            {
                Controlled = false;
            }
        }

        if (@event is InputEventKey eventKey && eventKey.Pressed)
        {
            if (Controlled) return;
            if (eventKey.Keycode == Key.R)
            {
                SelectionManager.TransformGizmo.Mode = Gizmo3D.ToolMode.Rotate;
            } else if (eventKey.Keycode == Key.T)
            {
                SelectionManager.TransformGizmo.Mode = Gizmo3D.ToolMode.Move;
            } else if (eventKey.Keycode == Key.S)
            {
                SelectionManager.TransformGizmo.Mode = Gizmo3D.ToolMode.Scale;
            }
            else if (eventKey.Keycode == Key.G)
            {
                SelectionManager.TransformGizmo.UseLocalSpace = !SelectionManager.TransformGizmo.UseLocalSpace;
            }
        }
    }

    private void PerformDepthTest(Vector2 mousePosition)
    {
        if (SelectionManager.TransformGizmo.Hovering)
        {
            return;
        }

        // Get the image from the object picking viewport
        var image = ObjectPicking.GetTexture().GetImage();
        if (image == null)
        {
            GD.Print("No image available from object picking viewport");
            return;
        }

        // Convert mouse position to viewport coordinates
        var viewportPos = new Vector2I(
            (int)(mousePosition.X * ObjectPicking.Size.X / Size.X),
            (int)(mousePosition.Y * ObjectPicking.Size.Y / Size.Y)
        );

        // Ensure coordinates are within bounds
        if (viewportPos.X < 0 || viewportPos.X >= ObjectPicking.Size.X ||
            viewportPos.Y < 0 || viewportPos.Y >= ObjectPicking.Size.Y)
        {
            return;
        }

        // Read the pixel color at the mouse position
        var pixelColor = image.GetPixel(viewportPos.X, viewportPos.Y);
        
        // Convert color back to object ID (red channel contains ID/255)
        if (pixelColor.R > 0)
        {
            int objectId = Mathf.RoundToInt(pixelColor.R * 255f);
            
            if (Objects.TryGetValue(objectId, out var sceneObject))
            {
                // Select the object - convert from 1-based objectId to 0-based index
                Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex = objectId - 1;
                
                // Update SelectionManager
                SelectionManager.ClearSelection();
                SelectionManager.Selection.Add(sceneObject);
                SelectionManager.TransformGizmo.Visible = true;
                SelectionManager.TransformGizmo.Position = sceneObject.Position;
                SelectionManager.TransformGizmo.Select(sceneObject);
                GD.Print($"Selected object with ID: {objectId}");
            }
            else
            {
                GD.Print($"No object found with ID: {objectId}");
            }
        }
        else
        {
            // No object selected (clicked on background)
            Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex = -1;
            SelectionManager.ClearSelection();
            GD.Print("No object selected");
        }
    }

    private SceneObject FindSceneObject(Node3D node)
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
}