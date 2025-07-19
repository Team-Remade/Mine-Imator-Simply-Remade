using System;
using Godot;
using MineImatorSimplyRemade.app.dialogues;
using MineImatorSimplyRemade.utility.file.json;
using Json = MineImatorSimplyRemade.utility.file.json.Json;

namespace MineImatorSimplyRemade.project;

public static class Legacy
{
    public static void Start()
    {
        GD.Print("Legacy.Start()");

        try
        {
            //TODO: Fix this
            //var legacy = (LegacyData)Json.Load(Macro.LegacyFile);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ErrorWindow.ShowMessage(e.Message);
        }
        
    }
}