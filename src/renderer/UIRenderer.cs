#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ImGuiGodot;
using ImGuiNET;
using SimplyRemadeMI.core;
using SimplyRemadeMI.ui;
using MenuBar = SimplyRemadeMI.ui.MenuBar;
using Vector2 = System.Numerics.Vector2;

namespace SimplyRemadeMI.renderer;

public class UIRenderer
{
    public SceneTreePanel SceneTreePanel { get; } = new();
    public readonly PropertiesPanel PropertiesPanel = new();
    public readonly Timeline Timeline = new();
    private readonly SpawnObjectsMenu SpawnObjectsMenu = new();
    private readonly ViewportObject _viewportObject;
    private readonly MenuBar menuBar = new();
    
    // Property to check if any blocking UI is visible
    public bool IsBlockingUIVisible => ShowSpawnMenu || dialog != null || menuBar.ShouldShowRenderSettings || menuBar.ShouldShowRenderAnimation || util.FileDialog.IsDialogOpen;
    
    private Camera3D? ActiveCamera;
    
    public bool ShowPreviewWindow = true;
    public bool ShowSpawnMenu = false;
    
    private Dialog? dialog;

    public UIRenderer(ViewportObject viewportObject)
    {
        _viewportObject = viewportObject;
        SceneTreePanel.World = Main.GetInstance().MainViewport.World;
        
        ImGuiGD.Connect(Render);
    }

    public void Update(float delta)
    {
        Timeline.Update(delta);

        ActiveCamera ??= Main.GetInstance().MainViewport.Camera;

        if (!Timeline.IsPlaying && !Timeline.IsScrubbing && !Timeline.IsDraggingKeyframe) return;
        foreach (var obj in SceneTreePanel.SceneObjects.Values)
        {
            if (!Timeline.HasKeyframes(obj)) continue;
            var animatedPosition = Timeline.GetAnimatedPosition(obj);
            var animatedRotation = Timeline.GetAnimatedRotation(obj);
            var animatedScale = Timeline.GetAnimatedScale(obj);
            var animatedAlpha = Timeline.GetAnimatedAlpha(obj);
                    
            var pos = obj.TargetPosition;

            if (obj.PosXKeyframes.Count > 0)
            {
                pos.X = animatedPosition.X;
            }

            if (obj.PosYKeyframes.Count > 0)
            {
                pos.Y = animatedPosition.Y;
            }

            if (obj.PosZKeyframes.Count > 0)
            {
                pos.Z = animatedPosition.Z;
            }

            obj.TargetPosition = pos;
                    
            var rot = obj.GetRotationDegrees();

            if (obj.RotXKeyframes.Count > 0)
            {
                rot.X = animatedRotation.X;
            }

            if (obj.RotYKeyframes.Count > 0)
            {
                rot.Y = animatedRotation.Y;
            }

            if (obj.RotZKeyframes.Count > 0)
            {
                rot.Z = animatedRotation.Z;
            }
                    
            obj.RotationDegrees = rot;
                    
            var scale = obj.GetScale();

            if (obj.ScaleXKeyframes.Count > 0)
            {
                scale.X = animatedScale.X;
            }

            if (obj.ScaleYKeyframes.Count > 0)
            {
                scale.Y = animatedScale.Y;
            }

            if (obj.ScaleZKeyframes.Count > 0)
            {
                scale.Z = animatedScale.Z;
            }
                    
            obj.Scale = scale;
            obj.Alpha = animatedAlpha;
        }
    }

    private void Render()
    {
        DrawUI();
    }

