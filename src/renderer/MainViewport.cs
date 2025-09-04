using Gizmo3DPlugin;
using Godot;
using SimplyRemadeMI.core;

namespace SimplyRemadeMI.renderer;

public partial class MainViewport : SubViewport
{
    [Export] public SceneWorld World { get; private set; }
    [Export] private Camera3D _camera;

    public bool Controlled;

    public override void _Ready()
    {
        // Make this viewport handle input
        SelectionManager.TransformGizmo.Visible = false;
        SelectionManager.TransformGizmo.Mode = Gizmo3D.ToolMode.Move;
        SelectionManager.TransformGizmo.TransformEnd += TransformGizmoOnTransformEnd;
        World.AddChild(SelectionManager.TransformGizmo);
        
        HandleInputLocally = true;
    }

    private void CreateSceneObject()
    {
        var sceneObject = Main.GetInstance().ObjectScene.Instantiate<SceneObject>();
        var cube = new MeshInstance3D();
        var cubeMesh = new BoxMesh();
        cube.Mesh = cubeMesh;
        sceneObject.ObjectType = SceneObject.Type.Cube;
        var material = new StandardMaterial3D();
        material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        cube.MaterialOverride = material;
        sceneObject.AddVisuals(cube);
        World.AddChild(sceneObject);
            
        Main.GetInstance().UI.sceneTreePanel.SceneObjects.Add(sceneObject);
        sceneObject.Name = $"Cube{Main.GetInstance().UI.sceneTreePanel.SceneObjects.Count}";
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
        if (Input.IsActionJustPressed("SpawnDebugCube"))
        {
            CreateSceneObject();
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
        
        if (_camera == null)
        {
            GD.PrintErr("Camera is not initialized");
            return;
        }

        if (World == null)
        {
            GD.PrintErr("World is not initialized");
            return;
        }

        // Create a ray from the camera through the mouse position
        var from = _camera.ProjectRayOrigin(mousePosition);
        var to = from + _camera.ProjectRayNormal(mousePosition) * 1000;
        var rayDir = (to - from).Normalized();

        SceneObject closestHit = null;
        float closestDistance = float.MaxValue;

        // Iterate through all SceneObjects in the World
        foreach (Node child in World.GetChildren())
        {
            if (child is SceneObject sceneObject)
            {
                // Check all MeshInstance3D children of the SceneObject
                foreach (Node node in sceneObject.GetChildren())
                {
                    if (node is MeshInstance3D meshInstance)
                    {
                        // Get the global AABB of the mesh instance
                        var aabb = meshInstance.GetAabb();
                        var globalTransform = meshInstance.GlobalTransform;
                        var globalAabb = new Aabb(globalTransform * aabb.Position, aabb.Size * globalTransform.Basis.Scale);

                        // Check ray intersection with AABB using manual calculation
                        if (RayIntersectsAabb(from, rayDir, globalAabb, out float distance))
                        {
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestHit = sceneObject;
                            }
                        }
                    }
                }
            }
        }

        if (closestHit != null)
        {
            SelectionManager.Selection.Add(closestHit);
            SelectionManager.QuerySelection(Input.IsKeyPressed(Key.Ctrl));
            Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex = Main.GetInstance().UI.sceneTreePanel.SceneObjects.IndexOf(closestHit);
        }
        else
        {
            SelectionManager.ClearSelection();
        }
    }

    private bool RayIntersectsAabb(Vector3 rayOrigin, Vector3 rayDir, Aabb aabb, out float distance)
    {
        distance = 0;
        var tmin = 0.0f;
        var tmax = float.MaxValue;

        for (int i = 0; i < 3; i++)
        {
            if (Mathf.Abs(rayDir[i]) < float.Epsilon)
            {
                // Ray is parallel to this axis
                if (rayOrigin[i] < aabb.Position[i] || rayOrigin[i] > aabb.Position[i] + aabb.Size[i])
                {
                    return false;
                }
            }
            else
            {
                var ood = 1.0f / rayDir[i];
                var t1 = (aabb.Position[i] - rayOrigin[i]) * ood;
                var t2 = (aabb.Position[i] + aabb.Size[i] - rayOrigin[i]) * ood;

                if (t1 > t2)
                {
                    (t1, t2) = (t2, t1);
                }

                tmin = Mathf.Max(tmin, t1);
                tmax = Mathf.Min(tmax, t2);

                if (tmin > tmax)
                {
                    return false;
                }
            }
        }

        distance = tmin;
        return true;
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
}