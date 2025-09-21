using ImGuiNET;

namespace SimplyRemadeMI.ui;

public class MenuBar
{
    public bool ShouldShowRenderSettings = false;
    public bool ShouldShowRenderAnimation = false;
    
    public void Render()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New: NOT IMPLEMENTED", "Ctrl+N"))
                {
                    //TODO: Project stuff
                }

                if (ImGui.MenuItem("Open: NOT IMPLEMENTED", "Ctrl+O"))
                {
                    
                }

                if (ImGui.MenuItem("Save: NOT IMPLEMENTED", "Ctrl+S"))
                {
                    
                }

                if (ImGui.MenuItem("Save As: NOT IMPLEMENTED", "Ctrl+Shift+S"))
                {
                    
                }
                
                ImGui.Separator();
                if (ImGui.MenuItem("Import: NOT IMPLEMENTED"))
                {
                    
                }

                if (ImGui.MenuItem("Export: NOT IMPLEMENTED"))
                {
                    
                }
                
                ImGui.Separator();
                if (ImGui.MenuItem("Exit"))
                {
                    Main.GetInstance().GetTree().Quit();
                }
                
                ImGui.EndMenu();
            }
            
            if (ImGui.BeginMenu("Render"))
            {
                if (ImGui.MenuItem("Render Frame", "F12"))
                {
                    ShouldShowRenderSettings = true;
                }

                if (ImGui.MenuItem("Render Animation", "Ctrl+F12"))
                {
                    ShouldShowRenderAnimation = true;
                }

                ImGui.Separator();
                if (ImGui.MenuItem("Render Settings"))
                {
                    // TODO: Open render settings dialog
                }

                ImGui.EndMenu();
            }
            
            ImGui.EndMainMenuBar();
        }
    }
}