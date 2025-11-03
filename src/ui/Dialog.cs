#nullable enable
using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using SimplyRemadeMI.util;

namespace SimplyRemadeMI.ui;

public enum DialogType
{
    DeleteConfirmation,
    SystemMessage,
    YesNoConfirmation,
    Information,
    RenderSettings,
    RenderAnimation,
    RenderProgress
}

public class Dialog
{
    public DialogType Type;
    public string Title;
    public string Message;
    public Vector2 Position;
    
    private Vector2 _dialogPos = Vector2.Zero;
    private Vector2 _dialogSize = Vector2.Zero;
    
    private readonly Action? _onYes;
    private readonly Action? _onNo;
    private readonly Action<int, int, string>? _onRenderComplete;
    private readonly Action<int, int, int, int, string, string>? _onAnimationRenderComplete;
    
    private PropertiesPanel? _propertiesPanel;

    public bool Visible = true;
    
    private int _currentFrame = 0;
    private int _totalFrames = 0;
    private float _encodingProgress = 0.0f;
    private string _progressText = "";
    private bool _isEncoding = false;

    public Dialog(DialogType type, string title, string message, Vector2 position, Action? onYes = null,
        Action? onNo = null, Action<int, int, string>? onRenderComplete = null,
        Action<int, int, int, int, string, string>? onAnimationRenderComplete = null,
        PropertiesPanel? propertiesPanel = null)
    {
        Type = type;
        Title = title;
        Message = message;
        Position = position;
        _onYes = onYes;
        _onNo = onNo;
        _onRenderComplete = onRenderComplete;
        _onAnimationRenderComplete = onAnimationRenderComplete;
        _propertiesPanel = propertiesPanel;
    }
    
