#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ImGuiGodot;
using ImGuiNET;
using SimplyRemadeMI.core;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace SimplyRemadeMI.ui;

public class Timeline
{
    public float TimelineZoom { get; set; } = 1.0f;
    public int TimelineStart { get; set; }
    public int TotalFrames { get; set; } = 100;
    public int CurrentFrame { get; set; }
    public float CurrentFrameFloat { get; private set; }
    public bool IsPlaying { get; set; }
    public bool IsScrubbing { get; private set; }
    public bool IsHovered { get; set; }
    
    private int _playStartFrame;
    private float _playStartFrameFloat;
    private Vector2 _scrubberPosition = Vector2.Zero;
    private float _scrubberWidth;
    
    public class SelectedKeyframe
    {
        public string Property { get; set; } = "";
        public int Frame { get; set; } = -1;
        public int ObjectIndex { get; set; } = -1;

        public override bool Equals(object? obj)
        {
            return obj is SelectedKeyframe other &&
                   Property == other.Property &&
                   Frame == other.Frame &&
                   ObjectIndex == other.ObjectIndex;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Property, Frame, ObjectIndex);
        }
    }
    
    private int _previousSelectedObjectIndex;
    
    public HashSet<SelectedKeyframe> SelectedKeyframes { get; private set; } = new HashSet<SelectedKeyframe>();
    
    public bool IsDraggingKeyframe;
    private SelectedKeyframe? _draggingKeyframeInfo = null;
    private Vector2 _dragStartMousePos = Vector2.Zero;
    private int _dragStartFrame = -1;
    
    private bool _isDragSelecting = false;
    private Vector2 _dragSelectionStart = Vector2.Zero;
    private Vector2 _dragSelectionEnd = Vector2.Zero;
    
    public SceneObject? CurrentObject =>
        Main.GetInstance().UI.sceneTreePanel.SceneObjects != null && Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex >= 0 && Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex < Main.GetInstance().UI.sceneTreePanel.SceneObjects.Count
            ? Main.GetInstance().UI.sceneTreePanel.SceneObjects[Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex]
            : null;

    public void Update(float delta)
    {
        if (_previousSelectedObjectIndex != Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex)
        {
            SelectedKeyframes.Clear();
            IsDraggingKeyframe = false;
            _draggingKeyframeInfo = null;
            _previousSelectedObjectIndex = Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex;
        }
        
        if (IsDraggingKeyframe && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            IsDraggingKeyframe = false;
            _draggingKeyframeInfo = null;
        }
        
        if (_isDragSelecting && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            _isDragSelecting = false;
        }
        
        if (IsPlaying)
        {
            // Smoothly advance fractional frame
            CurrentFrameFloat += Main.GetInstance().UI.propertiesPanel.project.FrameRate * delta;
            
            if (CurrentFrameFloat > TotalFrames)
                CurrentFrameFloat = 0.0f;
            
            CurrentFrame = (int)CurrentFrameFloat;
        }
        
        if (IsPlaying || IsScrubbing || IsDraggingKeyframe)
        {
            ApplyKeyframesToObjects();
        }

        if (Input.IsKeyPressed(Key.Delete) && IsHovered)
        {
            if (HasSelectedKeyframe())
            {
                DeleteSelectedKeyframe();
            }
        }
    }

    public void ApplyKeyframesToObjects()
    {
        if (Main.GetInstance().UI.sceneTreePanel.SceneObjects == null) return;

        foreach (var obj in Main.GetInstance().UI.sceneTreePanel.SceneObjects)
        {
            // Only apply position keyframes to components that actually have keyframes
            if (obj.PosXKeyframes.Count > 0)
                obj.Position = new Vector3(
                    EvaluateKeyframesWithDefault(obj.PosXKeyframes, CurrentFrame, obj.TargetPosition.X),
                    obj.PosYKeyframes.Count > 0 ? EvaluateKeyframesWithDefault(obj.PosYKeyframes, CurrentFrame, obj.TargetPosition.Y) : obj.TargetPosition.Y,
                    obj.PosZKeyframes.Count > 0 ? EvaluateKeyframesWithDefault(obj.PosZKeyframes, CurrentFrame, obj.TargetPosition.Z) : obj.TargetPosition.Z
                );
            
            // Only apply rotation keyframes to components that actually have keyframes
            if (obj.RotXKeyframes.Count > 0 || obj.RotYKeyframes.Count > 0 || obj.RotZKeyframes.Count > 0)
                obj.RotationDegrees = new Vector3(
                    obj.RotXKeyframes.Count > 0 ? EvaluateKeyframesWithDefault(obj.RotXKeyframes, CurrentFrame, obj.RotationDegrees.X) : obj.RotationDegrees.X,
                    obj.RotYKeyframes.Count > 0 ? EvaluateKeyframesWithDefault(obj.RotYKeyframes, CurrentFrame, obj.RotationDegrees.Y) : obj.RotationDegrees.Y,
                    obj.RotZKeyframes.Count > 0 ? EvaluateKeyframesWithDefault(obj.RotZKeyframes, CurrentFrame, obj.RotationDegrees.Z) : obj.RotationDegrees.Z
                );
            
            // Only apply scale keyframes to components that actually have keyframes
            if (obj.ScaleXKeyframes.Count > 0 || obj.ScaleYKeyframes.Count > 0 || obj.ScaleZKeyframes.Count > 0)
                obj.Scale = new Vector3(
                    obj.ScaleXKeyframes.Count > 0 ? EvaluateKeyframesWithDefault(obj.ScaleXKeyframes, CurrentFrame, obj.Scale.X) : obj.Scale.X,
                    obj.ScaleYKeyframes.Count > 0 ? EvaluateKeyframesWithDefault(obj.ScaleYKeyframes, CurrentFrame, obj.Scale.Y) : obj.Scale.Y,
                    obj.ScaleZKeyframes.Count > 0 ? EvaluateKeyframesWithDefault(obj.ScaleZKeyframes, CurrentFrame, obj.Scale.Z) : obj.Scale.Z
                );
            
            if (obj.AlphaKeyframes.Count > 0)
                obj.Alpha = EvaluateKeyframesWithDefault(obj.AlphaKeyframes, CurrentFrame, obj.Alpha);
        }
    }

    public float EvaluateKeyframesWithDefault(Dictionary<int, float> keyframes, int frame, float defaultValue)
    {
        if (keyframes.Count == 0)
            return defaultValue;
        
        if (keyframes.ContainsKey(frame))
            return keyframes[frame];
        
        var sortedFrames = keyframes.Keys.OrderBy(k => k).ToList();
        
        if (frame < sortedFrames[0])
            return keyframes[sortedFrames[0]];
        
        if (frame > sortedFrames[^1])
            return keyframes[sortedFrames[^1]];
        
        int leftFrame = -1, rightFrame = -1;
        for (int i = 0; i < sortedFrames.Count - 1; i++)
        {
            if (frame > sortedFrames[i] && frame < sortedFrames[i + 1])
            {
                leftFrame = sortedFrames[i];
                rightFrame = sortedFrames[i + 1];
                break;
            }
        }

        if (leftFrame == -1 || rightFrame == -1)
            return keyframes[sortedFrames[0]];

        // Linear interpolation
        float leftValue = keyframes[leftFrame];
        float rightValue = keyframes[rightFrame];
        float t = (float)(frame - leftFrame) / (rightFrame - leftFrame);

        return leftValue + (rightValue - leftValue) * t;
    }
    
    public void Render(Vector2I pos, Vector2I size)
    {
        ImGui.SetNextWindowPos(new Vector2(pos.X, pos.Y));
        ImGui.SetNextWindowSize(new Vector2(size.X, size.Y));

        if (ImGui.Begin("Timeline", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse |  ImGuiWindowFlags.AlwaysVerticalScrollbar))
        {
            IsHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows);
            HandleZoom();
            
            int visibleFrames = (int)(95 / TimelineZoom);
            int timelineEnd = Math.Min(TimelineStart + visibleFrames, TotalFrames);
            RenderControls(visibleFrames, timelineEnd);
            RenderContent(size, visibleFrames);
        }
        else
        {
            IsHovered = false;
        }
        
        ImGui.End();
    }

    private void HandleZoom()
    {
        var io = ImGui.GetIO();
        if (ImGui.IsWindowHovered() && io.KeyShift && io.MouseWheel != 0)
        {
            float zoomDelta = io.MouseWheel * 0.1f;
            TimelineZoom = Math.Max(0.1f, Math.Min(10.0f, TimelineZoom + zoomDelta));

            // Recalculate timeline bounds
            int tempVisibleFrames = (int)(95 / TimelineZoom);
            if (TimelineStart + tempVisibleFrames > TotalFrames)
                TimelineStart = Math.Max(0, TotalFrames - tempVisibleFrames);

            // Consume the mouse wheel to prevent vertical scrolling
            io.MouseWheel = 0;
        }
    }

    private void RenderControls(int visibleFrames, int timelineEnd)
    {
        ImGui.Text($"Visible: {TimelineStart} - {timelineEnd} ({visibleFrames} frames)");
        ImGui.SameLine();
        if (ImGui.Button("Fit All"))
        {
            TimelineZoom = 95.0f / TotalFrames;
            TimelineStart = 0;
        }
        
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        float zoom = TimelineZoom;
        if (ImGui.SliderFloat("Zoom", ref zoom, 0.1f, 10.0f, "%.1fx"))
        {
            // Clamp zoom
            TimelineZoom = Math.Max(0.1f, Math.Min(10.0f, zoom));
            // Recalculate visible frames after zoom change
            visibleFrames = (int)(95 / TimelineZoom);
            if (TimelineStart + visibleFrames > TotalFrames)
                TimelineStart = Math.Max(0, TotalFrames - visibleFrames);
        }
        
        ImGui.SameLine();
        ImGui.Spacing();
        ImGui.SameLine();
        if (ImGuiGD.ImageButton("##reset", Main.GetInstance().Icons["reset"], new Vector2(16, 16)))
        {
            IsPlaying = false;
            CurrentFrame = 0;
            CurrentFrameFloat = 0;
            ApplyKeyframesToObjects();
        }
        
        ImGui.SameLine();
        var iconTexture = IsPlaying ? Main.GetInstance().Icons["pause"] : Main.GetInstance().Icons["play"];
        if (ImGuiGD.ImageButton("playPause", iconTexture, new Vector2(16, 16)))
        {
            if (!IsPlaying)
            {
                _playStartFrame = CurrentFrame;
                _playStartFrameFloat = CurrentFrameFloat;
            }
            
            IsPlaying = !IsPlaying;
        }
        
        ImGui.SameLine();
        if (ImGuiGD.ImageButton("stopButton", Main.GetInstance().Icons["stop"], new Vector2(16, 16)))
        {
            IsPlaying = false;
            CurrentFrame = _playStartFrame;
            CurrentFrameFloat = _playStartFrameFloat;
            ApplyKeyframesToObjects();
        }

        ImGui.Spacing();
    }

    private void RenderContent(Vector2I size, int visibleFrames)
    {
        var availableWidth = ImGui.GetContentRegionAvail().X;
        var availableHeight = ImGui.GetContentRegionAvail().Y;
        var labelWidth = 150.0f;
        var trackWidth = availableWidth - labelWidth;
        
        ImGui.BeginGroup();
        RenderTrackNamesBox(labelWidth);
        ImGui.EndGroup();
        
        ImGui.SameLine(0, 0); // No spacing between the boxes
        ImGui.BeginGroup();
        RenderTimelineTracksBox(trackWidth, visibleFrames);
        ImGui.EndGroup();
        
        DrawPlayhead(visibleFrames);
        
        RenderTimelineHorizontalScrollbar(visibleFrames);
    }

    private void RenderTimelineHorizontalScrollbar(int visibleFrames)
    {
        var scrollbarHeight = ImGui.GetStyle().ScrollbarSize;
        var verticalScrollbarWidth = ImGui.GetStyle().ScrollbarSize;
        float scrollMax = Math.Max(0.0f, TotalFrames - visibleFrames);
        
        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();
        
        var scrollbarPos = new Vector2(windowPos.X, windowPos.Y + windowSize.Y - scrollbarHeight);
        var scrollbarWidth = windowSize.X - verticalScrollbarWidth;
        
        if (scrollMax > 0)
        {
            ImGui.SetCursorScreenPos(scrollbarPos);

            ImGui.Dummy(Vector2.Zero);

            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.2f, 0.2f, 0.2f, 1.0f)); // Dark background
            ImGui.BeginChild("##TimelineHorizontalScrollbar", new Vector2(scrollbarWidth, scrollbarHeight),
                ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);

            float zoomedTimelineWidth = TotalFrames * TimelineZoom;

            ImGui.InvisibleButton("##ScrollContent", new Vector2(zoomedTimelineWidth, 1));

            float currentScrollX = ImGui.GetScrollX();
            int newTimelineStart = (int)(currentScrollX / TimelineZoom);

            if (newTimelineStart != TimelineStart)
            {
                TimelineStart = Math.Max(0, Math.Min(TotalFrames - visibleFrames, newTimelineStart));
            }
            
            float expectedScrollX = TimelineStart * TimelineZoom;
            if (Math.Abs(currentScrollX - expectedScrollX) > 1.0f)
            {
                ImGui.SetScrollX(expectedScrollX);
            }

            ImGui.EndChild();
            ImGui.PopStyleColor();
        }
    }

    private void DrawPlayhead(int visibleFrames)
    {
        if (CurrentFrame < TimelineStart || CurrentFrame > TimelineStart + visibleFrames)
            return;

        var drawList = ImGui.GetWindowDrawList();
        var currentWindowPos = ImGui.GetWindowPos();
        var currentWindowSize = ImGui.GetWindowSize();
        
        float normalizedPos = (CurrentFrame - TimelineStart) / (float)visibleFrames;
        float playheadX = _scrubberPosition.X + (normalizedPos * _scrubberWidth);
        
        float topY = _scrubberPosition.Y;
        float bottomY =
            currentWindowPos.Y + currentWindowSize.Y -
            ImGui.GetStyle().ScrollbarSize;
        
        var playheadColor = 0xFF0000FFu; // Red color
        drawList.AddLine(new Vector2(playheadX, topY), new Vector2(playheadX, bottomY), playheadColor, 2.0f);
    }

    private void RenderTrackNamesBox(float labelWidth)
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(3, 3));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemSpacing);
        ImGui.BeginChild("##TrackNames", new Vector2(labelWidth, 0), ImGuiChildFlags.AutoResizeY);
        
        var barSize = new Vector2(-1, 18);
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.4f, 0.4f, 0.4f, 1.0f)); // Gray background for header
        ImGui.ProgressBar(0.0f, barSize, "Keyframe Tracks");
        ImGui.PopStyleColor();
        
        if (CurrentObject?.PosXKeyframes.Count > 0)
            RenderTrackLabel(CurrentObject.Name + " Position X:", new Vector4(0.4f, 0.2f, 0.2f, 1.0f));
        if (CurrentObject?.PosYKeyframes.Count > 0)
            RenderTrackLabel(CurrentObject.Name + " Position Y:", new Vector4(0.2f, 0.4f, 0.2f, 1.0f));
        if (CurrentObject?.PosZKeyframes.Count > 0)
            RenderTrackLabel(CurrentObject.Name + " Position Z:", new Vector4(0.2f, 0.2f, 0.4f, 1.0f));
        if (CurrentObject?.RotXKeyframes.Count > 0)
            RenderTrackLabel(CurrentObject.Name + " Rotation X:", new Vector4(0.6f, 0.3f, 0.3f, 1.0f));
        if (CurrentObject?.RotYKeyframes.Count > 0)
            RenderTrackLabel(CurrentObject.Name + " Rotation Y:", new Vector4(0.3f, 0.6f, 0.3f, 1.0f));
        if (CurrentObject?.RotZKeyframes.Count > 0)
            RenderTrackLabel(CurrentObject.Name + " Rotation Z:", new Vector4(0.3f, 0.3f, 0.6f, 1.0f));
        if (CurrentObject?.ScaleXKeyframes.Count > 0)
            RenderTrackLabel(CurrentObject.Name + " Scale X:", new Vector4(0.8f, 0.4f, 0.4f, 1.0f));
        if (CurrentObject?.ScaleYKeyframes.Count > 0)
            RenderTrackLabel(CurrentObject.Name + " Scale Y:", new Vector4(0.4f, 0.8f, 0.4f, 1.0f));
        if (CurrentObject?.ScaleZKeyframes.Count > 0)
            RenderTrackLabel(CurrentObject.Name + " Scale Z:", new Vector4(0.4f, 0.4f, 0.8f, 1.0f));
        
        ImGui.EndChild();
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor();
    }

    private void RenderTrackLabel(string labelText, Vector4 labelColor)
    {
        var barSize = new Vector2(-1, 18);
        var darkerColor = new Vector4(labelColor.X * 0.7f, labelColor.Y * 0.7f, labelColor.Z * 0.7f, labelColor.W);
        var lighterColor = new Vector4(Math.Min(1.0f, labelColor.X * 1.3f), Math.Min(1.0f, labelColor.Y * 1.3f),
            Math.Min(1.0f, labelColor.Z * 1.3f), labelColor.W);

        ImGui.PushStyleColor(ImGuiCol.Button, labelColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, lighterColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, darkerColor);
        ImGui.Button(labelText, barSize);
        ImGui.PopStyleColor(3);
    }

    private void RenderTimelineTracksBox(float trackWidth, int visibleFrames)
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.15f, 0.15f, 0.2f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(3, 3));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemSpacing);
        ImGui.BeginChild("##TimelineTracks", new Vector2(trackWidth, 0), ImGuiChildFlags.AutoResizeY);
        
        RenderFrameScrubber(visibleFrames);
        
        if (CurrentObject?.PosXKeyframes.Count > 0)
            RenderTimelineTrack(CurrentObject.PosXKeyframes.Keys.ToList(), "position.x", visibleFrames);
        if (CurrentObject?.PosYKeyframes.Count > 0)
            RenderTimelineTrack(CurrentObject.PosYKeyframes.Keys.ToList(), "position.y", visibleFrames);
        if (CurrentObject?.PosZKeyframes.Count > 0)
            RenderTimelineTrack(CurrentObject.PosZKeyframes.Keys.ToList(), "position.z", visibleFrames);
        if (CurrentObject?.RotXKeyframes.Count > 0)
            RenderTimelineTrack(CurrentObject.RotXKeyframes.Keys.ToList(), "rotation.x", visibleFrames);
        if (CurrentObject?.RotYKeyframes.Count > 0)
            RenderTimelineTrack(CurrentObject.RotYKeyframes.Keys.ToList(), "rotation.y", visibleFrames);
        if (CurrentObject?.RotZKeyframes.Count > 0)
            RenderTimelineTrack(CurrentObject.RotZKeyframes.Keys.ToList(), "rotation.z", visibleFrames);
        if (CurrentObject?.ScaleXKeyframes.Count > 0)
            RenderTimelineTrack(CurrentObject.ScaleXKeyframes.Keys.ToList(), "scale.x", visibleFrames);
        if (CurrentObject?.ScaleYKeyframes.Count > 0)
            RenderTimelineTrack(CurrentObject.ScaleYKeyframes.Keys.ToList(), "scale.y", visibleFrames);
        if (CurrentObject?.ScaleZKeyframes.Count > 0)
            RenderTimelineTrack(CurrentObject.ScaleZKeyframes.Keys.ToList(), "scale.z", visibleFrames);
        if (CurrentObject?.AlphaKeyframes.Count > 0)
            RenderTimelineTrack(CurrentObject.AlphaKeyframes.Keys.ToList(), "alpha", visibleFrames);
        
        ImGui.EndChild();
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor();
    }

    private void RenderTimelineTrack(List<int> keyframeFrames, string property, int visibleFrames)
    {
        var drawList = ImGui.GetWindowDrawList();
        var barSize = new Vector2(-1, 18);
        
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram,
            new Vector4(0.1f, 0.1f, 0.15f, 1.0f));
        ImGui.ProgressBar(0.0f, barSize, "");
        ImGui.PopStyleColor();
        
        var trackRect = ImGui.GetItemRectMin();
        var trackSize = ImGui.GetItemRectSize();
        
        bool isTrackHovered = ImGui.IsItemHovered();
        
        if (isTrackHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !IsDraggingKeyframe)
        {
            var mousePos = ImGui.GetMousePos();
            bool clickedOnKeyframe = false;
            
            foreach (var frame in keyframeFrames)
            {
                if (frame < TimelineStart || frame > TimelineStart + visibleFrames)
                    continue;

                float normalizedPos = (frame - TimelineStart) / (float)visibleFrames;
                float markerX = trackRect.X + (normalizedPos * trackSize.X);
                float markerY = trackRect.Y + (trackSize.Y * 0.5f);
                float distance = Vector2.Distance(mousePos, new Vector2(markerX, markerY));

                if (distance <= 6.0f)
                {
                    clickedOnKeyframe = true;
                    break;
                }
            }
            
            if (!clickedOnKeyframe)
            {
                var io = ImGui.GetIO();
                if (!io.KeyCtrl)
                {
                    SelectedKeyframes.Clear();
                }

                _isDragSelecting = true;
                _dragSelectionStart = mousePos;
                _dragSelectionEnd = mousePos;
            }
        }
        
        if (_isDragSelecting && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            _dragSelectionEnd = ImGui.GetMousePos();
        }
        
        if (IsDraggingKeyframe && _draggingKeyframeInfo != null &&
            _draggingKeyframeInfo.Property == property &&
            _draggingKeyframeInfo.ObjectIndex == Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex)
        {
            if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                var currentMousePos = ImGui.GetMousePos();
                float deltaX = currentMousePos.X - _dragStartMousePos.X;
                float framePixelWidth = trackSize.X / visibleFrames;
                int frameDelta = (int)(deltaX / framePixelWidth);
                int newFrame = Math.Max(0, _dragStartFrame + frameDelta);
                
                if (newFrame != _draggingKeyframeInfo.Frame)
                {
                    int actualFrameDelta = newFrame - _draggingKeyframeInfo.Frame;
                    MoveSelectedKeyframes(actualFrameDelta);
                    
                    _draggingKeyframeInfo.Frame = newFrame;
                }
            }
            else if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                IsDraggingKeyframe = false;
                _draggingKeyframeInfo = null;
            }
        }
        
        foreach (var frame in keyframeFrames)
        {
            if (frame < TimelineStart || frame > TimelineStart + visibleFrames)
                continue;

            float normalizedPos = (frame - TimelineStart) / (float)visibleFrames;
            float markerX = trackRect.X + (normalizedPos * trackSize.X);
            float markerY = trackRect.Y + (trackSize.Y * 0.5f);

            var keyframeId = new SelectedKeyframe
            {
                Property = property,
                Frame = frame,
                ObjectIndex = Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex
            };

            bool isSelected = SelectedKeyframes.Contains(keyframeId);

            var mousePos = ImGui.GetMousePos();
            float distance = Vector2.Distance(mousePos, new Vector2(markerX, markerY));
            bool isHovered = distance <= 6.0f && isTrackHovered;

            if (isHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !IsDraggingKeyframe)
            {
                var io = ImGui.GetIO();
                if (io.KeyCtrl)
                {
                    if (isSelected)
                    {
                        SelectedKeyframes.Remove(keyframeId);
                    }
                    else
                    {
                        SelectedKeyframes.Add(keyframeId);
                    }
                }
                else
                {
                    if (isSelected && SelectedKeyframes.Count > 1)
                    {
                        IsDraggingKeyframe = true;
                        _draggingKeyframeInfo = keyframeId;
                        _dragStartMousePos = mousePos;
                        _dragStartFrame = frame;
                    }
                    else
                    {
                        SelectedKeyframes.Clear();
                        SelectedKeyframes.Add(keyframeId);
                        
                        IsDraggingKeyframe = true;
                        _draggingKeyframeInfo = keyframeId;
                        _dragStartMousePos = mousePos;
                        _dragStartFrame = frame;
                    }
                }
            }
            
            uint color;
            if (isSelected)
                color = 0xFF00FF00; // Green for selected
            else if (isHovered)
                color = 0xFFFFFFFF; // White for hovered
            else
                color = 0xFF00FFFF; // Yellow for normal

            drawList.AddCircleFilled(new Vector2(markerX, markerY), isSelected ? 5.0f : 4.0f, color);
            
            if (isSelected)
            {
                drawList.AddCircle(new Vector2(markerX, markerY), 5.0f, 0xFF000000, 0, 1.5f); // Black outline
            }
            
            if (_isDragSelecting)
            {
                var selectionMin = new Vector2(Math.Min(_dragSelectionStart.X, _dragSelectionEnd.X),
                    Math.Min(_dragSelectionStart.Y, _dragSelectionEnd.Y));
                var selectionMax = new Vector2(Math.Max(_dragSelectionStart.X, _dragSelectionEnd.X),
                    Math.Max(_dragSelectionStart.Y, _dragSelectionEnd.Y));

                if (markerX >= selectionMin.X && markerX <= selectionMax.X &&
                    markerY >= selectionMin.Y && markerY <= selectionMax.Y)
                {
                    SelectedKeyframes.Add(keyframeId);
                }
            }
        }
        
        if (_isDragSelecting)
        {
            var selectionMin = new Vector2(Math.Min(_dragSelectionStart.X, _dragSelectionEnd.X),
                Math.Min(_dragSelectionStart.Y, _dragSelectionEnd.Y));
            var selectionMax = new Vector2(Math.Max(_dragSelectionStart.X, _dragSelectionEnd.X),
                Math.Max(_dragSelectionStart.Y, _dragSelectionEnd.Y));

            drawList.AddRect(selectionMin, selectionMax, 0xFF00FFFF, 0.0f, ImDrawFlags.None,
                1.5f); // Yellow selection box
            drawList.AddRectFilled(selectionMin, selectionMax, 0x2200FFFF); // Semi-transparent yellow fill
        }
    }

    public void MoveSelectedKeyframes(int frameDelta)
    {
        if (frameDelta == 0 || SelectedKeyframes.Count == 0) return;
        
        var keyframesToUpdate = new List<SelectedKeyframe>(SelectedKeyframes);
        
        var moveOperations = new List<(SelectedKeyframe keyframe, int newFrame)>();
        foreach (var keyframe in keyframesToUpdate)
        {
            int newFrame = Math.Max(0, keyframe.Frame + frameDelta);
            moveOperations.Add((keyframe, newFrame));
        }
        
        SelectedKeyframes.Clear();
        
        foreach (var (keyframe, newFrame) in moveOperations)
        {
            var keyframes = GetKeyframeDictionary(keyframe.Property);
            if (keyframes != null)
            {
                float value = keyframes.ContainsKey(keyframe.Frame) ? keyframes[keyframe.Frame] : 0.0f;
                
                MoveKeyframe(keyframe.Property, keyframe.Frame, newFrame);
                
                int actualFrame = newFrame;
                if (keyframes.ContainsKey(newFrame) && Math.Abs(keyframes[newFrame] - value) < 0.001f)
                {
                    actualFrame = newFrame;
                }
                else
                {
                    foreach (var kvp in keyframes)
                    {
                        if (Math.Abs(kvp.Value - value) < 0.001f)
                        {
                            actualFrame = kvp.Key;
                            break;
                        }
                    }
                }
                
                SelectedKeyframes.Add(new SelectedKeyframe
                {
                    Property = keyframe.Property,
                    Frame = actualFrame,
                    ObjectIndex = keyframe.ObjectIndex
                });
            }
        }
    }

    public void MoveKeyframe(string property, int oldFrame, int newFrame)
    {
        if (CurrentObject == null || oldFrame == newFrame) return;

        Dictionary<int, float> keyframes;
        switch (property.ToLower())
        {
            case "x":
            case "position.x":
                keyframes = CurrentObject.PosXKeyframes;
                break;
            case "y":
            case "position.y":
                keyframes = CurrentObject.PosYKeyframes;
                break;
            case "z":
            case "position.z":
                keyframes = CurrentObject.PosZKeyframes;
                break;
            case "rotation.x":
                keyframes = CurrentObject.RotXKeyframes;
                break;
            case "rotation.y":
                keyframes = CurrentObject.RotYKeyframes;
                break;
            case "rotation.z":
                keyframes = CurrentObject.RotZKeyframes;
                break;
            case "scale.x":
                keyframes = CurrentObject.ScaleXKeyframes;
                break;
            case "scale.y":
                keyframes = CurrentObject.ScaleYKeyframes;
                break;
            case "scale.z":
                keyframes = CurrentObject.ScaleZKeyframes;
                break;
            case "alpha":
                keyframes = CurrentObject.AlphaKeyframes;
                break;
            default:
                return; // Unknown property
        }
        
        if (keyframes.ContainsKey(oldFrame))
        {
            float value = keyframes[oldFrame];
            keyframes.Remove(oldFrame);
            
            int targetFrame = FindAvailableFrame(keyframes, newFrame, oldFrame);
            keyframes[targetFrame] = value;
            CalculateTotalFrames();
        }
    }
    
    public void CalculateTotalFrames()
    {
        int maxKeyframe = 0;
        if (Main.GetInstance().UI.sceneTreePanel.SceneObjects != null)
        {
            foreach (var obj in Main.GetInstance().UI.sceneTreePanel.SceneObjects)
            {
                foreach (var frame in obj.PosXKeyframes.Keys) maxKeyframe = Math.Max(maxKeyframe, frame);
                foreach (var frame in obj.PosYKeyframes.Keys) maxKeyframe = Math.Max(maxKeyframe, frame);
                foreach (var frame in obj.PosZKeyframes.Keys) maxKeyframe = Math.Max(maxKeyframe, frame);
                foreach (var frame in obj.RotXKeyframes.Keys) maxKeyframe = Math.Max(maxKeyframe, frame);
                foreach (var frame in obj.RotYKeyframes.Keys) maxKeyframe = Math.Max(maxKeyframe, frame);
                foreach (var frame in obj.RotZKeyframes.Keys) maxKeyframe = Math.Max(maxKeyframe, frame);
                foreach (var frame in obj.ScaleXKeyframes.Keys) maxKeyframe = Math.Max(maxKeyframe, frame);
                foreach (var frame in obj.ScaleYKeyframes.Keys) maxKeyframe = Math.Max(maxKeyframe, frame);
                foreach (var frame in obj.ScaleZKeyframes.Keys) maxKeyframe = Math.Max(maxKeyframe, frame);
                foreach (var frame in obj.AlphaKeyframes.Keys) maxKeyframe = Math.Max(maxKeyframe, frame);
            }
        }
        
        if (maxKeyframe == 0)
            TotalFrames = 100;
        else
            TotalFrames = Math.Max(maxKeyframe + 100, 500);
    }

    private int FindAvailableFrame(Dictionary<int, float> keyframes, int preferredFrame, int excludeFrame)
    {
        if (!keyframes.ContainsKey(preferredFrame) || preferredFrame == excludeFrame)
        {
            return preferredFrame;
        }
        
        for (int offset = 1; offset <= 10; offset++)
        {
            int rightFrame = preferredFrame + offset;
            if (!keyframes.ContainsKey(rightFrame) && rightFrame != excludeFrame)
            {
                return rightFrame;
            }
            
            int leftFrame = preferredFrame - offset;
            if (leftFrame >= 0 && !keyframes.ContainsKey(leftFrame) && leftFrame != excludeFrame)
            {
                return leftFrame;
            }
        }

        return preferredFrame;
    }

    private Dictionary<int, float>? GetKeyframeDictionary(string property)
    {
        if (CurrentObject == null) return null;

        switch (property.ToLower())
        {
            case "x":
            case "position.x":
                return CurrentObject.PosXKeyframes;
            case "y":
            case "position.y":
                return CurrentObject.PosYKeyframes;
            case "z":
            case "position.z":
                return CurrentObject.PosZKeyframes;
            case "rotation.x":
                return CurrentObject.RotXKeyframes;
            case "rotation.y":
                return CurrentObject.RotYKeyframes;
            case "rotation.z":
                return CurrentObject.RotZKeyframes;
            case "scale.x":
                return CurrentObject.ScaleXKeyframes;
            case "scale.y":
                return CurrentObject.ScaleYKeyframes;
            case "scale.z":
                return CurrentObject.ScaleZKeyframes;
            case "alpha":
                return CurrentObject.AlphaKeyframes;
            default:
                return null;
        }
    }

    private void RenderFrameScrubber(int visibleFrames)
    {
        int scrubberEnd = Math.Min(TimelineStart + visibleFrames, TotalFrames);
        
        var barSize = new Vector2(-1, 18);
        ImGui.ProgressBar(0.0f, barSize, "");
        
        var itemRect = ImGui.GetItemRectMin();
        var itemSize = ImGui.GetItemRectSize();
        
        _scrubberPosition = itemRect;
        _scrubberWidth = itemSize.X;
        
        IsScrubbing = ImGui.IsItemHovered() && ImGui.IsMouseDown(ImGuiMouseButton.Left);
        if (IsScrubbing)
        {
            var mousePos = ImGui.GetMousePos();
            float relativeX = (mousePos.X - itemRect.X) / itemSize.X;
            relativeX = Math.Max(0.0f, Math.Min(1.0f, relativeX));
            
            CurrentFrame = TimelineStart + (int)(relativeX * visibleFrames);
            CurrentFrame = Math.Max(0, Math.Min(TotalFrames, CurrentFrame));
            CurrentFrameFloat = CurrentFrame;
            
            ApplyKeyframesToObjects();
        }
        
        RenderFrameLabelsAndGrid(itemRect, itemSize, visibleFrames, scrubberEnd);
        RenderPlayheadOnScrubber(itemRect, itemSize, visibleFrames, scrubberEnd);
    }

    private void RenderFrameLabelsAndGrid(Vector2 itemRect, Vector2 itemSize, int visibleFrames, int scrubberEnd)
    {
        var drawList = ImGui.GetWindowDrawList();
        var textColor = 0xFFFFFFFF;
        
        int labelInterval;
        if (TimelineZoom >= 8.0f) labelInterval = 1; // 8x+ zoom: every 1 frame (~12 frames visible)
        else if (TimelineZoom >= 4.0f) labelInterval = 2; // 4x-8x zoom: every 2 frames (~24 frames visible)
        else if (TimelineZoom >= 2.0f) labelInterval = 5; // 2x-4x zoom: every 5 frames (~48 frames visible)
        else if (TimelineZoom >= 1.0f) labelInterval = 10; // 1x-2x zoom: every 10 frames (~95 frames visible)
        else if (TimelineZoom >= 0.5f) labelInterval = 20; // 0.5x-1x zoom: every 20 frames (~190 frames visible)
        else labelInterval = 50; // <0.5x zoom: every 50 frames (380+ frames visible)
        
        int startFrame = (TimelineStart / labelInterval) * labelInterval; // Round down to nearest interval
        for (int frame = startFrame; frame <= scrubberEnd; frame += labelInterval)
        {
            if (frame < TimelineStart) continue;

            float progress = (frame - TimelineStart) / (float)visibleFrames;
            float labelX = itemRect.X + (progress * itemSize.X);
            float labelY = itemRect.Y + (itemSize.Y * 0.5f);
            
            drawList.AddText(new Vector2(labelX - 10, labelY - 8), textColor, frame.ToString());
            
            var gridLineColor = 0x33FFFFFFu; // Semi-transparent white
            float lineTopY = itemRect.Y;
            float lineBottomY =
                itemRect.Y + itemSize.Y +
                (9 * (18 + ImGui.GetStyle().ItemSpacing.Y)); // Scrubber + 9 tracks (3 position + 3 rotation + 3 scale)
            drawList.AddLine(new Vector2(labelX, lineTopY), new Vector2(labelX, lineBottomY), gridLineColor, 1.0f);
        }
    }

    private void RenderPlayheadOnScrubber(Vector2 itemRect, Vector2 itemSize, int visibleFrames, int scrubberEnd)
    {
        if (CurrentFrame >= TimelineStart && CurrentFrame <= scrubberEnd)
        {
            var drawList = ImGui.GetWindowDrawList();
            var textColor = 0xFFFFFFFF;

            float progress = (CurrentFrame - TimelineStart) / (float)visibleFrames;
            float playheadX = itemRect.X + (progress * itemSize.X);
            float playheadY = itemRect.Y + (itemSize.Y * 0.5f);

            // Draw red triangle arrow at playhead position
            var playheadColor = 0xFF0000FF; // Red color
            var trianglePoints = new Vector2[]
            {
                new Vector2(playheadX, playheadY + 9),
                new Vector2(playheadX - 6, playheadY - 9),
                new Vector2(playheadX + 6, playheadY - 9)
            };
            drawList.AddTriangleFilled(trianglePoints[0], trianglePoints[1], trianglePoints[2], playheadColor);
            
            drawList.AddText(new Vector2(playheadX - 15, playheadY - 20), textColor, CurrentFrame.ToString());
        }
    }
    
    public void AddKeyframe(string property, int frame, float value)
    {
        if (CurrentObject == null) return;

        Dictionary<int, float> keyframes;
        switch (property.ToLower())
        {
            case "x":
            case "position.x":
                keyframes = CurrentObject.PosXKeyframes;
                break;
            case "y":
            case "position.y":
                keyframes = CurrentObject.PosYKeyframes;
                break;
            case "z":
            case "position.z":
                keyframes = CurrentObject.PosZKeyframes;
                break;
            case "rotation.x":
                keyframes = CurrentObject.RotXKeyframes;
                break;
            case "rotation.y":
                keyframes = CurrentObject.RotYKeyframes;
                break;
            case "rotation.z":
                keyframes = CurrentObject.RotZKeyframes;
                break;
            case "scale.x":
                keyframes = CurrentObject.ScaleXKeyframes;
                break;
            case "scale.y":
                keyframes = CurrentObject.ScaleYKeyframes;
                break;
            case "scale.z":
                keyframes = CurrentObject.ScaleZKeyframes;
                break;
            case "alpha":
                keyframes = CurrentObject.AlphaKeyframes;
                break;
            default:
                return; // Unknown property
        }

        // Check if there is any selected keyframe for the current object and property
        bool hasSelectedKeyframe = SelectedKeyframes.Any(k =>
            k.Property == property &&
            k.ObjectIndex == Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex);

        if (hasSelectedKeyframe)
        {
            // Update the value of the selected keyframe
            var fram = SelectedKeyframes.First(k => k.Property == property && k.ObjectIndex == Main.GetInstance().UI.sceneTreePanel.SelectedObjectIndex).Frame;
            
            keyframes[fram] = value;
        }
        else
        {
            // Add a new keyframe
            keyframes[frame] = value;
        }

        CalculateTotalFrames();
    }
    
    public void RemoveKeyframe(string property, int frame)
    {
        if (CurrentObject == null) return;

        Dictionary<int, float> keyframes;
        switch (property.ToLower())
        {
            case "x":
            case "position.x":
                keyframes = CurrentObject.PosXKeyframes;
                break;
            case "y":
            case "position.y":
                keyframes = CurrentObject.PosYKeyframes;
                break;
            case "z":
            case "position.z":
                keyframes = CurrentObject.PosZKeyframes;
                break;
            case "rotation.x":
                keyframes = CurrentObject.RotXKeyframes;
                break;
            case "rotation.y":
                keyframes = CurrentObject.RotYKeyframes;
                break;
            case "rotation.z":
                keyframes = CurrentObject.RotZKeyframes;
                break;
            case "scale.x":
                keyframes = CurrentObject.ScaleXKeyframes;
                break;
            case "scale.y":
                keyframes = CurrentObject.ScaleYKeyframes;
                break;
            case "scale.z":
                keyframes = CurrentObject.ScaleZKeyframes;
                break;
            case "alpha":
                keyframes = CurrentObject.AlphaKeyframes;
                break;
            default:
                return; // Unknown property
        }

        keyframes.Remove(frame);
        CalculateTotalFrames();
    }
    
    public float EvaluateKeyframes(string property, int frame)
    {
        Dictionary<int, float> keyframes;
        switch (property.ToLower())
        {
            case "x":
            case "position.x":
                keyframes = CurrentObject.PosXKeyframes;
                break;
            case "y":
            case "position.y":
                keyframes = CurrentObject.PosYKeyframes;
                break;
            case "z":
            case "position.z":
                keyframes = CurrentObject.PosZKeyframes;
                break;
            case "rotation.x":
                keyframes = CurrentObject.RotXKeyframes;
                break;
            case "rotation.y":
                keyframes = CurrentObject.RotYKeyframes;
                break;
            case "rotation.z":
                keyframes = CurrentObject.RotZKeyframes;
                break;
            case "scale.x":
                keyframes = CurrentObject.ScaleXKeyframes;
                break;
            case "scale.y":
                keyframes = CurrentObject.ScaleYKeyframes;
                break;
            case "scale.z":
                keyframes = CurrentObject.ScaleZKeyframes;
                break;
            case "alpha":
                keyframes = CurrentObject.AlphaKeyframes;
                break;
            default:
                if (property.StartsWith("scale"))
                    return 1.0f;
                if (property.StartsWith("alpha"))
                    return 1.0f;
                return 0.0f;
        }

        if (keyframes.Count == 0)
        {
            if (property.StartsWith("scale"))
                return 1.0f;
            if (property.StartsWith("alpha"))
                return 1.0f;
            return 3.0f;
        }
        
        if (keyframes.ContainsKey(frame))
            return keyframes[frame];
        
        var sortedFrames = keyframes.Keys.OrderBy(k => k).ToList();
        
        if (frame < sortedFrames[0])
            return keyframes[sortedFrames[0]];
        
        if (frame > sortedFrames[sortedFrames.Count - 1])
            return keyframes[sortedFrames[sortedFrames.Count - 1]];
        
        int leftFrame = -1, rightFrame = -1;
        for (int i = 0; i < sortedFrames.Count - 1; i++)
        {
            if (frame > sortedFrames[i] && frame < sortedFrames[i + 1])
            {
                leftFrame = sortedFrames[i];
                rightFrame = sortedFrames[i + 1];
                break;
            }
        }

        if (leftFrame == -1 || rightFrame == -1)
            return 0.0f; // Shouldn't happen

        // Linear interpolation
        float leftValue = keyframes[leftFrame];
        float rightValue = keyframes[rightFrame];
        float t = (float)(frame - leftFrame) / (rightFrame - leftFrame);
        return leftValue + (rightValue - leftValue) * t;
    }

    public float EvaluateKeyframesWithDefault(Dictionary<int, float> keyframes, float frame, float defaultValue)
    {
        if (keyframes.Count == 0)
            return defaultValue;
        
        var sortedFrames = keyframes.Keys.OrderBy(k => k).ToList();
        
        if (frame < sortedFrames[0])
            return keyframes[sortedFrames[0]];
        
        if (frame > sortedFrames[sortedFrames.Count - 1])
            return keyframes[sortedFrames[sortedFrames.Count - 1]];
        
        int leftFrame = -1, rightFrame = -1;
        for (int i = 0; i < sortedFrames.Count - 1; i++)
        {
            if (frame >= sortedFrames[i] && frame <= sortedFrames[i + 1])
            {
                leftFrame = sortedFrames[i];
                rightFrame = sortedFrames[i + 1];
                break;
            }
        }

        if (leftFrame == -1 || rightFrame == -1)
            return keyframes[sortedFrames[0]]; // Fallback
        
        if (Math.Abs(frame - leftFrame) < 0.001f)
            return keyframes[leftFrame];
        if (Math.Abs(frame - rightFrame) < 0.001f)
            return keyframes[rightFrame];

        // Linear interpolation with fractional frame
        float leftValue = keyframes[leftFrame];
        float rightValue = keyframes[rightFrame];
        float t = (frame - leftFrame) / (rightFrame - leftFrame);

        return leftValue + (rightValue - leftValue) * t;
    }
    
    public Vector3 GetAnimatedPosition()
    {
        var x = EvaluateKeyframes("position.x", CurrentFrame);
        var y = EvaluateKeyframes("position.y", CurrentFrame);
        var z = EvaluateKeyframes("position.z", CurrentFrame);

        return new Vector3(x, y, z);
    }
    
    public Vector3 GetAnimatedRotation()
    {
        var x = EvaluateKeyframes("rotation.x", CurrentFrame);
        var y = EvaluateKeyframes("rotation.y", CurrentFrame);
        var z = EvaluateKeyframes("rotation.z", CurrentFrame);

        return new Vector3(x, y, z);
    }
    
    public Vector3 GetAnimatedScale()
    {
        var x = EvaluateKeyframes("scale.x", CurrentFrame);
        var y = EvaluateKeyframes("scale.y", CurrentFrame);
        var z = EvaluateKeyframes("scale.z", CurrentFrame);

        return new Vector3(x, y, z);
    }
    
    public Vector3 GetAnimatedPosition(SceneObject obj)
    {
        // Use fractional frame for smooth interpolation when playing, integer frame when scrubbing
        float frameToUse = IsPlaying ? CurrentFrameFloat : CurrentFrame;
        var x = EvaluateKeyframesWithDefault(obj.PosXKeyframes, frameToUse, 0.0f);
        var y = EvaluateKeyframesWithDefault(obj.PosYKeyframes, frameToUse, 0.0f);
        var z = EvaluateKeyframesWithDefault(obj.PosZKeyframes, frameToUse, 0.0f);

        return new Vector3(x, y, z);
    }
    
    public Vector3 GetAnimatedRotation(SceneObject obj)
    {
        // Use fractional frame for smooth interpolation when playing, integer frame when scrubbing
        float frameToUse = IsPlaying ? CurrentFrameFloat : CurrentFrame;
        var x = EvaluateKeyframesWithDefault(obj.RotXKeyframes, frameToUse, 0.0f);
        var y = EvaluateKeyframesWithDefault(obj.RotYKeyframes, frameToUse, 0.0f);
        var z = EvaluateKeyframesWithDefault(obj.RotZKeyframes, frameToUse, 0.0f);

        return new Vector3(x, y, z);
    }
    
    public Vector3 GetAnimatedScale(SceneObject obj)
    {
        // Use fractional frame for smooth interpolation when playing, integer frame when scrubbing
        float frameToUse = IsPlaying ? CurrentFrameFloat : CurrentFrame;
        var x = EvaluateKeyframesWithDefault(obj.ScaleXKeyframes, frameToUse, 1.0f);
        var y = EvaluateKeyframesWithDefault(obj.ScaleYKeyframes, frameToUse, 1.0f);
        var z = EvaluateKeyframesWithDefault(obj.ScaleZKeyframes, frameToUse, 1.0f);

        return new Vector3(x, y, z);
    }
    
    public float GetAnimatedAlpha(SceneObject obj)
    {
        // Use fractional frame for smooth interpolation when playing, integer frame when scrubbing
        float frameToUse = IsPlaying ? CurrentFrameFloat : CurrentFrame;
        return EvaluateKeyframesWithDefault(obj.AlphaKeyframes, frameToUse, obj.Alpha);
    }
    
    public float GetAnimatedAlpha()
    {
        if (CurrentObject == null) return 1.0f;
        return GetAnimatedAlpha(CurrentObject);
    }
    
    public bool HasKeyframes(SceneObject obj)
    {
        return obj.PosXKeyframes.Count > 0 || obj.PosYKeyframes.Count > 0 || obj.PosZKeyframes.Count > 0 ||
               obj.RotXKeyframes.Count > 0 || obj.RotYKeyframes.Count > 0 || obj.RotZKeyframes.Count > 0 ||
               obj.ScaleXKeyframes.Count > 0 || obj.ScaleYKeyframes.Count > 0 || obj.ScaleZKeyframes.Count > 0 ||
               obj.AlphaKeyframes.Count > 0;
    }
    
    public bool HasKeyframes()
    {
        if (CurrentObject == null) return false;

        return CurrentObject.PosXKeyframes.Count > 0 || CurrentObject.PosYKeyframes.Count > 0 ||
               CurrentObject.PosZKeyframes.Count > 0 ||
               CurrentObject.RotXKeyframes.Count > 0 || CurrentObject.RotYKeyframes.Count > 0 ||
               CurrentObject.RotZKeyframes.Count > 0 ||
               CurrentObject.ScaleXKeyframes.Count > 0 || CurrentObject.ScaleYKeyframes.Count > 0 ||
               CurrentObject.ScaleZKeyframes.Count > 0 ||
               CurrentObject.AlphaKeyframes.Count > 0;
    }
    
    public bool HasSelectedKeyframe()
    {
        return SelectedKeyframes.Count > 0;
    }
    
    public void DeleteSelectedKeyframe()
    {
        if (SelectedKeyframes.Count > 0 && CurrentObject != null)
        {
            var keyframesToDelete = new List<SelectedKeyframe>(SelectedKeyframes);
            foreach (var keyframe in keyframesToDelete)
            {
                RemoveKeyframe(keyframe.Property, keyframe.Frame);
            }

            SelectedKeyframes.Clear();
        }
    }
}