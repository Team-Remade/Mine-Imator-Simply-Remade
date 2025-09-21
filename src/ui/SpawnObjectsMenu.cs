using System;
using ImGuiNET;
using System.Numerics;
using SimplyRemadeMI.core;

namespace SimplyRemadeMI.ui;

public class SpawnObjectsMenu
{
    private string _searchText = "";
    private int _selectedCategoryIndex = 0;
    private int _selectedCharacterIndex = -1;
    private string[] _categories = { "Character" };
    private string[] _allCharacters = { "Steve", "SomeTestGuy" };
    private ModelLoader _modelLoader = new ModelLoader();

    public void Render()
    {
        ImGui.SetNextWindowSize(new Vector2(600, 500), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Spawn Objects", ImGuiWindowFlags.NoCollapse))
        {
            // Get available space
            var contentRegion = ImGui.GetContentRegionAvail();
            
            // Create two panels side by side
            ImGui.BeginChild("##LeftPanel", new Vector2(contentRegion.X * 0.3f, contentRegion.Y), ImGuiChildFlags.None, ImGuiWindowFlags.None);
            RenderCategoriesPanel();
            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("##RightPanel", new Vector2(contentRegion.X * 0.7f, contentRegion.Y), ImGuiChildFlags.None, ImGuiWindowFlags.None);
            RenderContentPanel();
            ImGui.EndChild();
        }
        ImGui.End();
    }

    private void RenderCategoriesPanel()
    {
        ImGui.Text("Categories");
        ImGui.Separator();

        for (int i = 0; i < _categories.Length; i++)
        {
            if (ImGui.Selectable(_categories[i], _selectedCategoryIndex == i))
            {
                _selectedCategoryIndex = i;
            }
        }
    }

    private void RenderContentPanel()
    {
        // Preview section at the top
        ImGui.Text("Preview");
        ImGui.Separator();
        
        // Placeholder for preview - just a colored rectangle for now
        var previewSize = new Vector2(ImGui.GetContentRegionAvail().X, 150);
        var previewPos = ImGui.GetCursorScreenPos();
        ImGui.GetWindowDrawList().AddRectFilled(
            previewPos,
            previewPos + previewSize,
            ImGui.GetColorU32(new Vector4(0.3f, 0.3f, 0.3f, 1.0f))
        );
        ImGui.GetWindowDrawList().AddRect(
            previewPos,
            previewPos + previewSize,
            ImGui.GetColorU32(new Vector4(0.7f, 0.7f, 0.7f, 1.0f)),
            0.0f,
            0,
            2.0f
        );
        
        // Add preview text
        var textPos = previewPos + previewSize * 0.5f;
        var textSize = ImGui.CalcTextSize("Preview");
        ImGui.GetWindowDrawList().AddText(
            textPos - textSize * 0.5f,
            ImGui.GetColorU32(new Vector4(0.8f, 0.8f, 0.8f, 1.0f)),
            "Preview"
        );
        
        // Move cursor down past the preview
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + previewSize.Y + 10);

        // Content based on selected category
        switch (_selectedCategoryIndex)
        {
            case 0: // Character
                RenderCharacterContent();
                break;
            default:
                ImGui.Text("No content available for this category");
                break;
        }
    }

    private void RenderCharacterContent()
    {
        // Search bar
        ImGui.Text("Search Characters");
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputText("##CharacterSearch", ref _searchText, 256))
        {
            // Search functionality would go here
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Character list
        ImGui.BeginChild("##CharacterList", new Vector2(-1, ImGui.GetContentRegionAvail().Y - 100), ImGuiChildFlags.None, ImGuiWindowFlags.None);

        // Filter characters based on search text
        var filteredCharacters = string.IsNullOrEmpty(_searchText)
            ? _allCharacters
            : Array.FindAll(_allCharacters, c => c.ToLower().Contains(_searchText.ToLower()));
        
        for (int i = 0; i < filteredCharacters.Length; i++)
        {
            if (ImGui.Selectable(filteredCharacters[i], _selectedCharacterIndex == Array.IndexOf(_allCharacters, filteredCharacters[i])))
            {
                _selectedCharacterIndex = Array.IndexOf(_allCharacters, filteredCharacters[i]);
            }
        }

        ImGui.EndChild();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Texture dropdown (placeholder)
        ImGui.Text("Texture");
        int textureIndex = 0;
        string[] textureOptions = { "Default Texture" };
        ImGui.SetNextItemWidth(-1);
        ImGui.Combo("##TextureSelect", ref textureIndex, textureOptions, textureOptions.Length);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Create button at the bottom
        if (ImGui.Button("Create", new Vector2(-1, 30)))
        {
            Main.GetInstance().UI.ShowSpawnMenu = false;
            
            // Create functionality would go here
            if (_selectedCharacterIndex >= 0)
            {
                SpawnSelectedCharacter();
            }
        }
    }

    private void SpawnSelectedCharacter()
    {
        if (_selectedCharacterIndex < 0 || _selectedCharacterIndex >= _allCharacters.Length)
            return;

        var characterName = _allCharacters[_selectedCharacterIndex];
        
        if (characterName == "Steve")
        {
            SpawnSteve();
        }
        // Add other character cases here as needed
    }

    private void SpawnSteve()
    {
        var main = Main.GetInstance();
        if (main?.MainViewport?.World == null)
        {
            Godot.GD.PrintErr("Main viewport or world is not available");
            return;
        }

        // Load Steve.glb from assets folder
        string stevePath = "res://assets/mesh/Steve.glb";
        var steveObject = _modelLoader.LoadModel(stevePath, main.MainViewport.World);
        
        if (steveObject != null)
        {
            Godot.GD.Print($"Successfully spawned Steve at {steveObject.Position}");
        }
        else
        {
            Godot.GD.PrintErr("Failed to spawn Steve");
        }
    }
}