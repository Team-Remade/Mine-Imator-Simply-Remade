using Godot;
using SimplyRemadeMI.core;

namespace SimplyRemadeMI.renderer;

public partial class MainViewport : SubViewport
{
    [Export] public SceneWorld World { get; private set; }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("SpawnDebugCube"))
        {
            var sceneObject = new SceneObject();
            var cube = new MeshInstance3D();
            var cubeMesh = new BoxMesh();
            cube.Mesh = cubeMesh;
            sceneObject.ObjectType = SceneObject.Type.Cube;
            cube.MaterialOverride = new StandardMaterial3D();
            sceneObject.AddChild(cube);
            World.AddChild(sceneObject);
            
            Main.GetInstance().UI.sceneTreePanel.SceneObjects.Add(sceneObject);
            sceneObject.Name = $"Cube{Main.GetInstance().UI.sceneTreePanel.SceneObjects.Count}";
        }
    }
}