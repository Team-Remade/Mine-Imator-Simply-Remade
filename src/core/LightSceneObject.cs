using Godot;

namespace SimplyRemadeMI.core;

public partial class LightSceneObject : SceneObject
{
    [Export] public Light3D Light;

    public void SetRange(float val)
    {
        var light = Light as OmniLight3D;
        light.OmniRange = val;
    }

    public void SetColor(Color color)
    {
        Light.LightColor = color;
    }
}