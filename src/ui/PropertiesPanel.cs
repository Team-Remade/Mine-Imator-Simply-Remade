using System;
using Godot;
using ImGuiNET;
using SimplyRemadeMI.core;
using Vector2 = System.Numerics.Vector2;

namespace SimplyRemadeMI.ui;

public class PropertiesPanel
{
    public MIProject project = new();
    
    public void Render(Vector2I position, Vector2I size)
    {
        ImGui.SetNextWindowPos(new Vector2(position.X, position.Y));
        ImGui.SetNextWindowSize(new Vector2(size.X, size.Y));

        if (ImGui.Begin("Properties", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse))
        {
            if (ImGui.BeginTabBar("##PropertiesTabs"))
            {
                // Project Properties Tab
                if (ImGui.BeginTabItem("Project"))
                {
                    RenderProjectProperties();
                    ImGui.EndTabItem();
                }
                
                ImGui.EndTabBar();
            }
        }
        
        ImGui.End();
    }

    private void RenderProjectProperties()
    {
        // Toggle button for project settings
        if (ImGui.CollapsingHeader("Project Settings", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Project name
            ImGui.Text("Project Name: NOT IMPLEMENTED");
            if (ImGui.InputText("##ProjectName", ref project._projectName, 256))
            {
                // TODO: Implement project name saving
            }

            ImGui.Spacing();

            // Project author
            ImGui.Text("Author: NOT IMPLEMENTED");
            if (ImGui.InputText("##ProjectAuthor", ref project._projectAuthor, 256))
            {
                // TODO: Implement project author saving
            }

            ImGui.Spacing();

            // Project description
            ImGui.Text("Description: NOT IMPLEMENTED");
            if (ImGui.InputTextMultiline("##ProjectDescription", ref project._projectDescription, 512, new Vector2(250, 80)))
            {
                // TODO: Implement project description saving
            }

            ImGui.Spacing();
            ImGui.Separator();

            // Project thumbnail
            ImGui.Text("Thumbnail: NOT IMPLEMENTED");

            // Thumbnail display area (placeholder)
            var thumbnailSize = new Vector2(120, 90); // 4:3 aspect ratio
            var thumbnailPos = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddRectFilled(
                thumbnailPos,
                thumbnailPos + thumbnailSize,
                ImGui.GetColorU32(new System.Numerics.Vector4(0.3f, 0.3f, 0.3f, 1.0f)) // Dark gray background
            );
            ImGui.GetWindowDrawList().AddRect(
                thumbnailPos,
                thumbnailPos + thumbnailSize,
                ImGui.GetColorU32(new System.Numerics.Vector4(0.7f, 0.7f, 0.7f, 1.0f)), // Light gray border
                0.0f, // rounding
                0, // flags
                2.0f // thickness
            );

            // Placeholder text
            var textPos = thumbnailPos + thumbnailSize * 0.5f;
            var textSize = ImGui.CalcTextSize("No Thumbnail");
            ImGui.GetWindowDrawList().AddText(
                textPos - textSize * 0.5f,
                ImGui.GetColorU32(new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1.0f)), // Gray text
                "No Thumbnail"
            );

            // Reset cursor position for the next UI elements
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + thumbnailSize.Y + 10);

            // Thumbnail controls
            if (ImGui.Button("Set Thumbnail"))
            {
                // TODO: Implement thumbnail selection
                Console.WriteLine("Set project thumbnail");
            }

            ImGui.SameLine();
            if (ImGui.Button("Clear Thumbnail"))
            {
                // TODO: Implement thumbnail clearing
                Console.WriteLine("Clear project thumbnail");
            }

            ImGui.Spacing();
            ImGui.Separator();

            // Project save directory
            ImGui.Text("Project Save Directory: NOT IMPLEMENTED");
            if (ImGui.InputText("##ProjectSaveDir", ref project._projectSaveDir, 512))
            {
                // TODO: Implement save directory updating
            }

            if (ImGui.Button("Browse..."))
            {
                // TODO: Implement directory browser
                Console.WriteLine("Browse for project save directory");
            }

            ImGui.Spacing();
            ImGui.Separator();

            // Project render resolution
            ImGui.Text("Project Render Resolution: NOT IMPLEMENTED");

            // Resolution dropdown with presets (same as export dialog)
            if (ImGui.Combo("##ResolutionPreset", ref project._selectedResolutionIndex, project._resolutionOptions,
                    project._resolutionOptions.Length))
            {
                // Resolution changed - update project resolution values
                if (project._selectedResolutionIndex != 12) // Not Custom
                {
                    var (width, height) = ParseResolution(project._resolutionOptions[project._selectedResolutionIndex]);
                    project._projectRenderWidth = width;
                    project._projectRenderHeight = height;
                }
            }

            // Show custom resolution controls if "Custom" is selected
            if (project._selectedResolutionIndex == 12) // Custom option
            {
                ImGui.Spacing();
                ImGui.Text("Custom Resolution:");

                ImGui.Text("Width:");
                if (ImGui.InputInt("##RenderWidth", ref project._projectRenderWidth, 10, 100))
                {
                    if (project._projectRenderWidth < 1) project._projectRenderWidth = 1;
                    if (project._projectRenderWidth > 7680) project._projectRenderWidth = 7680; // 8K limit
                }

                ImGui.Text("Height:");
                if (ImGui.InputInt("##RenderHeight", ref project._projectRenderHeight, 10, 100))
                {
                    if (project._projectRenderHeight < 1) project._projectRenderHeight = 1;
                    if (project._projectRenderHeight > 4320) project._projectRenderHeight = 4320; // 8K limit
                }
            }

            // Current resolution display
            ImGui.Spacing();
            ImGui.Text($"Current Resolution: {project._projectRenderWidth}x{project._projectRenderHeight}");

            ImGui.Spacing();
            ImGui.Separator();

            // Project framerate
            ImGui.Text("Project Framerate: NOT IMPLEMENTED");
            if (ImGui.InputInt("##ProjectFramerate", ref project._projectFramerate, 1, 5))
            {
                if (project._projectFramerate < 1) project._projectFramerate = 1;
                if (project._projectFramerate > 120) project._projectFramerate = 120;
            }

            // Common framerate presets
            ImGui.Spacing();
            ImGui.Text("Presets:");
            if (ImGui.Button("24 fps"))
            {
                project._projectFramerate = 24;
            }

            ImGui.SameLine();
            if (ImGui.Button("30 fps"))
            {
                project._projectFramerate = 30;
            }

            ImGui.SameLine();
            if (ImGui.Button("60 fps"))
            {
                project._projectFramerate = 60;
            }
        }
        
