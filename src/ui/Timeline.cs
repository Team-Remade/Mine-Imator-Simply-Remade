using Godot;
using ImGuiNET;
using Vector2 = System.Numerics.Vector2;

namespace SimplyRemadeMI.ui;

public class Timeline
{
    public void Render(Vector2I pos, Vector2I size)
    {
        ImGui.SetNextWindowPos(new Vector2(pos.X, pos.Y));
        ImGui.SetNextWindowSize(new Vector2(size.X, size.Y));

        if (ImGui.Begin("Timeline", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse |  ImGuiWindowFlags.AlwaysVerticalScrollbar))
        {
            
        }
        
        ImGui.End();
    }
}