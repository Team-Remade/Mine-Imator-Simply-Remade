using Godot;

namespace MineImatorSimplyRemade.scripts;

public static class Lights
{
    public static void Reset()
    {
        for (int i = 0; i < App.Instance.UserInterface.Project.LightsAmount; i++)
        {
            Remove(App.Instance.UserInterface.Project.Lights[i], i);
        }
        
        AddLight(new Vector3I(-1500, 1000, -1500), 10000, Colors.White);
        AddLight(new Vector3I(-1500, 1000, 1500), 10000, Colors.White);
        AddLight(new Vector3I(1500, 1000, -1500), 10000, Colors.White);
        AddLight(new Vector3I(1500, 1000, 1500), 10000, Colors.White);
        AddLight(new Vector3I(0, 1000, 0), 10000, Colors.White);
    }

    public static void AddLight(Vector3I position, int strength, Color color)
    {
        var light = new OmniLight3D();
        light.Position = position;
        light.OmniRange = strength;
        light.LightColor = color;
        App.Instance.UserInterface.Project.Lights.Add(light);
        App.Instance.UserInterface.Project.LightsAmount++;
        App.Instance.UserInterface.Viewport.World.AddChild(light);
        App.Instance.UserInterface.Project.Changes++;
    }

    public static void Remove(Light3D light, int index)
    {
        App.Instance.UserInterface.Project.LightsAmount--;
        App.Instance.UserInterface.LightsSelect--;
        App.Instance.UserInterface.Project.Lights.RemoveAt(index);
        light.QueueFree();
        App.Instance.UserInterface.Project.Changes++;
    }
}