        // Background Settings Section
        if (ImGui.CollapsingHeader("Background Settings"))
        {
            // Background Image
            ImGui.Text("Background Image: NOT IMPLEMENTED");
            
            // Display current background image name
            ImGui.Text($"Current: {project._backgroundImageName}");
            if (ImGui.Button("Load Background Image"))
            {
                // Use file dialog to select background image
                //_ = Task.Run(async () =>
                //{
                //    var imagePath = await FileDialog.ShowOpenDialogAsync("Select Background Image");
                //    if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                //    {
                //        // Store the path to load on the main thread
                //        _pendingBackgroundImagePath = imagePath;
                //    }
                //    else if (!string.IsNullOrEmpty(imagePath))
                //    {
                //        Console.WriteLine($"Selected image not found: {imagePath}");
                //    }
                //});
            }
            
            if (ImGui.Button("Clear Background Image"))
            {
                //_viewport3D?.ClearBackgroundImage();
                project._backgroundImageName = "None";
            }

            ImGui.Spacing();

            // Stretch to Fit Toggle
            if (ImGui.Checkbox("Stretch to Fit", ref project._stretchBackgroundToFit))
            {
                // Notify the viewport that the stretch setting changed
                //_viewport3D?.SetBackgroundStretch(_stretchBackgroundToFit);
            }

            // Clear Color (Sky Color)
            ImGui.Text("Sky Color");
            if (ImGui.ColorEdit3("##ClearColor", ref project._clearColor))
            {
                //NotifyBackgroundChanged();
            }

            // Sky Color Presets
            ImGui.Spacing();
            ImGui.Text("Presets:");
            
            // First row of presets
            if (ImGui.Button("Sky Blue"))
            {
                project._clearColor = new System.Numerics.Vector3(0.576f, 0.576f, 1.0f); // Default sky blue #9393FF
                //NotifyBackgroundChanged();
            }
            
            ImGui.SameLine();
            if (ImGui.Button("Sunset"))
            {
                project._clearColor = new System.Numerics.Vector3(1.0f, 0.647f, 0.0f); // Orange sunset #FFA500
                //NotifyBackgroundChanged();
            }
            
            ImGui.SameLine();
            if (ImGui.Button("Dawn"))
            {
                project._clearColor = new System.Numerics.Vector3(1.0f, 0.753f, 0.796f); // Pink dawn #FFCCCB
                //NotifyBackgroundChanged();
            }
            
            // Second row of presets
            if (ImGui.Button("Storm"))
            {
                project._clearColor = new System.Numerics.Vector3(0.4f, 0.4f, 0.5f); // Dark stormy gray #666680
                //NotifyBackgroundChanged();
            }
            
            ImGui.SameLine();
            if (ImGui.Button("Night"))
            {
                project._clearColor = new System.Numerics.Vector3(0.1f, 0.1f, 0.2f); // Dark night blue #191933
                //NotifyBackgroundChanged();
            }
            
            ImGui.SameLine();
            if (ImGui.Button("White"))
            {
                project._clearColor = new System.Numerics.Vector3(1.0f, 1.0f, 1.0f); // Pure white #FFFFFF
                //NotifyBackgroundChanged();
            }

            ImGui.Spacing();

            // Floor Visibility
            if (ImGui.Checkbox("Show Floor", ref project._floorVisible))
            {
                //NotifyBackgroundChanged();
            }

            // Floor Tile Selection
            if (project._floorVisible)
            {
                ImGui.Spacing();
                ImGui.Text("Floor Tile");
                
                // Create a list of available tiles for selection
                var availableTiles = new[]
                {
                    "tile040", // Grass (default)
                    "tile002", // Dirt
                    "tile003", // Stone
                    "tile016", // Cobblestone
                    "tile017", // Bedrock
                    "tile018", // Sand
                    "tile019", // Gravel
                    "tile021", // Wood (log top)
                    "tile032", // Gold ore
                    "tile033", // Iron ore
                    "tile034", // Coal ore
                    "tile048", // Sponge
                    "tile049", // Glass
                    "tile053", // Leaves
                    "tile062", // Dispenser top
                    "tile074", // Note block
                    "tile079", // Birch sapling
                    "tile116", // Spruce log
                    "tile117", // Birch log
                    "tile134", // Bed top
                    "tile149", // Bed side
                    "tile150", // Bed bottom
                    "tile151", // Bed side
                    "tile152", // Bed back
                    "tile153", // Jungle log
                    "tile160", // Lapis ore
                    "tile163", // Powered rail
                    "tile176", // Sandstone top
                    "tile191", // Lava
                    "tile192", // Sandstone side
                    "tile195", // Detector rail
                    "tile208", // Sandstone bottom
                };

                //var currentTileIndex = Array.IndexOf(availableTiles, _floorTileId);
                //if (currentTileIndex == -1) currentTileIndex = 0;

                //if (ImGui.Combo("##FloorTile", ref currentTileIndex, availableTiles, availableTiles.Length))
                //{
                //    _floorTileId = availableTiles[currentTileIndex];
                //    NotifyBackgroundChanged();
                //}
            }
        }
    }
    
    private (int width, int height) ParseResolution(string resolutionString)
    {
        // Extract resolution from strings like "FHD 1080p 1920x1080" or "Avatar 512x512"
        var parts = resolutionString.Split(' ');
        foreach (var part in parts)
        {
            if (part.Contains('x'))
            {
                var dimensions = part.Split('x');
                if (dimensions.Length == 2 &&
                    int.TryParse(dimensions[0], out int width) &&
                    int.TryParse(dimensions[1], out int height))
                {
                    return (width, height);
                }
            }
        }

        // Fallback to 1920x1080 if parsing fails
        return (1920, 1080);
    }
}