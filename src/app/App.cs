using Godot;

namespace MineImatorSimplyRemade.app;

public partial class App : Node2D
{
    public static App Instance { get; private set; }
    
    public override void _Ready()
    {
        Instance = this;
        
        AppEvent.Create();
    }
}