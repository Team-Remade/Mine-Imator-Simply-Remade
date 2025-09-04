using System;
using Godot;
using ImGuiNET;
using SimplyRemadeMI.ui;
using MenuBar = SimplyRemadeMI.ui.MenuBar;

namespace SimplyRemadeMI.renderer;

public class UIRenderer
{
    public SceneTreePanel sceneTreePanel { get; private set; } = new();
    public PropertiesPanel propertiesPanel = new();
    public Timeline timeline = new();
    private ViewportObject ViewportObject;
    private MenuBar menuBar = new();

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
                    
                    var pos = obj.GetPosition();

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

                    obj.Position = pos;
                    
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
        
        return;
        
        if (timeline.HasKeyframes() && (timeline.IsPlaying || timeline.IsScrubbing || timeline.IsDraggingKeyframe))
        {
            var keyframePosition = timeline.GetAnimatedPosition();
            var keyframeRotation = timeline.GetAnimatedRotation();
            var keyframeScale = timeline.GetAnimatedScale();
            var keyframeAlpha = timeline.GetAnimatedAlpha();

            sceneTreePanel.SelectedObject.Position = keyframePosition;
            sceneTreePanel.SelectedObject.RotationDegrees = keyframeRotation;
            sceneTreePanel.SelectedObject.Scale = keyframeScale;
            sceneTreePanel.SelectedObject.Alpha = keyframeAlpha;
        }
    }
    
    public void Render()
    {
        DrawUI();
    }

    private void DrawUI()
    {
        var size = Main.GetInstance().WindowSize;

        var sceneTreeHeight = size.Y / 3;
        var scenePropertiesHeight = size.Y - sceneTreeHeight;
        var menuBarHeight = ImGui.GetFrameHeight();
        
        sceneTreePanel.Render(new Vector2I(size.X - 280, (int)menuBarHeight), new Vector2I(280, sceneTreeHeight - (int)menuBarHeight));
        propertiesPanel.Render(new Vector2I(size.X - 280, sceneTreeHeight), new Vector2I(280, scenePropertiesHeight));
        timeline.Render(new Vector2I(0, size.Y - 200), new Vector2I(size.X - 280, 200));
        ViewportObject.Render(new Vector2I(0, (int)menuBarHeight), new Vector2I(size.X - 280, size.Y - 200 - (int)menuBarHeight));
        menuBar.Render();
    }
}