    private void DrawUI()
    {
        ImGui.DockSpaceOverViewport();

        if (ActiveCamera != null) Main.GetInstance().Output.MainCamera.GlobalTransform = ActiveCamera.GlobalTransform;

        var size = Main.GetInstance().WindowSize;
        
        var menuBarHeight = ImGui.GetFrameHeight();
        
        SceneTreePanel.Render();
        PropertiesPanel.Render();
        Timeline.Render(new Vector2I(size.X - 280, 200));
        _viewportObject.Render(new Vector2I(0, (int)menuBarHeight));
        menuBar.Render();
        
        if (menuBar.ShouldShowRenderSettings)
        {
            RenderDialogInputBlocker(new Vector2(Main.GetInstance().GetWindow().Size.X, Main.GetInstance().GetWindow().Size.Y));
            ShowRenderSettingsDialog();
        }
        
        if (menuBar.ShouldShowRenderAnimation)
        {
            RenderDialogInputBlocker(new Vector2(Main.GetInstance().GetWindow().Size.X, Main.GetInstance().GetWindow().Size.Y));
            ShowRenderAnimationDialog();
        }

        if (ShowPreviewWindow)
        {
            RenderPreviewWindow();
        }
        
        if (dialog != null)
        {
            // Render overlay first
            RenderDialogOverlay(new Vector2(Main.GetInstance().WindowSize.X, Main.GetInstance().WindowSize.Y));

            // Finally render the dialog on top
            dialog.Render(new Vector2(Main.GetInstance().WindowSize.X, Main.GetInstance().WindowSize.Y));

            // Check for outside clicks and close dialog if needed
            if (dialog.CheckOutsideClick())
            {
                dialog = null;
            }
            // Remove dialog if it's no longer visible (only check if dialog still exists)
            else if (!dialog.Visible)
            {
                dialog = null;
            }
        }
        
        // Render spawn menu if visible
        if (ShowSpawnMenu)
        {
            RenderSpawnMenu();
        }
        
        // Render input blocker when FileDialog is open
        if (util.FileDialog.IsDialogOpen)
        {
            RenderDialogInputBlocker(new Vector2(Main.GetInstance().WindowSize.X, Main.GetInstance().WindowSize.Y));
        }
    }
    
    private void ShowRenderAnimationDialog()
    {
        if (dialog != null) return;
        var io = ImGui.GetIO();
        var centerPos = new Vector2(io.DisplaySize.X * 0.5f - 200, io.DisplaySize.Y * 0.5f - 150);

        dialog = new Dialog(
            DialogType.RenderAnimation,
            "RENDER ANIMATION",
            "",
            centerPos,
            () => {  },
            RemoveDialog,
            null, // No single frame render callback for animation dialog
            (width, height, framerate, bitrate, format, filePath) =>
            {
                Main.GetInstance().RenderAnimation(filePath, width, height, framerate, bitrate, format);
            },
            PropertiesPanel // Pass the properties panel reference
        );
    }

    private int _selectedCameraIndex = 0;
    private readonly List<Camera3D> _availableCameras = [];

    public void UpdateAvailableCameras()
    {
        _availableCameras.Clear();
        
        // Always include the main camera from RenderOutput
        _availableCameras.Add(Main.GetInstance().Output.MainCamera);
        
        // Find all SceneObjects of type Camera that have a valid camera component
        foreach (var camera in (from sceneObject in SceneTreePanel.SceneObjects.Values where sceneObject.ObjectType == SceneObject.Type.Camera select sceneObject.GetCamera()).OfType<Camera3D>())
        {
            _availableCameras.Add(camera);
        }
    }

    private void RenderPreviewWindow()
    {
        if (!Main.GetInstance().Output.PreviewMode)
            return;
        
        // Update available cameras list
        UpdateAvailableCameras();

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        var windowFlags = ImGuiWindowFlags.NoCollapse |
                          ImGuiWindowFlags.NoScrollbar;

        if (Main.GetInstance().MainViewport.Controlled)
        {
            windowFlags |= ImGuiWindowFlags.NoInputs;
        }

        if (ImGui.Begin("Preview", windowFlags))
        {
            // Camera selection dropdown at the top
            ImGui.Text("Camera:");
            ImGui.SetNextItemWidth(-1); // Full width
            if (ImGui.Combo("##CameraSelect", ref _selectedCameraIndex, GetCameraNames(), _availableCameras.Count))
            {
                // Camera selection changed, set the selected camera as current
                if (_selectedCameraIndex >= 0 && _selectedCameraIndex < _availableCameras.Count)
                {
                    // First set all cameras to non-current
                    foreach (var camera in _availableCameras)
                    {
                        camera.Visible = false;
                    }
                    
                    // Then set the selected camera as current
                    var selectedCamera = _availableCameras[_selectedCameraIndex];
                    selectedCamera.Visible = true;
                    ActiveCamera = selectedCamera;
                }
            }

            ImGui.Separator();

            var size = ImGui.GetContentRegionAvail();
            if (size.X > 5 && size.Y > 5)
            {
                Main.GetInstance().Output.CallDeferred(SubViewport.MethodName.SetSize,
                    new Vector2I((int)(size.Y * Main.GetInstance().UI.PropertiesPanel.Project.AspectRatio), (int)size.Y));

                ImGuiGD.SubViewport(Main.GetInstance().Output);
            }
            
            ImGui.End();
        }

        ImGui.PopStyleVar();
    }

