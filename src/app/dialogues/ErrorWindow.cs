using Godot;

namespace MineImatorSimplyRemade.app.dialogues;

public class ErrorWindow
{
    public static void ShowMessage(string error)
    {
        var errorWindow = new AcceptDialog();
        errorWindow.Title = "Error";
        errorWindow.DialogText = error;
        errorWindow.GetOkButton().Text = "Close";
        errorWindow.Exclusive = true;
        errorWindow.ProcessMode = Node.ProcessModeEnum.Always;
        App.Instance.AddChild(errorWindow);
        App.Instance.GetTree().Paused = true;
        errorWindow.PopupCentered();
        errorWindow.Confirmed += () =>
        {
            App.Instance.GetTree().Quit();
        };
    }
}