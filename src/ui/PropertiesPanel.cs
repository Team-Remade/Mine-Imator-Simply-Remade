using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using ImGuiGodot;
using ImGuiNET;
using SimplyRemadeMI.core;
using Vector2 = System.Numerics.Vector2;

namespace SimplyRemadeMI.ui;

public class PropertiesPanel
{
    public MIProject Project = new();
    
    private string _pendingBackgroundImagePath = null;
    private string _backgroundImageName = "None";

    public void Render()
    {
        if (!string.IsNullOrEmpty(_pendingBackgroundImagePath))
        {
            Main.GetInstance().MainViewport?.LoadBackgroundImage(_pendingBackgroundImagePath);
            _backgroundImageName = System.IO.Path.GetFileName(_pendingBackgroundImagePath);
            _pendingBackgroundImagePath = null;
        }

        if (ImGui.Begin("Properties",
                ImGuiWindowFlags.NoCollapse))
        {
            // Create tabs
            if (ImGui.BeginTabBar("##PropertiesTabs"))
            {
                // Project Properties Tab
                if (ImGui.BeginTabItem("Project"))
                {
                    RenderProjectProperties();
                    ImGui.EndTabItem();
                }

                // Object Properties Tab (only available when object is selected)
                if (ImGui.BeginTabItem("Object"))
                {
                    if (Main.GetInstance().UI.SceneTreePanel.SelectedObject != null)
                    {
                        RenderObjectProperties();
                        RenderTransformControls();
                        RenderOptionsControls();
                    }
                    else if (Main.GetInstance().UI.SceneTreePanel.SceneObjects.Count == 0)
                    {
                        RenderNoObjectsInScene();
                    }
                    else
                    {
                        RenderNoObjectSelected();
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        ImGui.End();
    }

    private void RenderObjectProperties()
    {
        ImGui.Text("Object Properties");
        ImGui.Separator();

        ImGui.Text("Name");
        string name = Main.GetInstance().UI.SceneTreePanel.SelectedObject?.Name;
        if (ImGui.InputText("##Name", ref name, 256))
        {
            var selectedObject = Main.GetInstance().UI.SceneTreePanel.SelectedObject;
            if (selectedObject != null)
                selectedObject.Name = name;
        }
        
        ImGui.Spacing();

        // Object type and mesh information
        ImGui.Text("Object Information");
        ImGui.Text($"Object ID: {Main.GetInstance().UI.SceneTreePanel.SelectedObjectGuid}");

        // Object statistics
        var totalKeyframes = Main.GetInstance().UI.SceneTreePanel.SelectedObject?.PosXKeyframes.Count + Main.GetInstance().UI.SceneTreePanel.SelectedObject?.PosYKeyframes.Count +
                             Main.GetInstance().UI.SceneTreePanel.SelectedObject?.PosZKeyframes.Count +
                             Main.GetInstance().UI.SceneTreePanel.SelectedObject?.RotXKeyframes.Count + Main.GetInstance().UI.SceneTreePanel.SelectedObject?.RotYKeyframes.Count +
                             Main.GetInstance().UI.SceneTreePanel.SelectedObject?.RotZKeyframes.Count +
                             Main.GetInstance().UI.SceneTreePanel.SelectedObject?.ScaleXKeyframes.Count + Main.GetInstance().UI.SceneTreePanel.SelectedObject?.ScaleYKeyframes.Count +
                             Main.GetInstance().UI.SceneTreePanel.SelectedObject?.ScaleZKeyframes.Count +
                             Main.GetInstance().UI.SceneTreePanel.SelectedObject?.AlphaKeyframes.Count;
        ImGui.Text($"Keyframes: {totalKeyframes}");

        ImGui.Spacing();
        ImGui.Separator();

        var availableParents = Main.GetInstance().UI.SceneTreePanel.SceneObjects.Values.Where(obj =>
                obj != Main.GetInstance().UI.SceneTreePanel.SelectedObject && !obj.IsDescendantOf(Main.GetInstance().UI.SceneTreePanel.SelectedObject))
            .ToList();
        availableParents.Insert(0, null); // Add "None" option

        var parentNames = availableParents.Select(p => p?.Name.ToString() ?? "None").ToArray();

        var ob = Main.GetInstance().UI.SceneTreePanel.SelectedObject?.GetParent();
        
        if (ob is not SceneObject sceneObject) return;
        
        var currentParentIndex =
            availableParents.IndexOf(sceneObject);
        if (currentParentIndex == -1) currentParentIndex = 0;

        if (ImGui.Combo("Parent", ref currentParentIndex, parentNames, parentNames.Length))
        {
            var newParent = availableParents[currentParentIndex];
            Main.GetInstance().UI.SceneTreePanel.SelectedObject?.SetParent(newParent);
        }

        ImGui.Spacing();
        ImGui.Separator();
    }

    private void RenderTransformControls()
    {
        ImGui.Text("Transform");
        ImGui.Separator();

        // Store previous values for change detection
        var selectedObject = Main.GetInstance().UI.SceneTreePanel.SelectedObject;
        if (selectedObject != null)
        {
            var previousPos = selectedObject.Position;
            var previousRot = selectedObject.RotationDegrees;
            var previousScale = selectedObject.Scale;

            // Position section
            if (ImGui.CollapsingHeader("Position", ImGuiTreeNodeFlags.DefaultOpen))
            {
                // Position spinboxes - display values multiplied by 16
                var scaledPos = selectedObject.TargetPosition * 16;
                if (ImGui.DragFloat("X", ref scaledPos.X, 0.1f))
                {
                    selectedObject.TargetPosition = scaledPos / 16;
                    if (Math.Abs(selectedObject.TargetPosition.X - previousPos.X) > 0.001f)
                        Main.GetInstance().UI.Timeline.AddKeyframe("position.x",
                            Main.GetInstance().UI.Timeline.CurrentFrame,
                            selectedObject.TargetPosition.X);
                }

                if (ImGui.DragFloat("Y", ref scaledPos.Y, 0.1f))
                {
                    selectedObject.TargetPosition = scaledPos / 16;
                    if (Math.Abs(selectedObject.TargetPosition.Y - previousPos.Y) > 0.001f)
                        Main.GetInstance().UI.Timeline.AddKeyframe("position.y",
                            Main.GetInstance().UI.Timeline.CurrentFrame,
                            selectedObject.TargetPosition.Y);
                }

                if (ImGui.DragFloat("Z", ref scaledPos.Z, 0.1f))
                {
                    selectedObject.TargetPosition = scaledPos / 16;
                    if (Math.Abs(selectedObject.TargetPosition.Z - previousPos.Z) > 0.001f)
                        Main.GetInstance().UI.Timeline.AddKeyframe("position.z",
                            Main.GetInstance().UI.Timeline.CurrentFrame,
                            selectedObject.TargetPosition.Z);
                }

                // Position reset button
                if (ImGui.Button("Reset Position"))
                {
                    selectedObject.TargetPosition = Vector3.Zero;
                    Main.GetInstance().UI.Timeline
                        .AddKeyframe("position.x", Main.GetInstance().UI.Timeline.CurrentFrame, 0);
                    Main.GetInstance().UI.Timeline
                        .AddKeyframe("position.y", Main.GetInstance().UI.Timeline.CurrentFrame, 0);
                    Main.GetInstance().UI.Timeline
                        .AddKeyframe("position.z", Main.GetInstance().UI.Timeline.CurrentFrame, 0);
                    
                }
            }

            // Only show Rotation controls if the object is rotatable
            if (selectedObject.Rotatable && ImGui.CollapsingHeader("Rotation", ImGuiTreeNodeFlags.DefaultOpen))
            {
                // Rotation spinboxes in degrees
                var rotation = selectedObject.RotationDegrees;
                if (ImGui.DragFloat("Pitch (X)", ref rotation.X, 1.0f))
                {
                    selectedObject.RotationDegrees = rotation;
                    if (Math.Abs(selectedObject.RotationDegrees.X - previousRot.X) >
                        0.1f)
                        Main.GetInstance().UI.Timeline.AddKeyframe("rotation.x",
                            Main.GetInstance().UI.Timeline.CurrentFrame,
                            selectedObject.RotationDegrees.X);
                }

                if (ImGui.DragFloat("Yaw (Y)", ref rotation.Y, 1.0f))
                {
                    selectedObject.RotationDegrees = rotation;
                    if (Math.Abs(selectedObject.RotationDegrees.Y - previousRot.Y) >
                        0.1f)
                        Main.GetInstance().UI.Timeline.AddKeyframe("rotation.y",
                            Main.GetInstance().UI.Timeline.CurrentFrame,
                            selectedObject.RotationDegrees.Y);
                }

                if (ImGui.DragFloat("Roll (Z)", ref rotation.Z, 1.0f))
                {
                    selectedObject.RotationDegrees = rotation;
                    if (Math.Abs(selectedObject.RotationDegrees.Z - previousRot.Z) >
                        0.1f)
                        Main.GetInstance().UI.Timeline.AddKeyframe("rotation.z",
                            Main.GetInstance().UI.Timeline.CurrentFrame,
                            selectedObject.RotationDegrees.Z);
                }

                // Rotation reset button
                if (ImGui.Button("Reset Rotation"))
                {
                    selectedObject.RotationDegrees = Vector3.Zero;
                    Main.GetInstance().UI.Timeline
                        .AddKeyframe("rotation.x", Main.GetInstance().UI.Timeline.CurrentFrame, 0);
                    Main.GetInstance().UI.Timeline
                        .AddKeyframe("rotation.y", Main.GetInstance().UI.Timeline.CurrentFrame, 0);
                    Main.GetInstance().UI.Timeline
                        .AddKeyframe("rotation.z", Main.GetInstance().UI.Timeline.CurrentFrame, 0);
                }
            }

            // Only show Scale controls if the object is scalable
            if (selectedObject.Scalable && ImGui.CollapsingHeader("Scale", ImGuiTreeNodeFlags.DefaultOpen))
            {
                // Scale spinboxes
                var scale = selectedObject.Scale;
                if (ImGui.DragFloat("Scale X", ref scale.X, 0.01f))
                {
                    selectedObject.Scale = scale;
                    if (Math.Abs(selectedObject.Scale.X - previousScale.X) > 0.01f)
                        Main.GetInstance().UI.Timeline.AddKeyframe("scale.x", Main.GetInstance().UI.Timeline.CurrentFrame,
                            selectedObject.Scale.X);
                }

                if (ImGui.DragFloat("Scale Y", ref scale.Y, 0.01f))
                {
                    selectedObject.Scale = scale;
                    if (Math.Abs(selectedObject.Scale.Y - previousScale.Y) > 0.01f)
                        Main.GetInstance().UI.Timeline.AddKeyframe("scale.y", Main.GetInstance().UI.Timeline.CurrentFrame,
                            selectedObject.Scale.Y);
                }

                if (ImGui.DragFloat("Scale Z", ref scale.Z, 0.01f))
                {
                    selectedObject.Scale = scale;
                    if (Math.Abs(selectedObject.Scale.Z - previousScale.Z) > 0.01f)
                        Main.GetInstance().UI.Timeline.AddKeyframe("scale.z", Main.GetInstance().UI.Timeline.CurrentFrame,
                            selectedObject.Scale.Z);
                }

                // Uniform scale spinbox
                var uniformScale = (selectedObject.Scale.X +
                                    selectedObject.Scale.Y +
                                    selectedObject.Scale.Z) / 3.0f;
                var previousUniformScale = uniformScale;
                if (ImGui.DragFloat("Uniform Scale", ref uniformScale, 0.01f))
                {
                    var scaleDelta = uniformScale - previousUniformScale;
                    var newScale = new Vector3(selectedObject.Scale.X + scaleDelta,
                        selectedObject.Scale.Y + scaleDelta,
                        selectedObject.Scale.Z + scaleDelta);
                    if (Math.Abs(scaleDelta) > 0.01f)
                    {
                        Main.GetInstance().UI.Timeline.AddKeyframe("scale.x", Main.GetInstance().UI.Timeline.CurrentFrame,
                            newScale.X);
                        Main.GetInstance().UI.Timeline.AddKeyframe("scale.y", Main.GetInstance().UI.Timeline.CurrentFrame,
                            newScale.Y);
                        Main.GetInstance().UI.Timeline.AddKeyframe("scale.z", Main.GetInstance().UI.Timeline.CurrentFrame,
                            newScale.Z);
                    }

                    selectedObject.Scale = newScale;
                }

                // Scale reset button
                if (ImGui.Button("Reset Scale"))
                {
                    selectedObject.Scale = Vector3.One;
                    Main.GetInstance().UI.Timeline.AddKeyframe("scale.x", Main.GetInstance().UI.Timeline.CurrentFrame,
                        selectedObject.Scale.X);
                    Main.GetInstance().UI.Timeline.AddKeyframe("scale.y", Main.GetInstance().UI.Timeline.CurrentFrame,
                        selectedObject.Scale.Y);
                    Main.GetInstance().UI.Timeline.AddKeyframe("scale.z", Main.GetInstance().UI.Timeline.CurrentFrame,
                        selectedObject.Scale.Z);
                }

                ImGui.Spacing();
                ImGui.Separator();
            }
        }

        if (ImGui.CollapsingHeader("Origin Offset", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Origin offset spinboxes - display values multiplied by 16 for precision
            var sceneObject = Main.GetInstance().UI.SceneTreePanel.SelectedObject;
            if (sceneObject != null)
            {
                var scaledOriginOffset = sceneObject.ObjectOriginOffset * 16;

                if (ImGui.DragFloat("Origin X", ref scaledOriginOffset.X, 0.1f))
                {
                    sceneObject.ObjectOriginOffset = scaledOriginOffset / 16;
                }

                if (ImGui.DragFloat("Origin Y", ref scaledOriginOffset.Y, 0.1f))
                {
                    sceneObject.ObjectOriginOffset = scaledOriginOffset / 16;
                }

                if (ImGui.DragFloat("Origin Z", ref scaledOriginOffset.Z, 0.1f))
                {
                    sceneObject.ObjectOriginOffset = scaledOriginOffset / 16;
                }
            }

            // Origin offset reset button
            if (ImGui.Button("Reset Origin Offset"))
            {
                if (Main.GetInstance().UI.SceneTreePanel.SelectedObject != null)
                {
                    // Reset to the original origin offset that was set on creation
                    var o = Main.GetInstance().UI.SceneTreePanel.SelectedObject;
                    if (o != null)
                        o.ObjectOriginOffset = o.OriginalOriginOffset;
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
        }

        // Only show Alpha controls if the object supports alpha control (not a camera)
        if (selectedObject.AlphaControl && ImGui.CollapsingHeader("Alpha", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Alpha slider - display as percentage
            var sceneObject = Main.GetInstance().UI.SceneTreePanel.SelectedObject;
            if (sceneObject != null)
            {
                var alphaPercent = sceneObject.Alpha * 100.0f;
                var oldAlpha = sceneObject.Alpha;
                if (ImGui.SliderFloat("Alpha (%)", ref alphaPercent, 0.0f, 100.0f, "%.0f%%"))
                {
                    var newAlpha = alphaPercent / 100.0f;
                    sceneObject.Alpha = newAlpha;

                    // Automatically add keyframe when alpha changes
                    if (Math.Abs(newAlpha - oldAlpha) > 0.01f)
                    {
                        Main.GetInstance().UI.Timeline
                            .AddKeyframe("alpha", Main.GetInstance().UI.Timeline.CurrentFrame, newAlpha);
                    }
                }
            }

            // Alpha reset button
            if (ImGui.Button("Reset Alpha"))
            {
                var o = Main.GetInstance().UI.SceneTreePanel.SelectedObject;
                if (o != null)
                    o.Alpha = 1.0f;
                
                Main.GetInstance().UI.Timeline.AddKeyframe("alpha", Main.GetInstance().UI.Timeline.CurrentFrame, 1.0f);
            }

            ImGui.Spacing();
            ImGui.Separator();
        }

        // Only show Light Color controls if the object is a light
        if (selectedObject is core.LightSceneObject lightObject && ImGui.CollapsingHeader("Light Color", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Light color picker - convert Godot Color to Vector3 for ImGui
            var lightColor = lightObject.Light.LightColor;
            var colorVector = new System.Numerics.Vector3(lightColor.R, lightColor.G, lightColor.B);
            
            if (ImGui.ColorEdit3("##LightColor", ref colorVector))
            {
                // Convert back to Godot Color and update the light
                var newColor = new Godot.Color(colorVector.X, colorVector.Y, colorVector.Z, lightColor.A);
                lightObject.SetColor(newColor);
            }

            // Color preset buttons
            ImGui.Spacing();
            ImGui.Text("Presets:");
            
            if (ImGui.Button("White"))
            {
                lightObject.SetColor(new Godot.Color(1.0f, 1.0f, 1.0f));
            }

            ImGui.SameLine();
            if (ImGui.Button("Warm White"))
            {
                lightObject.SetColor(new Godot.Color(1.0f, 0.9f, 0.8f));
            }

            ImGui.SameLine();
            if (ImGui.Button("Cool White"))
            {
                lightObject.SetColor(new Godot.Color(0.9f, 0.95f, 1.0f));
            }

            if (ImGui.Button("Red"))
            {
                lightObject.SetColor(new Godot.Color(1.0f, 0.2f, 0.2f));
            }

            ImGui.SameLine();
            if (ImGui.Button("Green"))
            {
                lightObject.SetColor(new Godot.Color(0.2f, 1.0f, 0.2f));
            }

            ImGui.SameLine();
            if (ImGui.Button("Blue"))
            {
                lightObject.SetColor(new Godot.Color(0.2f, 0.2f, 1.0f));
            }

            if (ImGui.Button("Yellow"))
            {
                lightObject.SetColor(new Godot.Color(1.0f, 1.0f, 0.2f));
            }

            ImGui.SameLine();
            if (ImGui.Button("Purple"))
            {
                lightObject.SetColor(new Godot.Color(0.8f, 0.2f, 1.0f));
            }

            ImGui.SameLine();
            if (ImGui.Button("Orange"))
            {
                lightObject.SetColor(new Godot.Color(1.0f, 0.5f, 0.2f));
            }

            ImGui.Spacing();
            ImGui.Separator();
        }

        ImGui.Spacing();
        ImGui.Separator();
    }

    private void RenderOptionsControls()
    {
        ImGui.Text("Object Options");
        ImGui.Separator();

        // Visibility options
        ImGui.Text("Visibility");
        var selectedObject = Main.GetInstance().UI.SceneTreePanel.SelectedObject;
        var visible = selectedObject != null && selectedObject.Visible;
        if (ImGui.Checkbox("Show Object", ref visible))
        {
            var sceneObject = Main.GetInstance().UI.SceneTreePanel.SelectedObject;
            if (sceneObject != null)
                sceneObject.Visible = visible;
        }

        // Object-specific options
        ImGui.Spacing();
        ImGui.Text("Object Settings");

        // Animation options - show easing mode for selected keyframes
        ImGui.Spacing();
        ImGui.Text("Animation");
        
        var timeline = Main.GetInstance().UI.Timeline;
        if (timeline != null && timeline.HasSelectedKeyframe())
        {
            ImGui.Text($"Selected Keyframes: {timeline.SelectedKeyframes.Count}");
            
            // Get the easing mode of the first selected keyframe
            var firstKeyframe = timeline.SelectedKeyframes.FirstOrDefault();
            if (firstKeyframe != null && selectedObject != null)
            {
                var keyframeDict = GetKeyframeDictionary(firstKeyframe.Property, selectedObject);
                if (keyframeDict != null && keyframeDict.TryGetValue(firstKeyframe.Frame, out core.Keyframe? keyframe))
                {
                    ImGui.Text($"Frame: {firstKeyframe.Frame}");
                    ImGui.Text($"Property: {firstKeyframe.Property}");
                    
                    // Show easing mode selector
                    var easingModes = core.EasingFunctions.GetAllEasingModes();
                    var easingModeNames = easingModes.Select(m => core.EasingFunctions.GetEasingModeName(m)).ToArray();
                    int currentEasingIndex = Array.IndexOf(easingModes, keyframe.EasingMode);
                    
                    if (ImGui.Combo("Easing Mode", ref currentEasingIndex, easingModeNames, easingModeNames.Length))
                    {
                        var newEasingMode = easingModes[currentEasingIndex];
                        timeline.SetEasingModeForSelectedKeyframes(newEasingMode);
                    }
                    
                    // Show helper text
                    ImGui.TextWrapped($"Easing mode controls interpolation to the next keyframe.");
                }
            }
        }
        else
        {
            ImGui.TextDisabled("Select keyframes in the timeline to change easing mode");
        }

        ImGui.Spacing();
        ImGui.Separator();
    }
    
    private System.Collections.Generic.Dictionary<int, core.Keyframe>? GetKeyframeDictionary(string property, core.SceneObject obj)
    {
        return property.ToLower() switch
        {
            "position.x" => obj.PosXKeyframes,
            "position.y" => obj.PosYKeyframes,
            "position.z" => obj.PosZKeyframes,
            "rotation.x" => obj.RotXKeyframes,
            "rotation.y" => obj.RotYKeyframes,
            "rotation.z" => obj.RotZKeyframes,
            "scale.x" => obj.ScaleXKeyframes,
            "scale.y" => obj.ScaleYKeyframes,
            "scale.z" => obj.ScaleZKeyframes,
            "alpha" => obj.AlphaKeyframes,
            _ => null
        };
    }
    
    private void RenderNoObjectSelected()
    {
        ImGui.TextColored(new System.Numerics.Vector4(1.0f, 0.5f, 0.5f, 1.0f),
            "No object selected. Please select an object in the Scene Tree.");
        ImGui.Spacing();
        ImGui.Text("Please use the Scene Tree to select an object.");
    }
    
    private void RenderNoObjectsInScene()
    {
        ImGui.TextColored(new System.Numerics.Vector4(1.0f, 0.5f, 0.5f, 1.0f), "No objects in the scene. Please add an object.");
        ImGui.Spacing();
        ImGui.Text("Please use the Scene Tree to add an object.");
    }

    private void RenderProjectProperties()
    {
        // Toggle button for project settings
        if (ImGui.CollapsingHeader("Project Settings", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Project name
            ImGui.Text("Project Name: NOT IMPLEMENTED");
            if (ImGui.InputText("##ProjectName", ref Project.ProjectName, 256))
            {
                // TODO: Implement project name saving
            }

            ImGui.Spacing();

            // Project author
            ImGui.Text("Author: NOT IMPLEMENTED");
            if (ImGui.InputText("##ProjectAuthor", ref Project.ProjectAuthor, 256))
            {
                // TODO: Implement project author saving
            }

            ImGui.Spacing();

            // Project description
            ImGui.Text("Description: NOT IMPLEMENTED");
            if (ImGui.InputTextMultiline("##ProjectDescription", ref Project.ProjectDescription, 512,
                    new Vector2(250, 80)))
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
            }

            ImGui.SameLine();
            if (ImGui.Button("Clear Thumbnail"))
            {
                // TODO: Implement thumbnail clearing
            }

            ImGui.Spacing();
            ImGui.Separator();

            // Project save directory
            ImGui.Text("Project Save Directory: NOT IMPLEMENTED");
            if (ImGui.InputText("##ProjectSaveDir", ref Project.ProjectSaveDir, 512))
            {
                // TODO: Implement save directory updating
            }

            if (ImGui.Button("Browse..."))
            {
                // TODO: Implement directory browser
            }

            ImGui.Spacing();
            ImGui.Separator();

            // Project render resolution
            ImGui.Text("Project Render Resolution");

            // Resolution dropdown with presets (same as export dialog)
            if (ImGui.Combo("##ResolutionPreset", ref Project.SelectedResolutionIndex, Project.ResolutionOptions,
                    Project.ResolutionOptions.Length))
            {
                // Resolution changed - update project resolution values
                if (Project.SelectedResolutionIndex != 12) // Not Custom
                {
                    var (width, height) = ParseResolution(Project.ResolutionOptions[Project.SelectedResolutionIndex]);
                    Project.ProjectRenderWidth = width;
                    Project.ProjectRenderHeight = height;
                }
            }

            // Show custom resolution controls if "Custom" is selected
            if (Project.SelectedResolutionIndex == 12) // Custom option
            {
                ImGui.Spacing();
                ImGui.Text("Custom Resolution:");

                ImGui.Text("Width:");
                if (ImGui.InputInt("##RenderWidth", ref Project.ProjectRenderWidth, 10, 100))
                {
                    if (Project.ProjectRenderWidth < 1) Project.ProjectRenderWidth = 1;
                    if (Project.ProjectRenderWidth > 7680) Project.ProjectRenderWidth = 7680; // 8K limit
                }

                ImGui.Text("Height:");
                if (ImGui.InputInt("##RenderHeight", ref Project.ProjectRenderHeight, 10, 100))
                {
                    if (Project.ProjectRenderHeight < 1) Project.ProjectRenderHeight = 1;
                    if (Project.ProjectRenderHeight > 4320) Project.ProjectRenderHeight = 4320; // 8K limit
                }
            }

            // Current resolution display
            ImGui.Spacing();
            ImGui.Text($"Current Resolution: {Project.ProjectRenderWidth}x{Project.ProjectRenderHeight}");

            ImGui.Spacing();
            ImGui.Separator();

            // Project framerate
            ImGui.Text("Project Framerate: NOT FINISHED");
            if (ImGui.InputInt("##ProjectFramerate", ref Project.FrameRate, 1, 5))
            {
                if (Project.FrameRate < 1) Project.FrameRate = 1;
                if (Project.FrameRate > 120) Project.FrameRate = 120;
            }

            // Common framerate presets
            ImGui.Spacing();
            ImGui.Text("Presets:");
            if (ImGui.Button("24 fps"))
            {
                Project.FrameRate = 24;
            }

            ImGui.SameLine();
            if (ImGui.Button("30 fps"))
            {
                Project.FrameRate = 30;
            }

            ImGui.SameLine();
            if (ImGui.Button("60 fps"))
            {
                Project.FrameRate = 60;
            }
        }

        // Background Settings Section
        if (ImGui.CollapsingHeader("Background Settings: NOT FINISHED"))
        {
            // Background Image
            ImGui.Text("Background Image");

            // Display current background image name
            ImGui.Text($"Current: {Project.BackgroundImageName}");
            if (ImGui.Button("Load Background Image"))
            {
                // Use file dialog to select background image
                _ = Task.Run(async () =>
                {
                    var imagePath = await util.FileDialog.ShowOpenDialogAsync("Select Background Image");
                    switch (string.IsNullOrEmpty(imagePath))
                    {
                        case false when System.IO.File.Exists(imagePath):
                            // Store the path to load on the main thread
                            _pendingBackgroundImagePath = imagePath;
                            break;
                        case false:
                            Console.WriteLine($"Selected image not found: {imagePath}");
                            break;
                    }
                });
            }

            if (ImGui.Button("Clear Background Image"))
            {
                Main.GetInstance().MainViewport.ClearBackgroundImage();
                Project.BackgroundImageName = "None";
            }

            ImGui.Spacing();

            // Stretch to Fit Toggle
            if (ImGui.Checkbox("Stretch to Fit", ref Project.StretchBackgroundToFit))
            {
                
            }

            // Clear Color (Sky Color)
            ImGui.Text("Sky Color");
            if (ImGui.ColorEdit3("##ClearColor", ref Project.ClearColor))
            {
                //NotifyBackgroundChanged();
            }

            // Sky Color Presets
            ImGui.Spacing();
            ImGui.Text("Presets:");

            // First row of presets
            if (ImGui.Button("Sky Blue"))
            {
                Project.ClearColor = new System.Numerics.Vector3(0.576f, 0.576f, 1.0f); // Default sky blue #9393FF
                //NotifyBackgroundChanged();
            }

            ImGui.SameLine();
            if (ImGui.Button("Sunset"))
            {
                Project.ClearColor = new System.Numerics.Vector3(1.0f, 0.647f, 0.0f); // Orange sunset #FFA500
                //NotifyBackgroundChanged();
            }

            ImGui.SameLine();
            if (ImGui.Button("Dawn"))
            {
                Project.ClearColor = new System.Numerics.Vector3(1.0f, 0.753f, 0.796f); // Pink dawn #FFCCCB
                //NotifyBackgroundChanged();
            }

            // Second row of presets
            if (ImGui.Button("Storm"))
            {
                Project.ClearColor = new System.Numerics.Vector3(0.4f, 0.4f, 0.5f); // Dark stormy gray #666680
                //NotifyBackgroundChanged();
            }

            ImGui.SameLine();
            if (ImGui.Button("Night"))
            {
                Project.ClearColor = new System.Numerics.Vector3(0.1f, 0.1f, 0.2f); // Dark night blue #191933
                //NotifyBackgroundChanged();
            }

            ImGui.SameLine();
            if (ImGui.Button("White"))
            {
                Project.ClearColor = new System.Numerics.Vector3(1.0f, 1.0f, 1.0f); // Pure white #FFFFFF
                //NotifyBackgroundChanged();
            }

            ImGui.Spacing();

            // Floor Visibility
            if (ImGui.Checkbox("Show Floor", ref Project.FloorVisible))
            {
                //NotifyBackgroundChanged();
            }

            // Floor Tile Selection
            if (Project.FloorVisible)
            {
                ImGui.Spacing();
                ImGui.Text("Floor Tile");
                
                // Get current floor texture and selected tile name
                var currentFloor = Main.GetInstance().MainViewport?.World?.Floor;
                string currentTextureName = "tile040"; // Default fallback
                
                // Find current texture name from the loaded texture
                if (currentFloor != null)
                {
                    var mat = (StandardMaterial3D)currentFloor.MaterialOverride;
                    var currentTexture = mat.AlbedoTexture as Texture2D;
                    if (currentTexture != null)
                    {
                        foreach (var kvp in Main.GetInstance().TerrainTextures)
                        {
                            if (kvp.Value == currentTexture)
                            {
                                currentTextureName = kvp.Key;
                                break;
                            }
                        }
                    }
                }
                
                // Create a scrollable area for terrain tiles with fixed height
                var availableSize = ImGui.GetContentRegionAvail();
                var tileGridHeight = 300f; // Fixed height for the tile grid
                
                if (ImGui.BeginChild("TerrainTiles", new Vector2(availableSize.X, tileGridHeight), ImGuiChildFlags.Borders))
                {
                    // Calculate grid layout (4 columns, responsive height)
                    var buttonSize = new Vector2(16, 16);
                    var spacing = 8f;
                    var columns = Math.Max(4, (int)((availableSize.X - 20) / (buttonSize.X + spacing))); // Ensure at least 4 columns
                    
                    int col = 0;
                    foreach (var kvp in Main.GetInstance().TerrainTextures)
                    {
                        string textureName = kvp.Key;
                        Texture2D texture = kvp.Value;
                        
                        // Highlight selected tile with different background color
                        bool isSelected = textureName == currentTextureName;
                        var bgColor = isSelected ? new System.Numerics.Vector4(0.2f, 0.6f, 1.0f, 1.0f) : new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1.0f);
                        var borderColor = isSelected ? new System.Numerics.Vector4(0.1f, 0.4f, 0.8f, 1.0f) : new System.Numerics.Vector4(0.6f, 0.6f, 0.6f, 1.0f);
                        
                        // Create button with texture using the correct ImGui ImageButton
                        var cursorPos = ImGui.GetCursorPos();
                        var rectMin = ImGui.GetCursorScreenPos();
                        var rectMax = rectMin + buttonSize;
                        
                        // Draw background
                        ImGui.GetWindowDrawList().AddRectFilled(
                            rectMin,
                            rectMax,
                            ImGui.GetColorU32(new System.Numerics.Vector4(bgColor.X, bgColor.Y, bgColor.Z, bgColor.W))
                        );
                        
                        // Draw texture
                        ImGui.SetCursorPos(cursorPos);
                        ImGuiGD.Image(texture, buttonSize);
                        
                        // Make it clickable
                        ImGui.SetCursorPos(cursorPos);
                        if (ImGui.InvisibleButton($"tile_{textureName}", buttonSize))
                        {
                            // Apply texture to floor when clicked
                            if (currentFloor != null)
                            {
                                currentFloor.SetTexture(texture);
                            }
                        }
                        
                        // Draw border for selection
                        if (isSelected)
                        {
                            var drawList = ImGui.GetWindowDrawList();
                            drawList.AddRect(
                                rectMin - new Vector2(2, 2),
                                rectMax + new Vector2(2, 2),
                                ImGui.GetColorU32(new System.Numerics.Vector4(borderColor.X, borderColor.Y, borderColor.Z, borderColor.W)),
                                0.0f, 0, 2.0f
                            );
                        }
                        
                        col++;
                        if (col >= columns)
                        {
                            ImGui.NewLine();
                            col = 0;
                        }
                        else
                        {
                            ImGui.SameLine();
                        }
                    }
                }
                ImGui.EndChild();
                
                // Show current selection info
                ImGui.Spacing();
                ImGui.Text($"Selected: {currentTextureName}");
            }
        }
    }

    public static (int width, int height) ParseResolution(string resolutionString)
    {
        // Extract resolution from strings like "FHD 1080p 1920x1080" or "Avatar 512x512"
        var parts = resolutionString.Split(' ');
        foreach (var part in parts)
        {
            if (!part.Contains('x')) continue;
            var dimensions = part.Split('x');
            if (dimensions.Length == 2 &&
                int.TryParse(dimensions[0], out int width) &&
                int.TryParse(dimensions[1], out int height))
            {
                return (width, height);
            }
        }

        // Fallback to 1920x1080 if parsing fails
        return (1920, 1080);
    }
}