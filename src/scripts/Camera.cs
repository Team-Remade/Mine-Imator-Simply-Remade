using Godot;

namespace MineImatorSimplyRemade.scripts;

public static class Camera
{
    public static void Reset()
    {
        App.Instance.UserInterface.Viewport.WorkCamera.RotationDegrees = new Vector3(315, 25, 0);
        App.Instance.UserInterface.Viewport.WorkCamera.Fov = 100;
        App.Instance.UserInterface.Viewport.WorkCamera.Position = new Vector3(0, 16, 0);
    }
}