using Godot;

namespace MineImatorSimplyRemade.app.dialogues;

public static class MissingFile
{
    public static bool ShowMessage(string file)
    {
        GD.PrintErr("Missing File: " + file);
        ErrorWindow.ShowMessage("The file " + file + " does not exist.");
        App.Instance.GetTree().Paused = true;
        
        return false;
    }
}