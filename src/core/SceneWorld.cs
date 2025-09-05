using Godot;
using SimplyRemadeMI.renderer;

namespace SimplyRemadeMI.core;

public partial class SceneWorld : Node3D
{
    [Export] public Floor Floor { get; private set; }
}