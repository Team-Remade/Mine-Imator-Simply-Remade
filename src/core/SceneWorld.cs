using Godot;
using SimplyRemadeMI.renderer;

namespace SimplyRemadeMI.core;

public partial class SceneWorld : Node3D
{
    [Export] public Floor Floor { get; private set; }
    [Export] public Marker3D CameraLocation { get; private set; }
}