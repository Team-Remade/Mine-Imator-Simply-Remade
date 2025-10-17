using System;
using Gizmo3DPlugin;
using Godot;
using ImGuiGodot;
using ImGuiNET;
using Vector2 = System.Numerics.Vector2;
using SimplyRemadeMI;
using SimplyRemadeMI.core;

namespace SimplyRemadeMI.renderer;

public class ViewportObject
{
    private SubViewport Viewport;

    public ViewportObject(SubViewport viewport)
    {
        Viewport = viewport;
    }
    
    public void Render(Vector2I position)
    {
        if (ImGui.Begin("Viewport", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoScrollbar |  ImGuiWindowFlags.NoScrollWithMouse))
        {
            var size = ImGui.GetContentRegionAvail();
            if (size.X > 5 && size.Y > 5)
            {
                Viewport.CallDeferred(SubViewport.MethodName.SetSize,
                    new Vector2I((int)size.X - 5, (int)size.Y - 5));

                // Render bench button first to ensure it's clickable
                var main = Main.GetInstance();
                if (main != null && main.Icons != null && main.Icons.TryGetValue("Bench", out var benchTexture))
                {
                    var benchPos = new Vector2(position.X + 20, position.Y + 15);
                    var benchSize = new Vector2(64, 64);
                    ImGui.SetCursorPos(benchPos);
                    if (ImGui.InvisibleButton("##Bench", benchSize))
                    {
                        Main.GetInstance().UI.ShowSpawnMenu = true;
                    }
                    
                    var windowPos = Main.GetInstance().GetWindow().Position;
                    
                    ImGui.SetCursorPos(new Vector2(position.X + 10, position.Y + 10));
                
                    ImGuiGD.SubViewport(Viewport);
                    
                    ImGui.SetCursorPos(benchPos);
                    ImGuiGD.Image(benchTexture, benchSize);
                }
                
                
                
                // Gizmo mode text rendering
                string gizmoModeText;
                uint textColor;

                if ((SelectionManager.TransformGizmo.Mode & Gizmo3D.ToolMode.Move) != 0)
                {
                    gizmoModeText = "Translation Mode";
                    textColor = 0xFF00FFFF;
                } else if ((SelectionManager.TransformGizmo.Mode & Gizmo3D.ToolMode.Rotate) != 0)
                {
                    gizmoModeText = "Rotation Mode";
                    textColor = 0xFF00FFFF;
                } else
                {
                    gizmoModeText = "Scale Mode";
                    textColor = 0xFFFF8000;
                }

                var pos = ImGui.GetWindowPos();
            
                var gizmoModePos = new Vector2(pos.X + 15, pos.Y + 10 + 64 + 35);
                var gizmoModeSize = ImGui.CalcTextSize(gizmoModeText);
            
                var bgRectMin = gizmoModePos - new Vector2(2, 2);
                var bgRectMax = gizmoModePos + gizmoModeSize + new Vector2(2, 2);
                uint bgColor = 0x80000000; // Semi-transparent black
                
                var drawList = ImGui.GetWindowDrawList();
                
                drawList.AddRectFilled(bgRectMin, bgRectMax, bgColor);
                drawList.AddText(gizmoModePos, textColor, gizmoModeText);
                
            }
        }
        
        ImGui.End();
    }
    
    public Vector2 GetBenchPosition()
    {
        var windowPos = Main.GetInstance().GetWindow().Position;
        return new Vector2(windowPos.X + 20, windowPos.Y + 15);
    }
}