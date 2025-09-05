namespace SimplyRemadeMI.core;

public class MIProject
{
    public int FrameRate = 30;
    
    // Background image loading
    public string? _pendingBackgroundImagePath = null;
    public string _backgroundImageName = "None";
    public bool _stretchBackgroundToFit = true;
    
    // Project-level properties
    public string _projectName = "Untitled Project";
    public string _projectAuthor = "";
    public string _projectDescription = "";
    public string _projectSaveDir = "C:\\Projects\\Untitled";
    public int _projectRenderWidth = 1920;
    public int _projectRenderHeight = 1080;
    public int _selectedResolutionIndex = 3; // Default to FHD 1080p
    
    // Background properties
    public string _backgroundImagePath = "";
    public System.Numerics.Vector3 _clearColor = new System.Numerics.Vector3(0.576f, 0.576f, 1.0f); // Default sky blue #9393FF
    public bool _floorVisible = true;
    public string _floorTileId = "tile040"; // Default floor tile
    
    // Resolution options
    public readonly string[] ResolutionOptions =
    {
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
    };
}