using Godot;

namespace SimplyRemadeMI.renderer;

public partial class Background : CanvasLayer
{
    [Export] public ColorRect BackgroundColor { get; private set; }
    [Export] public TextureRect BackgroundTexture { get; private set; }
}