    private string[] GetCameraNames()
    {
        var names = new string[_availableCameras.Count];
        for (int i = 0; i < _availableCameras.Count; i++)
        {
            if (i == 0)
            {
                names[i] = "Main Camera";
            }
            else
            {
                // For user cameras, try to get the name from the parent SceneObject
                var camera = _availableCameras[i];
                var sceneObject = camera.GetParent()?.GetParent() as SceneObject;
                names[i] = sceneObject != null ? sceneObject.Name : $"Camera {i}";
            }
        }
        return names;
    }
    
    private static void RenderDialogOverlay(Vector2 windowSize)
    {
        // Create a fullscreen overlay - just provides the dark visual background
        ImGui.SetNextWindowPos(new Vector2(Main.GetInstance().GetWindow().Position.X, Main.GetInstance().GetWindow().Position.Y));
        ImGui.SetNextWindowSize(windowSize);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0.0f, 0.0f, 0.0f, 0.5f)); // Semi-transparent black overlay

        if (ImGui.Begin("##DialogOverlay", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                                           ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar |
                                           ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs))
        {
            // Overlay just provides visual background - no input blocking
        }

        ImGui.End();
        ImGui.PopStyleColor();
    }
    
    private void RenderDialogInputBlocker(Vector2 windowSize)
    {
        // Create a fullscreen invisible input blocker
        ImGui.SetNextWindowPos(new Vector2(Main.GetInstance().GetWindow().Position.X, Main.GetInstance().GetWindow().Position.Y));
        ImGui.SetNextWindowSize(windowSize);
        if (ImGui.Begin("##DialogInputBlocker", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                                                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar |
                                                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoBringToFrontOnFocus))
        {
            // Invisible button that captures all input to block UI behind the dialog
            ImGui.InvisibleButton("##InputBlocker", windowSize);
        }

        ImGui.End();
    }

    private void ShowRenderSettingsDialog()
    {
        if (dialog != null) return;
        var io = ImGui.GetIO();
        var center = new Vector2(io.DisplaySize.X * 0.5f - 150, io.DisplaySize.Y * 0.5f - 100);
            
        dialog = new Dialog(
            DialogType.RenderSettings,
            "RENDER SETTINGS",
            "",
            center,
            () => {  },
            RemoveDialog,
            (width, height, filePath) =>
            {
                Main.GetInstance().MainViewport.ShouldRenderToFile = true;
                Main.GetInstance().MainViewport.RenderWidth = width;
                Main.GetInstance().MainViewport.RenderHeight = height;
                Main.GetInstance().MainViewport.RenderFilePath = filePath;
            },
            propertiesPanel: PropertiesPanel
        );
    }

    public void ShowDeleteConfirmationDialog()
    {
        if (SceneTreePanel.SelectedObject == null)
            return;

        var io = ImGui.GetIO();
        var centerPos = new Vector2(io.DisplaySize.X * 0.5f - 150, io.DisplaySize.Y * 0.5f - 50);

        dialog = new Dialog(
            DialogType.DeleteConfirmation,
            "DELETE OBJECT",
            $"Are you sure you want to delete '{SceneTreePanel.SelectedObject.Name}'?",
            centerPos,
            () =>
            {
                // Delete the object
                SceneTreePanel.DeleteSelectedObject();
            },
            () =>
            {
                // Cancel - do nothing
            }
        );
    }

    private void RemoveDialog()
    {
        menuBar.ShouldShowRenderSettings = false;
        menuBar.ShouldShowRenderAnimation = false;
    }
    
    private void RenderSpawnMenu()
    {
        // Get the bench position from ViewportObject
        var benchPos = ViewportObject.GetBenchPosition();
        var benchSize = new Vector2(64, 64);
        
        // Position the menu to the right of the bench
        var menuPos = new Vector2(benchPos.X + benchSize.X + 10, benchPos.Y);
        ImGui.SetNextWindowPos(menuPos);
        
        // Render the spawn menu
        SpawnObjectsMenu.Render();
    }
}