using Godot;

namespace SimplyRemadeMI.renderer;

public partial class RenderOutput : SubViewport
{
    [Export] public Camera3D MainCamera { get; private set; }
    
    [Export] public Background BackgroundObject { get; private set; }
    [Export] private Control FrameCounter { get; set; }

    public bool PreviewMode { get; set; } = true;
    public bool RenderedMode { get; set; } = false;

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ToggleRenderMode"))
        {
            RenderedMode = !RenderedMode;
        }

        if (RenderedMode)
        {
            DebugDraw = DebugDrawEnum.Disabled;
            Msaa3D = Msaa.Msaa8X;
        }
        else
        {
            DebugDraw = DebugDrawEnum.Unshaded;
            Msaa3D = Msaa.Disabled;
        }

        if (RenderedMode && PreviewMode)
        {
            FrameCounter.Visible = true;
        }
        else
        {
            FrameCounter.Visible = false;
        }
        
        if (PreviewMode)
        {
            return;
            
            var aspect = Main.GetInstance().UI.propertiesPanel.project._aspectRatio;
            
            // Fixed width of 480 pixels, calculate height based on aspect ratio
            int width = 480;
            int height = (int)(width / aspect);
            
            // Ensure height is at least 1 to avoid division by zero or negative values
            if (height < 1)
            {
                height = 1;
            }
            
            // Set the viewport size
            Size = new Vector2I(width, height);
        }

        if (!PreviewMode)
        {
            Size = new Vector2I(Main.GetInstance().UI.propertiesPanel.project._projectRenderWidth, Main.GetInstance().UI.propertiesPanel.project._projectRenderHeight);
        }
    }
}