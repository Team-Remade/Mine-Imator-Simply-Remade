using Godot;

namespace MineImatorSimplyRemade;

public partial class TheView : SubViewportContainer
{
    [Export] public MineImatorWorld World { get; private set; }
    [Export] public WorldEnvironment Environment { get; private set; }
    [Export] public Camera3D WorkCamera { get; private set; }
}