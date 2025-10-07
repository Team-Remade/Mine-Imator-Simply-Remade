using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Godot;
using ImGuiNET;
using SimplyRemadeMI.renderer;

namespace SimplyRemadeMI;

public partial class Main : Control
{
    private static Main Instance;
    
    [Export] public MainViewport MainViewport { get; private set; }
    [Export] public RenderOutput Output { get; private set; }
    [Export] public PackedScene ObjectScene { get; private set; }
    [Export] public PackedScene ItemObjectScene { get; private set; }
    [Export] public PackedScene CameraObjectScene { get; private set; }
    [Export] public PackedScene SpriteMesh { get; private set; }
    [Export] public SubViewportContainer Second { get; private set; }
    
    [Export] public Godot.Collections.Dictionary<string, Texture2D> Icons { get; private set; }
    [Export] public Godot.Collections.Dictionary<string, Texture2D> PreviewTextures { get; private set; }
    
    private ViewportObject ViewportObject;
    
    public UIRenderer UI { get; private set; }
    
    private TextureAtlas _terrainAtlas;
    private TextureAtlas _itemAtlas;
    
    public TextureAtlas TerrainAtlas => _terrainAtlas;
    public TextureAtlas ItemAtlas => _itemAtlas;
    
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

        // Copy this once
        if (File.Exists(OS.GetExecutablePath().GetBaseDir() + "data/imgui.ini") && !File.Exists(OS.GetUserDataDir() + "imgui.ini"))
        {
            File.Copy(OS.GetExecutablePath().GetBaseDir() + "data/imgui.ini", OS.GetUserDataDir() + "imgui.ini");
        }
        
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        
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
        
        // Initialize item texture atlas
        _itemAtlas = new TextureAtlas(2048, 2048);
        _itemAtlas.LoadTexturesFromPattern("res://assets/sprite/item/tile###.png");
        _itemAtlas.GenerateAtlas();
        GD.Print($"Item atlas generated with {_itemAtlas.GetTextureCount()} textures");
        
