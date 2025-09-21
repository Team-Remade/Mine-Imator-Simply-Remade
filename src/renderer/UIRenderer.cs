#nullable enable
using System;
using Godot;
using ImGuiGodot;
using ImGuiNET;
using SimplyRemadeMI.ui;
using MenuBar = SimplyRemadeMI.ui.MenuBar;
using Vector2 = System.Numerics.Vector2;

namespace SimplyRemadeMI.renderer;

public class UIRenderer
{
    public SceneTreePanel sceneTreePanel { get; private set; } = new();
    public PropertiesPanel propertiesPanel = new();
    public Timeline timeline = new();
    public SpawnObjectsMenu spawnObjectsMenu = new();
    private ViewportObject ViewportObject;
    private MenuBar menuBar = new();
    
    public bool ShowPreviewWindow = true;
    public bool ShowSpawnMenu = false;
    
    private Dialog? dialog;

    public UIRenderer(ViewportObject viewportObject)
    {
        ViewportObject = viewportObject;
        sceneTreePanel.World = Main.GetInstance().MainViewport.World;
    }

    public void Update(float delta)
    {
        timeline.Update(delta);
        
        if (timeline.IsPlaying || timeline.IsScrubbing || timeline.IsDraggingKeyframe)
        {
            foreach (var obj in sceneTreePanel.SceneObjects)
            {
                if (timeline.HasKeyframes(obj))
                {
                    var animatedPosition = timeline.GetAnimatedPosition(obj);
                    var animatedRotation = timeline.GetAnimatedRotation(obj);
                    var animatedScale = timeline.GetAnimatedScale(obj);
                    var animatedAlpha = timeline.GetAnimatedAlpha(obj);
                    
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
        }
    }
    
    public void Render()
    {
        DrawUI();
    }

    private void DrawUI()
    {
        //ImGuiGD.SetMainViewport(Main.GetInstance().GetWindow());

        ImGui.DockSpaceOverViewport();
        
        var size = Main.GetInstance().WindowSize;

        var sceneTreeHeight = size.Y / 3;
        var scenePropertiesHeight = size.Y - sceneTreeHeight;
        var menuBarHeight = ImGui.GetFrameHeight();
        
        sceneTreePanel.Render(new Vector2I(size.X - 280, (int)menuBarHeight), new Vector2I(280, sceneTreeHeight - (int)menuBarHeight));
        propertiesPanel.Render(new Vector2I(size.X - 280, sceneTreeHeight), new Vector2I(280, scenePropertiesHeight));
        timeline.Render(new Vector2I(0, size.Y - 200), new Vector2I(size.X - 280, 200));
        ViewportObject.Render(new Vector2I(0, (int)menuBarHeight), new Vector2I(size.X - 280, size.Y - 200 - (int)menuBarHeight));
        menuBar.Render();
        
        if (menuBar.ShouldShowRenderSettings)
        {
            RenderDialogInputBlocker(new System.Numerics.Vector2(Main.GetInstance().GetWindow().Size.X, Main.GetInstance().GetWindow().Size.Y));
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
            RenderDialogOverlay(new System.Numerics.Vector2(Main.GetInstance().WindowSize.X, Main.GetInstance().WindowSize.Y));

            // Finally render the dialog on top
            dialog.Render(new System.Numerics.Vector2(Main.GetInstance().WindowSize.X, Main.GetInstance().WindowSize.Y));

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
    }
    
    private void ShowRenderAnimationDialog()
    {
        if (dialog == null)
        {
            var io = ImGui.GetIO();
            var centerPos = new Vector2(io.DisplaySize.X * 0.5f - 200, io.DisplaySize.Y * 0.5f - 150);

            dialog = new Dialog(
                DialogType.RenderAnimation,
                "RENDER ANIMATION",
                "",
                centerPos,
                () => {  },
                () =>
                {
                    RemoveDialog();
                },
                null, // No single frame render callback for animation dialog
                (width, height, framerate, bitrate, format, filePath) =>
                {
                    Main.GetInstance().RenderAnimation(filePath, width, height, framerate, bitrate, format);
                },
                propertiesPanel // Pass the properties panel reference
            );
        }
    }

    private void RenderPreviewWindow()
    {
        if (!Main.GetInstance().Output.PreviewMode)
            return;
        
        //ImGui.SetNextWindowSize(new Vector2(Main.GetInstance().Output.Size.X, Main.GetInstance().Output.Size.Y));

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        var windowFlags = ImGuiWindowFlags.NoCollapse |
                          ImGuiWindowFlags.NoScrollbar;

        if (Main.GetInstance().MainViewport.Controlled)
        {
            windowFlags |= ImGuiWindowFlags.NoInputs;
        }

        if (ImGui.Begin("Preview", windowFlags))
        {
            var size = ImGui.GetContentRegionAvail();
            if (size.X > 5 && size.Y > 5)
            {
                Main.GetInstance().Output.CallDeferred(SubViewport.MethodName.SetSize,
                    new Vector2I((int)(size.Y * Main.GetInstance().UI.propertiesPanel.project._aspectRatio), (int)size.Y));

                ImGuiGD.SubViewport(Main.GetInstance().Output);

                //ImGuiGD.SubViewport(Main.GetInstance().Output);

                
            }
            
            ImGui.End();
        }

        ImGui.PopStyleVar();
    }
    
    private void RenderDialogOverlay(System.Numerics.Vector2 windowSize)
    {
        // Create a fullscreen overlay - just provides the dark visual background
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(Main.GetInstance().GetWindow().Position.X, Main.GetInstance().GetWindow().Position.Y));
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
    
    private void RenderDialogInputBlocker(System.Numerics.Vector2 windowSize)
    {
        // Create a fullscreen invisible input blocker
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(Main.GetInstance().GetWindow().Position.X, Main.GetInstance().GetWindow().Position.Y));
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
        if (dialog == null)
        {
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
                    Main.GetInstance().MainViewport._shouldRenderToFile = true;
                    Main.GetInstance().MainViewport._renderWidth = width;
                    Main.GetInstance().MainViewport._renderHeight = height;
                    Main.GetInstance().MainViewport._renderFilePath = filePath;
                },
                propertiesPanel: propertiesPanel
            );
        }
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
        spawnObjectsMenu.Render();
        
        // Check for outside clicks to close the menu
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow))
        {
            //ShowSpawnMenu = false;
        }
    }
}