    public void Render(Vector2 windowSize)
    {
        if (!Visible) return;

        // Create the dialog window
        ImGui.SetNextWindowPos(Position);
        ImGui.SetNextWindowFocus();

        // Make it modal-like with no close button and always on top
        var flags = ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar;

        // For render settings, animation, and progress dialogs, use fixed size instead of auto-resize
        if (Type == DialogType.RenderSettings || Type == DialogType.RenderAnimation ||
            Type == DialogType.RenderProgress)
        {
            ImGui.SetNextWindowSize(new Vector2(400, 0)); // Fixed width, auto height
            flags |= ImGuiWindowFlags.NoResize;
        }
        else
        {
            flags |= ImGuiWindowFlags.AlwaysAutoResize;
        }

        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.2f, 0.2f, 0.2f, 1.0f)); // Solid dark background
        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(1.0f, 0.0f, 0.0f, 1.0f)); // Red border

        if (ImGui.Begin("##Dialog", flags))
        {
            // Adjust position to keep auto-sized dialog fully on screen
            var currentDialogSize = ImGui.GetWindowSize();
            var currentDialogPos = ImGui.GetWindowPos();

            var adjustedPos = currentDialogPos;
            if (currentDialogPos.X + currentDialogSize.X > windowSize.X)
                adjustedPos.X = windowSize.X - currentDialogSize.X - 10;
            if (currentDialogPos.Y + currentDialogSize.Y > windowSize.Y)
                adjustedPos.Y = windowSize.Y - currentDialogSize.Y - 10;
            if (adjustedPos.X < 10)
                adjustedPos.X = 10;
            if (adjustedPos.Y < 10)
                adjustedPos.Y = 10;

            if (Math.Abs(adjustedPos.X - currentDialogPos.X) > 0.01f || Math.Abs(adjustedPos.Y - currentDialogPos.Y) > 0.01f)
                ImGui.SetWindowPos(adjustedPos);

            // Store final dialog bounds for outside-click detection
            _dialogPos = ImGui.GetWindowPos();
            _dialogSize = ImGui.GetWindowSize();

            // Render content based on dialog type
            RenderDialogContent();
        }

        ImGui.End();
        ImGui.PopStyleColor(2);
    }
    
    private void RenderDialogContent()
    {
        switch (Type)
        {
            case DialogType.DeleteConfirmation:
                RenderDeleteConfirmation();
                break;
            case DialogType.SystemMessage:
                RenderSystemMessage();
                break;
            case DialogType.YesNoConfirmation:
                RenderYesNoConfirmation();
                break;
            case DialogType.Information:
                RenderInformation();
                break;
            case DialogType.RenderSettings:
                RenderRenderSettings();
                break;
            case DialogType.RenderAnimation:
                RenderRenderAnimation();
                break;
            case DialogType.RenderProgress:
                RenderRenderProgress();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void RenderDeleteConfirmation()
    {
        ImGui.Text("DELETE OBJECT");
        ImGui.Separator();
        ImGui.Text(Message);
        ImGui.Spacing();

        // Center buttons
        float buttonWidth = 80.0f;
        float spacing = ImGui.GetStyle().ItemSpacing.X;
        float totalWidth = buttonWidth * 2 + spacing;
        float windowWidth = ImGui.GetWindowSize().X;
        ImGui.SetCursorPosX((windowWidth - totalWidth) * 0.5f);

        // Yes button with red color scheme
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.6f, 0.1f, 0.1f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.9f, 0.3f, 0.3f, 1.0f));

        if (ImGui.Button("Yes", new Vector2(buttonWidth, 0)))
        {
            _onYes?.Invoke();
            Visible = false;
        }

        ImGui.PopStyleColor(3);
        ImGui.SameLine();

        // No button with gray color scheme
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));

        if (ImGui.Button("No", new Vector2(buttonWidth, 0)))
        {
            _onNo?.Invoke();
            Visible = false;
        }

        ImGui.PopStyleColor(3);
    }
    
    private void RenderSystemMessage()
    {
        ImGui.Text(Title);
        ImGui.Separator();
        ImGui.Text(Message);
        ImGui.Spacing();

        float buttonWidth = 60.0f;
        float windowWidth = ImGui.GetWindowSize().X;
        ImGui.SetCursorPosX((windowWidth - buttonWidth) * 0.5f);

        // OK button with neutral color scheme
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));

        if (ImGui.Button("OK", new Vector2(buttonWidth, 0)))
        {
            _onYes?.Invoke();
            Visible = false;
        }

        ImGui.PopStyleColor(3);
    }
    
    private void RenderYesNoConfirmation()
    {
        ImGui.Text(Title);
        ImGui.Separator();
        ImGui.Text(Message);
        ImGui.Spacing();

        float buttonWidth = 80.0f;
        float spacing = ImGui.GetStyle().ItemSpacing.X;
        float totalWidth = buttonWidth * 2 + spacing;
        float windowWidth = ImGui.GetWindowSize().X;
        ImGui.SetCursorPosX((windowWidth - totalWidth) * 0.5f);

        // Yes button with red color scheme
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.6f, 0.1f, 0.1f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.9f, 0.3f, 0.3f, 1.0f));

        if (ImGui.Button("Yes", new Vector2(buttonWidth, 0)))
        {
            _onYes?.Invoke();
            Visible = false;
        }

        ImGui.PopStyleColor(3);
        ImGui.SameLine();

        // No button with gray color scheme
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));

        if (ImGui.Button("No", new Vector2(buttonWidth, 0)))
        {
            _onNo?.Invoke();
            Visible = false;
        }

        ImGui.PopStyleColor(3);
    }
    
    private void RenderInformation()
    {
        ImGui.Text(Title);
        ImGui.Separator();
        ImGui.Text(Message);
        ImGui.Spacing();

        float buttonWidth = 60.0f;
        float windowWidth = ImGui.GetWindowSize().X;
        ImGui.SetCursorPosX((windowWidth - buttonWidth) * 0.5f);

        // OK button with neutral color scheme
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));

        if (ImGui.Button("OK", new Vector2(buttonWidth, 0)))
        {
            _onYes?.Invoke();
            Visible = false;
        }

        ImGui.PopStyleColor(3);
    }
    
    private void RenderRenderSettings()
    {
        var windowWidth = ImGui.GetWindowSize().X;

        // Center the title
        var titleText = "RENDER SETTINGS";
        var titleWidth = ImGui.CalcTextSize(titleText).X;
        ImGui.SetCursorPosX((windowWidth - titleWidth) * 0.5f);
        ImGui.Text(titleText);
        ImGui.Separator();
        ImGui.Spacing();

        // Center "Image Size:" label
        var labelText = "Image Size:";
        var labelWidth = ImGui.CalcTextSize(labelText).X;
        ImGui.SetCursorPosX((windowWidth - labelWidth) * 0.5f);
        ImGui.Text(labelText);

        // Center the dropdown
        const float dropdownWidth = 200f;
        ImGui.SetCursorPosX((windowWidth - dropdownWidth) * 0.5f);
        ImGui.SetNextItemWidth(dropdownWidth);
        if (_propertiesPanel != null && ImGui.Combo("##ImageSize", ref _propertiesPanel.Project.SelectedResolutionIndex, _propertiesPanel.Project.ResolutionOptions, _propertiesPanel.Project.ResolutionOptions.Length))
        {
            // Resolution changed - update aspect ratio if switching to custom
            if (_propertiesPanel.Project.SelectedResolutionIndex == 12) // Custom
            {
                _propertiesPanel.Project.AspectRatio = (float)_propertiesPanel.Project.ProjectRenderWidth / _propertiesPanel.Project.ProjectRenderHeight;
            }
        }

        // Show custom resolution controls if "Custom" is selected
        if (_propertiesPanel != null && _propertiesPanel.Project.SelectedResolutionIndex == 12) // Custom option
        {
            ImGui.Spacing();

            // Calculate total width for the width/height controls
            var widthLabelSize = ImGui.CalcTextSize("Width:").X;
            var heightLabelSize = ImGui.CalcTextSize("Height:").X;
            const float inputWidth = 80f;
            var itemSpacing = ImGui.GetStyle().ItemSpacing.X;
            var totalControlsWidth = widthLabelSize + itemSpacing + inputWidth + itemSpacing + heightLabelSize +
                                     itemSpacing + inputWidth;

            // Center the width/height controls
            ImGui.SetCursorPosX((windowWidth - totalControlsWidth) * 0.5f);

            // Width spinner
            ImGui.Text("Width:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(inputWidth);
            int oldWidth = _propertiesPanel.Project.ProjectRenderWidth;
            if (ImGui.InputInt("##Width", ref _propertiesPanel.Project.ProjectRenderWidth, 1, 10))
            {
                if (_propertiesPanel.Project.ProjectRenderWidth < 1) _propertiesPanel.Project.ProjectRenderWidth = 1;
                if (_propertiesPanel.Project.KeepAspectRatio && oldWidth != _propertiesPanel.Project.ProjectRenderWidth)
                {
                    _propertiesPanel.Project.ProjectRenderHeight = (int)(_propertiesPanel.Project.ProjectRenderHeight / _propertiesPanel.Project.AspectRatio);
                    if (_propertiesPanel.Project.ProjectRenderHeight < 1) _propertiesPanel.Project.ProjectRenderHeight = 1;
                }
            }

            ImGui.SameLine();
            ImGui.Text("Height:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(inputWidth);
            int oldHeight = _propertiesPanel.Project.ProjectRenderHeight;
            if (ImGui.InputInt("##Height", ref _propertiesPanel.Project.ProjectRenderHeight, 1, 10))
            {
                if (_propertiesPanel.Project.ProjectRenderHeight < 1) _propertiesPanel.Project.ProjectRenderHeight = 1;
                if (_propertiesPanel.Project.KeepAspectRatio && oldHeight != _propertiesPanel.Project.ProjectRenderHeight)
                {
                    _propertiesPanel.Project.ProjectRenderWidth = (int)(_propertiesPanel.Project.ProjectRenderHeight * _propertiesPanel.Project.AspectRatio);
                    if (_propertiesPanel.Project.ProjectRenderWidth < 1) _propertiesPanel.Project.ProjectRenderWidth = 1;
                }
            }

            ImGui.Spacing();

            // Center the checkbox
            var checkboxText = "Keep Aspect Ratio";
            var checkboxWidth = ImGui.CalcTextSize(checkboxText).X + ImGui.GetFrameHeight() + itemSpacing;
            ImGui.SetCursorPosX((windowWidth - checkboxWidth) * 0.5f);
            if (ImGui.Checkbox(checkboxText, ref _propertiesPanel.Project.KeepAspectRatio))
            {
                if (_propertiesPanel.Project.KeepAspectRatio)
                {
                    _propertiesPanel.Project.AspectRatio = (float)_propertiesPanel.Project.ProjectRenderWidth / _propertiesPanel.Project.ProjectRenderHeight;
                }
            }
        }

        ImGui.Spacing();
        ImGui.Spacing();

        // Buttons
        float buttonWidth = 80.0f;
        float buttonSpacing = ImGui.GetStyle().ItemSpacing.X;
        float totalWidth = buttonWidth * 2 + buttonSpacing;
        ImGui.SetCursorPosX((windowWidth - totalWidth) * 0.5f);

        // Save button with green color scheme
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.1f, 0.6f, 0.1f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.2f, 0.8f, 0.2f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.3f, 0.9f, 0.3f, 1.0f));

        if (ImGui.Button("Save", new Vector2(buttonWidth, 0)))
        {
            ShowFileSaveDialog();
            Visible = false;
        }

        ImGui.PopStyleColor(3);
        ImGui.SameLine();

        // Cancel button with gray color scheme
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));

        if (ImGui.Button("Cancel", new Vector2(buttonWidth, 0)))
        {
            _onNo?.Invoke();
            Visible = false;
        }

        ImGui.PopStyleColor(3);
    }
    
    private async void ShowFileSaveDialog()
    {
        try
        {
            var selectedPath = await FileDialog.ShowSaveDialogAsync("render");

            if (string.IsNullOrEmpty(selectedPath)) return;
            // Get actual resolution values
            int width, height;
            if (_propertiesPanel?.Project.SelectedResolutionIndex == 12) // Custom
            {
                width = _propertiesPanel.Project.ProjectRenderWidth;
                height = _propertiesPanel.Project.ProjectRenderHeight;
            }
            else
            {
                // Parse resolution from preset options
                Debug.Assert(_propertiesPanel != null, nameof(_propertiesPanel) + " != null");
                (width, height) = PropertiesPanel.ParseResolution(_propertiesPanel.Project.ResolutionOptions[_propertiesPanel.Project.SelectedResolutionIndex]);
            }

            // Save the Output viewport texture with the specified resolution
            Main.GetInstance().SaveOutputTexture(selectedPath, width, height);
            _onYes?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing file save dialog: {ex.Message}");
        }
    }
    
    private void RenderRenderAnimation()
    {
        var windowWidth = ImGui.GetWindowSize().X;

        // Center the title
        const string titleText = "RENDER ANIMATION";
        var titleWidth = ImGui.CalcTextSize(titleText).X;
        ImGui.SetCursorPosX((windowWidth - titleWidth) * 0.5f);
        ImGui.Text(titleText);
        ImGui.Separator();
        ImGui.Spacing();

        // Center "Image Size:" label
        const string imageSizeText = "Image Size:";
        var imageSizeWidth = ImGui.CalcTextSize(imageSizeText).X;
        ImGui.SetCursorPosX((windowWidth - imageSizeWidth) * 0.5f);
        ImGui.Text(imageSizeText);

        // Center the resolution dropdown
        const float dropdownWidth = 200f;
        ImGui.SetCursorPosX((windowWidth - dropdownWidth) * 0.5f);
        ImGui.SetNextItemWidth(dropdownWidth);
        Debug.Assert(_propertiesPanel != null, nameof(_propertiesPanel) + " != null");
        if (ImGui.Combo("##ImageSize", ref _propertiesPanel.Project.SelectedResolutionIndex, _propertiesPanel.Project.ResolutionOptions, _propertiesPanel.Project.ResolutionOptions.Length))
        {
            // Resolution changed - update aspect ratio if switching to custom
            if (_propertiesPanel.Project.SelectedResolutionIndex == 12) // Custom
            {
                _propertiesPanel.Project.AspectRatio = (float)_propertiesPanel.Project.ProjectRenderWidth / _propertiesPanel.Project.ProjectRenderHeight;
            }
            else
            {
                // Update values with selected preset
                var (width, height) = PropertiesPanel.ParseResolution(_propertiesPanel.Project.ResolutionOptions[_propertiesPanel.Project.SelectedResolutionIndex]);
                _propertiesPanel.Project.ProjectRenderWidth = width;
                _propertiesPanel.Project.ProjectRenderHeight = height;
                _propertiesPanel.Project.AspectRatio = (float)_propertiesPanel.Project.ProjectRenderWidth / _propertiesPanel.Project.ProjectRenderHeight;

                // TODO: Sync back to project properties
                Console.WriteLine($"Resolution preset selected: {width}x{height}");
            }
        }

        // Show custom resolution controls if "Custom" is selected
        if (_propertiesPanel.Project.SelectedResolutionIndex == 12) // Custom option
        {
            ImGui.Spacing();

            // Calculate total width for the width/height controls
            var widthLabelSize = ImGui.CalcTextSize("Width:").X;
            var heightLabelSize = ImGui.CalcTextSize("Height:").X;
            const float inputWidth = 80f;
            var itemSpacing = ImGui.GetStyle().ItemSpacing.X;
            var totalControlsWidth = widthLabelSize + itemSpacing + inputWidth + itemSpacing + heightLabelSize +
                                     itemSpacing + inputWidth;

            // Center the width/height controls
            ImGui.SetCursorPosX((windowWidth - totalControlsWidth) * 0.5f);

            // Width spinner
            ImGui.Text("Width:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(inputWidth);
            int oldWidth = _propertiesPanel.Project.ProjectRenderWidth;
            if (ImGui.InputInt("##Width", ref _propertiesPanel.Project.ProjectRenderWidth, 1, 10))
            {
                if (_propertiesPanel.Project.ProjectRenderWidth < 1) _propertiesPanel.Project.ProjectRenderWidth = 1;
                if (_propertiesPanel.Project.KeepAspectRatio && oldWidth != _propertiesPanel.Project.ProjectRenderWidth)
                {
                    _propertiesPanel.Project.ProjectRenderHeight = (int)(_propertiesPanel.Project.ProjectRenderWidth / _propertiesPanel.Project.AspectRatio);
                    if (_propertiesPanel.Project.ProjectRenderHeight < 1) _propertiesPanel.Project.ProjectRenderHeight = 1;
                }
            }

            ImGui.SameLine();
            ImGui.Text("Height:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(inputWidth);
            int oldHeight = _propertiesPanel.Project.ProjectRenderHeight;
            if (ImGui.InputInt("##Height", ref _propertiesPanel.Project.ProjectRenderHeight, 1, 10))
            {
                if (_propertiesPanel.Project.ProjectRenderHeight < 1) _propertiesPanel.Project.ProjectRenderHeight = 1;
                if (_propertiesPanel.Project.KeepAspectRatio && oldHeight != _propertiesPanel.Project.ProjectRenderHeight)
                {
                    _propertiesPanel.Project.ProjectRenderWidth = (int)(_propertiesPanel.Project.ProjectRenderHeight * _propertiesPanel.Project.AspectRatio);
                    if (_propertiesPanel.Project.ProjectRenderWidth < 1) _propertiesPanel.Project.ProjectRenderWidth = 1;
                }
            }

            ImGui.Spacing();

            // Center the checkbox
            const string checkboxText = "Keep Aspect Ratio";
            var checkboxWidth = ImGui.CalcTextSize(checkboxText).X + ImGui.GetFrameHeight() + itemSpacing;
            ImGui.SetCursorPosX((windowWidth - checkboxWidth) * 0.5f);
            if (ImGui.Checkbox(checkboxText, ref _propertiesPanel.Project.KeepAspectRatio))
            {
                if (_propertiesPanel.Project.KeepAspectRatio)
                {
                    _propertiesPanel.Project.AspectRatio = (float)_propertiesPanel.Project.ProjectRenderWidth / _propertiesPanel.Project.ProjectRenderHeight;
                }
            }
        }

        ImGui.Spacing();

        // Center "Video Format:" label
        const string formatText = "Video Format:";
        var formatWidth = ImGui.CalcTextSize(formatText).X;
        ImGui.SetCursorPosX((windowWidth - formatWidth) * 0.5f);
        ImGui.Text(formatText);

        // Center the video format dropdown
        ImGui.SetCursorPosX((windowWidth - dropdownWidth) * 0.5f);
        ImGui.SetNextItemWidth(dropdownWidth);
        ImGui.Combo("##VideoFormat", ref _propertiesPanel.Project.SelectedVideoFormatIndex, _propertiesPanel.Project.VideoFormatOptions, _propertiesPanel.Project.VideoFormatOptions.Length);

        ImGui.Spacing();

        // Center bitrate controls (disabled for PNG sequence)
        bool isPngSequence = _propertiesPanel.Project.SelectedVideoFormatIndex == 3; // PNG Sequence
        if (!isPngSequence)
        {
            var bitrateText = "Bitrate (Mbps):";
            var bitrateWidth = ImGui.CalcTextSize(bitrateText).X;
            var bitrateInputWidth = 120f; // Increased width for decimal values
            var bitrateSpacing = ImGui.GetStyle().ItemSpacing.X;
            var totalBitrateWidth = bitrateWidth + bitrateSpacing + bitrateInputWidth;
            ImGui.SetCursorPosX((windowWidth - totalBitrateWidth) * 0.5f);
            ImGui.Text(bitrateText);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(bitrateInputWidth);
            if (ImGui.InputFloat("##Bitrate", ref _propertiesPanel.Project.Bitrate, 0.5f, 5.0f, "%.1f"))
            {
                if (_propertiesPanel.Project.Bitrate < 0.6f) _propertiesPanel.Project.Bitrate = 0.6f;
                if (_propertiesPanel.Project.Bitrate > 75.0f) _propertiesPanel.Project.Bitrate = 75.0f;
            }

            // Add help text for bitrate range
            const string helpText = "Range: 0.6 - 75.0 Mbps";
            var helpTextWidth = ImGui.CalcTextSize(helpText).X;
            ImGui.SetCursorPosX((windowWidth - helpTextWidth) * 0.5f);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f)); // Gray text
            ImGui.Text(helpText);
            ImGui.PopStyleColor();

            ImGui.Spacing();
        }

        ImGui.Spacing();

        // Center framerate controls
        var framerateText = "Framerate (fps):";
        var framerateWidth = ImGui.CalcTextSize(framerateText).X;
        var framerateInputWidth = 80f;
        var framerateSpacing = ImGui.GetStyle().ItemSpacing.X;
        var totalFramerateWidth = framerateWidth + framerateSpacing + framerateInputWidth;
        ImGui.SetCursorPosX((windowWidth - totalFramerateWidth) * 0.5f);
        ImGui.Text(framerateText);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(framerateInputWidth);
        if (ImGui.InputInt("##Framerate", ref _propertiesPanel.Project.FrameRate, 1, 10))
        {
            if (_propertiesPanel.Project.FrameRate < 1) _propertiesPanel.Project.FrameRate = 1;
            if (_propertiesPanel.Project.FrameRate > 120) _propertiesPanel.Project.FrameRate = 120;
        }

        ImGui.Spacing();
        ImGui.Spacing();

        // Buttons
        float buttonWidth = 80.0f;
        float buttonSpacing = ImGui.GetStyle().ItemSpacing.X;
        float totalWidth = buttonWidth * 2 + buttonSpacing;
        ImGui.SetCursorPosX((windowWidth - totalWidth) * 0.5f);

        // Save button with green color scheme
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.1f, 0.6f, 0.1f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.2f, 0.8f, 0.2f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.3f, 0.9f, 0.3f, 1.0f));

        if (ImGui.Button("Save", new Vector2(buttonWidth, 0)))
        {
            ShowAnimationSaveDialog();
            Visible = false;
        }

        ImGui.PopStyleColor(3);
        ImGui.SameLine();

        // Cancel button with gray color scheme
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));

        if (ImGui.Button("Cancel", new Vector2(buttonWidth, 0)))
        {
            _onNo?.Invoke();
            Visible = false;
        }

        ImGui.PopStyleColor(3);
    }
    
    private async void ShowAnimationSaveDialog()
    {
        try
        {
            // Determine file filter and default name based on format
            string filter;
            string defaultName;
            bool isPngSequence = _propertiesPanel != null && _propertiesPanel.Project.SelectedVideoFormatIndex == 3;

            if (isPngSequence)
            {
                filter = "PNG Files (*.png)|*.png";
                defaultName = "frame_001"; // First frame of sequence
            }
            else
            {
                // Set filter and default name based on selected video format
                Debug.Assert(_propertiesPanel != null, nameof(_propertiesPanel) + " != null");
                switch (_propertiesPanel.Project.SelectedVideoFormatIndex)
                {
                    case 0: // MP4
                        filter = "MP4 Files (*.mp4)|*.mp4";
                        defaultName = "animation.mp4";
                        break;
                    case 1: // MOV
                        filter = "MOV Files (*.mov)|*.mov";
                        defaultName = "animation.mov";
                        break;
                    case 2: // WMV
                        filter = "WMV Files (*.wmv)|*.wmv";
                        defaultName = "animation.wmv";
                        break;
                    default:
                        filter = "MP4 Files (*.mp4)|*.mp4|MOV Files (*.mov)|*.mov|WMV Files (*.wmv)|*.wmv";
                        defaultName = "animation.mp4";
                        break;
                }
            }

            var selectedPath = await FileDialog.ShowSaveDialogAsync(defaultName, filter);

            if (string.IsNullOrEmpty(selectedPath)) return;
            var selectedFormat = _propertiesPanel?.Project.VideoFormatOptions[_propertiesPanel.Project.SelectedVideoFormatIndex];

            // Get actual resolution values
            int width, height;
            if (_propertiesPanel != null && _propertiesPanel.Project.SelectedResolutionIndex == 12) // Custom
            {
                width = _propertiesPanel.Project.ProjectRenderWidth;
                height = _propertiesPanel.Project.ProjectRenderHeight;
            }
            else
            {
                // Parse resolution from preset options
                Debug.Assert(_propertiesPanel != null, nameof(_propertiesPanel) + " != null");
                (width, height) = PropertiesPanel.ParseResolution(_propertiesPanel.Project.ResolutionOptions[_propertiesPanel.Project.SelectedResolutionIndex]);
            }

            // Trigger the animation render operation
            // Convert Mbps to kbps for FFmpeg (1 Mbps = 1000 kbps)
            int bitrateKbps = (int)(_propertiesPanel.Project.Bitrate * 1000);
            if (selectedFormat != null)
                _onAnimationRenderComplete?.Invoke(width, height, _propertiesPanel.Project.FrameRate, bitrateKbps,
                    selectedFormat,
                    selectedPath);
            _onYes?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing animation save dialog: {ex.Message}");
        }
    }
    
    private void RenderRenderProgress()
    {
        var windowWidth = ImGui.GetWindowSize().X;

        // Center the title
        var titleText = _isEncoding ? "ENCODING VIDEO" : "RENDERING ANIMATION";
        var titleWidth = ImGui.CalcTextSize(titleText).X;
        ImGui.SetCursorPosX((windowWidth - titleWidth) * 0.5f);
        ImGui.Text(titleText);
        ImGui.Separator();
        ImGui.Spacing();

        // Center the progress text
        var progressTextWidth = ImGui.CalcTextSize(_progressText).X;
        ImGui.SetCursorPosX((windowWidth - progressTextWidth) * 0.5f);
        ImGui.Text(_progressText);
        ImGui.Spacing();

        // Progress bars
        if (_isEncoding)
        {
            // Encoding progress bar
            var encodingBarWidth = 300f;
            ImGui.SetCursorPosX((windowWidth - encodingBarWidth) * 0.5f);
            ImGui.ProgressBar(_encodingProgress / 100.0f, new Vector2(encodingBarWidth, 0), $"{_encodingProgress:F1}%");
        }
        else
        {
            // Frame rendering progress bar
            var frameProgress = _totalFrames > 0 ? (float)_currentFrame / _totalFrames : 0.0f;
            var frameBarWidth = 300f;
            ImGui.SetCursorPosX((windowWidth - frameBarWidth) * 0.5f);
            ImGui.ProgressBar(frameProgress, new Vector2(frameBarWidth, 0), $"{_currentFrame}/{_totalFrames}");
        }

        ImGui.Spacing();
        ImGui.Spacing();

        // Cancel button (only show if not encoding - can't safely cancel during FFmpeg)
        if (!_isEncoding)
        {
            float buttonWidth = 80.0f;
            ImGui.SetCursorPosX((windowWidth - buttonWidth) * 0.5f);

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.6f, 0.1f, 0.1f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.9f, 0.3f, 0.3f, 1.0f));

            if (ImGui.Button("Cancel", new Vector2(buttonWidth, 0)))
            {
                _onNo?.Invoke();
                Visible = false;
            }

            ImGui.PopStyleColor(3);
        }
    }
    
    public bool CheckOutsideClick()
    {
        if (!Visible) return false;

        // Render settings, animation, and progress dialogs should not close on outside clicks
        if (Type == DialogType.RenderSettings || Type == DialogType.RenderAnimation ||
            Type == DialogType.RenderProgress) return false;

        // Check for clicks outside dialog bounds (with 5 pixel buffer)
        var mousePos = ImGui.GetMousePos();
        var dialogMinX = _dialogPos.X - 5;
        var dialogMaxX = _dialogPos.X + _dialogSize.X + 5;
        var dialogMinY = _dialogPos.Y - 5;
        var dialogMaxY = _dialogPos.Y + _dialogSize.Y + 5;

        // Return true if clicked outside bounds
        return ImGui.IsMouseClicked(ImGuiMouseButton.Left) &&
               (mousePos.X < dialogMinX || mousePos.X > dialogMaxX ||
                mousePos.Y < dialogMinY || mousePos.Y > dialogMaxY);
    }
}