using System.IO;
using Godot;
using MineImatorSimplyRemade.app.dialogues;

namespace MineImatorSimplyRemade.app.libraries;

public static class Lib
{
    public static bool Start()
    {
        GD.Print("Lib.Start()");

        var libPath = "Data/Libraries/";
        var pathFile = libPath + "file.dll";
        var pathMovie = libPath + "movie.dll";
        var pathWindow = libPath + "window.dll";
        var pathffmpeg = libPath + "ffmpeg.exe";

        if (!File.Exists(pathFile))
        {
            return MissingFile.ShowMessage(pathFile);
        }

        if (!File.Exists(pathMovie))
        {
            return MissingFile.ShowMessage(pathMovie);
        }

        if (!File.Exists(pathWindow))
        {
            return MissingFile.ShowMessage(pathWindow);
        }

        if (!File.Exists(pathffmpeg))
        {
            return MissingFile.ShowMessage(pathffmpeg);
        }

        return true;
    }
}