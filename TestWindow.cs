using Godot;
using ImGuiGodot;
using ImGuiNET;

namespace SimplyRemadeMI;

public partial class TestWindow : Window
{
    public override void _Ready()
    {
        ImGuiGD.Connect(Render);
    }
    
    public override void _Process(double delta)
    {
        
    }

    private void Render()
    {
        return;
        
        if (ImGui.Begin("Test", ImGuiWindowFlags.NoResize))
        {
            ImGui.Text("Hello World!");
            ImGui.End();
        }
    }
}