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
    // Timeline state
    public float TimelineZoom { get; set; } = 1.0f;
    public int TimelineStart { get; set; }

    public int TotalFrames { get; set; } = 5000;
    public int CurrentFrame { get; set; }
    public float CurrentFrameFloat { get; private set; }
    public bool IsPlaying { get; set; }
    public bool IsScrubbing { get; private set; }
    public bool IsHovered { get; set; }

    private int _playStartFrame;
    private float _playStartFrameFloat;
    private Vector2 _scrubberPosition = Vector2.Zero;
    private float _scrubberWidth;

    // Auto-keyframe control
    private bool _wasPlayingBeforeManipulation = false;
    private int _manipulationStartFrame = -1;

    // Region interaction
    private enum RegionHandleType { None, Left, Right, Body }
    private bool _isDraggingRegion = false;
    private bool _isResizingRegion = false;
    private RegionHandleType _activeRegionHandle = RegionHandleType.None;
    private int _regionDragStartFrameLeft = 0;
    private int _regionDragStartFrameRight = 0;
    private Vector2 _regionDragStartMouse = Vector2.Zero;

    // Focus control
    private bool _timelineHasFocus = false;
    private bool _spaceWasPressed = false;

    // Track selection
    private string? _selectedTrackProperty = null;

    // Manual scroll control
    private bool _isManuallyScrolling = false;
    private float _manualScrollTimer = 0.0f;
    private const float MANUAL_SCROLL_COOLDOWN = 0.3f;

    // Arrow key navigation
    private float _arrowKeyHoldTime = 0.0f;
    private const float ARROW_KEY_INITIAL_DELAY = 0.3f;
    private const float ARROW_KEY_REPEAT_RATE = 0.05f;
    private float _arrowKeyRepeatTimer = 0.0f;

    // Copy/Cut/Paste
    private readonly List<(SelectedKeyframe keyframe, float value)> _copiedKeyframes = new();
    private int _copiedKeyframesBaseFrame = 0;
    private bool _isCutOperation = false;

    // Region loop functionality
    private bool _isCreatingRegion = false;
    private Vector2 _regionStartPos = Vector2.Zero;
    private Vector2 _regionEndPos = Vector2.Zero;
    private int _regionStartFrame = 0;
    private int _regionEndFrame = 0;
    private bool _regionActive = false;
    private bool _loopInRegion = false;

    // Markers functionality
    public class TimelineMarker
    {
        public int Frame { get; set; }
        public string Label { get; set; } = "New marker";
        public MarkerColor Color { get; set; } = MarkerColor.Red;
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }

    public enum MarkerColor
    {
        Red,
        Orange,
        Yellow,
        Green,
        ForestGreen,
        Teal,
        Blue,
        Purple,
        Pink
    }

    private List<TimelineMarker> _markers = new();
    private TimelineMarker? _hoveredMarker = null;
    private TimelineMarker? _draggedMarker = null;
    private TimelineMarker? _editingMarker = null;
    private Vector2 _markerDragStartMouse = Vector2.Zero;
    private int _markerDragStartFrame = 0;
    private int _markerDragStartTimelineStart = 0;
    private bool _markerPopupJustOpened = false;

    // Keyframe selection
    public class SelectedKeyframe
    {
        public string Property { get; init; } = "";
        public int Frame { get; init; } = -1;
        public Guid? ObjectGuid { get; init; } = null;

        public override bool Equals(object? obj)
        {
            return obj is SelectedKeyframe other &&
                   Property == other.Property &&
                   Frame == other.Frame &&
                   ObjectGuid == other.ObjectGuid;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Property, Frame, ObjectGuid);
        }
    }

    private int _previousSelectedObjectIndex = -1;
    public HashSet<SelectedKeyframe> SelectedKeyframes { get; private set; } = [];

    // Dragging state
    public bool IsDraggingKeyframe { get; private set; } = false;

    private readonly Dictionary<SelectedKeyframe, (int originalFrame, float value)> _draggedKeyframesData = new();
    private Vector2 _dragStartMousePos = Vector2.Zero;
    private int _dragStartTimelineStart = 0;
    private int _currentDragOffset = 0;
    private readonly Dictionary<SelectedKeyframe, int> _previewFrames = new();
    private SelectedKeyframe? _dragAnchorKeyframe = null;

    private float _dragStartScrubberWidth = 0f;
    private float _dragStartScrubberX = 0f;

    // Box selection
    private bool _isDragSelecting = false;
    private Vector2 _dragSelectionStart = Vector2.Zero;
    private Vector2 _dragSelectionEnd = Vector2.Zero;
    private int _dragSelectionStartFrame = 0;
    private int _dragSelectionStartTimelineStart = 0;
    private bool _shouldApplyBoxSelection = false;
    private Vector2 _finalSelectionMin = Vector2.Zero;
    private Vector2 _finalSelectionMax = Vector2.Zero;

    // Animated border
    private float _dashAnimationOffset = 0.0f;
    private float _dashOffset = 0.0f;

    // Scrubber positioning
    private bool _isPendingScrubberMove = false;
    private Vector2 _pendingScrubberPosition = Vector2.Zero;
    private float _pendingScrubberTrackWidth = 0.0f;

    // Relative keyframe editing
    private bool _isEditingSelectedKeyframes = false;
    private readonly Dictionary<SelectedKeyframe, float> _keyframeEditStartValues = new();
    private Vector3 _objectEditStartPosition = Vector3.Zero;
    private Vector3 _objectEditStartRotation = Vector3.Zero;
    private Vector3 _objectEditStartScale = Vector3.One;
    private float _objectEditStartAlpha = 1.0f;

    // cached property arrays
    private static readonly (string, Func<SceneObject, Dictionary<int, Keyframe>>)[] PropertyAccessors =
    [
        ("position.x", obj => obj.PosXKeyframes),
        ("position.y", obj => obj.PosYKeyframes),
        ("position.z", obj => obj.PosZKeyframes),
        ("rotation.x", obj => obj.RotXKeyframes),
        ("rotation.y", obj => obj.RotYKeyframes),
        ("rotation.z", obj => obj.RotZKeyframes),
        ("scale.x", obj => obj.ScaleXKeyframes),
        ("scale.y", obj => obj.ScaleYKeyframes),
        ("scale.z", obj => obj.ScaleZKeyframes),
        ("alpha", obj => obj.AlphaKeyframes)
    ];

    // cache for sorted keyframe lists
    private readonly Dictionary<string, List<int>> _sortedKeyframeCache = new();
    private int _lastCacheObjectHash = -1;

    // Reusable lists
    private readonly List<SelectedKeyframe> _tempKeyframeList = [];
    private List<int> _tempFrameList = [];

    // Ruler step calculation cache
    private int _cachedMinorStep = 1;
    private int _cachedMediumStep = 5;
    private int _cachedMajorStep = 10;
    private float _lastCalculatedPxPerFrame = -1f;
    private int _lastCalculatedVisibleFrames = -1;

    // Scroll tuning constants
    private const float MAX_SCROLL_DISTANCE = 250f;
    private const float MIN_SCROLL_SPEED = 0.5f;
    private const float MAX_SCROLL_SPEED = 2.5f;
    private const float SCROLL_MARGIN = 40.0f;
    private const float DRAG_SCROLL_MAX_DISTANCE = 200f;
    private const float DRAG_SCROLL_MIN_SPEED = 1.0f;
    private const float DRAG_SCROLL_MAX_SPEED = 3.0f;
    private const float SCRUBBER_SCROLL_SPEED = 0.25f;
    private const int MAX_FRAMES_PER_TICK = 2;
    private const float NUDGE_MARGIN = 24.0f;

    public static SceneObject? CurrentObject
    {
        get
        {
            var instance = Main.GetInstance();
            if (instance?.UI?.SceneTreePanel == null)
                return null;

            return instance.UI.SceneTreePanel.SelectedObject;
        }
    }

    public void Update(float delta)
    {
        UpdateManualScrollTimer(delta);
        UpdateDashAnimation(delta);
        HandleObjectSelectionChange();
        HandleDragRelease();
        HandleBoxSelectionRelease();
        HandleScrubberRelease();
        HandleSpacebarPlayback();
        HandleArrowKeyNavigation(delta);
        HandlePlayback(delta);
        HandleKeyboardShortcuts();
    }

    private void UpdateManualScrollTimer(float delta)
    {
        if (!_isManuallyScrolling) return;
        _manualScrollTimer -= delta;
        if (_manualScrollTimer <= 0.0f)
        {
            _isManuallyScrolling = false;
        }
    }

    private void HandleObjectSelectionChange()
    {
        // Clear cache when object changes
        var newObjectHash = CurrentObject?.GetHashCode() ?? 0;
        if (newObjectHash == _lastCacheObjectHash) return;
        
        // Only clear keyframe-related state when object actually changes
        SelectedKeyframes.Clear();
        IsDraggingKeyframe = false;
        _draggedKeyframesData.Clear();
        _dragAnchorKeyframe = null;
        _selectedTrackProperty = null;
        _sortedKeyframeCache.Clear();
        _lastCacheObjectHash = newObjectHash;
    }

    private void UpdateDashAnimation(float delta)
    {
        _dashAnimationOffset += delta * 20.0f;
        if (_dashAnimationOffset > 10.0f)
        {
            _dashAnimationOffset -= 10.0f;
        }
    }

    private void HandleDragRelease()
    {
        if (IsDraggingKeyframe && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            ApplyDraggedKeyframes();
            IsDraggingKeyframe = false;
            _draggedKeyframesData.Clear();
            _previewFrames.Clear();
            _currentDragOffset = 0;
            _dragAnchorKeyframe = null;
        }

        // Release marker drag
        if (_draggedMarker != null && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            _draggedMarker = null;
        }
    }

    private void HandleBoxSelectionRelease()
    {
        if (!_isDragSelecting || ImGui.IsMouseDown(ImGuiMouseButton.Left)) return;
        var io = ImGui.GetIO();
        if (!io.KeyCtrl)
        {
            SelectedKeyframes.Clear();
        }

        _finalSelectionMax = _dragSelectionEnd;
        _shouldApplyBoxSelection = true;
        _isDragSelecting = false;
        _isPendingScrubberMove = false;
    }

    private void DrawBoxSelectionWithAnimatedDashes(float ceilingY)
    {
        var drawList = ImGui.GetWindowDrawList();
        float scrollbarWidth = ImGui.GetStyle().ScrollbarSize;

        int visibleFrames = Math.Max(1, (int)(95 / TimelineZoom));

        // Calculate start X position based on stored frame
        float normalizedStartPos = (_dragSelectionStartFrame - TimelineStart) / (float)visibleFrames;

        var trackRect = _scrubberPosition;
        float trackWidth = _scrubberWidth - scrollbarWidth;

        // Calculate start X based on original frame
        float adjustedStartX = trackRect.X + (normalizedStartPos * trackWidth);

        // Use original stored Y
        var adjustedStart = new Vector2(adjustedStartX, _dragSelectionStart.Y);

        // Clamp minimum Y to ceiling (scrubber)
        float minY = Math.Max(ceilingY, Math.Min(adjustedStart.Y, _dragSelectionEnd.Y));
        float maxY = Math.Max(adjustedStart.Y, _dragSelectionEnd.Y);

        // Calculate bounds X with scrollbar limit
        float minX = Math.Min(adjustedStart.X, _dragSelectionEnd.X);
        float maxX = Math.Max(adjustedStart.X, _dragSelectionEnd.X);

        // Limit maxX before scrollbar
        float scrollbarLimit = trackRect.X + trackWidth;
        maxX = Math.Min(maxX, scrollbarLimit);

        // For drawing, clamp only inside timeline area
        float drawMinX = Math.Max(trackRect.X, minX);
        float drawMaxX = Math.Min(scrollbarLimit, maxX);

        // If box selection is completely outside drawing area, still draw indicators
        bool selectionStartsBeforeView = minX < trackRect.X;
        bool selectionEndsAfterView = maxX > scrollbarLimit;

        // Draw only if something is visible
        if (drawMaxX > trackRect.X && drawMinX < scrollbarLimit)
        {
            // Ensure drawMinX and drawMaxX are in order
            drawMinX = Math.Max(trackRect.X, Math.Min(drawMinX, scrollbarLimit));
            drawMaxX = Math.Max(trackRect.X, Math.Min(drawMaxX, scrollbarLimit));

            if (drawMaxX > drawMinX)
            {
                var selectionMin = new Vector2(drawMinX, minY);
                var selectionMax = new Vector2(drawMaxX, maxY);

                // Calculate total perimeter of box selection
                float width = selectionMax.X - selectionMin.X;
                float height = selectionMax.Y - selectionMin.Y;
                float perimeter = 2 * (width + height);

                // Dash offset based on perimeter
                _dashOffset = perimeter * 0.05f;

                // Semi-transparent blue fill #98BBFF
                drawList.AddRectFilled(selectionMin, selectionMax, 0x22FFBB98);

                // Draw animated dashed border
                DrawAnimatedDashedBorder(drawList, selectionMin, selectionMax, _dashOffset);
            }
        }

        // VISUAL INDICATORS: Triangles indicating selection beyond viewport
        if (selectionStartsBeforeView)
        {
            float indicatorSize = 8.0f;
            float centerY = (minY + maxY) / 2.0f;
            var triangle1 = new Vector2(trackRect.X, centerY);
            var triangle2 = new Vector2(trackRect.X + indicatorSize, centerY - indicatorSize);
            var triangle3 = new Vector2(trackRect.X + indicatorSize, centerY + indicatorSize);
            drawList.AddTriangleFilled(triangle1, triangle2, triangle3, 0xFFFFBB98);
        }

        if (selectionEndsAfterView)
        {
            float indicatorSize = 8.0f;
            float centerY = (minY + maxY) / 2.0f;
            float rightEdge = scrollbarLimit;
            var triangle1 = new Vector2(rightEdge, centerY);
            var triangle2 = new Vector2(rightEdge - indicatorSize, centerY - indicatorSize);
            var triangle3 = new Vector2(rightEdge - indicatorSize, centerY + indicatorSize);
            drawList.AddTriangleFilled(triangle1, triangle2, triangle3, 0xFFFFBB98);
        }
    }

    private void DrawAnimatedDashedBorder(ImDrawListPtr drawList, Vector2 min, Vector2 max, float offset)
    {
        uint borderColor = 0xFFFFBB98;
        float dashLength = 5.0f;
        float gapLength = 5.0f;
        float thickness = 1.5f;

        // Top line
        DrawDashedLine(drawList,
            new Vector2(min.X, min.Y),
            new Vector2(max.X, min.Y),
            borderColor, thickness, dashLength, gapLength, offset);

        // Right line
        DrawDashedLine(drawList,
            new Vector2(max.X, min.Y),
            new Vector2(max.X, max.Y),
            borderColor, thickness, dashLength, gapLength, offset);

        // Bottom line
        DrawDashedLine(drawList,
            new Vector2(max.X, max.Y),
            new Vector2(min.X, max.Y),
            borderColor, thickness, dashLength, gapLength, offset);

        // Left line
        DrawDashedLine(drawList,
            new Vector2(min.X, max.Y),
            new Vector2(min.X, min.Y),
            borderColor, thickness, dashLength, gapLength, offset);
    }

    private static void DrawDashedLine(ImDrawListPtr drawList, Vector2 start, Vector2 end,
        uint color, float thickness, float dashLength, float gapLength, float offset)
    {
        float totalLength = Vector2.Distance(start, end);
        if (totalLength < 0.001f) return;

        Vector2 direction = (end - start) / totalLength;
        float patternLength = dashLength + gapLength;
        float currentPos = -offset % patternLength;

        while (currentPos < totalLength)
        {
            float dashStart = Math.Max(0, currentPos);
            float dashEnd = Math.Min(totalLength, currentPos + dashLength);

            if (dashEnd > dashStart)
            {
                Vector2 p1 = start + direction * dashStart;
                Vector2 p2 = start + direction * dashEnd;
                drawList.AddLine(p1, p2, color, thickness);
            }

            currentPos += patternLength;
        }
    }

    private void HandleScrubberRelease()
    {
        if (!_isPendingScrubberMove || ImGui.IsMouseDown(ImGuiMouseButton.Left)) return;
        if (!_isDragSelecting)
        {
            float relativeX = (_pendingScrubberPosition.X - _scrubberPosition.X) / _pendingScrubberTrackWidth;
            relativeX = Math.Clamp(relativeX, 0.0f, 1.0f);

            int visibleFrames = Math.Max(1, (int)(95 / TimelineZoom));
            float frameFloat = relativeX * visibleFrames;
            int targetFrame = TimelineStart + (int)Math.Round(frameFloat);

            CurrentFrame = Math.Clamp(targetFrame, 0, TotalFrames);
            CurrentFrameFloat = CurrentFrame;
            ApplyKeyframesToObjects();

            var io = ImGui.GetIO();
            if (!io.KeyCtrl)
            {
                SelectedKeyframes.Clear();
            }
        }
        _isPendingScrubberMove = false;
    }

    private (bool isOverHandle, bool isOverBody, RegionHandleType handleType) DrawRegionWithHandles(
    Vector2 itemRect, Vector2 itemSize, int visibleFrames)
    {
        var drawList = ImGui.GetWindowDrawList();
        var mousePos = ImGui.GetMousePos();

        // Ensure correct order
        int minFrame = Math.Min(_regionStartFrame, _regionEndFrame);
        int maxFrame = Math.Max(_regionStartFrame, _regionEndFrame);

        // Use TOTAL Scrubber width (including scrollbar area)
        float usableWidth = _scrubberWidth;
        float startX = itemRect.X + ((minFrame - TimelineStart) / (float)visibleFrames) * usableWidth;
        float endX = itemRect.X + ((maxFrame - TimelineStart) / (float)visibleFrames) * usableWidth;

        startX = Math.Clamp(startX, itemRect.X, itemRect.X + usableWidth);
        endX = Math.Clamp(endX, itemRect.X, itemRect.X + usableWidth);

        Vector2 regionMin = new Vector2(startX, itemRect.Y);
        Vector2 regionMax = new Vector2(endX, itemRect.Y + itemSize.Y);

        // Colors
        uint regionColor = _loopInRegion ? 0x4000FF00u : 0x40FFA500u;
        uint borderColor = _loopInRegion ? 0xFF00FF00u : 0xFFFFA500u;
        uint handleColor = 0xFFFFFFFFu;
        uint handleHoverColor = 0xFF00FFFFu;
        uint handleActiveColor = 0xFFFFFF00u;

        // Draw region body
        drawList.AddRectFilled(regionMin, regionMax, regionColor);
        drawList.AddRect(regionMin, regionMax, borderColor, 0.0f, ImDrawFlags.None, 2.0f);

        // Handle sizes (pins)
        float handleWidth = 10.0f;
        float handleHeight = itemSize.Y;

        // Left handle
        Vector2 leftHandleMin = new Vector2(startX - handleWidth / 2, itemRect.Y);
        Vector2 leftHandleMax = new Vector2(startX + handleWidth / 2, itemRect.Y + handleHeight);

        // Right handle
        Vector2 rightHandleMin = new Vector2(endX - handleWidth / 2, itemRect.Y);
        Vector2 rightHandleMax = new Vector2(endX + handleWidth / 2, itemRect.Y + handleHeight);

        // Check hover (only if not dragging)
        bool isOverLeftHandle = false;
        bool isOverRightHandle = false;
        bool isOverBody = false;

        if (!_isDraggingRegion && !_isResizingRegion)
        {
            isOverLeftHandle = mousePos.X >= leftHandleMin.X && mousePos.X <= leftHandleMax.X &&
                              mousePos.Y >= leftHandleMin.Y && mousePos.Y <= leftHandleMax.Y;

            isOverRightHandle = mousePos.X >= rightHandleMin.X && mousePos.X <= rightHandleMax.X &&
                               mousePos.Y >= rightHandleMin.Y && mousePos.Y <= rightHandleMax.Y;

            isOverBody = mousePos.X >= regionMin.X + handleWidth && mousePos.X <= regionMax.X - handleWidth &&
                        mousePos.Y >= regionMin.Y && mousePos.Y <= regionMax.Y;
        }

        // Handle colors based on state
        uint leftColor = handleColor;
        uint rightColor = handleColor;

        if (_isResizingRegion && _activeRegionHandle == RegionHandleType.Left)
            leftColor = handleActiveColor;
        else if (isOverLeftHandle)
            leftColor = handleHoverColor;

        if (_isResizingRegion && _activeRegionHandle == RegionHandleType.Right)
            rightColor = handleActiveColor;
        else if (isOverRightHandle)
            rightColor = handleHoverColor;

        // Draw handles (pins)
        // Left handle
        drawList.AddRectFilled(leftHandleMin, leftHandleMax, leftColor);
        drawList.AddRect(leftHandleMin, leftHandleMax, 0xFF000000u, 0.0f, ImDrawFlags.None, 1.0f);
        drawList.AddLine(
            new Vector2(startX, leftHandleMin.Y + 2),
            new Vector2(startX, leftHandleMax.Y - 2),
            0xFF000000u, 2.0f);

        // Right handle
        drawList.AddRectFilled(rightHandleMin, rightHandleMax, rightColor);
        drawList.AddRect(rightHandleMin, rightHandleMax, 0xFF000000u, 0.0f, ImDrawFlags.None, 1.0f);
        drawList.AddLine(
            new Vector2(endX, rightHandleMin.Y + 2),
            new Vector2(endX, rightHandleMax.Y - 2),
            0xFF000000u, 2.0f);

        // Labels
        string startLabel = minFrame.ToString();
        string endLabel = maxFrame.ToString();

        var startTextSize = ImGui.CalcTextSize(startLabel);
        var endTextSize = ImGui.CalcTextSize(endLabel);

        drawList.AddText(new Vector2(startX - startTextSize.X / 2, itemRect.Y - 20), 0xFFFFFFFFu, startLabel);
        drawList.AddText(new Vector2(endX - endTextSize.X / 2, itemRect.Y - 20), 0xFFFFFFFFu, endLabel);

        if (_loopInRegion)
        {
            string loopLabel = "LOOP";
            var loopTextSize = ImGui.CalcTextSize(loopLabel);
            float centerX = (startX + endX) / 2 - loopTextSize.X / 2;
            drawList.AddText(new Vector2(centerX, itemRect.Y - 35), 0xFF00FF00u, loopLabel);
        }

        // Appropriate cursor
        if (isOverLeftHandle || isOverRightHandle || (_isResizingRegion && _activeRegionHandle != RegionHandleType.Body))
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEW);
        }
        else if (isOverBody || (_isDraggingRegion && _activeRegionHandle == RegionHandleType.Body))
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        RegionHandleType handleType = RegionHandleType.None;
        if (isOverLeftHandle) handleType = RegionHandleType.Left;
        else if (isOverRightHandle) handleType = RegionHandleType.Right;
        else if (isOverBody) handleType = RegionHandleType.Body;

        return (isOverLeftHandle || isOverRightHandle, isOverBody, handleType);
    }

    private void HandleRegionInteraction(Vector2 itemRect, Vector2 itemSize, int visibleFrames,
        bool isOverHandle, bool isOverBody, RegionHandleType handleType, bool isScrubberHovered)
    {
        var mousePos = ImGui.GetMousePos();

        // Start drag/resize (only if over Scrubber)
        if (isScrubberHovered && (isOverHandle || isOverBody) && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            switch (handleType)
            {
                case RegionHandleType.Body:
                    _isDraggingRegion = true;
                    _isResizingRegion = false;
                    break;
                case RegionHandleType.Left:
                case RegionHandleType.Right:
                    _isResizingRegion = true;
                    _isDraggingRegion = false;
                    break;
                case RegionHandleType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(handleType), handleType, null);
            }

            _activeRegionHandle = handleType;
            _regionDragStartMouse = mousePos;
            _dragStartTimelineStart = TimelineStart;

            // Store current frame positions at drag start
            _regionDragStartFrameLeft = Math.Min(_regionStartFrame, _regionEndFrame);
            _regionDragStartFrameRight = Math.Max(_regionStartFrame, _regionEndFrame);
        }

        // Process drag/resize (works anywhere after start)
        if ((_isDraggingRegion || _isResizingRegion) && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            // Calculate delta considering accumulated scroll
            int scrollDelta = TimelineStart - _dragStartTimelineStart;

            // CRITICAL: Use _scrubberWidth instead of itemSize.X to ensure alignment
            float pixelsPerFrame = _scrubberWidth / (float)Math.Max(1, visibleFrames);

            float deltaX = mousePos.X - _regionDragStartMouse.X;
            float adjustedDeltaX = deltaX + (scrollDelta * pixelsPerFrame);
            int deltaFrames = (int)Math.Round(adjustedDeltaX / pixelsPerFrame);

            if (_isDraggingRegion && _activeRegionHandle == RegionHandleType.Body)
            {
                // Move entire region
                int regionLength = _regionDragStartFrameRight - _regionDragStartFrameLeft;
                int newLeft = _regionDragStartFrameLeft + deltaFrames;

                // Limit movement
                newLeft = Math.Clamp(newLeft, 0, TotalFrames - regionLength);

                _regionStartFrame = newLeft;
                _regionEndFrame = newLeft + regionLength;
            }
            else if (_isResizingRegion)
            {
                switch (_activeRegionHandle)
                {
                    // Resize region
                    case RegionHandleType.Left:
                    {
                        int newLeft = _regionDragStartFrameLeft + deltaFrames;
                        newLeft = Math.Clamp(newLeft, 0, _regionDragStartFrameRight - 1);

                        _regionStartFrame = newLeft;
                        _regionEndFrame = _regionDragStartFrameRight;
                        break;
                    }
                    case RegionHandleType.Right:
                    {
                        int newRight = _regionDragStartFrameRight + deltaFrames;
                        newRight = Math.Clamp(newRight, _regionDragStartFrameLeft + 1, TotalFrames);

                        _regionStartFrame = _regionDragStartFrameLeft;
                        _regionEndFrame = newRight;
                        break;
                    }
                    case RegionHandleType.None:
                    case RegionHandleType.Body:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        // End drag/resize
        if (ImGui.IsMouseDown(ImGuiMouseButton.Left)) return;
        _isDraggingRegion = false;
        _isResizingRegion = false;
        _activeRegionHandle = RegionHandleType.None;
    }

    private void HandleSpacebarPlayback()
    {
        bool spaceIsPressed = Input.IsKeyPressed(Key.Space);

        if (spaceIsPressed && !_spaceWasPressed)
        {
            if (!IsPlaying)
            {
                _playStartFrame = CurrentFrame;
                _playStartFrameFloat = CurrentFrameFloat;
                IsPlaying = true;
            }
            else
            {
                IsPlaying = false;
            }
        }

        _spaceWasPressed = spaceIsPressed;
    }

    private void HandlePlayback(float delta)
    {
        if (IsPlaying)
        {
            var instance = Main.GetInstance();
            float frameRate = instance?.UI?.PropertiesPanel?.Project?.FrameRate ?? 30f;
            CurrentFrameFloat += frameRate * delta;

            // Region loop logic
            if (_loopInRegion && _regionActive)
            {
                if (CurrentFrameFloat > _regionEndFrame || CurrentFrameFloat < _regionStartFrame)
                {
                    CurrentFrameFloat = _regionStartFrame;
                }
            }
            else if (CurrentFrameFloat > TotalFrames)
            {
                CurrentFrameFloat = 0.0f;
            }

            CurrentFrame = (int)CurrentFrameFloat;

            // FIX: Always allow auto-scroll during playback
            UpdateAutoScroll();

            ApplyKeyframesToObjects();
        }
        else if (IsScrubbing || IsDraggingKeyframe)
        {
            if (IsDraggingKeyframe && _dragAnchorKeyframe != null)
            {
                if (_previewFrames.TryGetValue(_dragAnchorKeyframe, out int anchorPreviewFrame))
                {
                    CurrentFrame = anchorPreviewFrame;
                    CurrentFrameFloat = CurrentFrame;

                    if (!_isManuallyScrolling)
                    {
                        UpdateAutoScrollWhileDragging();
                    }
                }
            }

            ApplyKeyframesToObjects();
        }

        if (_timelineHasFocus && !IsPlaying)
        {
            HandleArrowKeyNavigation(delta);
        }
        else
        {
            _arrowKeyHoldTime = 0.0f;
            _arrowKeyRepeatTimer = 0.0f;
        }
    }

    private void HandleKeyboardShortcuts()
    {
        if (Input.IsKeyPressed(Key.Delete) && IsHovered && HasSelectedKeyframe())
        {
            DeleteSelectedKeyframes();
        }

        if (Input.IsKeyPressed(Key.C) && Input.IsKeyPressed(Key.Ctrl) && _timelineHasFocus && HasSelectedKeyframe() && !IsDraggingKeyframe)
        {
            CopySelectedKeyframes();
        }

        if (Input.IsKeyPressed(Key.X) && Input.IsKeyPressed(Key.Ctrl) && _timelineHasFocus && HasSelectedKeyframe() && !IsDraggingKeyframe)
        {
            CutSelectedKeyframes();
        }

        if (Input.IsKeyPressed(Key.V) && Input.IsKeyPressed(Key.Ctrl) && _timelineHasFocus && _copiedKeyframes.Count > 0 && !IsDraggingKeyframe)
        {
            PasteKeyframes(CurrentFrame);
        }

        if (Input.IsKeyPressed(Key.I) && _timelineHasFocus && CurrentObject != null && !IsDraggingKeyframe)
        {
            CreateKeyframesAtCurrentFrame();
        }
    }

    public void ApplyKeyframesToObjects()
    {
        var instance = Main.GetInstance();
        if (instance?.UI?.SceneTreePanel?.SceneObjects == null) return;

        foreach (var obj in instance.UI.SceneTreePanel.SceneObjects.Values)
        {
            if (obj.PosXKeyframes.Count > 0 || obj.PosYKeyframes.Count > 0 || obj.PosZKeyframes.Count > 0)
            {
                obj.TargetPosition = new Vector3(
                    EvaluateKeyframesWithDefault(obj.PosXKeyframes, CurrentFrame, obj.TargetPosition.X),
                    EvaluateKeyframesWithDefault(obj.PosYKeyframes, CurrentFrame, obj.TargetPosition.Y),
                    EvaluateKeyframesWithDefault(obj.PosZKeyframes, CurrentFrame, obj.TargetPosition.Z)
                );
            }

            if (obj.RotXKeyframes.Count > 0 || obj.RotYKeyframes.Count > 0 || obj.RotZKeyframes.Count > 0)
            {
                obj.RotationDegrees = new Vector3(
                    EvaluateKeyframesWithDefault(obj.RotXKeyframes, CurrentFrame, obj.RotationDegrees.X),
                    EvaluateKeyframesWithDefault(obj.RotYKeyframes, CurrentFrame, obj.RotationDegrees.Y),
                    EvaluateKeyframesWithDefault(obj.RotZKeyframes, CurrentFrame, obj.RotationDegrees.Z)
                );
            }

            if (obj.ScaleXKeyframes.Count > 0 || obj.ScaleYKeyframes.Count > 0 || obj.ScaleZKeyframes.Count > 0)
            {
                obj.Scale = new Vector3(
                    EvaluateKeyframesWithDefault(obj.ScaleXKeyframes, CurrentFrame, obj.Scale.X),
                    EvaluateKeyframesWithDefault(obj.ScaleYKeyframes, CurrentFrame, obj.Scale.Y),
                    EvaluateKeyframesWithDefault(obj.ScaleZKeyframes, CurrentFrame, obj.Scale.Z)
                );
            }

            if (obj.AlphaKeyframes.Count > 0)
                obj.Alpha = EvaluateKeyframesWithDefault(obj.AlphaKeyframes, CurrentFrame, obj.Alpha);
        }
    }

    public float EvaluateKeyframesWithDefault(Dictionary<int, Keyframe> keyframes, float frame, float defaultValue)
    {
        if (keyframes.Count == 0)
            return defaultValue;

        int frameInt = (int)Math.Round(frame);

        if (keyframes.TryGetValue(frameInt, out Keyframe? exactKeyframe))
            return exactKeyframe.Value;

        string cacheKey = $"{keyframes.Count}_{string.Join(",", keyframes.Keys.OrderBy(x => x))}";
        if (!_sortedKeyframeCache.TryGetValue(cacheKey, out var sortedFrames))
        {
            sortedFrames = keyframes.Keys.OrderBy(k => k).ToList();
            _sortedKeyframeCache[cacheKey] = sortedFrames;
        }

        if (sortedFrames.Count == 0)
            return defaultValue;

        if (frame < sortedFrames[0])
            return keyframes[sortedFrames[0]].Value;

        if (frame > sortedFrames[^1])
            return keyframes[sortedFrames[^1]].Value;

        int left = 0, right = sortedFrames.Count - 1;
        while (left < right - 1)
        {
            int mid = (left + right) / 2;
            if (sortedFrames[mid] == frameInt)
                return keyframes[sortedFrames[mid]].Value;
            if (sortedFrames[mid] < frame)
                left = mid;
            else
                right = mid;
        }

        int leftFrame = sortedFrames[left];
        int rightFrame = sortedFrames[right];

        if (rightFrame == leftFrame)
            return keyframes[leftFrame].Value;

        Keyframe leftKeyframe = keyframes[leftFrame];
        Keyframe rightKeyframe = keyframes[rightFrame];
        float t = (frame - leftFrame) / (rightFrame - leftFrame);
        
        // Apply easing from the left keyframe
        float easedT = EasingFunctions.ApplyEasing(t, leftKeyframe.EasingMode);
        
        return leftKeyframe.Value + (rightKeyframe.Value - leftKeyframe.Value) * easedT;
    }

    public float EvaluateKeyframesWithDefault(Dictionary<int, Keyframe> keyframes, int frame, float defaultValue)
    {
        return EvaluateKeyframesWithDefault(keyframes, (float)frame, defaultValue);
    }

    private void HandleArrowKeyNavigation(float delta)
    {
        bool rightPressed = Input.IsKeyPressed(Key.Right);
        bool leftPressed = Input.IsKeyPressed(Key.Left);
        bool shiftPressed = Input.IsKeyPressed(Key.Shift);

        if (!rightPressed && !leftPressed)
        {
            _arrowKeyHoldTime = 0.0f;
            _arrowKeyRepeatTimer = 0.0f;
            return;
        }

        if (shiftPressed)
        {
            if (_arrowKeyHoldTime != 0.0f) return;
            if (rightPressed)
            {
                MoveToFrame(CurrentFrame + 1);
            }
            else if (leftPressed)
            {
                MoveToFrame(CurrentFrame - 1);
            }

            _arrowKeyHoldTime = 0.01f;
            return;
        }

        _arrowKeyHoldTime += delta;

        if (_arrowKeyHoldTime < ARROW_KEY_INITIAL_DELAY)
        {
            if (!(_arrowKeyHoldTime - delta <= 0)) return;
            if (rightPressed)
            {
                MoveToFrame(CurrentFrame + 1);
            }
            else if (leftPressed)
            {
                MoveToFrame(CurrentFrame - 1);
            }
            return;
        }

        _arrowKeyRepeatTimer += delta;

        if (!(_arrowKeyRepeatTimer >= ARROW_KEY_REPEAT_RATE)) return;
        _arrowKeyRepeatTimer = 0.0f;

        if (rightPressed)
        {
            MoveToFrame(CurrentFrame + 1);
        }
        else if (leftPressed)
        {
            MoveToFrame(CurrentFrame - 1);
        }
    }

    private void MoveToFrame(int targetFrame)
    {
        CurrentFrame = Math.Clamp(targetFrame, 0, TotalFrames);
        CurrentFrameFloat = CurrentFrame;
        ApplyKeyframesToObjects();

        if (!IsPlaying) return;
        int visibleFrames = Math.Max(1, (int)(95 / TimelineZoom));

        if (CurrentFrame > TimelineStart + visibleFrames - 10)
        {
            TimelineStart = Math.Min(CurrentFrame - visibleFrames + 10, TotalFrames - visibleFrames);
            TimelineStart = Math.Max(0, TimelineStart);
        }
        else if (CurrentFrame < TimelineStart + 10)
        {
            TimelineStart = Math.Max(CurrentFrame - 10, 0);
        }
    }

    private void UpdateAutoScroll()
    {
        int visibleFrames = Math.Max(1, (int)(95 / TimelineZoom));
        int viewportEnd = TimelineStart + visibleFrames;

        if (CurrentFrame >= viewportEnd)
        {
            int pagesToJump = Math.Max(1, (CurrentFrame - TimelineStart) / visibleFrames);
            TimelineStart += pagesToJump * visibleFrames;

            int maxStart = Math.Max(0, TotalFrames - visibleFrames);
            TimelineStart = Math.Min(TimelineStart, maxStart);
        }
        else if (CurrentFrame < TimelineStart)
        {
            TimelineStart = Math.Max(0, CurrentFrame - visibleFrames + 10);
        }

        // If looping in region, ensure region is visible
        if (!_loopInRegion || !_regionActive) return;
        if (CurrentFrame >= TimelineStart && CurrentFrame <= TimelineStart + visibleFrames) return;
        // Center region in viewport
        int regionCenter = (_regionStartFrame + _regionEndFrame) / 2;
        TimelineStart = Math.Max(0, Math.Min(TotalFrames - visibleFrames, regionCenter - visibleFrames / 2));
    }

    private void HandleRegionDragAutoScroll(Vector2 trackRect, Vector2 trackSize, int visibleFrames)
    {
        if ((!_isDraggingRegion && !_isResizingRegion) || !ImGui.IsMouseDown(ImGuiMouseButton.Left))
            return;

        var mousePos = ImGui.GetMousePos();
        float leftEdge = trackRect.X;
        float rightEdge = trackRect.X + trackSize.X;
        float mouseX = mousePos.X;
        int maxStart = Math.Max(0, TotalFrames - visibleFrames);

        float deltaPixelsRight = Math.Max(0f, mouseX - rightEdge);
        float deltaPixelsLeft = Math.Max(0f, leftEdge - mouseX);

        bool isMouseBeyondRight = deltaPixelsRight > 0;
        bool isMouseBeyondLeft = deltaPixelsLeft > 0;

        // Base speed proportional to zoom
        float baseScrollSpeed = Math.Max(1.0f, visibleFrames / 100.0f);

        if (isMouseBeyondRight)
        {
            float intensity = Math.Clamp(deltaPixelsRight / DRAG_SCROLL_MAX_DISTANCE, 0.0f, 1.0f);
            float speed = (DRAG_SCROLL_MIN_SPEED + (DRAG_SCROLL_MAX_SPEED - DRAG_SCROLL_MIN_SPEED) * intensity) * baseScrollSpeed;
            int frames = (int)Math.Ceiling(speed);
            TimelineStart = Math.Min(maxStart, TimelineStart + frames);

            _isManuallyScrolling = true;
            _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
        }
        else if (isMouseBeyondLeft)
        {
            float intensity = Math.Clamp(deltaPixelsLeft / DRAG_SCROLL_MAX_DISTANCE, 0.0f, 1.0f);
            float speed = (DRAG_SCROLL_MIN_SPEED + (DRAG_SCROLL_MAX_SPEED - DRAG_SCROLL_MIN_SPEED) * intensity) * baseScrollSpeed;
            int frames = (int)Math.Ceiling(speed);
            TimelineStart = Math.Max(0, TimelineStart - frames);

            _isManuallyScrolling = true;
            _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
        }
        else
        {
            // Nudge at edges
            if (mouseX > rightEdge - SCROLL_MARGIN)
            {
                float t = (mouseX - (rightEdge - SCROLL_MARGIN)) / SCROLL_MARGIN;
                float nudgeSpeed = t * t * 0.5f * baseScrollSpeed;

                if (!(nudgeSpeed >= 0.3f)) return;
                int frames = Math.Max(1, (int)Math.Ceiling(nudgeSpeed));
                TimelineStart = Math.Min(maxStart, TimelineStart + frames);
                _isManuallyScrolling = true;
                _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
            }
            else if (mouseX < leftEdge + SCROLL_MARGIN)
            {
                float t = ((leftEdge + SCROLL_MARGIN) - mouseX) / SCROLL_MARGIN;
                float nudgeSpeed = t * t * 0.5f * baseScrollSpeed;

                if (!(nudgeSpeed >= 0.3f)) return;
                int frames = Math.Max(1, (int)Math.Ceiling(nudgeSpeed));
                TimelineStart = Math.Max(0, TimelineStart - frames);
                _isManuallyScrolling = true;
                _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
            }
        }
    }

    private void UpdateAutoScrollWhileDragging()
    {
        int visibleFrames = Math.Max(1, (int)(95 / TimelineZoom));
        float markerRelativePosition = (CurrentFrame - TimelineStart) / (float)visibleFrames;

        float safeZoneStart = 0.05f;
        float safeZoneEnd = 0.95f;

        if (markerRelativePosition > safeZoneEnd)
        {
            float overshoot = markerRelativePosition - safeZoneEnd;

            // Speed proportional to zoom - smaller zoom, more frames advance
            float baseSpeed = visibleFrames * 0.02f;
            int framesToScroll = (int)Math.Ceiling(overshoot * baseSpeed * 2.0f);
            framesToScroll = Math.Max(1, framesToScroll);

            int maxStart = Math.Max(0, TotalFrames - visibleFrames);
            int newStart = Math.Min(maxStart, TimelineStart + framesToScroll);

            if (newStart != TimelineStart)
            {
                TimelineStart = newStart;
            }
        }
        else if (markerRelativePosition < safeZoneStart)
        {
            float overshoot = safeZoneStart - markerRelativePosition;

            float baseSpeed = visibleFrames * 0.02f;
            int framesToScroll = (int)Math.Ceiling(overshoot * baseSpeed * 2.0f);
            framesToScroll = Math.Max(1, framesToScroll);

            int newStart = Math.Max(0, TimelineStart - framesToScroll);

            if (newStart != TimelineStart)
            {
                TimelineStart = newStart;
            }
        }
    }

    public void Render(Vector2I size)
    {
        if (ImGui.Begin("Timeline", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            IsHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows);

            // CRITICAL: Move cursor (4 arrows) ONLY when dragging keyframes
            if (IsDraggingKeyframe)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeAll);
            }
            else if (IsHovered)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            UpdateTimelineFocus();
            HandleZoom();

            int visibleFrames = Math.Max(1, (int)(95 / TimelineZoom));
            int timelineEnd = Math.Min(TimelineStart + visibleFrames, TotalFrames);

            RenderControls(visibleFrames, timelineEnd);
            RenderContent(visibleFrames);
        }
        else
        {
            IsHovered = false;
        }

        ImGui.End();
    }

    private void UpdateTimelineFocus()
    {
        if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows) && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            var mousePos = ImGui.GetMousePos();
            var windowPos = ImGui.GetWindowPos();
            float labelWidth = 150.0f;

            if (mousePos.X > windowPos.X + labelWidth)
            {
                _timelineHasFocus = true;
            }
        }
        else if (!ImGui.IsWindowHovered(ImGuiHoveredFlags.RootWindow) && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            _timelineHasFocus = false;
            _selectedTrackProperty = null;
        }
    }

    private void HandleZoom()
    {
        var io = ImGui.GetIO();

        float minZoom = 95.0f / 5000.0f;
        float maxZoom = 95.0f / 40.0f;

        if ((ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows | ImGuiHoveredFlags.RootWindow) || IsHovered) && io.KeyCtrl && io.MouseWheel != 0)
        {
            var mousePos = ImGui.GetMousePos();
            var windowPos = ImGui.GetWindowPos();
            float labelWidth = 150.0f;

            float mouseXInTimeline = mousePos.X - (windowPos.X + labelWidth);
            float timelineWidth = ImGui.GetContentRegionAvail().X - labelWidth;

            float focusRelativePosition = 0.5f;
            int frameAtFocus;

            if (IsPlaying)
            {
                int visibleFramesBefore = Math.Max(1, (int)(95 / TimelineZoom));
                frameAtFocus = CurrentFrame;

                focusRelativePosition = (float)(CurrentFrame - TimelineStart) / visibleFramesBefore;
                focusRelativePosition = Math.Clamp(focusRelativePosition, 0.0f, 1.0f);
            }
            else
            {
                if (timelineWidth > 0)
                {
                    focusRelativePosition = Math.Clamp(mouseXInTimeline / timelineWidth, 0.0f, 1.0f);
                }

                int visibleFramesBefore = Math.Max(1, (int)(95 / TimelineZoom));
                frameAtFocus = TimelineStart + (int)(focusRelativePosition * visibleFramesBefore);
                frameAtFocus = Math.Clamp(frameAtFocus, 0, TotalFrames);
            }

            float zoomFactor = io.MouseWheel > 0 ? 1.6f : 0.625f;
            TimelineZoom *= zoomFactor;
            TimelineZoom = Math.Max(minZoom, Math.Min(maxZoom, TimelineZoom));

            int visibleFramesAfter = Math.Max(1, (int)(95 / TimelineZoom));

            int newStart = frameAtFocus - (int)(focusRelativePosition * visibleFramesAfter);
            newStart = Math.Max(0, Math.Min(TotalFrames - visibleFramesAfter, newStart));

            TimelineStart = newStart;

            if (!IsPlaying)
            {
                _isManuallyScrolling = true;
                _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
            }

            io.MouseWheel = 0;
        }

        if (_timelineHasFocus && io.KeyCtrl)
        {
            bool zoomIn = Input.IsKeyPressed(Key.Equal) || Input.IsKeyPressed(Key.KpAdd);
            bool zoomOut = Input.IsKeyPressed(Key.Minus) || Input.IsKeyPressed(Key.KpSubtract);

            if (!zoomIn && !zoomOut) return;
            int visibleFramesBefore = Math.Max(1, (int)(95 / TimelineZoom));

            float focusRelativePos;
            int frameAtFocus;

            if (IsPlaying)
            {
                frameAtFocus = CurrentFrame;
                focusRelativePos = (float)(CurrentFrame - TimelineStart) / visibleFramesBefore;
                focusRelativePos = Math.Clamp(focusRelativePos, 0.0f, 1.0f);
            }
            else
            {
                frameAtFocus = CurrentFrame;
                focusRelativePos = (float)(CurrentFrame - TimelineStart) / visibleFramesBefore;
                focusRelativePos = Math.Clamp(focusRelativePos, 0.0f, 1.0f);
            }

            float zoomFactor = zoomIn ? 1.2f : 0.833f;
            TimelineZoom *= zoomFactor;
            TimelineZoom = Math.Max(minZoom, Math.Min(maxZoom, TimelineZoom));

            int visibleFramesAfter = Math.Max(1, (int)(95 / TimelineZoom));

            int newStart = frameAtFocus - (int)(focusRelativePos * visibleFramesAfter);
            newStart = Math.Max(0, Math.Min(TotalFrames - visibleFramesAfter, newStart));

            TimelineStart = newStart;

            if (IsPlaying) return;
            _isManuallyScrolling = true;
            _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
        }
    }

    private void RenderControls(int visibleFrames, int timelineEnd)
    {
        ImGui.Text($"Visible: {TimelineStart} - {timelineEnd} ({visibleFrames} frames)");
        ImGui.SameLine();

        ImGui.TextDisabled($"[Steps: {_cachedMinorStep}/{_cachedMediumStep}/{_cachedMajorStep}]");
        ImGui.SameLine();

        if (ImGui.Button("Fit All"))
        {
            TimelineZoom = 95.0f / TotalFrames;
            TimelineStart = 0;
        }

        ImGui.SameLine();

        float minZoom = 95.0f / 5000.0f;
        float maxZoom = 95.0f / 40.0f;

        if (ImGui.Button("Max Zoom (40f)"))
        {
            TimelineZoom = maxZoom;
            int tempVisibleFrames = Math.Max(1, (int)(95 / TimelineZoom));
            if (TimelineStart + tempVisibleFrames > TotalFrames)
                TimelineStart = Math.Max(0, TotalFrames - tempVisibleFrames);
        }

        ImGui.SameLine();
        if (ImGui.Button("Mid Zoom"))
        {
            TimelineZoom = 95.0f / 500.0f;
            int tempVisibleFrames = Math.Max(1, (int)(95 / TimelineZoom));
            if (TimelineStart + tempVisibleFrames > TotalFrames)
                TimelineStart = Math.Max(0, TotalFrames - tempVisibleFrames);
        }

        ImGui.SameLine();
        if (ImGui.Button("Min Zoom (5000f)"))
        {
            TimelineZoom = minZoom;
            TimelineStart = 0;
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(120);
        float zoom = TimelineZoom;
        if (ImGui.SliderFloat("Zoom", ref zoom, minZoom, maxZoom, $"{zoom:F3}x"))
        {
            TimelineZoom = Math.Max(minZoom, Math.Min(maxZoom, zoom));
            visibleFrames = Math.Max(1, (int)(95 / TimelineZoom));
            if (TimelineStart + visibleFrames > TotalFrames)
                TimelineStart = Math.Max(0, TotalFrames - visibleFrames);
        }

        if (ImGui.IsItemHovered())
        {
            int currentVisibleFrames = Math.Max(1, (int)(95 / TimelineZoom));
            ImGui.SetTooltip($"Showing {currentVisibleFrames} frames\nCtrl+Scroll: Zoom at cursor\nCtrl+ / Ctrl-: Zoom at current frame\nKey I: Create keyframe at current frame");
        }

        ImGui.SameLine();
        ImGui.Spacing();
        ImGui.SameLine();

        // Region loop button
        if (_regionActive)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, _loopInRegion ? new Vector4(0.0f, 0.5f, 0.0f, 1.0f) : new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            if (ImGui.Button(_loopInRegion ? "Loop ON" : "Loop OFF"))
            {
                _loopInRegion = !_loopInRegion;
            }
            ImGui.PopStyleColor();
            ImGui.SameLine();

            // Clear region button
            if (ImGui.Button("Clear Region"))
            {
                ClearActiveRegion();
            }
            ImGui.SameLine();
        }

        var instance = Main.GetInstance();
        if (instance?.Icons != null)
        {
            if (ImGuiGD.ImageButton("##reset", instance.Icons.GetValueOrDefault("reset"), new Vector2(16, 16)))
            {
                IsPlaying = false;
                CurrentFrame = 0;
                CurrentFrameFloat = 0;
                ApplyKeyframesToObjects();
            }

            ImGui.SameLine();
            var iconTexture = IsPlaying ? instance.Icons.GetValueOrDefault("pause") : instance.Icons.GetValueOrDefault("play");
            if (ImGuiGD.ImageButton("playPause", iconTexture, new Vector2(16, 16)))
            {
                if (!IsPlaying)
                {
                    _playStartFrame = CurrentFrame;
                    _playStartFrameFloat = CurrentFrameFloat;
                    IsPlaying = true;
                }
                else
                {
                    IsPlaying = false;
                }
            }

            ImGui.SameLine();
            if (ImGuiGD.ImageButton("stopButton", instance.Icons.GetValueOrDefault("stop"), new Vector2(16, 16)))
            {
                IsPlaying = false;
                CurrentFrame = _playStartFrame;
                CurrentFrameFloat = _playStartFrameFloat;
                ApplyKeyframesToObjects();
            }
        }

        // Visual indicator when editing selected keyframes
        if (_isEditingSelectedKeyframes)
        {
            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();

            // Animated text with colored background
            float time = (float)Engine.GetProcessFrames() * 0.05f;
            float pulse = (MathF.Sin(time) + 1.0f) * 0.5f; // 0.0 to 1.0

            var bgColor = new Vector4(
                0.2f + (pulse * 0.3f),
                0.6f + (pulse * 0.2f),
                0.2f + (pulse * 0.3f),
                0.9f
            );

            ImGui.PushStyleColor(ImGuiCol.Button, bgColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, bgColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, bgColor);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));

            ImGui.Button($"✎ Editing {SelectedKeyframes.Count} Keyframe{(SelectedKeyframes.Count > 1 ? "s" : "")}");

            ImGui.PopStyleColor(4);

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(
                    "Relative Edit Mode:\n" +
                    "Transformations will be added to selected keyframes\n" +
                    "Click elsewhere to deselect keyframes"
                );
            }
        }

        ImGui.Spacing();
    }

    private void RenderContent(int visibleFrames)
    {
        var availableWidth = ImGui.GetContentRegionAvail().X;
        var labelWidth = 150.0f;
        // FIX: Use total available width (vertical scrollbar is part of internal BeginChild)
        var trackWidth = Math.Max(0, availableWidth - labelWidth);

        // Store starting position for box selection
        var contentStartPos = ImGui.GetCursorScreenPos();

        // Calculate available height
        var windowSize = ImGui.GetWindowSize();
        float scrollbarHeight = ImGui.GetStyle().ScrollbarSize;
        float markerBarHeight = _markers.Count > 0 ? 26.0f : 0.0f;
        float scrubberHeight = 18.0f + ImGui.GetStyle().ItemSpacing.Y; // Scrubber + spacing

        float controlsHeight = ImGui.GetCursorPosY(); // height of controls at top
        float availableHeight = windowSize.Y - scrollbarHeight - markerBarHeight - controlsHeight - scrubberHeight;

        // ===== 1. RENDER Scrubber (FIXED, NO SCROLL) =====
        ImGui.BeginGroup();

        // Empty space on left side (aligned with labels)
        ImGui.BeginChild("##ScrubberLeftSpace", new Vector2(labelWidth, scrubberHeight), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar);
        ImGui.EndChild();

        ImGui.SameLine(0, 0);

        // Scrubber/Scrubber on right side - NOW WITH CORRECT width
        ImGui.BeginChild("##ScrubberArea", new Vector2(trackWidth, scrubberHeight), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar);
        RenderFrameScrubber(visibleFrames);
        ImGui.EndChild();

        ImGui.EndGroup();

        // ===== 2. RENDER TRACKS WITH SCROLL =====
        ImGui.BeginGroup();

        // Track labels (left) - WITH SCROLL
        RenderTrackNamesBoxScrollable(labelWidth, availableHeight);

        ImGui.SameLine(0, 0);

        // Keyframe tracks (right) - WITH SCROLL
        RenderTimelineTracksBoxScrollable(trackWidth, visibleFrames, availableHeight);

        ImGui.EndGroup();

        // ===== 3. PLAYHEAD (FIXED) =====
        DrawPlayheadComplete(visibleFrames);

        // ===== 4. BOX SELECTION =====
        HandleBoxSelectionInContentArea();

        if (_isDragSelecting)
        {
            HandleBoxSelectionAutoScroll(new Vector2(_scrubberPosition.X, _scrubberPosition.Y),
                                         new Vector2(_scrubberWidth, 18), visibleFrames);
        }

        // ===== 5. CONTEXT MENU =====
        HandleTimelineAreaContextMenu();

        if (ImGui.BeginPopup("KeyframeContextMenu"))
        {
            RenderContextMenuItems();
            ImGui.EndPopup();
        }

        // ===== 6. MARKERS FOOTER (FIXED) =====
        if (_markers.Count > 0)
        {
            RenderMarkersFooterBar(visibleFrames);
        }

        if (_editingMarker != null)
        {
            RenderMarkerEditPopup(_editingMarker);
        }

        // ===== 7. HORIZONTAL SCROLLBAR (FIXED) =====
        RenderTimelineHorizontalScrollbar(visibleFrames);
    }

    private void RenderMarkersFooterBar(int visibleFrames)
    {
        var drawList = ImGui.GetWindowDrawList();
        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();

        // Marker bar height
        float markerBarHeight = 26.0f;

        // Position: just above horizontal scrollbar
        float scrollbarHeight = ImGui.GetStyle().ScrollbarSize;
        float markerBarY = windowPos.Y + windowSize.Y - scrollbarHeight - markerBarHeight;

        // The bar starts exactly where Scrubber starts (_scrubberPosition.X) and goes to end
        var markerBarMin = new Vector2(_scrubberPosition.X, markerBarY);
        var markerBarMax = new Vector2(_scrubberPosition.X + _scrubberWidth, markerBarY + markerBarHeight);

        // Opaque SOLID BLACK background (NO OUTLINE)
        drawList.AddRectFilled(markerBarMin, markerBarMax, 0xFF000000u);

        // Draw the markers in the footer bar
        DrawMarkersInFooterBar(visibleFrames, markerBarMin, markerBarMax);
    }

    // NOTE: This is never used - Forest
    private void RenderMarkersArea(float trackWidth, int visibleFrames)
    {
        // Store current cursor position (right after Scrubber)
        var markerAreaStartY = ImGui.GetCursorScreenPos().Y;

        // Marker area height
        float markerAreaHeight = 24.0f;

        // Draw marker area background
        var drawList = ImGui.GetWindowDrawList();
        var markerAreaMin = new Vector2(_scrubberPosition.X, _scrubberPosition.Y + 18);
        var markerAreaMax = new Vector2(_scrubberPosition.X + _scrubberWidth, markerAreaMin.Y + markerAreaHeight);

        // Dark background for marker area
        drawList.AddRectFilled(markerAreaMin, markerAreaMax, 0xFF1A1A26);

        // Top and bottom border
        drawList.AddLine(
            new Vector2(markerAreaMin.X, markerAreaMin.Y),
            new Vector2(markerAreaMax.X, markerAreaMin.Y),
            0xFF3A3C3E, 1.0f);

        drawList.AddLine(
            new Vector2(markerAreaMin.X, markerAreaMax.Y),
            new Vector2(markerAreaMax.X, markerAreaMax.Y),
            0xFF3A3C3E, 1.0f);

        // Draw the markers in the new area
        DrawMarkersInDedicatedArea(visibleFrames, markerAreaMin, markerAreaMax);

        var windowPos = ImGui.GetWindowPos();
        ImGui.SetCursorScreenPos(new Vector2(windowPos.X, markerAreaMax.Y));
        ImGui.Dummy(new Vector2(trackWidth + 150.0f, 0)); // trackWidth + labelWidth
    }

    private void DrawMarkersInFooterBar(int visibleFrames, Vector2 markerBarMin, Vector2 markerBarMax)
    {
        if (_markers.Count == 0) return;

        var drawList = ImGui.GetWindowDrawList();
        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();

        _hoveredMarker = null;

        foreach (var marker in _markers)
        {
            if (marker.Frame < TimelineStart || marker.Frame > TimelineStart + visibleFrames)
                continue;

            float normalizedPos = (marker.Frame - TimelineStart) / (float)visibleFrames;
            float markerX = markerBarMin.X + (normalizedPos * (markerBarMax.X - markerBarMin.X));

            uint markerColor = GetMarkerColorUInt(marker.Color);

            // ===== Draw MARKER HEAD ON Scrubber =====
            float rulerY = _scrubberPosition.Y;
            float rulerHeight = 18.0f;

            float trapezoidTopY = rulerY + 2;
            float trapezoidBottomY = rulerY + rulerHeight - 2;
            float topWidth = 5.0f;
            float bottomWidth = 1.5f;

            var trapezoidPoints = new Vector2[]
            {
            new(markerX - topWidth, trapezoidTopY),
            new(markerX + topWidth, trapezoidTopY),
            new(markerX + bottomWidth, trapezoidBottomY),
            new(markerX - bottomWidth, trapezoidBottomY)
            };

            drawList.AddQuadFilled(
                trapezoidPoints[0],
                trapezoidPoints[1],
                trapezoidPoints[2],
                trapezoidPoints[3],
                markerColor
            );

            drawList.AddQuad(
                trapezoidPoints[0],
                trapezoidPoints[1],
                trapezoidPoints[2],
                trapezoidPoints[3],
                0xFF000000u,
                1.0f
            );

            // ===== Draw VERTICAL DASHED LINE =====
            float lineStartY = _scrubberPosition.Y + rulerHeight;
            DrawDashedMarkerLine(drawList, markerX, lineStartY, markerBarMin.Y, markerColor);

            // ===== Draw LABEL ALIGNED TO LEFT (GROWING RIGHT) =====
            float labelY = markerBarMin.Y + 5;
            var textSize = ImGui.CalcTextSize(marker.Label);

            // Label starts at marker X position and grows to the right
            float labelX = markerX + 2; // Small offset to not stick to line

            string displayLabel = marker.Label;
            bool wasTruncated = false;

            // Calculate available space until end of bar
            float availableWidth = markerBarMax.X - 2 - labelX - 6; // -6 for padding

            // If doesn't fit, truncate
            if (textSize.X > availableWidth)
            {
                while (textSize.X > availableWidth && displayLabel.Length > 3)
                {
                    displayLabel = displayLabel.Substring(0, displayLabel.Length - 1);
                    textSize = ImGui.CalcTextSize(displayLabel + "...");
                    wasTruncated = true;
                }

                if (wasTruncated)
                {
                    displayLabel += "...";
                    textSize = ImGui.CalcTextSize(displayLabel);
                }
            }

            float finalLabelX = labelX;

            // ===== Draw LABEL BACKGROUND =====
            var markerColorVec = GetMarkerColorVector(marker.Color);
            var bgColor = new Vector4(
                markerColorVec.X * 0.6f,
                markerColorVec.Y * 0.6f,
                markerColorVec.Z * 0.6f,
                1.0f
            );

            var textBgMin = new Vector2(finalLabelX - 3, labelY - 1);
            var textBgMax = new Vector2(finalLabelX + textSize.X + 3, labelY + textSize.Y + 1);

            // Ensure background doesn't go out of bounds
            textBgMin.X = Math.Max(markerBarMin.X + 2, textBgMin.X);
            textBgMax.X = Math.Min(markerBarMax.X - 2, textBgMax.X);

            drawList.AddRectFilled(textBgMin, textBgMax, ImGui.ColorConvertFloat4ToU32(bgColor), 3.0f);
            drawList.AddRect(textBgMin, textBgMax, markerColor, 3.0f, ImDrawFlags.None, 1.0f);

            // ===== Draw TEXT =====
            drawList.AddText(new Vector2(finalLabelX, labelY), 0xFFFFFFFFu, displayLabel);

            // ===== HOVER/CLICK INTERACTION =====
            var mousePos = ImGui.GetMousePos();
            bool isHoveringLabel = mousePos.X >= textBgMin.X && mousePos.X <= textBgMax.X &&
                                   mousePos.Y >= textBgMin.Y && mousePos.Y <= textBgMax.Y;

            if (!isHoveringLabel) continue;
            _hoveredMarker = marker;
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

            // Create invisible clickable area
            ImGui.SetCursorScreenPos(textBgMin);
            ImGui.InvisibleButton($"##MarkerLabel_{marker.Id}", new Vector2(textBgMax.X - textBgMin.X, textBgMax.Y - textBgMin.Y));

            // Show tooltip with full label if truncated
            if (wasTruncated)
            {
                ImGui.SetTooltip(marker.Label);
            }

            // Start drag with left mouse button
            if (ImGui.IsItemActive() && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && _draggedMarker == null)
            {
                _draggedMarker = marker;
                _markerDragStartMouse = mousePos;
                _markerDragStartFrame = marker.Frame;
                _markerDragStartTimelineStart = TimelineStart;
            }

            // Open edit menu with right mouse button
            if (!ImGui.IsItemClicked(ImGuiMouseButton.Right) || _editingMarker != null) continue;
            _editingMarker = marker;

            float popupWidth = 220f;
            float popupHeight = 150f;

            // Center popup horizontally on marker
            float popupX = markerX - (popupWidth * 0.5f);
            popupX = Math.Max(windowPos.X + 10, popupX);
            popupX = Math.Min(windowPos.X + windowSize.X - popupWidth - 10, popupX);

            // Popup ALWAYS above the marker bar
            float popupY = markerBarMin.Y - popupHeight - 10;

            ImGui.SetNextWindowPos(new Vector2(popupX, popupY), ImGuiCond.Always);
            ImGui.OpenPopup($"MarkerEdit_{marker.Id}");
        }

        // ===== PROCESS MARKER drag WITH AUTO-SCROLL =====
        if (_draggedMarker != null && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            var currentMousePos = ImGui.GetMousePos();

            // AUTO-SCROLL: Check if the mouse is beyond the edges
            float leftEdge = markerBarMin.X;
            float rightEdge = markerBarMax.X;
            float mouseX = currentMousePos.X;
            int maxStart = Math.Max(0, TotalFrames - visibleFrames);

            float deltaPixelsRight = Math.Max(0f, mouseX - rightEdge);
            float deltaPixelsLeft = Math.Max(0f, leftEdge - mouseX);

            bool isMouseBeyondRight = deltaPixelsRight > 0;
            bool isMouseBeyondLeft = deltaPixelsLeft > 0;

            // Base speed proportional to zoom
            float baseScrollSpeed = Math.Max(1.0f, visibleFrames / 100.0f);

            if (isMouseBeyondRight)
            {
                float intensity = Math.Clamp(deltaPixelsRight / DRAG_SCROLL_MAX_DISTANCE, 0.0f, 1.0f);
                float speed = (DRAG_SCROLL_MIN_SPEED + (DRAG_SCROLL_MAX_SPEED - DRAG_SCROLL_MIN_SPEED) * intensity) * baseScrollSpeed;
                int frames = (int)Math.Ceiling(speed);
                TimelineStart = Math.Min(maxStart, TimelineStart + frames);

                _isManuallyScrolling = true;
                _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
            }
            else if (isMouseBeyondLeft)
            {
                float intensity = Math.Clamp(deltaPixelsLeft / DRAG_SCROLL_MAX_DISTANCE, 0.0f, 1.0f);
                float speed = (DRAG_SCROLL_MIN_SPEED + (DRAG_SCROLL_MAX_SPEED - DRAG_SCROLL_MIN_SPEED) * intensity) * baseScrollSpeed;
                int frames = (int)Math.Ceiling(speed);
                TimelineStart = Math.Max(0, TimelineStart - frames);

                _isManuallyScrolling = true;
                _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
            }
            else
            {
                // Nudge at edges
                if (mouseX > rightEdge - SCROLL_MARGIN)
                {
                    float t = (mouseX - (rightEdge - SCROLL_MARGIN)) / SCROLL_MARGIN;
                    float nudgeSpeed = t * t * 0.5f * baseScrollSpeed;

                    if (nudgeSpeed >= 0.3f)
                    {
                        int frames = Math.Max(1, (int)Math.Ceiling(nudgeSpeed));
                        TimelineStart = Math.Min(maxStart, TimelineStart + frames);
                        _isManuallyScrolling = true;
                        _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
                    }
                }
                else if (mouseX < leftEdge + SCROLL_MARGIN)
                {
                    float t = ((leftEdge + SCROLL_MARGIN) - mouseX) / SCROLL_MARGIN;
                    float nudgeSpeed = t * t * 0.5f * baseScrollSpeed;

                    if (nudgeSpeed >= 0.3f)
                    {
                        int frames = Math.Max(1, (int)Math.Ceiling(nudgeSpeed));
                        TimelineStart = Math.Max(0, TimelineStart - frames);
                        _isManuallyScrolling = true;
                        _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
                    }
                }
            }

            // Calculate frame delta based on mouse movement AND accumulated scroll
            int scrollDelta = TimelineStart - _markerDragStartTimelineStart;
            float pixelsPerFrame = (markerBarMax.X - markerBarMin.X) / Math.Max(1, visibleFrames);

            float deltaX = currentMousePos.X - _markerDragStartMouse.X;
            float adjustedDeltaX = deltaX + (scrollDelta * pixelsPerFrame);
            int deltaFrames = (int)Math.Round(adjustedDeltaX / pixelsPerFrame);

            int newFrame = _markerDragStartFrame + deltaFrames;
            newFrame = Math.Clamp(newFrame, 0, TotalFrames);

            _draggedMarker.Frame = newFrame;

            // ReOrder markers by frame
            _markers = _markers.OrderBy(m => m.Frame).ToList();
        }
        else if (_draggedMarker != null && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            _draggedMarker = null;
        }
    }

    private void DrawMarkersInDedicatedArea(int visibleFrames, Vector2 markerAreaMin, Vector2 markerAreaMax)
    {
        if (_markers.Count == 0) return;

        var drawList = ImGui.GetWindowDrawList();
        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();

        float timelineBottom = windowPos.Y + windowSize.Y - ImGui.GetStyle().ScrollbarSize;

        _hoveredMarker = null;

        foreach (var marker in _markers)
        {
            if (marker.Frame < TimelineStart || marker.Frame > TimelineStart + visibleFrames)
                continue;

            float normalizedPos = (marker.Frame - TimelineStart) / (float)visibleFrames;
            float markerX = markerAreaMin.X + (normalizedPos * (markerAreaMax.X - markerAreaMin.X));

            uint markerColor = GetMarkerColorUInt(marker.Color);

            // Draw marker dashed line (from marker area to end of timeline)
            DrawDashedMarkerLine(drawList, markerX, markerAreaMax.Y, timelineBottom, markerColor);

            // Draw marker head (INVERTED TRAPEZOID) IN MARKER AREA
            float trapezoidTopY = markerAreaMin.Y + 2;
            float trapezoidBottomY = markerAreaMin.Y + 10;
            float topWidth = 5.0f;
            float bottomWidth = 1.5f;

            var trapezoidPoints = new Vector2[]
            {
            new(markerX - topWidth, trapezoidTopY),
            new(markerX + topWidth, trapezoidTopY),
            new(markerX + bottomWidth, trapezoidBottomY),
            new(markerX - bottomWidth, trapezoidBottomY)
            };

            drawList.AddQuadFilled(
                trapezoidPoints[0],
                trapezoidPoints[1],
                trapezoidPoints[2],
                trapezoidPoints[3],
                markerColor
            );

            drawList.AddQuad(
                trapezoidPoints[0],
                trapezoidPoints[1],
                trapezoidPoints[2],
                trapezoidPoints[3],
                0xFF000000u,
                1.0f
            );

            // Draw label CENTERED in marker area
            float labelY = markerAreaMin.Y + 4;
            var textSize = ImGui.CalcTextSize(marker.Label);

            // Center text
            float labelX = markerX - (textSize.X * 0.5f);

            // Check if label fits inside marker area
            // If not, truncate text
            float maxLabelWidth = (markerAreaMax.X - markerAreaMin.X) / Math.Max(1, _markers.Count(m =>
                m.Frame >= TimelineStart && m.Frame <= TimelineStart + visibleFrames));

            string displayLabel = marker.Label;
            if (textSize.X > maxLabelWidth - 10)
            {
                // Truncate text
                while (textSize.X > maxLabelWidth - 20 && displayLabel.Length > 3)
                {
                    displayLabel = displayLabel.Substring(0, displayLabel.Length - 1);
                    textSize = ImGui.CalcTextSize(displayLabel + "...");
                }
                displayLabel += "...";
                textSize = ImGui.CalcTextSize(displayLabel);
            }

            // Background with marker color (darker for contrast)
            var markerColorVec = GetMarkerColorVector(marker.Color);
            var bgColor = new Vector4(
                markerColorVec.X * 0.4f,
                markerColorVec.Y * 0.4f,
                markerColorVec.Z * 0.4f,
                0.9f
            );

            var textBgMin = new Vector2(labelX - 3, labelY - 1);
            var textBgMax = new Vector2(labelX + textSize.X + 3, labelY + textSize.Y + 1);

            // Ensure label doesn't leave marker area
            textBgMin.X = Math.Max(markerAreaMin.X + 2, textBgMin.X);
            textBgMax.X = Math.Min(markerAreaMax.X - 2, textBgMax.X);
            textBgMin.Y = Math.Max(markerAreaMin.Y + 2, textBgMin.Y);
            textBgMax.Y = Math.Min(markerAreaMax.Y - 2, textBgMax.Y);

            drawList.AddRectFilled(textBgMin, textBgMax, ImGui.ColorConvertFloat4ToU32(bgColor), 2.0f);
            drawList.AddRect(textBgMin, textBgMax, markerColor, 2.0f, ImDrawFlags.None, 1.0f);

            // White text
            drawList.AddText(new Vector2(Math.Max(markerAreaMin.X + 2, labelX), labelY), 0xFFFFFFFFu, displayLabel);

            // Check hover on label
            var mousePos = ImGui.GetMousePos();
            bool isHoveringLabel = mousePos.X >= textBgMin.X && mousePos.X <= textBgMax.X &&
                                   mousePos.Y >= textBgMin.Y && mousePos.Y <= textBgMax.Y;

            if (!isHoveringLabel) continue;
            _hoveredMarker = marker;
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

            // Create invisible clickable area
            ImGui.SetCursorScreenPos(textBgMin);
            ImGui.InvisibleButton($"##MarkerLabel_{marker.Id}", new Vector2(textBgMax.X - textBgMin.X, textBgMax.Y - textBgMin.Y));

            // Show tooltip with full label if truncated
            if (displayLabel != marker.Label)
            {
                ImGui.SetTooltip(marker.Label);
            }

            if (ImGui.IsItemActive() && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && _draggedMarker == null)
            {
                _draggedMarker = marker;
                _markerDragStartMouse = mousePos;
                _markerDragStartFrame = marker.Frame;
                _markerDragStartTimelineStart = TimelineStart;
            }

            if (!ImGui.IsItemClicked(ImGuiMouseButton.Right) || _editingMarker != null) continue;
            _editingMarker = marker;

            float popupWidth = 220f;
            float popupHeight = 150f;

            float popupX = markerX - (popupWidth * 0.5f);
            popupX = Math.Max(windowPos.X + 10, popupX);
            popupX = Math.Min(windowPos.X + windowSize.X - popupWidth - 10, popupX);

            float popupY = markerAreaMin.Y - popupHeight - 10;

            if (popupY < windowPos.Y + 50)
            {
                popupY = markerAreaMax.Y + 10;
            }

            ImGui.SetNextWindowPos(new Vector2(popupX, popupY), ImGuiCond.Always);
            ImGui.OpenPopup($"MarkerEdit_{marker.Id}");
        }

        // Process marker drag
        if (_draggedMarker != null && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            var currentMousePos = ImGui.GetMousePos();
            float deltaX = currentMousePos.X - _markerDragStartMouse.X;

            int scrollDelta = TimelineStart - _markerDragStartTimelineStart;
            float pixelsPerFrame = (markerAreaMax.X - markerAreaMin.X) / Math.Max(1, visibleFrames);

            float adjustedDeltaX = deltaX + (scrollDelta * pixelsPerFrame);
            int deltaFrames = (int)Math.Round(adjustedDeltaX / pixelsPerFrame);

            int newFrame = _markerDragStartFrame + deltaFrames;
            newFrame = Math.Clamp(newFrame, 0, TotalFrames);

            _draggedMarker.Frame = newFrame;

            _markers = _markers.OrderBy(m => m.Frame).ToList();
        }
        else if (_draggedMarker != null && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            _draggedMarker = null;
        }
    }

    private void HandleTimelineAreaContextMenu()
    {
        // CRITICAL: Check if any popup is open or blocking UI is visible
        bool isAnyPopupOpen = _editingMarker != null ||
                              ImGui.IsPopupOpen("KeyframeContextMenu") ||
                              _markers.Any(m => ImGui.IsPopupOpen($"MarkerEdit_{m.Id}")) ||
                              ImGui.IsPopupOpen("MarkerColorPicker");

        if (isAnyPopupOpen) return;
        
        // Check if blocking UI (spawn menu or dialogs) is visible
        if (Main.GetInstance()?.UI?.IsBlockingUIVisible == true) return;
        
        // Calculate marker footer area
        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();
        float scrollbarHeight = ImGui.GetStyle().ScrollbarSize;
        float scrollbarWidth = ImGui.GetStyle().ScrollbarSize;
        float markerBarHeight = 26.0f;
        float markerBarY = windowPos.Y + windowSize.Y - scrollbarHeight - markerBarHeight;

        // Timeline area (everything below Scrubber)
        var scrubberBottom = _scrubberPosition.Y + 18;

        // FIX: If no markers, include footer area in timeline
        float bottomLimit = _markers.Count > 0 ? markerBarY : (windowPos.Y + windowSize.Y - scrollbarHeight);

        var timelineAreaMin = _scrubberPosition with { Y = scrubberBottom };
        var timelineAreaMax = new Vector2(
            _scrubberPosition.X + _scrubberWidth - scrollbarWidth,
            bottomLimit  // CHANGE: dynamic limit based on markers
        );

        var mousePos = ImGui.GetMousePos();
        bool isMouseInTimelineArea = mousePos.X >= timelineAreaMin.X &&
                                     mousePos.X <= timelineAreaMax.X &&
                                     mousePos.Y >= timelineAreaMin.Y &&
                                     mousePos.Y <= timelineAreaMax.Y;

        // Important: Check if mouse is over any marker label (only if there are markers)
        bool isMouseOverMarkerLabel = _markers.Count > 0 && IsMouseOverAnyMarkerLabel(mousePos);

        bool canOpenContextMenu = isMouseInTimelineArea &&
                                 ImGui.IsMouseClicked(ImGuiMouseButton.Right) &&
                                 !IsDraggingKeyframe &&
                                 !_isCreatingRegion &&
                                 !_isDraggingRegion &&
                                 !_isResizingRegion &&
                                 !isMouseOverMarkerLabel;

        if (!canOpenContextMenu) return;
        IsScrubbing = false;
        IsDraggingKeyframe = false;
        _isDragSelecting = false;
        _isPendingScrubberMove = false;

        ImGui.OpenPopup("KeyframeContextMenu");
    }

    // Helper method to check if mouse is over any marker label
    private bool IsMouseOverAnyMarkerLabel(Vector2 mousePos)
    {
        // NEW: Return false immediately if no markers
        if (_markers.Count == 0) return false;

        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();
        float scrollbarHeight = ImGui.GetStyle().ScrollbarSize;
        float markerBarHeight = 26.0f;
        float markerBarY = windowPos.Y + windowSize.Y - scrollbarHeight - markerBarHeight;

        // Marker footer area
        float markerBarMin = markerBarY;

        int visibleFrames = Math.Max(1, (int)(95 / TimelineZoom));

        foreach (var marker in _markers)
        {
            // Check if marker is visible
            if (marker.Frame < TimelineStart || marker.Frame > TimelineStart + visibleFrames)
                continue;

            float normalizedPos = (marker.Frame - TimelineStart) / (float)visibleFrames;
            float markerX = _scrubberPosition.X + (normalizedPos * _scrubberWidth);

            var textSize = ImGui.CalcTextSize(marker.Label);
            float labelX = markerX - (textSize.X * 0.5f);
            float labelY = markerBarMin + 5;

            // Label bounds
            var textBgMin = new Vector2(labelX - 3, labelY - 1);
            var textBgMax = new Vector2(labelX + textSize.X + 3, labelY + textSize.Y + 1);

            // Check if mouse is over this label
            if (mousePos.X >= textBgMin.X && mousePos.X <= textBgMax.X &&
                mousePos.Y >= textBgMin.Y && mousePos.Y <= textBgMax.Y)
            {
                return true;
            }
        }

        return false;
    }

    private void HandleBoxSelectionInContentArea()
    {
        // CRITICAL: Check if any popup is open or blocking UI is visible
        bool isAnyPopupOpen = _editingMarker != null ||
                              ImGui.IsPopupOpen("KeyframeContextMenu") ||
                              _markers.Any(m => ImGui.IsPopupOpen($"MarkerEdit_{m.Id}")) ||
                              ImGui.IsPopupOpen("MarkerColorPicker");

        if (isAnyPopupOpen) return;
        
        // Check if blocking UI (spawn menu or dialogs) is visible
        if (Main.GetInstance()?.UI?.IsBlockingUIVisible == true) return;
        
        // Calculate marker footer area
        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();
        float scrollbarHeight = ImGui.GetStyle().ScrollbarSize;
        float scrollbarWidth = ImGui.GetStyle().ScrollbarSize;
        float markerBarHeight = 26.0f;
        float markerBarY = windowPos.Y + windowSize.Y - scrollbarHeight - markerBarHeight;

        // Clickable area for box selection (starts after Scrubber)
        var scrubberBottom = _scrubberPosition.Y + 18;

        // FIX: If no markers, include footer area in selection
        float bottomLimit = _markers.Count > 0 ? markerBarY : (windowPos.Y + windowSize.Y - scrollbarHeight);

        var selectableAreaMin = _scrubberPosition with { Y = scrubberBottom };
        var selectableAreaMax = new Vector2(
            _scrubberPosition.X + _scrubberWidth - scrollbarWidth,
            bottomLimit  // CHANGE: dynamic limit based on markers
        );

        var mousePos = ImGui.GetMousePos();
        bool isMouseInSelectableArea = mousePos.X >= selectableAreaMin.X &&
                                       mousePos.X <= selectableAreaMax.X &&
                                       mousePos.Y >= selectableAreaMin.Y &&
                                       mousePos.Y <= selectableAreaMax.Y;

        // Important: Check if mouse is over any marker label (only if there are markers)
        bool isMouseOverMarkerLabel = _markers.Count > 0 && IsMouseOverAnyMarkerLabel(mousePos);

        bool canStartBoxSelection = isMouseInSelectableArea &&
                                   ImGui.IsMouseClicked(ImGuiMouseButton.Left) &&
                                   !IsDraggingKeyframe &&
                                   !_isDraggingRegion &&
                                   !_isResizingRegion &&
                                   !IsScrubbing &&
                                   !_isCreatingRegion &&
                                   !isMouseOverMarkerLabel;

        // Start box selection
        if (canStartBoxSelection)
        {
            if (IsPlaying)
            {
                IsPlaying = false;
            }

            bool clickedOnKeyframe = false;

            if (CurrentObject != null)
            {
                foreach (var (_, accessor) in PropertyAccessors)
                {
                    var keyframes = accessor(CurrentObject);
                    if (keyframes.Count <= 0) continue;
                    int visibleFrames = Math.Max(1, (int)(95 / TimelineZoom));
                    if (!IsMouseNearAnyKeyframe(mousePos, keyframes, visibleFrames)) continue;
                    clickedOnKeyframe = true;
                    break;
                }
            }

            if (!clickedOnKeyframe)
            {
                _isPendingScrubberMove = true;
                _pendingScrubberPosition = mousePos;
                _pendingScrubberTrackWidth = _scrubberWidth;

                _dragSelectionStart = mousePos;
                _dragSelectionEnd = mousePos;

                float relativeX = (mousePos.X - _scrubberPosition.X) / _scrubberWidth;
                int visibleFrames = Math.Max(1, (int)(95 / TimelineZoom));

                _dragSelectionStartFrame = TimelineStart + (int)(relativeX * visibleFrames);
                _dragSelectionStartFrame = Math.Clamp(_dragSelectionStartFrame, 0, TotalFrames);

                _dragSelectionStartTimelineStart = TimelineStart;
            }
        }

        // Update box selection
        if (_isPendingScrubberMove && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            var currentMousePos = ImGui.GetMousePos();

            // FIX: Limit mouse position BEFORE scrollbar
            float maxX = _scrubberPosition.X + _scrubberWidth - scrollbarWidth;
            currentMousePos.X = Math.Min(currentMousePos.X, maxX);

            _dragSelectionEnd = currentMousePos;

            float dragDistance = Vector2.Distance(_dragSelectionStart, _dragSelectionEnd);
            if (dragDistance > 3.0f)
            {
                _isDragSelecting = true;
                _isPendingScrubberMove = false;
            }
        }

        if (_isDragSelecting && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            var currentMousePos = ImGui.GetMousePos();

            // FIX: Limit mouse position BEFORE scrollbar
            float maxX = _scrubberPosition.X + _scrubberWidth - scrollbarWidth;
            currentMousePos.X = Math.Min(currentMousePos.X, maxX);

            _dragSelectionEnd = currentMousePos;
        }

        // Draw box selection with upper limit at Scrubber
        if (_isDragSelecting)
        {
            DrawBoxSelectionWithAnimatedDashes(scrubberBottom);
        }
    }

    private bool IsMouseNearAnyKeyframe(Vector2 mousePos, Dictionary<int, Keyframe> keyframes, int visibleFrames)
    {
        if (keyframes.Count == 0) return false;

        float scrollbarWidth = ImGui.GetStyle().ScrollbarSize;
        // FIX: Use _scrubberWidth - scrollbarWidth
        float usableWidth = _scrubberWidth - scrollbarWidth;

        return (from kvp in keyframes where kvp.Key >= TimelineStart && kvp.Key <= TimelineStart + visibleFrames select (kvp.Key - TimelineStart) / (float)visibleFrames into normalizedPos select _scrubberPosition.X + (normalizedPos * usableWidth) into markerX select Math.Abs(mousePos.X - markerX)).Any(deltaX => deltaX <= 8.0f);
    }

    private void RenderTimelineHorizontalScrollbar(int visibleFrames)
    {
        if (IsPlaying) return;
        
        var scrollbarHeight = ImGui.GetStyle().ScrollbarSize;
        var verticalScrollbarWidth = ImGui.GetStyle().ScrollbarSize;
        float scrollMax = Math.Max(0.0f, TotalFrames - visibleFrames);

        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();
        var scrollbarPos = new Vector2(windowPos.X, windowPos.Y + windowSize.Y - scrollbarHeight);
        var scrollbarWidth = Math.Max(0, windowSize.X - verticalScrollbarWidth);

        if (scrollMax > 0 && scrollbarWidth > 0)
        {
            ImGui.SetCursorScreenPos(scrollbarPos);
            ImGui.Dummy(Vector2.Zero);

            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            ImGui.BeginChild("##TimelineHorizontalScrollbar", new Vector2(scrollbarWidth, scrollbarHeight),
                ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);

            float zoomedTimelineWidth = TotalFrames * TimelineZoom;
            ImGui.InvisibleButton("##ScrollContent", new Vector2(zoomedTimelineWidth, 1));

            float currentScrollX = ImGui.GetScrollX();
            float expectedScrollX = TimelineStart * TimelineZoom;

            bool isUserInteracting = ImGui.IsWindowHovered() || ImGui.IsItemActive() || ImGui.IsWindowFocused();

            if (isUserInteracting)
            {
                int newTimelineStart = (int)(currentScrollX / TimelineZoom);

                if (newTimelineStart != TimelineStart)
                {
                    TimelineStart = Math.Max(0, Math.Min(TotalFrames - visibleFrames, newTimelineStart));

                    _isManuallyScrolling = true;
                    _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
                }
            }
            else if (!_isManuallyScrolling)
            {
                if (Math.Abs(currentScrollX - expectedScrollX) > 2.0f)
                {
                    ImGui.SetScrollX(expectedScrollX);
                }
            }

            ImGui.EndChild();
            ImGui.PopStyleColor();
        }
    }

    private void DrawPlayheadComplete(int visibleFrames)
    {
        if (CurrentFrame < TimelineStart || CurrentFrame > TimelineStart + visibleFrames)
            return;

        var drawList = ImGui.GetWindowDrawList();
        var currentWindowPos = ImGui.GetWindowPos();
        var currentWindowSize = ImGui.GetWindowSize();

        float normalizedPos = (CurrentFrame - TimelineStart) / (float)visibleFrames;
        float playheadX = _scrubberPosition.X + (normalizedPos * _scrubberWidth);

        // Blue cyan color (#00D4FF or similar)
        var playheadColor = 0xFFFF6A00u;

        // Vertical marker line
        float topY = _scrubberPosition.Y;
        float bottomY = currentWindowPos.Y + currentWindowSize.Y - ImGui.GetStyle().ScrollbarSize;
        drawList.AddLine(new Vector2(playheadX, topY), new Vector2(playheadX, bottomY), playheadColor, 2.0f);

        // Marker head in INVERTED TRAPEZOID shape (wide base on top, point at bottom)
        float scrubberHeight = 18.0f;
        float trapezoidTopY = _scrubberPosition.Y;
        float trapezoidBottomY = _scrubberPosition.Y + (scrubberHeight * 0.45f);

        // Top base width (wide)
        float topWidth = 8.0f;
        // Bottom base width (narrow - almost a point)
        float bottomWidth = 2.0f;

        // Four points of INVERTED TRAPEZOID
        var trapezoidPoints = new Vector2[]
        {
        new(playheadX - topWidth, trapezoidTopY),
        new(playheadX + topWidth, trapezoidTopY),
        new(playheadX + bottomWidth, trapezoidBottomY),
        new(playheadX - bottomWidth, trapezoidBottomY)
        };

        // Draw the filled trapezoid
        drawList.AddQuadFilled(
            trapezoidPoints[0],
            trapezoidPoints[1],
            trapezoidPoints[2],
            trapezoidPoints[3],
            playheadColor
        );

        // Thin black border on the trapezoid for emphasis
        drawList.AddQuad(
            trapezoidPoints[0],
            trapezoidPoints[1],
            trapezoidPoints[2],
            trapezoidPoints[3],
            0xFF000000u,
            1.0f
        );

        // Frame number above the head
        string frameText = CurrentFrame.ToString();
        var textSize = ImGui.CalcTextSize(frameText);
        float textX = playheadX - (textSize.X * 0.5f);
        float textY = _scrubberPosition.Y - textSize.Y - 2;

        // Semi-transparent background for the text
        var textBgMin = new Vector2(textX - 2, textY - 1);
        var textBgMax = new Vector2(textX + textSize.X + 2, textY + textSize.Y + 1);
        drawList.AddRectFilled(textBgMin, textBgMax, 0xAA000000u, 2.0f);

        drawList.AddText(new Vector2(textX, textY), 0xFFFFFFFFu, frameText);
    }

    private void RenderTrackNamesBoxScrollable(float labelWidth, float maxHeight)
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(3, 3));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(ImGui.GetStyle().ItemSpacing.X, 2.0f));

        // BeginChild WITHOUT scroll - just container
        ImGui.BeginChild("##TrackNames", new Vector2(labelWidth, maxHeight),
            ImGuiChildFlags.None,
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

        if (CurrentObject != null)
        {
            if (CurrentObject.PosXKeyframes.Count > 0)
                RenderTrackLabel($"{CurrentObject.Name} Position X", new Vector4(0.4f, 0.2f, 0.2f, 1.0f));
            if (CurrentObject.PosYKeyframes.Count > 0)
                RenderTrackLabel($"{CurrentObject.Name} Position Y", new Vector4(0.2f, 0.4f, 0.2f, 1.0f));
            if (CurrentObject.PosZKeyframes.Count > 0)
                RenderTrackLabel($"{CurrentObject.Name} Position Z", new Vector4(0.2f, 0.2f, 0.4f, 1.0f));
            if (CurrentObject.RotXKeyframes.Count > 0)
                RenderTrackLabel($"{CurrentObject.Name} Rotation X", new Vector4(0.6f, 0.3f, 0.3f, 1.0f));
            if (CurrentObject.RotYKeyframes.Count > 0)
                RenderTrackLabel($"{CurrentObject.Name} Rotation Y", new Vector4(0.3f, 0.6f, 0.3f, 1.0f));
            if (CurrentObject.RotZKeyframes.Count > 0)
                RenderTrackLabel($"{CurrentObject.Name} Rotation Z", new Vector4(0.3f, 0.3f, 0.6f, 1.0f));
            if (CurrentObject.ScaleXKeyframes.Count > 0)
                RenderTrackLabel($"{CurrentObject.Name} Scale X", new Vector4(0.8f, 0.4f, 0.4f, 1.0f));
            if (CurrentObject.ScaleYKeyframes.Count > 0)
                RenderTrackLabel($"{CurrentObject.Name} Scale Y", new Vector4(0.4f, 0.8f, 0.4f, 1.0f));
            if (CurrentObject.ScaleZKeyframes.Count > 0)
                RenderTrackLabel($"{CurrentObject.Name} Scale Z", new Vector4(0.4f, 0.4f, 0.8f, 1.0f));
            if (CurrentObject.AlphaKeyframes.Count > 0)
                RenderTrackLabel($"{CurrentObject.Name} Alpha", new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
        }

        // FIX: Force apply scroll each frame
        ImGui.SetScrollY(_trackScrollY);

        ImGui.EndChild();
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor();
    }

    private void RenderTrackLabel(string labelText, Vector4 labelColor)
    {
        var barSize = new Vector2(-1, 18);

        // Extract property from label text
        string propertyName = ExtractPropertyFromLabel(labelText);

        // Check if this track is selected
        bool isSelected = _selectedTrackProperty == propertyName;

        // If selected, make it lighter (multiply by 1.5)
        Vector4 displayColor = isSelected
            ? new Vector4(
                Math.Min(1.0f, labelColor.X * 1.5f),
                Math.Min(1.0f, labelColor.Y * 1.5f),
                Math.Min(1.0f, labelColor.Z * 1.5f),
                labelColor.W)
            : labelColor;

        var darkerColor = new Vector4(displayColor.X * 0.7f, displayColor.Y * 0.7f, displayColor.Z * 0.7f, displayColor.W);
        var lighterColor = new Vector4(
            Math.Min(1.0f, displayColor.X * 1.3f),
            Math.Min(1.0f, displayColor.Y * 1.3f),
            Math.Min(1.0f, displayColor.Z * 1.3f),
            displayColor.W);

        ImGui.PushStyleColor(ImGuiCol.Button, displayColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, lighterColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, darkerColor);

        // Render the button and detect click
        if (ImGui.Button(labelText, barSize))
        {
            // On click, select this track
            _selectedTrackProperty = propertyName;
        }

        ImGui.PopStyleColor(3);
    }

    // Helper method to extract the property name from the label
    private string ExtractPropertyFromLabel(string labelText)
    {
        string lower = labelText.ToLower();

        if (lower.Contains("position x")) return "position.x";
        if (lower.Contains("position y")) return "position.y";
        if (lower.Contains("position z")) return "position.z";
        if (lower.Contains("rotation x")) return "rotation.x";
        if (lower.Contains("rotation y")) return "rotation.y";
        if (lower.Contains("rotation z")) return "rotation.z";
        if (lower.Contains("scale x")) return "scale.x";
        if (lower.Contains("scale y")) return "scale.y";
        if (lower.Contains("scale z")) return "scale.z";
        if (lower.Contains("alpha")) return "alpha";

        return "";
    }

    private float _trackScrollY = 0f;

    private void RenderTimelineTracksBoxScrollable(float trackWidth, int visibleFrames, float maxHeight)
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.15f, 0.15f, 0.2f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(3, 3));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(ImGui.GetStyle().ItemSpacing.X, 2.0f));

        // BeginChild WITH vertical scroll
        ImGui.BeginChild("##TimelineTracks", new Vector2(trackWidth, maxHeight),
            ImGuiChildFlags.None,
            ImGuiWindowFlags.AlwaysVerticalScrollbar);

        if (CurrentObject != null)
        {
            if (CurrentObject.PosXKeyframes.Count > 0)
                RenderTimelineTrack(CurrentObject.PosXKeyframes, "position.x", visibleFrames);
            if (CurrentObject.PosYKeyframes.Count > 0)
                RenderTimelineTrack(CurrentObject.PosYKeyframes, "position.y", visibleFrames);
            if (CurrentObject.PosZKeyframes.Count > 0)
                RenderTimelineTrack(CurrentObject.PosZKeyframes, "position.z", visibleFrames);
            if (CurrentObject.RotXKeyframes.Count > 0)
                RenderTimelineTrack(CurrentObject.RotXKeyframes, "rotation.x", visibleFrames);
            if (CurrentObject.RotYKeyframes.Count > 0)
                RenderTimelineTrack(CurrentObject.RotYKeyframes, "rotation.y", visibleFrames);
            if (CurrentObject.RotZKeyframes.Count > 0)
                RenderTimelineTrack(CurrentObject.RotZKeyframes, "rotation.z", visibleFrames);
            if (CurrentObject.ScaleXKeyframes.Count > 0)
                RenderTimelineTrack(CurrentObject.ScaleXKeyframes, "scale.x", visibleFrames);
            if (CurrentObject.ScaleYKeyframes.Count > 0)
                RenderTimelineTrack(CurrentObject.ScaleYKeyframes, "scale.y", visibleFrames);
            if (CurrentObject.ScaleZKeyframes.Count > 0)
                RenderTimelineTrack(CurrentObject.ScaleZKeyframes, "scale.z", visibleFrames);
            if (CurrentObject.AlphaKeyframes.Count > 0)
                RenderTimelineTrack(CurrentObject.AlphaKeyframes, "alpha", visibleFrames);
        }

        // CRITICAL: Read the CURRENT scroll and store it
        _trackScrollY = ImGui.GetScrollY();

        // FIX BUG #1: Reset the flag AFTER processing ALL tracks
        // This ensures all tracks see the flag before it is reset
        if (_shouldApplyBoxSelection) _shouldApplyBoxSelection = false;

        ImGui.EndChild();
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor();
    }

    private void RenderTimelineTrack(Dictionary<int, Keyframe> keyframes, string property, int visibleFrames)
    {
        var drawList = ImGui.GetWindowDrawList();
        var barSize = new Vector2(-1, 18);

        // Check if this track is selected
        bool isSelected = _selectedTrackProperty == property;

        // Base track color - lighter if selected
        Vector4 trackColor = isSelected
            ? new Vector4(0.2f, 0.2f, 0.3f, 1.0f)
            : new Vector4(0.1f, 0.1f, 0.15f, 1.0f);

        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, trackColor);
        ImGui.ProgressBar(0.0f, barSize, "");
        ImGui.PopStyleColor();

        var trackRect = ImGui.GetItemRectMin();
        var trackSize = ImGui.GetItemRectSize();
        bool isTrackHovered = ImGui.IsItemHovered();

        // Detect click on track to select it
        if (isTrackHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !IsDraggingKeyframe)
        {
            _selectedTrackProperty = property;
        }

        HandleTrackDoubleClick(isTrackHovered, keyframes, property, visibleFrames, trackRect, trackSize);

        HandleBoxSelectionAutoScroll(trackRect, trackSize, visibleFrames);
        HandleKeyframeDragAutoScroll(visibleFrames);
        HandleRegionDragAutoScroll(trackRect, trackSize, visibleFrames);

        // Auto-scroll for markers is processed directly in DrawMarkersInFooterBar

        // IMPORTANT: Render keyframes BEFORE resetting _shouldApplyBoxSelection
        RenderKeyframesOnTrack(keyframes, property, visibleFrames, trackRect, trackSize, isTrackHovered, drawList);
    }

    private void HandleTrackDoubleClick(bool isTrackHovered, Dictionary<int, Keyframe> keyframes, string property,
     int visibleFrames, Vector2 trackRect, Vector2 trackSize)
    {
        if (!isTrackHovered || !ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) || IsDraggingKeyframe ||
            CurrentObject == null) return;
        if (IsPlaying) IsPlaying = false;

        var mousePos = ImGui.GetMousePos();

        // USE THE EXACT SAME SCRUBBER CALCULATION
        float relativeX = (mousePos.X - trackRect.X) / trackSize.X;
        relativeX = Math.Clamp(relativeX, 0.0f, 1.0f);
        float frameFloat = relativeX * visibleFrames;
        int clickedFrame = TimelineStart + (int)Math.Round(frameFloat);
        clickedFrame = Math.Clamp(clickedFrame, 0, TotalFrames);

        bool clickedOnExistingKeyframe = IsMouseOnKeyframe(mousePos, keyframes, visibleFrames, trackRect, trackSize);

        if (clickedOnExistingKeyframe) return;
        
        try
        {
            float currentValue = GetCurrentPropertyValue(property);
            EasingMode easingMode = GetEasingModeForNewKeyframe(keyframes, clickedFrame);
            keyframes[clickedFrame] = new Keyframe(currentValue, easingMode);

            SelectedKeyframes.Clear();
            SelectedKeyframes.Add(new SelectedKeyframe
            {
                Property = property,
                Frame = clickedFrame,
                ObjectGuid = Main.GetInstance()?.UI?.SceneTreePanel?.SelectedObjectGuid
            });
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error creating keyframe: {ex.Message}");
            return;
        }

        // Synchronize the marker with the created keyframe
        CurrentFrame = clickedFrame;
        CurrentFrameFloat = clickedFrame;

        // IMPORTANT: Cancel any pending scrubber move that may be active
        _isPendingScrubberMove = false;

        ApplyKeyframesToObjects();
        CalculateTotalFrames();
    }

    private bool IsMouseOnKeyframe(Vector2 mousePos, Dictionary<int, Keyframe> keyframes,
    int visibleFrames, Vector2 trackRect, Vector2 trackSize)
    {
        if (keyframes.Count == 0) return false;

        float scrollbarWidth = ImGui.GetStyle().ScrollbarSize;
        // CRITICAL: Use _scrubberWidth
        float usableWidth = _scrubberWidth - scrollbarWidth;
        float trackCenterY = trackRect.Y + (trackSize.Y * 0.5f);

        if (Math.Abs(mousePos.Y - trackCenterY) > 10.0f)
            return false;

        return (from kvp in keyframes where kvp.Key >= TimelineStart && kvp.Key <= TimelineStart + visibleFrames select (kvp.Key - TimelineStart) / (float)visibleFrames into normalizedPos select _scrubberPosition.X + (normalizedPos * usableWidth) into markerX select Math.Abs(mousePos.X - markerX) into deltaX where !(deltaX > 6.0f) let deltaY = Math.Abs(mousePos.Y - trackCenterY) where !(deltaY > 6.0f) select (deltaX * deltaX) + (deltaY * deltaY)).Any(distanceSq => distanceSq <= 36.0f);
    }

    private void HandleBoxSelectionAutoScroll(Vector2 trackRect, Vector2 trackSize, int visibleFrames)
    {
        if (!_isDragSelecting || !ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            return;

        var mousePos = _dragSelectionEnd;
        float leftEdge = trackRect.X;
        float rightEdge = trackRect.X + trackSize.X;
        int maxStart = Math.Max(0, TotalFrames - visibleFrames);

        // Base speed proportional to zoom
        float baseScrollSpeed = Math.Max(1.0f, visibleFrames / 100.0f);

        if (mousePos.X > rightEdge)
        {
            float deltaPixels = mousePos.X - rightEdge;
            float intensity = Math.Clamp(deltaPixels / MAX_SCROLL_DISTANCE, 0.0f, 1.0f);
            float speed = (MIN_SCROLL_SPEED + (MAX_SCROLL_SPEED - MIN_SCROLL_SPEED) * intensity) * baseScrollSpeed;
            int frames = Math.Max(1, (int)Math.Ceiling(speed));

            TimelineStart = Math.Min(maxStart, TimelineStart + frames);
            _isManuallyScrolling = true;
            _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
            _dragSelectionEnd.X = rightEdge - 1;
        }
        else if (mousePos.X < leftEdge)
        {
            float deltaPixels = leftEdge - mousePos.X;
            float intensity = Math.Clamp(deltaPixels / MAX_SCROLL_DISTANCE, 0.0f, 1.0f);
            float speed = (MIN_SCROLL_SPEED + (MAX_SCROLL_SPEED - MIN_SCROLL_SPEED) * intensity) * baseScrollSpeed;
            int frames = Math.Max(1, (int)Math.Ceiling(speed));

            TimelineStart = Math.Max(0, TimelineStart - frames);
            _isManuallyScrolling = true;
            _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
            _dragSelectionEnd.X = leftEdge + 1;
        }
        else
        {
            if (mousePos.X > rightEdge - SCROLL_MARGIN)
            {
                float t = (mousePos.X - (rightEdge - SCROLL_MARGIN)) / SCROLL_MARGIN;
                float nudgeSpeed = t * t * 0.3f * baseScrollSpeed;

                if (!(nudgeSpeed >= 0.5f)) return;
                int frames = Math.Max(1, (int)Math.Ceiling(nudgeSpeed));
                TimelineStart = Math.Min(maxStart, TimelineStart + frames);
                _isManuallyScrolling = true;
                _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
            }
            else if (mousePos.X < leftEdge + SCROLL_MARGIN)
            {
                float t = ((leftEdge + SCROLL_MARGIN) - mousePos.X) / SCROLL_MARGIN;
                float nudgeSpeed = t * t * 0.3f * baseScrollSpeed;

                if (!(nudgeSpeed >= 0.5f)) return;
                int frames = Math.Max(1, (int)Math.Ceiling(nudgeSpeed));
                TimelineStart = Math.Max(0, TimelineStart - frames);
                _isManuallyScrolling = true;
                _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
            }
        }
    }

    private void HandleKeyframeDragAutoScroll(int visibleFrames)
    {
        if (!IsDraggingKeyframe || !ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            return;

        var currentMousePos = ImGui.GetMousePos();

        // CRITICAL: _dragStartScrubberWidth is already the usable width
        float usableWidth = _dragStartScrubberWidth;
        float leftEdge = _dragStartScrubberX;
        float rightEdge = _dragStartScrubberX + usableWidth;
        float mouseX = currentMousePos.X;
        int maxStart = Math.Max(0, TotalFrames - visibleFrames);

        float deltaPixelsRight = Math.Max(0f, mouseX - rightEdge);
        float deltaPixelsLeft = Math.Max(0f, leftEdge - mouseX);

        bool isMouseBeyondRight = deltaPixelsRight > 0;
        bool isMouseBeyondLeft = deltaPixelsLeft > 0;

        float baseScrollSpeed = Math.Max(1.0f, visibleFrames / 100.0f);

        // CRITICAL: Capture TimelineStart at start for consistent calculations
        int timelineStartSnapshot = TimelineStart;

        if (isMouseBeyondRight && _dragAnchorKeyframe != null)
        {
            float intensity = Math.Clamp(deltaPixelsRight / DRAG_SCROLL_MAX_DISTANCE, 0.0f, 1.0f);
            float speed = (DRAG_SCROLL_MIN_SPEED + (DRAG_SCROLL_MAX_SPEED - DRAG_SCROLL_MIN_SPEED) * intensity) * baseScrollSpeed;
            int frames = (int)Math.Ceiling(speed);

            int newTimelineStart = Math.Min(maxStart, timelineStartSnapshot + frames);

            int currentAnchorFrame = -1;
            if (_draggedKeyframesData.ContainsKey(_dragAnchorKeyframe))
            {
                float framePixelWidth = usableWidth / Math.Max(1, visibleFrames);

                float deltaX = currentMousePos.X - _dragStartMousePos.X;
                int scrollDelta = timelineStartSnapshot - _dragStartTimelineStart;

                int rawOffset = (int)Math.Round((deltaX + (scrollDelta * framePixelWidth)) / framePixelWidth);
                currentAnchorFrame = _draggedKeyframesData[_dragAnchorKeyframe].originalFrame + rawOffset;
            }

            if (currentAnchorFrame != -1)
            {
                int safetyMargin = Math.Max(1, (int)(visibleFrames * 0.02f));
                int targetAnchorFrame = newTimelineStart + visibleFrames - safetyMargin - 1;
                targetAnchorFrame = Math.Min(TotalFrames, targetAnchorFrame);

                int originalAnchorFrame = _draggedKeyframesData[_dragAnchorKeyframe].originalFrame;
                int newOffset = targetAnchorFrame - originalAnchorFrame;

                TimelineStart = newTimelineStart;
                _currentDragOffset = newOffset;

                _isManuallyScrolling = true;
                _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;

                UpdateDragPreviewWavePassThrough();
            }
        }
        else if (isMouseBeyondLeft && _dragAnchorKeyframe != null)
        {
            float intensity = Math.Clamp(deltaPixelsLeft / DRAG_SCROLL_MAX_DISTANCE, 0.0f, 1.0f);
            float speed = (DRAG_SCROLL_MIN_SPEED + (DRAG_SCROLL_MAX_SPEED - DRAG_SCROLL_MIN_SPEED) * intensity) * baseScrollSpeed;
            int frames = (int)Math.Ceiling(speed);

            int newTimelineStart = Math.Max(0, timelineStartSnapshot - frames);

            int currentAnchorFrame = -1;
            if (_draggedKeyframesData.ContainsKey(_dragAnchorKeyframe))
            {
                float framePixelWidth = usableWidth / Math.Max(1, visibleFrames);

                float deltaX = currentMousePos.X - _dragStartMousePos.X;
                int scrollDelta = timelineStartSnapshot - _dragStartTimelineStart;

                int rawOffset = (int)Math.Round((deltaX + (scrollDelta * framePixelWidth)) / framePixelWidth);
                currentAnchorFrame = _draggedKeyframesData[_dragAnchorKeyframe].originalFrame + rawOffset;
            }

            if (currentAnchorFrame != -1)
            {
                int safetyMargin = Math.Max(1, (int)(visibleFrames * 0.02f));
                int targetAnchorFrame = newTimelineStart + safetyMargin;
                targetAnchorFrame = Math.Max(0, targetAnchorFrame);

                int originalAnchorFrame = _draggedKeyframesData[_dragAnchorKeyframe].originalFrame;
                int newOffset = targetAnchorFrame - originalAnchorFrame;

                TimelineStart = newTimelineStart;
                _currentDragOffset = newOffset;

                _isManuallyScrolling = true;
                _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;

                UpdateDragPreviewWavePassThrough();
            }
        }
        else
        {
            // INSIDE VIEW
            float framePixelWidth = usableWidth / Math.Max(1, visibleFrames);

            float deltaX = currentMousePos.X - _dragStartMousePos.X;
            int scrollDelta = timelineStartSnapshot - _dragStartTimelineStart;

            int rawOffset = (int)Math.Round((deltaX + (scrollDelta * framePixelWidth)) / framePixelWidth);

            if (rawOffset != _currentDragOffset)
            {
                _currentDragOffset = rawOffset;
                UpdateDragPreviewWavePassThrough();
            }

            int currentAnchorFrame = -1;
            if (_dragAnchorKeyframe != null && _draggedKeyframesData.ContainsKey(_dragAnchorKeyframe))
            {
                currentAnchorFrame = _draggedKeyframesData[_dragAnchorKeyframe].originalFrame + rawOffset;
            }

            float nudgeRightEdge = rightEdge - NUDGE_MARGIN;
            float nudgeLeftEdge = leftEdge + NUDGE_MARGIN;

            if (mouseX > nudgeRightEdge && mouseX <= rightEdge && currentAnchorFrame != -1)
            {
                float t = (mouseX - nudgeRightEdge) / NUDGE_MARGIN;
                float nudgeSpeed = t * t * 0.5f * baseScrollSpeed;

                if (nudgeSpeed >= 0.3f)
                {
                    int frames = Math.Max(1, (int)Math.Ceiling(nudgeSpeed));
                    int newTimelineStart = Math.Min(maxStart, timelineStartSnapshot + frames);

                    int scrollChange = newTimelineStart - timelineStartSnapshot;
                    TimelineStart = newTimelineStart;
                    _currentDragOffset += scrollChange;

                    _isManuallyScrolling = true;
                    _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;

                    UpdateDragPreviewWavePassThrough();
                }
            }
            else if (mouseX < nudgeLeftEdge && mouseX >= leftEdge && currentAnchorFrame != -1)
            {
                float t = (nudgeLeftEdge - mouseX) / NUDGE_MARGIN;
                float nudgeSpeed = t * t * 0.5f * baseScrollSpeed;

                if (nudgeSpeed >= 0.3f)
                {
                    int frames = Math.Max(1, (int)Math.Ceiling(nudgeSpeed));
                    int newTimelineStart = Math.Max(0, timelineStartSnapshot - frames);

                    int scrollChange = newTimelineStart - timelineStartSnapshot;
                    TimelineStart = newTimelineStart;
                    _currentDragOffset += scrollChange;

                    _isManuallyScrolling = true;
                    _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;

                    UpdateDragPreviewWavePassThrough();
                }
            }
        }

        if (_dragAnchorKeyframe == null ||
            !_previewFrames.TryGetValue(_dragAnchorKeyframe, out int anchorPreviewFrame)) return;
        CurrentFrame = anchorPreviewFrame;
        CurrentFrameFloat = CurrentFrame;
    }

    private void RenderKeyframesOnTrack(Dictionary<int, Keyframe> keyframes, string property,
    int visibleFrames, Vector2 trackRect, Vector2 trackSize, bool isTrackHovered, ImDrawListPtr drawList)
    {
        var keyframeList = keyframes.Keys.OrderBy(k => k).ToList();

        // CRITICAL: _scrubberWidth is already the usable width (without scrollbar)
        float usableWidth = IsDraggingKeyframe ? _dragStartScrubberWidth : _scrubberWidth;
        float scrubberX = IsDraggingKeyframe ? _dragStartScrubberX : _scrubberPosition.X;

        // FIRST PASS: Check box selection on ALL keyframes (even invisible ones)
        if (_shouldApplyBoxSelection)
        {
            foreach (var frame in keyframeList)
            {
                var keyframeId = new SelectedKeyframe
                {
                    Property = property,
                    Frame = frame,
                    ObjectGuid = Main.GetInstance()?.UI?.SceneTreePanel?.SelectedObjectGuid
                };

                float normalizedPos = (frame - TimelineStart) / (float)visibleFrames;
                float markerX = scrubberX + (normalizedPos * usableWidth);
                float markerY = trackRect.Y + (trackSize.Y * 0.5f);

                HandleKeyframeBoxSelection(keyframeId, markerX, markerY);
            }
        }

        // CRITICAL: Capture TimelineStart BEFORE the loop to ensure consistency
        int currentTimelineStart = TimelineStart;

        // SECOND PASS: Draw and interact only with visible keyframes
        foreach (var frame in keyframeList)
        {
            if (!IsDraggingKeyframe && (frame < currentTimelineStart || frame > currentTimelineStart + visibleFrames))
                continue;

            var keyframeId = new SelectedKeyframe
            {
                Property = property,
                Frame = frame,
                ObjectGuid = Main.GetInstance()?.UI?.SceneTreePanel?.SelectedObjectGuid
            };

            bool isSelected = SelectedKeyframes.Contains(keyframeId);

            int displayFrame = frame;
            if (isSelected && IsDraggingKeyframe && _previewFrames.TryGetValue(keyframeId, out int previewFrame))
            {
                displayFrame = previewFrame;
            }

            if (displayFrame < currentTimelineStart || displayFrame > currentTimelineStart + visibleFrames)
                continue;

            // CRITICAL: Use values captured during drag
            float normalizedPos = (displayFrame - currentTimelineStart) / (float)visibleFrames;
            float markerX = scrubberX + (normalizedPos * usableWidth);
            float markerY = trackRect.Y + (trackSize.Y * 0.5f);

            var mousePos = ImGui.GetMousePos();
            float distance = Vector2.Distance(mousePos, new Vector2(markerX, markerY));
            bool isHovered = distance <= 10.0f && isTrackHovered;

            HandleKeyframeInteraction(isHovered, keyframeId, isSelected, mousePos);

            uint color = isSelected ? 0xFFFFBB98 : 0xFFB7B7B7;
            float size = 8.5f;

            // Draw outer diamond
            Vector2 top = new Vector2(markerX, markerY - size);
            Vector2 right = new Vector2(markerX + size, markerY);
            Vector2 bottom = new Vector2(markerX, markerY + size);
            Vector2 left = new Vector2(markerX - size, markerY);

            drawList.AddQuadFilled(top, right, bottom, left, color);

            if (isSelected)
            {
                float innerSize = size * 0.55f;
                Vector2 innerTop = new Vector2(markerX, markerY - innerSize);
                Vector2 innerRight = new Vector2(markerX + innerSize, markerY);
                Vector2 innerBottom = new Vector2(markerX, markerY + innerSize);
                Vector2 innerLeft = new Vector2(markerX - innerSize, markerY);

                uint holeColor = 0xFF1A1A26;
                drawList.AddQuadFilled(innerTop, innerRight, innerBottom, innerLeft, holeColor);
            }

            drawList.AddQuad(top, right, bottom, left, 0xFF000000, 1.0f);
        }
    }

    private void HandleKeyframeInteraction(bool isHovered, SelectedKeyframe keyframeId, bool isSelected, Vector2 mousePos)
    {
        if (isHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !IsDraggingKeyframe)
        {
            GD.Print($"Keyframe clicked: {keyframeId.Property} at frame {keyframeId.Frame}");
            
            if (IsPlaying)
            {
                IsPlaying = false;
            }

            var io = ImGui.GetIO();

            if (io.KeyCtrl)
            {
                if (isSelected)
                {
                    SelectedKeyframes.Remove(keyframeId);
                    GD.Print($"Deselected keyframe. Total selected: {SelectedKeyframes.Count}");
                }
                else
                {
                    SelectedKeyframes.Add(keyframeId);
                    GD.Print($"Added to selection. Total selected: {SelectedKeyframes.Count}");
                }
            }
            else
            {
                if (!isSelected)
                {
                    SelectedKeyframes.Clear();
                    SelectedKeyframes.Add(keyframeId);
                    GD.Print($"Selected keyframe. Total selected: {SelectedKeyframes.Count}");
                }

                IsDraggingKeyframe = true;
                _dragStartMousePos = mousePos;
                _dragStartTimelineStart = TimelineStart;
                _currentDragOffset = 0;

                // CRITICAL: Capture scrubber dimensions at drag start
                _dragStartScrubberWidth = _scrubberWidth;
                _dragStartScrubberX = _scrubberPosition.X;

                _dragAnchorKeyframe = keyframeId;

                _draggedKeyframesData.Clear();
                _previewFrames.Clear();
                foreach (var kf in SelectedKeyframes)
                {
                    var kfDict = GetKeyframeDictionary(kf.Property);
                    if (kfDict != null && kfDict.TryGetValue(kf.Frame, out Keyframe? keyframe))
                    {
                        _draggedKeyframesData[kf] = (kf.Frame, keyframe.Value);
                        _previewFrames[kf] = kf.Frame;
                    }
                }
            }
        }

        // right mouse button on keyframe only selects - menu is opened by HandleTimelineAreaContextMenu
        if (!isHovered || !ImGui.IsMouseClicked(ImGuiMouseButton.Right) || IsDraggingKeyframe) return;
        if (IsPlaying) IsPlaying = false;

        if (isSelected) return;
        SelectedKeyframes.Clear();
        SelectedKeyframes.Add(keyframeId);
    }

    private void HandleKeyframeBoxSelection(SelectedKeyframe keyframeId, float markerX, float markerY)
    {
        if (!_shouldApplyBoxSelection) return;

        int visibleFrames = Math.Max(1, (int)(95 / TimelineZoom));

        // Calculate the start X position based on the stored frame (not the visual position)
        float normalizedStartPos = (_dragSelectionStartFrame - TimelineStart) / (float)visibleFrames;

        var trackRect = _scrubberPosition;
        float scrollbarWidth = ImGui.GetStyle().ScrollbarSize;
        // FIX: Use _scrubberWidth - scrollbarWidth
        float usableWidth = _scrubberWidth - scrollbarWidth;

        // Calculate start X based on the original frame
        float adjustedStartX = trackRect.X + (normalizedStartPos * usableWidth);

        // Use the original stored Y
        var adjustedStart = new Vector2(adjustedStartX, _dragSelectionStart.Y);

        // Calculate selection bounds WITHOUT limiting X (allows detecting outside view)
        float selectionMinX = Math.Min(adjustedStart.X, _finalSelectionMax.X);
        float selectionMaxX = Math.Max(adjustedStart.X, _finalSelectionMax.X);
        float selectionMinY = Math.Min(adjustedStart.Y, _finalSelectionMax.Y);
        float selectionMaxY = Math.Max(adjustedStart.Y, _finalSelectionMax.Y);

        // Calculate diamond bounds of the keyframe
        float keyframeSize = 8.5f;

        float keyframeMinX = markerX - keyframeSize;
        float keyframeMaxX = markerX + keyframeSize;
        float keyframeMinY = markerY - keyframeSize;
        float keyframeMaxY = markerY + keyframeSize;

        // Check for intersection between the box selection and the keyframe diamond
        bool intersects = !(keyframeMaxX < selectionMinX ||
                           keyframeMinX > selectionMaxX ||
                           keyframeMaxY < selectionMinY ||
                           keyframeMinY > selectionMaxY);

        if (intersects) SelectedKeyframes.Add(keyframeId);
    }

    private void UpdateDragPreviewWavePassThrough()
    {
        _previewFrames.Clear();
        if (_draggedKeyframesData.Count == 0) return;

        var groupedByProperty = _draggedKeyframesData
            .GroupBy(kv => kv.Key.Property)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var propertyGroup in groupedByProperty)
        {
            var property = propertyGroup.Key;
            var keyframesInGroup = propertyGroup.Value;

            var allKeyframes = GetKeyframeDictionary(property);

            // PROTECTION BUG #6: Skip if null
            if (allKeyframes == null) continue;

            // Walls are keyframes that are NOT being dragged for this property
            var wallsSet = new HashSet<int>(
                allKeyframes.Keys.Where(frame =>
                    !_draggedKeyframesData.Keys.Any(k => k.Property == property && k.Frame == frame))
            );

            var sortedSelected = keyframesInGroup
                .OrderBy(kv => kv.Value.originalFrame)
                .ToList();

            ApplyCompressExpandMovement(sortedSelected, wallsSet, _currentDragOffset);
        }
    }

    private void ApplyCompressExpandMovement(
    List<KeyValuePair<SelectedKeyframe, (int originalFrame, float value)>> sortedSelected,
    HashSet<int> walls,
    int totalOffset)
    {
        if (sortedSelected.Count == 0) return;

        int baseFrame = sortedSelected[0].Value.originalFrame;
        var originalOffsets = new Dictionary<SelectedKeyframe, int>();

        foreach (var kvp in sortedSelected)
        {
            originalOffsets[kvp.Key] = kvp.Value.originalFrame - baseFrame;
        }

        int targetBaseFrame = baseFrame + totalOffset;

        int minOffset = originalOffsets.Values.Min();
        int maxOffset = originalOffsets.Values.Max();

        // Clamp entire group to avoid going out of bounds
        if (targetBaseFrame + minOffset < 0)
        {
            targetBaseFrame = -minOffset;
        }
        if (targetBaseFrame + maxOffset > TotalFrames)
        {
            targetBaseFrame = TotalFrames - maxOffset;
        }

        // FIX: Order by movement direction to avoid internal collisions
        bool movingForward = totalOffset > 0;
        var orderedKeyframes = movingForward
            ? sortedSelected.OrderByDescending(kvp => kvp.Value.originalFrame).ToList()
            : sortedSelected.OrderBy(kvp => kvp.Value.originalFrame).ToList();

        var usedPositions = new HashSet<int>();

        foreach (var kvp in orderedKeyframes)
        {
            var keyframe = kvp.Key;
            int originalOffset = originalOffsets[keyframe];

            int targetPosition = targetBaseFrame + originalOffset;

            // Check collision with walls or with other already processed keyframes
            if (walls.Contains(targetPosition) || usedPositions.Contains(targetPosition))
            {
                targetPosition = FindNextFreeSlot(targetPosition, walls, usedPositions, movingForward);
            }

            targetPosition = Math.Clamp(targetPosition, 0, TotalFrames);

            usedPositions.Add(targetPosition);
            _previewFrames[keyframe] = targetPosition;
        }
    }

    private int FindNextFreeSlot(int startPos, HashSet<int> walls, HashSet<int> usedPositions, bool searchForward)
    {
        int direction = searchForward ? 1 : -1;
        int maxSearchDistance = TotalFrames;

        // FIX: Start from 1 (not 0) and alternate between both directions
        for (int offset = 1; offset <= maxSearchDistance; offset++)
        {
            // Try preferred direction first
            int testPos = startPos + (offset * direction);

            if (testPos >= 0 && testPos <= TotalFrames)
            {
                if (!walls.Contains(testPos) && !usedPositions.Contains(testPos))
                {
                    return testPos;
                }
            }

            // Try opposite direction
            testPos = startPos - (offset * direction);
            if (testPos < 0 || testPos > TotalFrames) continue;
            if (!walls.Contains(testPos) && !usedPositions.Contains(testPos))
            {
                return testPos;
            }
        }

        // Fallback: If no free slot found, return original position
        // (better than returning invalid or occupied position)
        return startPos;
    }

    private void ApplyDraggedKeyframes()
    {
        if (_draggedKeyframesData.Count == 0) return;

        var keyframesToMove = new List<(SelectedKeyframe keyframe, int originalFrame, int newFrame, float value)>();

        foreach (var (keyframe, valueTuple) in _draggedKeyframesData)
        {
            var (originalFrame, value) = valueTuple;

            int newFrame = _previewFrames.GetValueOrDefault(keyframe, originalFrame);

            keyframesToMove.Add((keyframe, originalFrame, newFrame, value));
        }

        var groupedByProperty = keyframesToMove
            .GroupBy(m => m.keyframe.Property)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var move in keyframesToMove)
        {
            var keyframes = GetKeyframeDictionary(move.keyframe.Property);
            keyframes?.Remove(move.originalFrame);
        }

        var newSelection = new HashSet<SelectedKeyframe>();
        foreach (var (property, moves) in groupedByProperty)
        {
            var keyframes = GetKeyframeDictionary(property);

            // Protection BUG #6: Check null before using
            if (keyframes == null) continue;

            foreach (var move in moves)
            {
                int targetFrame = move.newFrame;

                // If keyframe already exists at target, find next available slot
                if (keyframes.ContainsKey(targetFrame))
                {
                    targetFrame = FindNextAvailableFrame(keyframes, targetFrame);
                }

                EasingMode easingMode = GetEasingModeForNewKeyframe(keyframes, targetFrame);
                keyframes[targetFrame] = new Keyframe(move.value, easingMode);

                newSelection.Add(new SelectedKeyframe
                {
                    Property = move.keyframe.Property,
                    Frame = targetFrame,
                    ObjectGuid = move.keyframe.ObjectGuid
                });
            }
        }

        SelectedKeyframes = newSelection;
        _sortedKeyframeCache.Clear();
        CalculateTotalFrames();
        ApplyKeyframesToObjects();
    }

    private int FindNextAvailableFrame(Dictionary<int, Keyframe> existingKeyframes, int startFrame)
    {
        // FIX: Search alternating left/right, prioritizing smaller distance
        for (int offset = 1; offset <= TotalFrames; offset++)
        {
            int rightFrame = startFrame + offset;
            if (rightFrame <= TotalFrames && !existingKeyframes.ContainsKey(rightFrame))
                return rightFrame;

            int leftFrame = startFrame - offset;
            if (leftFrame >= 0 && !existingKeyframes.ContainsKey(leftFrame))
                return leftFrame;
        }

        // Fallback: return original frame (shouldn't happen)
        return startFrame;
    }

    private Dictionary<int, Keyframe>? GetKeyframeDictionary(string property)
    {
        if (CurrentObject == null) return null;

        return property.ToLower() switch
        {
            "position.x" => CurrentObject.PosXKeyframes,
            "position.y" => CurrentObject.PosYKeyframes,
            "position.z" => CurrentObject.PosZKeyframes,
            "rotation.x" => CurrentObject.RotXKeyframes,
            "rotation.y" => CurrentObject.RotYKeyframes,
            "rotation.z" => CurrentObject.RotZKeyframes,
            "scale.x" => CurrentObject.ScaleXKeyframes,
            "scale.y" => CurrentObject.ScaleYKeyframes,
            "scale.z" => CurrentObject.ScaleZKeyframes,
            "alpha" => CurrentObject.AlphaKeyframes,
            _ => null
        };
    }

    private void RenderFrameScrubber(int visibleFrames)
    {
        int scrubberEnd = Math.Min(TimelineStart + visibleFrames, TotalFrames);

        var barSize = new Vector2(-1, 18);
        ImGui.ProgressBar(0.0f, barSize, "");

        var itemRect = ImGui.GetItemRectMin();
        var itemSize = ImGui.GetItemRectSize();

        _scrubberPosition = itemRect;

        // The usable width is itemSize.X (which comes from BeginChild with trackWidth)
        // But we need to consider that tracks have vertical scrollbar
        _scrubberWidth = itemSize.X;

        bool isScrubberHovered = ImGui.IsItemHovered();

        // Draw region BEFORE processing interactions
        bool isOverRegionHandle = false;
        bool isOverRegionBody = false;
        RegionHandleType hoveredHandle = RegionHandleType.None;

        if (_regionActive)
        {
            var (isOverHandle, isOverBody, handleType) = DrawRegionWithHandles(itemRect, itemSize, visibleFrames);
            isOverRegionHandle = isOverHandle;
            isOverRegionBody = isOverBody;
            hoveredHandle = handleType;
        }

        // CRITICAL: Check if any popup is open (marker or context) or blocking UI is visible
        bool isAnyPopupOpen = _editingMarker != null ||
                              ImGui.IsPopupOpen("KeyframeContextMenu") ||
                              _markers.Any(m => ImGui.IsPopupOpen($"MarkerEdit_{m.Id}")) ||
                              ImGui.IsPopupOpen("MarkerColorPicker");
        
        bool isBlockingUIVisible = Main.GetInstance()?.UI?.IsBlockingUIVisible == true;

        // PROCESS REGION INTERACTION (even if mouse is not over Scrubber)
        if (_regionActive && !_isCreatingRegion && !isAnyPopupOpen && !isBlockingUIVisible)
        {
            HandleRegionInteraction(itemRect, itemSize, visibleFrames, isOverRegionHandle, isOverRegionBody, hoveredHandle, isScrubberHovered);
        }

        // Important: Check if mouse is over any marker label
        var mousePos = ImGui.GetMousePos();
        bool isMouseOverMarkerLabel = IsMouseOverAnyMarkerLabel(mousePos);

        // Create new region with right mouse button
        if (isScrubberHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Right) && !IsDraggingKeyframe && !isAnyPopupOpen && !isBlockingUIVisible)
        {
            bool clickedOnExistingRegion = _regionActive && (isOverRegionHandle || isOverRegionBody);

            if (!clickedOnExistingRegion)
            {
                _regionActive = false;
                _loopInRegion = false;

                _isCreatingRegion = true;
                _regionStartPos = ImGui.GetMousePos();
                _regionEndPos = _regionStartPos;

                float relativeX = (_regionStartPos.X - itemRect.X) / itemSize.X;
                float frameFloat = relativeX * visibleFrames;
                _regionStartFrame = TimelineStart + (int)Math.Round(frameFloat);
                _regionStartFrame = Math.Clamp(_regionStartFrame, 0, TotalFrames);
                _regionEndFrame = _regionStartFrame;
            }
        }

        if (_isCreatingRegion && ImGui.IsMouseDown(ImGuiMouseButton.Right))
        {
            _regionEndPos = ImGui.GetMousePos();

            float relativeX = (_regionEndPos.X - itemRect.X) / itemSize.X;
            float frameFloat = relativeX * visibleFrames;
            _regionEndFrame = TimelineStart + (int)Math.Round(frameFloat);
            _regionEndFrame = Math.Clamp(_regionEndFrame, 0, TotalFrames);

            if (Math.Abs(_regionEndPos.X - _regionStartPos.X) > 5.0f)
            {
                _regionActive = true;
            }

            // Auto-scroll during region creation
            HandleRegionCreationAutoScroll(itemRect, itemSize, visibleFrames);
        }

        if (_isCreatingRegion && !ImGui.IsMouseDown(ImGuiMouseButton.Right))
        {
            _isCreatingRegion = false;

            if (Math.Abs(_regionEndPos.X - _regionStartPos.X) <= 5.0f)
            {
                _regionActive = false;
                _loopInRegion = false;
            }
            else
            {
                if (_regionStartFrame > _regionEndFrame)
                {
                    (_regionStartFrame, _regionEndFrame) = (_regionEndFrame, _regionStartFrame);
                }
            }
        }

        // Original scrubber logic (only if not interacting with region, marker label, popup, OR blocking UI)
        if (!_isCreatingRegion && !_isDraggingRegion && !_isResizingRegion && !isMouseOverMarkerLabel && !isAnyPopupOpen && !isBlockingUIVisible)
        {
            // Only start scrubbing if NOT over marker label AND NO popup open AND NO blocking UI
            if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !IsDraggingKeyframe)
            {
                IsScrubbing = true;
            }

            if (IsScrubbing)
            {
                if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    var currentMousePos = ImGui.GetMousePos();
                    float leftEdge = itemRect.X;
                    float rightEdge = itemRect.X + itemSize.X;

                    float pixelsPerFrame = Math.Max(1.0f, itemSize.X / (float)Math.Max(1, visibleFrames));
                    int maxStart = Math.Max(0, TotalFrames - visibleFrames);

                    if (currentMousePos.X > rightEdge)
                    {
                        float deltaPixels = currentMousePos.X - rightEdge;
                        int frames = (int)Math.Ceiling(deltaPixels / pixelsPerFrame * SCRUBBER_SCROLL_SPEED);
                        frames = Math.Clamp(frames, 1, MAX_FRAMES_PER_TICK);

                        TimelineStart = Math.Min(maxStart, TimelineStart + frames);
                        CurrentFrame = TimelineStart + visibleFrames - 1;
                        CurrentFrameFloat = CurrentFrame;

                        _isManuallyScrolling = true;
                        _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
                    }
                    else if (currentMousePos.X < leftEdge)
                    {
                        float deltaPixels = leftEdge - currentMousePos.X;
                        int frames = (int)Math.Ceiling(deltaPixels / pixelsPerFrame * SCRUBBER_SCROLL_SPEED);
                        frames = Math.Clamp(frames, 1, MAX_FRAMES_PER_TICK);

                        TimelineStart = Math.Max(0, TimelineStart - frames);
                        CurrentFrame = TimelineStart;
                        CurrentFrameFloat = CurrentFrame;

                        _isManuallyScrolling = true;
                        _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
                    }
                    else
                    {
                        float relativeX = (currentMousePos.X - itemRect.X) / itemSize.X;
                        float frameFloat = relativeX * visibleFrames;
                        int targetFrame = TimelineStart + (int)Math.Round(frameFloat);

                        CurrentFrame = Math.Clamp(targetFrame, 0, TotalFrames);
                        CurrentFrameFloat = CurrentFrame;

                        if (currentMousePos.X > rightEdge - NUDGE_MARGIN)
                        {
                            float t = (currentMousePos.X - (rightEdge - NUDGE_MARGIN)) / NUDGE_MARGIN;
                            int frames = Math.Clamp((int)Math.Ceiling(t * 0.6f * SCRUBBER_SCROLL_SPEED), 0, MAX_FRAMES_PER_TICK);
                            if (frames > 0)
                            {
                                TimelineStart = Math.Min(maxStart, TimelineStart + frames);
                                _isManuallyScrolling = true;
                                _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
                            }
                        }
                        else if (currentMousePos.X < leftEdge + NUDGE_MARGIN)
                        {
                            float t = ((leftEdge + NUDGE_MARGIN) - currentMousePos.X) / NUDGE_MARGIN;
                            int frames = Math.Clamp((int)Math.Ceiling(t * 0.6f * SCRUBBER_SCROLL_SPEED), 0, MAX_FRAMES_PER_TICK);
                            if (frames > 0)
                            {
                                TimelineStart = Math.Max(0, TimelineStart - frames);
                                _isManuallyScrolling = true;
                                _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
                            }
                        }
                    }

                    ApplyKeyframesToObjects();
                }
                else
                {
                    IsScrubbing = false;
                }
            }
        }
        else if (isAnyPopupOpen)
        {
            // If a popup is open, cancel any active scrubbing
            IsScrubbing = false;
        }

        // Auto-scroll for region being dragged/resized
        if (!isAnyPopupOpen && !isBlockingUIVisible)
        {
            HandleRegionDragAutoScroll(itemRect, itemSize, visibleFrames);
        }

        RenderFrameLabelsAndGrid(itemRect, itemSize, visibleFrames, scrubberEnd);
    }

    private void HandleRegionCreationAutoScroll(Vector2 trackRect, Vector2 trackSize, int visibleFrames)
    {
        if (!_isCreatingRegion || !ImGui.IsMouseDown(ImGuiMouseButton.Right))
            return;

        var mousePos = ImGui.GetMousePos();
        float leftEdge = trackRect.X;
        float rightEdge = trackRect.X + trackSize.X;
        float mouseX = mousePos.X;
        int maxStart = Math.Max(0, TotalFrames - visibleFrames);

        float deltaPixelsRight = Math.Max(0f, mouseX - rightEdge);
        float deltaPixelsLeft = Math.Max(0f, leftEdge - mouseX);

        bool isMouseBeyondRight = deltaPixelsRight > 0;
        bool isMouseBeyondLeft = deltaPixelsLeft > 0;

        // Base speed proportional to zoom
        float baseScrollSpeed = Math.Max(1.0f, visibleFrames / 100.0f);

        if (isMouseBeyondRight)
        {
            float intensity = Math.Clamp(deltaPixelsRight / DRAG_SCROLL_MAX_DISTANCE, 0.0f, 1.0f);
            float speed = (DRAG_SCROLL_MIN_SPEED + (DRAG_SCROLL_MAX_SPEED - DRAG_SCROLL_MIN_SPEED) * intensity) * baseScrollSpeed;
            int frames = (int)Math.Ceiling(speed);
            TimelineStart = Math.Min(maxStart, TimelineStart + frames);

            _isManuallyScrolling = true;
            _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
        }
        else if (isMouseBeyondLeft)
        {
            float intensity = Math.Clamp(deltaPixelsLeft / DRAG_SCROLL_MAX_DISTANCE, 0.0f, 1.0f);
            float speed = (DRAG_SCROLL_MIN_SPEED + (DRAG_SCROLL_MAX_SPEED - DRAG_SCROLL_MIN_SPEED) * intensity) * baseScrollSpeed;
            int frames = (int)Math.Ceiling(speed);
            TimelineStart = Math.Max(0, TimelineStart - frames);

            _isManuallyScrolling = true;
            _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
        }
        else
        {
            // Nudge at edges
            if (mouseX > rightEdge - SCROLL_MARGIN)
            {
                float t = (mouseX - (rightEdge - SCROLL_MARGIN)) / SCROLL_MARGIN;
                float nudgeSpeed = t * t * 0.5f * baseScrollSpeed;

                if (!(nudgeSpeed >= 0.3f)) return;
                int frames = Math.Max(1, (int)Math.Ceiling(nudgeSpeed));
                TimelineStart = Math.Min(maxStart, TimelineStart + frames);
                _isManuallyScrolling = true;
                _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
            }
            else if (mouseX < leftEdge + SCROLL_MARGIN)
            {
                float t = ((leftEdge + SCROLL_MARGIN) - mouseX) / SCROLL_MARGIN;
                float nudgeSpeed = t * t * 0.5f * baseScrollSpeed;

                if (!(nudgeSpeed >= 0.3f)) return;
                int frames = Math.Max(1, (int)Math.Ceiling(nudgeSpeed));
                TimelineStart = Math.Max(0, TimelineStart - frames);
                _isManuallyScrolling = true;
                _manualScrollTimer = MANUAL_SCROLL_COOLDOWN;
            }
        }
    }

    private void RenderFrameLabelsAndGrid(Vector2 itemRect, Vector2 itemSize, int visibleFrames, int scrubberEnd)
    {
        var drawList = ImGui.GetWindowDrawList();

        const float minMinorPx = 8f;
        const float minMediumPx = 24f;
        const float minMajorPx = 80f;

        int[] possibleSteps = { 5, 10, 25, 50, 100, 250, 500, 1000, 2500, 5000 };

        // Use TOTAL Scrubber width for calculations
        float usableWidth = _scrubberWidth;
        float pxPerFrame = visibleFrames > 0 ? usableWidth / visibleFrames : 1f;

        if (Math.Abs(pxPerFrame - _lastCalculatedPxPerFrame) > 0.01f || visibleFrames != _lastCalculatedVisibleFrames)
        {
            _lastCalculatedPxPerFrame = pxPerFrame;
            _lastCalculatedVisibleFrames = visibleFrames;

            _cachedMinorStep = 5;
            foreach (var step in possibleSteps)
            {
                if (!(pxPerFrame * step >= minMinorPx)) continue;
                _cachedMinorStep = step;
                break;
            }

            _cachedMediumStep = _cachedMinorStep;
            foreach (var step in possibleSteps)
            {
                if (step < _cachedMinorStep || !(pxPerFrame * step >= minMediumPx)) continue;
                _cachedMediumStep = step;
                break;
            }

            _cachedMajorStep = _cachedMediumStep;
            foreach (var step in possibleSteps)
            {
                if (step < _cachedMediumStep || !(pxPerFrame * step >= minMajorPx)) continue;
                _cachedMajorStep = step;
                break;
            }

            if (_cachedMajorStep < _cachedMediumStep)
                _cachedMajorStep = _cachedMediumStep;
            if (_cachedMediumStep < _cachedMinorStep)
                _cachedMediumStep = _cachedMinorStep;
        }

        int minorStep = _cachedMinorStep;
        int mediumStep = _cachedMediumStep;
        int majorStep = _cachedMajorStep;

        float minorTickHeight = itemSize.Y * 0.20f;
        float mediumTickHeight = itemSize.Y * 0.35f;
        float majorTickHeight = itemSize.Y * 0.55f;

        uint minorTickColor = 0x40FFFFFF;
        uint mediumTickColor = 0x80FFFFFF;
        uint majorTickColor = 0xFFFFFFFF;
        uint gridLineColor = 0x15FFFFFF;
        uint textColor = 0xFFFFFFFF;

        int startFrame = (TimelineStart / minorStep) * minorStep;

        for (int frame = startFrame; frame <= scrubberEnd; frame += minorStep)
        {
            if (frame < 0) continue;
            if (frame < TimelineStart) continue;

            float progress = (frame - TimelineStart) / (float)visibleFrames;

            // Use total width to position ticks
            float tickX = itemRect.X + (progress * usableWidth);

            if (tickX < itemRect.X - 5 || tickX > itemRect.X + usableWidth + 5)
                continue;

            bool isMajor = (majorStep > 0) && (frame % majorStep == 0);
            bool isMedium = !isMajor && (mediumStep > 0) && (frame % mediumStep == 0);

            float tickHeight;
            uint tickColor;

            if (isMajor)
            {
                tickHeight = majorTickHeight;
                tickColor = majorTickColor;

                string label = frame.ToString();
                var textSize = ImGui.CalcTextSize(label);
                float labelX = tickX - (textSize.X * 0.5f);
                float labelY = itemRect.Y + 2;

                drawList.AddText(new Vector2(labelX, labelY), textColor, label);

                float gridLineTopY = itemRect.Y + itemSize.Y;
                float gridLineBottomY = itemRect.Y + itemSize.Y + (10 * (18 + ImGui.GetStyle().ItemSpacing.Y));
                drawList.AddLine(new Vector2(tickX, gridLineTopY), new Vector2(tickX, gridLineBottomY), gridLineColor, 1.0f);
            }
            else if (isMedium)
            {
                tickHeight = mediumTickHeight;
                tickColor = mediumTickColor;

                float gridLineTopY = itemRect.Y + itemSize.Y;
                float gridLineBottomY = itemRect.Y + itemSize.Y + (10 * (18 + ImGui.GetStyle().ItemSpacing.Y));
                drawList.AddLine(new Vector2(tickX, gridLineTopY), new Vector2(tickX, gridLineBottomY), gridLineColor & 0x0AFFFFFF, 1.0f);
            }
            else
            {
                tickHeight = minorTickHeight;
                tickColor = minorTickColor;
            }

            float tickTopY = itemRect.Y + itemSize.Y - tickHeight;
            float tickBottomY = itemRect.Y + itemSize.Y;
            drawList.AddLine(new Vector2(tickX, tickTopY), new Vector2(tickX, tickBottomY), tickColor, 1.0f);
        }

        // Draw border using the total width
        drawList.AddRect(itemRect, new Vector2(itemRect.X + usableWidth, itemRect.Y + itemSize.Y), 0x50FFFFFF, 0.0f, ImDrawFlags.None, 1.0f);
    }

    public void AddKeyframe(string property, int frame, float value)
    {
        if (CurrentObject == null) return;

        var keyframes = GetKeyframeDictionary(property);
        if (keyframes == null)
        {
            // Silent log: invalid property
            return;
        }

        var selectedForProperty = SelectedKeyframes.FirstOrDefault(k =>
            k.Property == property &&
            k.ObjectGuid == Main.GetInstance()?.UI?.SceneTreePanel?.SelectedObjectGuid);

        int targetFrame = selectedForProperty?.Frame ?? frame;
        EasingMode easingMode = GetEasingModeForNewKeyframe(keyframes, targetFrame);
        keyframes[targetFrame] = new Keyframe(value, easingMode);

        _sortedKeyframeCache.Clear();
        CalculateTotalFrames();
    }

    public void RemoveKeyframe(string property, int frame)
    {
        if (CurrentObject == null) return;

        var keyframes = GetKeyframeDictionary(property);
        if (keyframes == null) return; // PROTECTION: do not crash

        keyframes.Remove(frame);

        _sortedKeyframeCache.Clear();
        CalculateTotalFrames();
    }

    public void CalculateTotalFrames()
    {
        int maxFrame = 5000;
        var instance = Main.GetInstance();
        if (instance?.UI?.SceneTreePanel?.SceneObjects != null)
        {
            foreach (var obj in instance.UI.SceneTreePanel.SceneObjects.Values)
            {
                maxFrame = Math.Max(maxFrame, GetMaxFrameFromKeyframes(obj.PosXKeyframes));
                maxFrame = Math.Max(maxFrame, GetMaxFrameFromKeyframes(obj.PosYKeyframes));
                maxFrame = Math.Max(maxFrame, GetMaxFrameFromKeyframes(obj.PosZKeyframes));
                maxFrame = Math.Max(maxFrame, GetMaxFrameFromKeyframes(obj.RotXKeyframes));
                maxFrame = Math.Max(maxFrame, GetMaxFrameFromKeyframes(obj.RotYKeyframes));
                maxFrame = Math.Max(maxFrame, GetMaxFrameFromKeyframes(obj.RotZKeyframes));
                maxFrame = Math.Max(maxFrame, GetMaxFrameFromKeyframes(obj.ScaleXKeyframes));
                maxFrame = Math.Max(maxFrame, GetMaxFrameFromKeyframes(obj.ScaleYKeyframes));
                maxFrame = Math.Max(maxFrame, GetMaxFrameFromKeyframes(obj.ScaleZKeyframes));
                maxFrame = Math.Max(maxFrame, GetMaxFrameFromKeyframes(obj.AlphaKeyframes));
            }
        }
        TotalFrames = Math.Max(5000, maxFrame + 100);
    }

    private int GetMaxFrameFromKeyframes(Dictionary<int, Keyframe> keyframes)
    {
        return keyframes.Count > 0 ? keyframes.Keys.Max() : 0;
    }

    // NOTE: Also unused - Forest
    public void OnObjectManipulationStart()
    {
        if (CurrentObject == null) return;

        // If there are selected keyframes, start relative editing
        if (SelectedKeyframes.Count > 0)
        {
            _isEditingSelectedKeyframes = true;
            _keyframeEditStartValues.Clear();

            // Group by property for better visualization
            var groupedByProperty = SelectedKeyframes.GroupBy(k => k.Property).ToList();

            // Store initial values of selected keyframes
            int storedCount = 0;
            int notFoundCount = 0;

            foreach (var selectedKf in SelectedKeyframes)
            {
                var keyframes = GetKeyframeDictionary(selectedKf.Property);
                if (keyframes != null && keyframes.TryGetValue(selectedKf.Frame, out Keyframe? keyframe))
                {
                    _keyframeEditStartValues[selectedKf] = keyframe.Value;
                    storedCount++;
                }
                else
                {
                    notFoundCount++;
                }
            }

            // Store initial object state
            _objectEditStartPosition = CurrentObject.TargetPosition;
            _objectEditStartRotation = CurrentObject.RotationDegrees;
            _objectEditStartScale = CurrentObject.Scale;
            _objectEditStartAlpha = CurrentObject.Alpha;

        }
        else
        {
            // Original behavior: auto-keyframe during playback
            if (IsPlaying)
            {
                _wasPlayingBeforeManipulation = true;
                _manipulationStartFrame = CurrentFrame;
                IsPlaying = false;
            }
        }
    }

    // NOTE: Unused - Forest
    public void OnObjectManipulationEnd()
    {
        if (CurrentObject == null)
        {
            _isEditingSelectedKeyframes = false;
            _keyframeEditStartValues.Clear();
            _wasPlayingBeforeManipulation = false;
            _manipulationStartFrame = -1;
            return;
        }

        // If editing selected keyframes, apply relative changes
        if (_isEditingSelectedKeyframes)
        {
            // CRITICAL: Make a COPY of SelectedKeyframes to avoid modifications during iteration
            var selectedKeyframesCopy = new List<SelectedKeyframe>(SelectedKeyframes);

            // Calculate transformation deltas (differences) - ALL ADDITIVE
            Vector3 positionDelta = CurrentObject.TargetPosition - _objectEditStartPosition;
            Vector3 rotationDelta = CurrentObject.RotationDegrees - _objectEditStartRotation;
            Vector3 scaleDelta = CurrentObject.Scale - _objectEditStartScale;
            float alphaDelta = CurrentObject.Alpha - _objectEditStartAlpha;

            // Threshold for significant change
            const float EPSILON = 0.0001f;

            // Counter for Debug
            int updatedCount = 0;
            int skippedCount = 0;
            int errorCount = 0;

            // CRITICAL: Iterate over COPY, not SelectedKeyframes directly
            foreach (var selectedKf in selectedKeyframesCopy)
            {
                if (!_keyframeEditStartValues.ContainsKey(selectedKf))
                {
                    errorCount++;
                    continue;
                }

                var keyframes = GetKeyframeDictionary(selectedKf.Property);
                if (keyframes == null)
                {
                    errorCount++;
                    continue;
                }

                // CRITICAL: Check if keyframe still exists in dictionary
                if (!keyframes.ContainsKey(selectedKf.Frame))
                {
                    errorCount++;
                    continue;
                }

                float originalValue = _keyframeEditStartValues[selectedKf];

                // CRITICAL: Check CURRENT value in dictionary BEFORE modifying
                float currentValueInDict = keyframes[selectedKf.Frame].Value;

                // Get appropriate delta for this property
                float delta = GetDeltaForProperty(
                    selectedKf.Property,
                    positionDelta,
                    rotationDelta,
                    scaleDelta,
                    alphaDelta
                );

                // ALL properties use addition (including scale)
                float newValue = originalValue + delta;

                // Only apply if delta is significant
                if (Math.Abs(delta) > EPSILON)
                {
                    // CRITICAL: Assign directly to dictionary, preserving existing easing mode
                    var existingKeyframe = keyframes[selectedKf.Frame];
                    keyframes[selectedKf.Frame] = new Keyframe(newValue, existingKeyframe.EasingMode);
                    updatedCount++;

                    // CRITICAL: Check IMMEDIATELY if really updated
                    if (keyframes.TryGetValue(selectedKf.Frame, out Keyframe? verifyKeyframe))
                    {
                        // Verification successful
                    }
                    else
                    {
                        // Keyframe disappeared after update
                    }
                }
                else
                {
                    skippedCount++;
                }
            }

            // Restore object to initial state (keyframes already updated)
            CurrentObject.TargetPosition = _objectEditStartPosition;
            CurrentObject.RotationDegrees = _objectEditStartRotation;
            CurrentObject.Scale = _objectEditStartScale;
            CurrentObject.Alpha = _objectEditStartAlpha;

            _isEditingSelectedKeyframes = false;
            _keyframeEditStartValues.Clear();

            // CRITICAL: clear cache BEFORE applying
            _sortedKeyframeCache.Clear();

            ApplyKeyframesToObjects();

            return;
        }

        // Original behavior: auto-keyframe
        if (!_wasPlayingBeforeManipulation || CurrentObject == null)
        {
            _wasPlayingBeforeManipulation = false;
            _manipulationStartFrame = -1;
            return;
        }

        _wasPlayingBeforeManipulation = false;

        int targetFrame = CurrentFrame;
        int keyframesCreated = 0;
        Guid? selectedObjectGuid = Main.GetInstance()?.UI?.SceneTreePanel?.SelectedObjectGuid;

        SelectedKeyframes.Clear();

        bool hasAnyKeyframes = false;
        try
        {
            hasAnyKeyframes = PropertyAccessors.Any(p => p.Item2(CurrentObject).Count > 0);
        }
        catch (NullReferenceException)
        {
            _manipulationStartFrame = -1;
            return;
        }

        foreach (var (propertyName, accessor) in PropertyAccessors)
        {
            if (CurrentObject == null)
            {
                break;
            }

            Dictionary<int, Keyframe>? keyframeDict = null;
            try
            {
                keyframeDict = accessor(CurrentObject);
            }
            catch (NullReferenceException)
            {
                continue;
            }

            if (keyframeDict == null)
            {
                continue;
            }

            if (keyframeDict.Count > 0 || !hasAnyKeyframes)
            {
                float currentValue = GetCurrentPropertyValue(propertyName);
                EasingMode easingMode = GetEasingModeForNewKeyframe(keyframeDict, targetFrame);
                keyframeDict[targetFrame] = new Keyframe(currentValue, easingMode);
                keyframesCreated++;

                SelectedKeyframes.Add(new SelectedKeyframe
                {
                    Property = propertyName,
                    Frame = targetFrame,
                    ObjectGuid = selectedObjectGuid
                });
            }
        }

        if (keyframesCreated > 0)
        {
            _sortedKeyframeCache.Clear();
            CalculateTotalFrames();
            ApplyKeyframesToObjects();
        }

        _manipulationStartFrame = -1;
    }

    private float GetDeltaForProperty(
     string property,
     Vector3 posDelta,
     Vector3 rotDelta,
     Vector3 scaleDelta,
     float alphaDelta)
    {
        // Normalize to lowercase to avoid case-sensitivity issues
        string propLower = property.ToLower();

        return propLower switch
        {
            "position.x" => posDelta.X,
            "position.y" => posDelta.Y,
            "position.z" => posDelta.Z,
            "rotation.x" => rotDelta.X,
            "rotation.y" => rotDelta.Y,
            "rotation.z" => rotDelta.Z,
            "scale.x" => scaleDelta.X,
            "scale.y" => scaleDelta.Y,
            "scale.z" => scaleDelta.Z,
            "alpha" => alphaDelta,
            _ => 0f
        };
    }

    // NOTE: Unused - Forest
    public float EvaluateKeyframes(string property, int frame)
    {
        var keyframes = GetKeyframeDictionary(property);
        if (keyframes == null || keyframes.Count == 0)
        {
            return property.ToLower() switch
            {
                var p when p.StartsWith("scale") => 1.0f,
                var p when p.StartsWith("alpha") => 1.0f,
                _ => 0.0f
            };
        }

        float defaultValue = property.ToLower().StartsWith("scale") || property.ToLower().StartsWith("alpha") ? 1.0f : 0.0f;
        return EvaluateKeyframesWithDefault(keyframes, (float)frame, defaultValue);
    }

    public Vector3 GetAnimatedPosition(SceneObject obj)
    {
        float frameToUse = IsPlaying ? CurrentFrameFloat : CurrentFrame;
        return new Vector3(
            EvaluateKeyframesWithDefault(obj.PosXKeyframes, frameToUse, 0.0f),
            EvaluateKeyframesWithDefault(obj.PosYKeyframes, frameToUse, 0.0f),
            EvaluateKeyframesWithDefault(obj.PosZKeyframes, frameToUse, 0.0f)
        );
    }

    public Vector3 GetAnimatedRotation(SceneObject obj)
    {
        float frameToUse = IsPlaying ? CurrentFrameFloat : CurrentFrame;
        return new Vector3(
            EvaluateKeyframesWithDefault(obj.RotXKeyframes, frameToUse, 0.0f),
            EvaluateKeyframesWithDefault(obj.RotYKeyframes, frameToUse, 0.0f),
            EvaluateKeyframesWithDefault(obj.RotZKeyframes, frameToUse, 0.0f)
        );
    }

    public Vector3 GetAnimatedScale(SceneObject obj)
    {
        float frameToUse = IsPlaying ? CurrentFrameFloat : CurrentFrame;
        return new Vector3(
            EvaluateKeyframesWithDefault(obj.ScaleXKeyframes, frameToUse, 1.0f),
            EvaluateKeyframesWithDefault(obj.ScaleYKeyframes, frameToUse, 1.0f),
            EvaluateKeyframesWithDefault(obj.ScaleZKeyframes, frameToUse, 1.0f)
        );
    }

    public float GetAnimatedAlpha(SceneObject obj)
    {
        float frameToUse = IsPlaying ? CurrentFrameFloat : CurrentFrame;
        return EvaluateKeyframesWithDefault(obj.AlphaKeyframes, frameToUse, obj.Alpha);
    }

    // NOTE: Unused - Forest
    public Vector3 GetAnimatedPosition()
    {
        if (CurrentObject == null) return Vector3.Zero;
        return GetAnimatedPosition(CurrentObject);
    }

    // NOTE: Unused - Forest
    public Vector3 GetAnimatedRotation()
    {
        if (CurrentObject == null) return Vector3.Zero;
        return GetAnimatedRotation(CurrentObject);
    }

    // NOTE: Unused - Forest
    public Vector3 GetAnimatedScale()
    {
        if (CurrentObject == null) return Vector3.One;
        return GetAnimatedScale(CurrentObject);
    }

    // NOTE: Unused - Forest
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
        return CurrentObject != null && HasKeyframes(CurrentObject);
    }

    public bool HasSelectedKeyframe()
    {
        return SelectedKeyframes.Count > 0;
    }

    public void DeleteSelectedKeyframes()
    {
        if (SelectedKeyframes.Count == 0 || CurrentObject == null) return;

        _tempKeyframeList.Clear();
        _tempKeyframeList.AddRange(SelectedKeyframes);

        foreach (var keyframe in _tempKeyframeList)
        {
            RemoveKeyframe(keyframe.Property, keyframe.Frame);
        }

        SelectedKeyframes.Clear();
    }

    /// <summary>
    /// Get the easing mode to use for a new keyframe, inheriting from previous keyframe if available
    /// </summary>
    private EasingMode GetEasingModeForNewKeyframe(Dictionary<int, Keyframe> keyframes, int newFrame)
    {
        if (keyframes.Count == 0)
            return EasingMode.Linear;
        
        // Find all keyframes before this one
        var previousKeyframes = keyframes.Keys.Where(f => f < newFrame).ToList();
        
        if (previousKeyframes.Count == 0)
            return EasingMode.Linear;
        
        // Get the most recent one
        int previousFrame = previousKeyframes.Max();
        
        if (keyframes.TryGetValue(previousFrame, out Keyframe? previousKeyframe))
        {
            GD.Print($"Inheriting easing mode {previousKeyframe.EasingMode} from frame {previousFrame} for new keyframe at {newFrame}");
            return previousKeyframe.EasingMode;
        }
        
        return EasingMode.Linear;
    }
    
    public void SetEasingModeForSelectedKeyframes(EasingMode easingMode)
    {
        if (!HasSelectedKeyframe() || CurrentObject == null)
            return;

        foreach (var selectedKf in SelectedKeyframes)
        {
            var keyframes = GetKeyframeDictionary(selectedKf.Property);
            if (keyframes != null && keyframes.TryGetValue(selectedKf.Frame, out Keyframe? keyframe))
            {
                // Update the keyframe with the new easing mode
                keyframes[selectedKf.Frame] = new Keyframe(keyframe.Value, easingMode);
            }
        }

        _sortedKeyframeCache.Clear();
        ApplyKeyframesToObjects();
    }

    private void RenderContextMenuItems()
    {
        IsScrubbing = false;
        IsDraggingKeyframe = false;
        _isDragSelecting = false;
        _isPendingScrubberMove = false;

        bool canCopy = HasSelectedKeyframe();

        if (!canCopy)
            ImGui.BeginDisabled();

        if (ImGui.MenuItem("Copy Keyframes", "Ctrl+C"))
        {
            CopySelectedKeyframes();
        }

        if (!canCopy)
            ImGui.EndDisabled();

        if (!canCopy)
            ImGui.BeginDisabled();

        if (ImGui.MenuItem("Cut Keyframes", "Ctrl+X"))
        {
            CutSelectedKeyframes();
        }

        if (!canCopy)
            ImGui.EndDisabled();

        bool canPaste = _copiedKeyframes.Count > 0;

        if (!canPaste)
            ImGui.BeginDisabled();

        if (ImGui.MenuItem("Paste Keyframes", "Ctrl+V"))
        {
            PasteKeyframes(CurrentFrame);
        }

        if (!canPaste)
            ImGui.EndDisabled();

        if (!canCopy)
            ImGui.BeginDisabled();

        if (ImGui.MenuItem("Delete Keyframes", "Delete"))
        {
            DeleteSelectedKeyframes();
        }

        if (!canCopy)
            ImGui.EndDisabled();

        ImGui.Separator();

        // Easing Mode submenu
        if (!canCopy)
            ImGui.BeginDisabled();

        if (ImGui.BeginMenu("Easing Mode"))
        {
            foreach (var easingMode in EasingFunctions.GetAllEasingModes())
            {
                string modeName = EasingFunctions.GetEasingModeName(easingMode);
                if (ImGui.MenuItem(modeName))
                {
                    SetEasingModeForSelectedKeyframes(easingMode);
                }
            }
            ImGui.EndMenu();
        }

        if (!canCopy)
            ImGui.EndDisabled();

        ImGui.Separator();

        bool hasObject = CurrentObject != null && HasKeyframes();

        if (!hasObject)
            ImGui.BeginDisabled();

        if (ImGui.BeginMenu("Select Keyframes..."))
        {
            if (ImGui.MenuItem("Before Marker"))
            {
                SelectKeyframesBeforeMarker();
            }

            if (ImGui.MenuItem("After Marker"))
            {
                SelectKeyframesAfterMarker();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("First in Timeline"))
            {
                SelectFirstKeyframes();
            }

            if (ImGui.MenuItem("Last in Timeline"))
            {
                SelectLastKeyframes();
            }

            ImGui.Separator();

            bool hasActiveRegion = _regionActive;

            if (!hasActiveRegion)
                ImGui.BeginDisabled();

            if (ImGui.MenuItem("In Selected Region"))
            {
                SelectKeyframesInRegion();
            }

            if (!hasActiveRegion)
                ImGui.EndDisabled();

            ImGui.EndMenu();
        }

        if (!hasObject)
            ImGui.EndDisabled();

        ImGui.Separator();

        if (ImGui.MenuItem("Add Marker"))
        {
            AddMarkerAtCurrentFrame();
        }
    }

    private void SelectKeyframesBeforeMarker()
    {
        if (CurrentObject == null) return;

        if (IsPlaying)
        {
            IsPlaying = false;
        }

        SelectedKeyframes.Clear();
        Guid? selectedObjectGuid = Main.GetInstance()?.UI?.SceneTreePanel?.SelectedObjectGuid;

        foreach (var (property, accessor) in PropertyAccessors)
        {
            var keyframes = accessor(CurrentObject);
            foreach (var frame in keyframes.Keys.Where(frame => frame < CurrentFrame))
            {
                SelectedKeyframes.Add(new SelectedKeyframe
                {
                    Property = property,
                    Frame = frame,
                    ObjectGuid = selectedObjectGuid
                });
            }
        }
    }

    private void SelectKeyframesAfterMarker()
    {
        if (CurrentObject == null) return;

        if (IsPlaying) IsPlaying = false;
        
        SelectedKeyframes.Clear();
        Guid? selectedObjectGuid = Main.GetInstance()?.UI?.SceneTreePanel?.SelectedObjectGuid;

        foreach (var (property, accessor) in PropertyAccessors)
        {
            var keyframes = accessor(CurrentObject);
            foreach (var frame in keyframes.Keys.Where(frame => frame > CurrentFrame))
            {
                SelectedKeyframes.Add(new SelectedKeyframe
                {
                    Property = property,
                    Frame = frame,
                    ObjectGuid = selectedObjectGuid
                });
            }
        }
    }

    private void SelectFirstKeyframes()
    {
        if (CurrentObject == null) return;

        if (IsPlaying)
        {
            IsPlaying = false;
        }

        SelectedKeyframes.Clear();
        Guid? selectedObjectGuid = Main.GetInstance()?.UI?.SceneTreePanel?.SelectedObjectGuid;

        foreach (var (property, accessor) in PropertyAccessors)
        {
            var keyframes = accessor(CurrentObject);
            if (keyframes.Count <= 0) continue;
            int firstFrame = keyframes.Keys.Min();
            SelectedKeyframes.Add(new SelectedKeyframe
            {
                Property = property,
                Frame = firstFrame,
                ObjectGuid = selectedObjectGuid
            });
        }
    }

    private void SelectLastKeyframes()
    {
        if (CurrentObject == null) return;

        if (IsPlaying)
        {
            IsPlaying = false;
        }

        SelectedKeyframes.Clear();
        Guid? selectedObjectGuid = Main.GetInstance()?.UI?.SceneTreePanel?.SelectedObjectGuid;

        foreach (var (property, accessor) in PropertyAccessors)
        {
            var keyframes = accessor(CurrentObject);
            if (keyframes.Count <= 0) continue;
            int lastFrame = keyframes.Keys.Max();
            SelectedKeyframes.Add(new SelectedKeyframe
            {
                Property = property,
                Frame = lastFrame,
                ObjectGuid = selectedObjectGuid
            });
        }
    }

    private void SelectKeyframesInRegion()
    {
        if (CurrentObject == null) return;

        if (!_regionActive) return;

        if (IsPlaying) IsPlaying = false;
        
        SelectedKeyframes.Clear();

        Guid? selectedObjectGuid = Main.GetInstance()?.UI?.SceneTreePanel?.SelectedObjectGuid;

        int minFrame = Math.Min(_regionStartFrame, _regionEndFrame);
        int maxFrame = Math.Max(_regionStartFrame, _regionEndFrame);

        foreach (var (property, accessor) in PropertyAccessors)
        {
            var keyframes = accessor(CurrentObject);
            foreach (var frame in keyframes.Keys.Where(frame => frame >= minFrame && frame <= maxFrame))
            {
                SelectedKeyframes.Add(new SelectedKeyframe
                {
                    Property = property,
                    Frame = frame,
                    ObjectGuid = selectedObjectGuid
                });
            }
        }
    }

    private void CopySelectedKeyframes()
    {
        if (!HasSelectedKeyframe() || CurrentObject == null)
            return;

        _copiedKeyframes.Clear();
        _isCutOperation = false;

        _copiedKeyframesBaseFrame = SelectedKeyframes.Min(k => k.Frame);

        foreach (var selectedKf in SelectedKeyframes)
        {
            var keyframes = GetKeyframeDictionary(selectedKf.Property);
            if (keyframes != null && keyframes.TryGetValue(selectedKf.Frame, out Keyframe? keyframe))
            {
                _copiedKeyframes.Add((selectedKf, keyframe.Value));
            }
        }
    }

    private void CutSelectedKeyframes()
    {
        if (!HasSelectedKeyframe() || CurrentObject == null)
            return;

        _copiedKeyframes.Clear();
        _isCutOperation = true;

        _copiedKeyframesBaseFrame = SelectedKeyframes.Min(k => k.Frame);

        foreach (var selectedKf in SelectedKeyframes)
        {
            var keyframes = GetKeyframeDictionary(selectedKf.Property);
            if (keyframes != null && keyframes.TryGetValue(selectedKf.Frame, out Keyframe? keyframe))
            {
                _copiedKeyframes.Add((selectedKf, keyframe.Value));
            }
        }

        _tempKeyframeList.Clear();
        _tempKeyframeList.AddRange(SelectedKeyframes);

        foreach (var keyframe in _tempKeyframeList)
        {
            RemoveKeyframe(keyframe.Property, keyframe.Frame);
        }

        SelectedKeyframes.Clear();

        _sortedKeyframeCache.Clear();
        CalculateTotalFrames();
        ApplyKeyframesToObjects();
    }

    private void PasteKeyframes(int targetBaseFrame)
    {
        if (_copiedKeyframes.Count == 0 || CurrentObject == null)
            return;

        int frameOffset = targetBaseFrame - _copiedKeyframesBaseFrame;

        SelectedKeyframes.Clear();

        foreach (var (copiedKf, value) in _copiedKeyframes)
        {
            int newFrame = copiedKf.Frame + frameOffset;

            if (newFrame < 0 || newFrame > TotalFrames)
                continue;

            var keyframes = GetKeyframeDictionary(copiedKf.Property);

            if (keyframes == null) continue;
            EasingMode easingMode = GetEasingModeForNewKeyframe(keyframes, newFrame);
            keyframes[newFrame] = new Keyframe(value, easingMode);

            SelectedKeyframes.Add(new SelectedKeyframe
            {
                Property = copiedKf.Property,
                Frame = newFrame,
                ObjectGuid = Main.GetInstance()?.UI?.SceneTreePanel?.SelectedObjectGuid
            });
        }

        _sortedKeyframeCache.Clear();
        CalculateTotalFrames();
        ApplyKeyframesToObjects();

        if (!_isCutOperation) return;
        _copiedKeyframes.Clear();
        _isCutOperation = false;
    }

    private static float GetCurrentPropertyValue(string property)
    {
        if (CurrentObject == null)
            return 0f;

        string propLower = property.ToLower();

        return propLower switch
        {
            "position.x" => CurrentObject.TargetPosition.X,
            "position.y" => CurrentObject.TargetPosition.Y,
            "position.z" => CurrentObject.TargetPosition.Z,
            "rotation.x" => CurrentObject.RotationDegrees.X,
            "rotation.y" => CurrentObject.RotationDegrees.Y,
            "rotation.z" => CurrentObject.RotationDegrees.Z,
            "scale.x" => CurrentObject.Scale.X,
            "scale.y" => CurrentObject.Scale.Y,
            "scale.z" => CurrentObject.Scale.Z,
            "alpha" => CurrentObject.Alpha,
            _ => 0f
        };
    }

    private void CreateKeyframesAtCurrentFrame()
    {
        // FIX CRITICAL BUG: Check null at start
        if (CurrentObject == null) return;

        int targetFrame = CurrentFrame;
        int keyframesCreated = 0;
        Guid? selectedObjectGuid = Main.GetInstance()?.UI?.SceneTreePanel?.SelectedObjectGuid;

        SelectedKeyframes.Clear();

        // PROTECTION: Try-catch for PropertyAccessors
        bool hasAnyKeyframes = false;
        try
        {
            hasAnyKeyframes = PropertyAccessors.Any(p => p.Item2(CurrentObject).Count > 0);
        }
        catch (NullReferenceException)
        {
            // Object was destroyed during operation
            return;
        }

        foreach (var (propertyName, accessor) in PropertyAccessors)
        {
            // PROTECTION: Check object on each iteration
            // NOTE: Null checking at the beginning of the function is enough - Forest
            //if (CurrentObject == null)
            //{
            //    break;
            //}

            Dictionary<int, Keyframe>? keyframeDict = null;
            try
            {
                keyframeDict = accessor(CurrentObject);
            }
            catch (NullReferenceException)
            {
                // Object destroyed during iteration
                continue;
            }

            if (keyframeDict == null)
            {
                continue;
            }

            if (keyframeDict.Count <= 0 && hasAnyKeyframes) continue;
            float currentValue = GetCurrentPropertyValue(propertyName);
            EasingMode easingMode = GetEasingModeForNewKeyframe(keyframeDict, targetFrame);
            keyframeDict[targetFrame] = new Keyframe(currentValue, easingMode);
            keyframesCreated++;

            SelectedKeyframes.Add(new SelectedKeyframe
            {
                Property = propertyName,
                Frame = targetFrame,
                ObjectGuid = selectedObjectGuid
            });
        }

        if (keyframesCreated <= 0) return;
        _sortedKeyframeCache.Clear();
        CalculateTotalFrames();
        ApplyKeyframesToObjects();
    }

    // Methods for loop region control
    public void ClearActiveRegion()
    {
        _regionActive = false;
        _isCreatingRegion = false;
        _loopInRegion = false;
    }

    // NOTE: Unused - Forest
    public (int startFrame, int endFrame, bool isActive, bool isLooping) GetActiveRegion()
    {
        return (_regionStartFrame, _regionEndFrame, _regionActive, _loopInRegion);
    }

    // ===== MARKER METHODS =====

    private void AddMarkerAtCurrentFrame()
    {
        var newMarker = new TimelineMarker
        {
            Frame = CurrentFrame,
            Label = "New marker",
            Color = MarkerColor.Red
        };

        _markers.Add(newMarker);
        _editingMarker = newMarker;
        _markerPopupJustOpened = true;

        // Order markers by frame
        _markers = _markers.OrderBy(m => m.Frame).ToList();

        // Calculate the popup position
        int visibleFrames = Math.Max(1, (int)(95 / TimelineZoom));
        float normalizedPos = (CurrentFrame - TimelineStart) / (float)visibleFrames;
        float markerX = _scrubberPosition.X + (normalizedPos * _scrubberWidth);

        // Calculate position ABOVE the scrubber
        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();

        float popupWidth = 220f;
        float popupHeight = 180f;

        // Center on the marker
        float popupX = markerX - (popupWidth * 0.5f);
        popupX = Math.Max(windowPos.X + 10, popupX);
        popupX = Math.Min(windowPos.X + windowSize.X - popupWidth - 10, popupX);

        // Place above the scrubber
        float popupY = _scrubberPosition.Y - popupHeight - 10;

        // If it doesn't fit above, place below
        if (popupY < windowPos.Y + 50)
        {
            popupY = _scrubberPosition.Y + 30; // Below Scrubber
        }

        ImGui.SetNextWindowPos(new Vector2(popupX, popupY), ImGuiCond.Appearing);
        ImGui.OpenPopup($"MarkerEdit_{newMarker.Id}");
    }

    private string GetMarkerColorName(MarkerColor color)
    {
        return color switch
        {
            MarkerColor.Red => "Red",
            MarkerColor.Orange => "Orange",
            MarkerColor.Yellow => "Yellow",
            MarkerColor.Green => "Green",
            MarkerColor.ForestGreen => "Forest green",
            MarkerColor.Teal => "Teal",
            MarkerColor.Blue => "Blue",
            MarkerColor.Purple => "Purple",
            MarkerColor.Pink => "Pink",
            _ => "Red"
        };
    }

    private void DrawDashedMarkerLine(ImDrawListPtr drawList, float x, float startY, float endY, uint color)
    {
        float dashLength = 3.0f;
        float gapLength = 3.0f;
        float currentY = startY;

        while (currentY < endY)
        {
            float dashEnd = Math.Min(currentY + dashLength, endY);
            drawList.AddLine(new Vector2(x, currentY), new Vector2(x, dashEnd), color, 1.5f);
            currentY += dashLength + gapLength;
        }
    }

    private void RenderMarkerEditPopup(TimelineMarker marker)
    {
        // Custom colors
        var bgColor = new Vector4(0x18 / 255f, 0x1A / 255f, 0x1C / 255f, 1.0f); // #181A1C
        var borderColor = new Vector4(0x3A / 255f, 0x3C / 255f, 0x3E / 255f, 1.0f); // #3A3C3E

        // Apply theme colors
        ImGui.PushStyleColor(ImGuiCol.PopupBg, bgColor);
        ImGui.PushStyleColor(ImGuiCol.Border, borderColor);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 8));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 2));

        // Set fixed size AND disable resizing
        ImGui.SetNextWindowSize(new Vector2(220, 150), ImGuiCond.Always);

        if (ImGui.BeginPopup($"MarkerEdit_{marker.Id}", ImGuiWindowFlags.NoResize))
        {
            ImGui.Text("Label");

            // Input border color
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.15f, 0.15f, 0.15f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.20f, 0.20f, 0.20f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new Vector4(0.25f, 0.25f, 0.25f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.Border, borderColor);

            // Input field with fixed width
            ImGui.SetNextItemWidth(200);

            // Auto-focus on text field when popup opens
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }

            string label = marker.Label;
            if (ImGui.InputText("##MarkerLabel", ref label, 100))
            {
                marker.Label = label;
            }

            ImGui.PopStyleColor(4);

            ImGui.Spacing();
            ImGui.Text("Color");

            // CUSTOM BUTTON WITH COLOR SQUARE + COLOR NAME + ARROW
            var currentColor = GetMarkerColorVector(marker.Color);
            string currentColorName = GetMarkerColorName(marker.Color);

            // Draw button with colored square + name + arrow
            var buttonPos = ImGui.GetCursorScreenPos();

            // Button style
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.15f, 0.15f, 0.15f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.20f, 0.20f, 0.20f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.25f, 0.25f, 0.25f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.Border, borderColor);

            if (ImGui.Button($"##ColorPickerButton", new Vector2(200, 22)))
            {
                // Calculate popup position (open upwards, OVER main menu)
                var currentButtonPos = ImGui.GetItemRectMin();
                
                float popupHeight = 9 * 22f + 16f; // 9 colors * height + padding

                // position X: align with button
                float popupX = currentButtonPos.X;

                // position Y: above button
                float popupY = currentButtonPos.Y - popupHeight - 2;

                ImGui.SetNextWindowPos(new Vector2(popupX, popupY), ImGuiCond.Always);
                ImGui.OpenPopup("MarkerColorPicker");
            }

            ImGui.PopStyleColor(4);

            // Draw colored square + text + arrow INSIDE button
            var drawList = ImGui.GetWindowDrawList();
            var textColor = 0xFFFFFFFFu;

            // Color square (16x16 pixels)
            float squareSize = 16f;
            float marginLeft = 4f;
            float marginTop = 3f;
            var squareMin = new Vector2(buttonPos.X + marginLeft, buttonPos.Y + marginTop);
            var squareMax = new Vector2(squareMin.X + squareSize, squareMin.Y + squareSize);

            drawList.AddRectFilled(squareMin, squareMax, ImGui.ColorConvertFloat4ToU32(currentColor));
            drawList.AddRect(squareMin, squareMax, 0xFF000000u, 0.0f, ImDrawFlags.None, 1.0f);

            // Color name text
            var textPos = new Vector2(squareMax.X + 8, buttonPos.Y + 3);
            drawList.AddText(textPos, textColor, currentColorName);

            // UP ARROW (triangle) at end of button
            float arrowSize = 6f;
            float arrowX = buttonPos.X + 200 - arrowSize - 8;
            float arrowY = buttonPos.Y + 11;

            var arrowTop = new Vector2(arrowX, arrowY - 3);
            var arrowLeft = new Vector2(arrowX - arrowSize, arrowY + 3);
            var arrowRight = new Vector2(arrowX + arrowSize, arrowY + 3);

            drawList.AddTriangleFilled(arrowTop, arrowLeft, arrowRight, 0xFFAAAAAA);

            // COLOR selection POPUP (Modal to appear OVER everything)
            ImGui.PushStyleColor(ImGuiCol.PopupBg, bgColor);
            ImGui.PushStyleColor(ImGuiCol.Border, borderColor);
            ImGui.SetNextWindowSize(new Vector2(200, 0), ImGuiCond.Always);

            if (ImGui.BeginPopup("MarkerColorPicker", ImGuiWindowFlags.NoMove))
            {
                var allColors = new[]
                {
                (MarkerColor.Red, "Red"),
                (MarkerColor.Orange, "Orange"),
                (MarkerColor.Yellow, "Yellow"),
                (MarkerColor.Green, "Green"),
                (MarkerColor.ForestGreen, "Forest green"),
                (MarkerColor.Teal, "Teal"),
                (MarkerColor.Blue, "Blue"),
                (MarkerColor.Purple, "Purple"),
                (MarkerColor.Pink, "Pink")
            };

                foreach (var (color, colorName) in allColors)
                {
                    var colorVec = GetMarkerColorVector(color);

                    var itemButtonPos = ImGui.GetCursorScreenPos();

                    // Option button style
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.15f, 0.15f, 0.15f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.20f, 0.20f, 0.20f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.25f, 0.25f, 0.25f, 1.0f));

                    if (ImGui.Button($"##Color{color}", new Vector2(-1, 22)))
                    {
                        marker.Color = color; // FIX: use 'color' variable directly
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.PopStyleColor(3);

                    // Draw square + text inside option button
                    var optionDrawList = ImGui.GetWindowDrawList();

                    float optionSquareSize = 16f;
                    float optionMarginLeft = 4f;
                    float optionMarginTop = 3f;
                    var optionSquareMin = new Vector2(itemButtonPos.X + optionMarginLeft, itemButtonPos.Y + optionMarginTop);
                    var optionSquareMax = new Vector2(optionSquareMin.X + optionSquareSize, optionSquareMin.Y + optionSquareSize);

                    optionDrawList.AddRectFilled(optionSquareMin, optionSquareMax, ImGui.ColorConvertFloat4ToU32(colorVec));
                    optionDrawList.AddRect(optionSquareMin, optionSquareMax, 0xFF000000u, 0.0f, ImDrawFlags.None, 1.0f);

                    var optionTextPos = new Vector2(optionSquareMax.X + 8, itemButtonPos.Y + 3);
                    optionDrawList.AddText(optionTextPos, 0xFFFFFFFFu, colorName);
                }

                ImGui.EndPopup();
            }

            ImGui.PopStyleColor(2);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Delete marker button
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.6f, 0.2f, 0.2f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.7f, 0.3f, 0.3f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.8f, 0.4f, 0.4f, 1.0f));

            if (ImGui.Button("Delete Marker", new Vector2(200, 0)))
            {
                _markers.Remove(marker);
                _editingMarker = null;
                ImGui.CloseCurrentPopup();
            }

            ImGui.PopStyleColor(3);

            ImGui.EndPopup();
        }
        else
        {
            // Popup was closed
            _editingMarker = null;
        }

        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor(2);
    }

    private static uint GetMarkerColorUInt(MarkerColor color)
    {
        return color switch
        {
            MarkerColor.Red => 0xFF0000FFu,        // ARGB: Red
            MarkerColor.Orange => 0xFF00A5FFu,     // ARGB: Orange
            MarkerColor.Yellow => 0xFF00FFFFu,     // ARGB: Yellow
            MarkerColor.Green => 0xFF00FF00u,      // ARGB: Green
            MarkerColor.ForestGreen => 0xFF228B22u,// ARGB: Forest Green
            MarkerColor.Teal => 0xFF808000u,       // ARGB: Teal (Dark Cyan)
            MarkerColor.Blue => 0xFFFF0000u,       // ARGB: Blue
            MarkerColor.Purple => 0xFF800080u,     // ARGB: Purple
            MarkerColor.Pink => 0xFFCBC0FFu,       // ARGB: Pink
            _ => 0xFF0000FFu
        };
    }

    private static Vector4 GetMarkerColorVector(MarkerColor color)
    {
        return color switch
        {
            MarkerColor.Red => new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
            MarkerColor.Orange => new Vector4(1.0f, 0.65f, 0.0f, 1.0f),
            MarkerColor.Yellow => new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
            MarkerColor.Green => new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
            MarkerColor.ForestGreen => new Vector4(0.13f, 0.55f, 0.13f, 1.0f),
            MarkerColor.Teal => new Vector4(0.0f, 0.5f, 0.5f, 1.0f),
            MarkerColor.Blue => new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
            MarkerColor.Purple => new Vector4(0.5f, 0.0f, 0.5f, 1.0f),
            MarkerColor.Pink => new Vector4(1.0f, 0.75f, 0.8f, 1.0f),
            _ => new Vector4(1.0f, 0.0f, 0.0f, 1.0f)
        };
    }
}
