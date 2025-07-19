using Godot;

namespace MineImatorSimplyRemade.app;

public static class AppEvent
{
    static bool DebugInfo = Macro.DevMode;
    static RandomNumberGenerator  RNG = new();
    
    public static void Create()
    {
        RNG.Randomize();

        if (!AppStartup.Start())
        {
            GD.PrintErr("Failed to start the app.");
        }
    }
}