namespace SimplyRemadeMI.core;

public class MIProject
{
    public int FrameRate = 30;
    
    // Background image loading
    public string PendingBackgroundImagePath = null;
    public string BackgroundImageName = "None";
    public bool StretchBackgroundToFit = true;
    
    // Project-level properties
    public string ProjectName = "Untitled Project";
    public string ProjectAuthor = "";
    public string ProjectDescription = "";
    public string ProjectSaveDir = "C:/Projects/Untitled";
    public int ProjectRenderWidth = 1920;
    public int ProjectRenderHeight = 1080;
    public int SelectedResolutionIndex = 3; // Default to FHD 1080p
    public float AspectRatio;
    public bool KeepAspectRatio = true;
    
    // Background properties
    public string BackgroundImagePath = "";
    public System.Numerics.Vector3 ClearColor = new (0.576f, 0.576f, 1.0f); // Default sky blue #9393FF
    public bool FloorVisible = true;
    public string FloorTileId = "tile040"; // Default floor tile
    
    // Resolution options
    public readonly string[] ResolutionOptions =
    [
        "Avatar 512x512",
        "VGA 640x480",
        "HD 720p 1280x720",
        "FHD 1080p 1920x1080",
        "QHD 1440p 2560x1440",
        "UHD 4K 3840x2160",
        "HD Cinematic 1680x720",
        "FHD Cinematic 2560x1080",
        "QHD Cinematic 3440x1440",
        "QHD+ Cinematic 3840x1600",
        "UW4k Cinematic 4320x1800",
        "UW5k Cinematic 5120x2160",
        "Custom"
    ];

    public int SelectedVideoFormatIndex;
    public readonly string[] VideoFormatOptions = ["MP4", "MOV", "WMV", "PNG Sequence"];
    public float Bitrate = 25.0f; // Mbps

    public MIProject()
    {
        AspectRatio = (float)ProjectRenderWidth / ProjectRenderHeight;
    }
    
    public (int width, int height, int framerate) GetProjectRenderSettings()
    {
        return (ProjectRenderWidth, ProjectRenderHeight, FrameRate);
    }
}