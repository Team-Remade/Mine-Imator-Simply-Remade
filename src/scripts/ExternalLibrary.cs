using System.IO;
using Godot;

namespace MineImatorSimplyRemade.scripts;

public static class ExternalLibrary
{
    public static string FFmpegWin = OS.GetExecutablePath().GetBaseDir() + "/ffmpeg.exe";
    public static string FFmpegLinux = OS.GetExecutablePath().GetBaseDir() + "/ffmpeg";
    
    public static void Start()
    {
        bool error = OS.GetName() == "Windows" && !File.Exists(FFmpegWin) || OS.GetName() == "Linux" && !File.Exists(FFmpegLinux);

        if (error && !OS.HasFeature("editor"))
        {
            App.Instance.MessageWindow.ShowMessage("FFmpeg not found.", true);
        }
    }
}