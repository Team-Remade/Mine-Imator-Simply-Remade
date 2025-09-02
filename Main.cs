using System;
using System.IO;
using System.Runtime.InteropServices;
using Godot;
using SimplyRemadeMI.renderer;

namespace SimplyRemadeMI;

public partial class Main : Control
{
    private static Main Instance;
    
    [Export] public MainViewport MainViewport { get; private set; }
    
    private ViewportObject ViewportObject;
    
    public UIRenderer UI { get; private set; }
    
    private TextureAtlas _terrainAtlas;
    
    public TextureAtlas TerrainAtlas => _terrainAtlas;
    
    public Vector2I WindowSize { get; private set; }
    
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    private string GetFFmpegPath()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";
    }

    public void ShowErrorDialog(string message, string title)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            MessageBox(IntPtr.Zero, message, title, 0x10); // MB_ICONERROR
        }
    }

    public static Main GetInstance()
    {
        return Instance;
    }
    
    public override void _Ready()
    {
        Instance = this;
        
        ViewportObject = new ViewportObject(MainViewport);
        UI = new UIRenderer(ViewportObject);
        
        // Check if ffmpeg binary exists in the application directory
        var ffmpegExecutable = GetFFmpegPath();
        var ffmpegPath = Path.Combine(OS.GetExecutablePath().GetBaseDir(), "data/lib/ffmpeg", ffmpegExecutable);
        if (!File.Exists(ffmpegPath))
        {
            //ShowErrorDialog(
            //    $"FFmpeg binary ({ffmpegExecutable}) not found.",
            //    "Mine Imator Simply Remade - Missing Dependency");
            //GetTree().Quit(1);
            //return;
        }
        
        var screenSize = DisplayServer.ScreenGetSize();
        WindowSize = screenSize - new Vector2I(200, 160);
        var windowPosition = (screenSize - WindowSize) / 2;
        
        DisplayServer.WindowSetSize(WindowSize);
        DisplayServer.WindowSetPosition(windowPosition);
        GD.Print("Initialized Vulkan...");
        
        // Initialize terrain texture atlas
        _terrainAtlas = new TextureAtlas(2048, 2048);
        _terrainAtlas.LoadTexturesFromPattern("res://assets/sprite/terrain/tile###.png");
        _terrainAtlas.GenerateAtlas();
        GD.Print($"Terrain atlas generated with {_terrainAtlas.GetTextureCount()} textures");
        
        CheckRandomWindowIcon();
    }

    public override void _Process(double delta)
    {
        DisplayServer.WindowSetTitle("Mine Imator Simply Remade");
        UI.Render();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            //TODO: Implement unsaved project detection
            GD.Print("Window closing...");
            GetTree().Quit();
        }
        else if (what == NotificationWMSizeChanged)
        {
            WindowSize = DisplayServer.WindowGetSize();
        }
    }

    private void CheckRandomWindowIcon()
    {
        var random = new Random();
        var useChegg = random.Next(1000) == 0;

        if (useChegg)
        {
            var img = ResourceLoader.Load<Texture2D>("res://assets/chegg.png");
            
            DisplayServer.SetIcon(img.GetImage());
        }
    }
}