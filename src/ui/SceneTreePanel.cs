using Godot;
using ImGuiNET;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace SimplyRemadeMI.ui;

public class SceneTreePanel
{
    public void Render(Vector2I position, Vector2I size)
    {
        ImGui.SetNextWindowPos(new Vector2(position.X, position.Y));
        ImGui.SetNextWindowSize(new Vector2(size.X, size.Y));
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.2f, 0.2f, 0.25f, 1.0f));

        if (ImGui.Begin("Scene Tree", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text("Scene Objects:");
            ImGui.Separator();
            
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.18f, 0.18f, 0.22f, 1.0f));
            ImGui.BeginChild("##RootDropTarget", new Vector2(0, 28), ImGuiChildFlags.None, ImGuiWindowFlags.None);
            ImGui.Text("Drop here to unparent");
            
            ImGui.EndChild();
            ImGui.PopStyleColor();
        }
        
        ImGui.End();
        ImGui.PopStyleColor();
    }
}