using System.Collections.Generic;
using Godot;

namespace MineImatorSimplyRemade;

public static class GlobalVar
{
    public static int DebugIndent = 0;
    public static double DebugTimer = 0;
    public static bool FileCopyTemp = false;
    
    //libOpenUrl
    //libExecute
    //libUnzip
    //libGzUnzip
    //libFileRename
    //libFileCopy
    //libFileDelete
    //libFileExists
    //libJsonConvertUnicode
    
    //libDirectoryCreate
    //libDirectoryExists
    //libDirectoryDelete
    
    //libMovieInit
    //libMovieSet
    //libMovieStart
    //libMovieAudioFileDecode
    //libMovieAudioSoundAdd
    //libMovieFrame
    //libMovieDone
    
    //libWindowMaximize
    //libWindowSetFocus
    
    //vbufferCurrent
    //vertexFormat
    public static Enums.VertexWave VertexWave = Enums.VertexWave.None;
    //vertexWaveZMin
    //vertexWaveZMax
    public static float VertexEmissive = 0.0f;
    public static float VertexSubsurface = 0.0f;

    public static Color VertexRgb = Colors.White;
    public static float VertexAlpha = 1.0f;
    
    public static List<string> ValueNames = new();
    public static List<string> CameraValues = new();
}