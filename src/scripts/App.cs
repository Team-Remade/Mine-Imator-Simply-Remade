using Godot;

namespace MineImatorSimplyRemade.scripts;

public partial class App : Node2D
{
    public static App Instance  { get; private set; }
    
    [Export] public MessageWindow MessageWindow  { get; private set; }
    public UserInterface UserInterface  { get; set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void CenterScreen()
    {
        var screenCenter = DisplayServer.ScreenGetPosition() + DisplayServer.ScreenGetSize() / 2;
        var windowSize = GetWindow().GetSizeWithDecorations();
        GetWindow().MoveToCenter();
    }
}