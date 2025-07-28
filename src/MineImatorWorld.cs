using Godot;

namespace MineImatorSimplyRemade;

public partial class MineImatorWorld : Node3D
{
    [Export] public MeshInstance3D GroundMesh { get; private set; }
}