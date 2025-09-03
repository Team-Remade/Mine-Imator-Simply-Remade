using System;
using Godot;
using ImGuiGodot;
using ImGuiNET;
using Vector2 = System.Numerics.Vector2;

namespace SimplyRemadeMI.renderer;

public class ViewportObject
{
    private SubViewport Viewport;

    public ViewportObject(SubViewport viewport)
    {
        Viewport = viewport;
    }
    
    public void Render(Vector2I position, Vector2I sizez)
    {
        ImGui.SetNextWindowPos(new Vector2(position.X, position.Y));
        ImGui.SetNextWindowSize(new Vector2(sizez.X, sizez.Y));
        
        if (ImGui.Begin("Viewport", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoScrollbar |  ImGuiWindowFlags.NoScrollWithMouse))
        {
            var size = ImGui.GetContentRegionAvail();
            if (size.X > 5 && size.Y > 5)
            {
                Viewport.CallDeferred(SubViewport.MethodName.SetSize,
                    new Vector2I((int)size.X - 5, (int)size.Y - 5));

                ImGuiGD.SubViewport(Viewport);
                
            }
            
            
        }
        
        ImGui.End();
    }
}