        CheckRandomWindowIcon();
    }

    public override void _Process(double delta)
    {
        DisplayServer.WindowSetTitle("Mine Imator Simply Remade");
        UI.Update((float)delta);
        
        WindowSize = DisplayServer.WindowGetSize();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            //TODO: Implement unsaved project detection
            GD.Print("Window closing...");
            GetTree().Quit();
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

    public async void SaveOutputTexture(string filePath, int width, int height)
    {
        try
        {
            if (Output == null)
            {
                GD.PrintErr("Output viewport is null");
                return;
            }

            // Store original size and preview mode
            var originalSize = Output.Size;
            var originalPreviewMode = Output.PreviewMode;

            // Set the viewport to the desired render size and disable preview mode
            Output.PreviewMode = false;
            Output.Size = new Vector2I(width, height);

            // Wait for multiple frames to ensure the viewport is fully rendered with the new size
            // This helps avoid buffer artifacts during resize
            await ToSignal(GetTree(), "process_frame");
            await ToSignal(GetTree(), "process_frame");
            await ToSignal(GetTree(), "process_frame");
            await ToSignal(GetTree(), "process_frame");
            await ToSignal(GetTree(), "process_frame");

            // Get the texture from the Output viewport
            var viewportTexture = Output.GetTexture();
            if (viewportTexture == null)
            {
                GD.PrintErr("Output viewport texture is null");
                // Restore original settings
                Output.Size = originalSize;
                Output.PreviewMode = originalPreviewMode;
                return;
            }

            // Get the image from the texture
            var image = viewportTexture.GetImage();
            if (image == null)
            {
                GD.PrintErr("Failed to get image from texture");
                // Restore original settings
                Output.Size = originalSize;
                Output.PreviewMode = originalPreviewMode;
                return;
            }

            // Save the image to the specified file path
            var error = image.SavePng(filePath);
            if (error != Error.Ok)
            {
                GD.PrintErr($"Failed to save image to {filePath}: {error}");
            }
            else
            {
                GD.Print($"Successfully saved output texture to {filePath}");
            }

            // Restore original settings
            Output.Size = originalSize;
            Output.PreviewMode = originalPreviewMode;
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }

    public async void RenderAnimation(string filePath, int width, int height, int framerate, int bitrate, string format)
    {
        try
        {
            // Create temporary directory for frames
            string tempDir = Path.Combine(Path.GetTempPath(), "SimplyRemadeMI_Render");
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempDir);

            // Collect all keyframe frames from all scene objects to determine the range
            var keyframeFrames = new HashSet<int>();
            if (UI.sceneTreePanel.SceneObjects != null)
            {
                foreach (var obj in UI.sceneTreePanel.SceneObjects)
                {
                    keyframeFrames.UnionWith(obj.PosXKeyframes.Keys);
                    keyframeFrames.UnionWith(obj.PosYKeyframes.Keys);
                    keyframeFrames.UnionWith(obj.PosZKeyframes.Keys);
                    keyframeFrames.UnionWith(obj.RotXKeyframes.Keys);
                    keyframeFrames.UnionWith(obj.RotYKeyframes.Keys);
                    keyframeFrames.UnionWith(obj.RotZKeyframes.Keys);
                    keyframeFrames.UnionWith(obj.ScaleXKeyframes.Keys);
                    keyframeFrames.UnionWith(obj.ScaleYKeyframes.Keys);
                    keyframeFrames.UnionWith(obj.ScaleZKeyframes.Keys);
                    keyframeFrames.UnionWith(obj.AlphaKeyframes.Keys);
                }
            }

            int startFrame = 0;
            int endFrame = UI.timeline.TotalFrames;
            
            // If there are keyframes, use the min and max keyframe values
            if (keyframeFrames.Count > 0)
            {
                startFrame = keyframeFrames.Min();
                endFrame = keyframeFrames.Max();
            }

            bool isPngSequence = format.Equals("PNG Sequence", StringComparison.OrdinalIgnoreCase);

            // Store original timeline state
            int originalFrame = UI.timeline.CurrentFrame;
            bool wasPlaying = UI.timeline.IsPlaying;

            // Stop playback if active
            UI.timeline.IsPlaying = false;

            // Store original engine settings
            var originalVsyncMode = DisplayServer.WindowGetVsyncMode();
            var originalMaxFps = Engine.MaxFps;

            // Disable VSync and set max FPS to 0 for unlimited rendering during animation export
            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
            Engine.MaxFps = 0;

            // Store original viewport size and settings
            var originalOutputSize = Output.Size;
            var originalOutputPreviewMode = Output.PreviewMode;

            // Set viewport to render size and disable preview mode
            Output.PreviewMode = false;
            Output.Size = new Vector2I(width, height);

            // Wait for viewport to resize and stabilize
            await ToSignal(GetTree(), "process_frame");
            await ToSignal(GetTree(), "process_frame");
            await ToSignal(GetTree(), "process_frame");
            await ToSignal(GetTree(), "process_frame");
            await ToSignal(GetTree(), "process_frame");

            // Render each frame from startFrame to endFrame inclusive
            for (int frame = startFrame; frame <= endFrame; frame++)
            {
                // Set current frame and apply keyframes to update object positions
                UI.timeline.CurrentFrame = frame;
                UI.timeline.ApplyKeyframesToObjects();

                // Wait for rendering to complete - more frames to ensure proper visual updates
                await ToSignal(GetTree(), "process_frame");
                await ToSignal(GetTree(), "process_frame");
                await ToSignal(GetTree(), "process_frame");
                await ToSignal(GetTree(), "process_frame");
                await ToSignal(GetTree(), "process_frame");
                await ToSignal(GetTree(), "process_frame");
                await ToSignal(GetTree(), "process_frame");
                await ToSignal(GetTree(), "process_frame");
                await ToSignal(GetTree(), "process_frame");
                await ToSignal(GetTree(), "process_frame");

                // Save frame to temporary directory
                string framePath = Path.Combine(tempDir, $"frame_{frame:D4}.png");
                SaveOutputTexture(framePath, width, height);

                // Additional delay to ensure smooth rendering between frames
                await ToSignal(GetTree(), "process_frame");
            }

            // Restore original viewport settings
            Output.Size = originalOutputSize;
            Output.PreviewMode = originalOutputPreviewMode;

            // Restore original engine settings
            DisplayServer.WindowSetVsyncMode(originalVsyncMode);
            Engine.MaxFps = originalMaxFps;

            // Restore original timeline state
            UI.timeline.CurrentFrame = originalFrame;
            UI.timeline.IsPlaying = wasPlaying;
            UI.timeline.ApplyKeyframesToObjects();

            if (isPngSequence)
            {
                // For PNG sequence, copy frames to output directory
                string outputDir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // Copy all frames to the output directory
                foreach (string frameFile in Directory.GetFiles(tempDir, "frame_*.png"))
                {
                    string fileName = Path.GetFileName(frameFile);
                    string destFile = Path.Combine(outputDir, fileName);
                    File.Copy(frameFile, destFile, true);
                }

                GD.Print($"PNG sequence rendered successfully to {outputDir}");
            }
            else
            {
                // For video formats, use FFMPEG to encode
                string ffmpegPath = Path.Combine(OS.GetExecutablePath().GetBaseDir(), "data/lib/ffmpeg", GetFFmpegPath());
                
                if (!File.Exists(ffmpegPath))
                {
                    ShowErrorDialog("FFmpeg executable not found. Please ensure FFmpeg is installed.", "FFmpeg Missing");
                    return;
                }

                // Prepare FFmpeg command
                string arguments = $"-y -framerate {framerate} -i \"{tempDir}/frame_%04d.png\" " +
                                  $"-c:v libx264 -pix_fmt yuv420p -b:v {bitrate}k \"{filePath}\"";

                // Execute FFmpeg
                using (var process = new Process())
                {
                    process.StartInfo.FileName = ffmpegPath;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();
                    
                    // Read output asynchronously to avoid blocking
                    string output = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        ShowErrorDialog($"FFmpeg encoding failed: {output}", "Encoding Error");
                    }
                    else
                    {
                        GD.Print($"Video encoded successfully to {filePath}");
                    }
                }
            }

            // Clean up temporary files
            Directory.Delete(tempDir, true);
        }
        catch (Exception ex)
        {
            ShowErrorDialog($"Animation rendering failed: {ex.Message}", "Rendering Error");
        }
    }
}