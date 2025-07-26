using System.IO;
using Godot;

namespace MineImatorSimplyRemade.scripts;

public static class ExternalLibrary
{
    public static string FFmpegExe = OS.GetExecutablePath().GetBaseDir() + "/ffmpeg.exe";
    
    public static void Start()
    {
        if (!File.Exists(FFmpegExe))
        {
            //App.Instance.MessageWindow.ShowMessage("FFmpeg not found.", true);
        }
    }
}