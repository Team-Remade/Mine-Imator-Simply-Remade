using System.IO;
using MineImatorSimplyRemade.app.dialogues;
using MineImatorSimplyRemade.app.libraries;
using MineImatorSimplyRemade.utility.vertex;

namespace MineImatorSimplyRemade.app;

public static class AppStartup
{
    public static bool Start()
    {
        var startupError = true;

        if (!Lib.Start())
            return false;

        if (!File.Exists(Macro.LegacyFile))
            return MissingFile.ShowMessage(Macro.LegacyFile);

        if (!File.Exists(Macro.LanguageFile))
            return MissingFile.ShowMessage(Macro.LanguageFile);
        
        VertexFormat.Start();
        
        startupError = false;
        
        return startupError;
    }
}