using Godot;
using ImGuiNET;
using SimplyRemadeMI.ui;
using MenuBar = SimplyRemadeMI.ui.MenuBar;

namespace SimplyRemadeMI.renderer;

public class UIRenderer
{
    public SceneTreePanel sceneTreePanel { get; private set; } = new();
    private PropertiesPanel propertiesPanel = new();
    private Timeline timeline = new();
    private ViewportObject ViewportObject;
    private MenuBar menuBar = new();

    public UIRenderer(ViewportObject viewportObject)
    {
        ViewportObject = viewportObject;
        sceneTreePanel.World = Main.GetInstance().MainViewport.World;
    }
    
    public void Render()
    {
        DrawUI();
    }

    private void DrawUI()
    {
        var size = Main.GetInstance().WindowSize;

        var sceneTreeHeight = size.Y / 3;
        var scenePropertiesHeight = size.Y - sceneTreeHeight;
        var menuBarHeight = ImGui.GetFrameHeight();
        
        sceneTreePanel.Render(new Vector2I(size.X - 280, (int)menuBarHeight), new Vector2I(280, sceneTreeHeight - (int)menuBarHeight));
        propertiesPanel.Render(new Vector2I(size.X - 280, sceneTreeHeight), new Vector2I(280, scenePropertiesHeight));
        timeline.Render(new Vector2I(0, size.Y - 200), new Vector2I(size.X - 280, 200));
        ViewportObject.Render(new Vector2I(0, (int)menuBarHeight), new Vector2I(size.X - 280, size.Y - 200 - (int)menuBarHeight));
        menuBar.Render();
    }
}