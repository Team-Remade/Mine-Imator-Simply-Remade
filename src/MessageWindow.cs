using Godot;

namespace MineImatorSimplyRemade;

public partial class MessageWindow : Window
{
    [Export] Label TextLabel { get; set; }
    private bool ErrorMessage;

    public void ShowMessage(string message, bool error = false)
    {
        ErrorMessage = error;
        TextLabel.Text = message;
        PopupCentered();
        GetTree().Paused = true;
    }

    private void OnAcceptPressed()
    {
        if (ErrorMessage)
        {
            GetTree().Quit(1);
        }
        else
        {
            Hide();
            GetTree().Paused = false;
        }
    }
}