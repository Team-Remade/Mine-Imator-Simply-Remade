using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Godot;
using ImGuiGodot;
using SimplyRemadeMI.core;
using SimplyRemadeMI.renderer;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace SimplyRemadeMI.ui;

public class SpawnObjectsMenu
{
    private string _searchText = "";
    private int _selectedCategoryIndex = 0;
    private int _selectedCharacterIndex = -1;
    private int _selectedItemIndex = -1;
    private bool _use3DMode = true;
    private int _selectedTextureSheet = 0; // 0 = Item, 1 = Terrain
    private string[] _textureSheets = { "Item Sheet", "Terrain Sheet" };
    private string[] _categories = { "Character", "Item" };
    private string[] _allCharacters = { "Steve", "Balloonicorn", "PyroBaby" };
    private Dictionary<string, Texture2D> _itemTextures = new Dictionary<string, Texture2D>();
    private Dictionary<string, Texture2D> _terrainTextures = new Dictionary<string, Texture2D>();
    private ModelLoader _modelLoader = new ModelLoader();

    public void Render()
    {
        ImGui.SetNextWindowSize(new Vector2(600, 500), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Spawn Objects", ImGuiWindowFlags.NoCollapse))
        {
            // Add close button at top-right
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 30);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
            if (ImGui.Button("X", new Vector2(25, 25)))
            {
                Main.GetInstance().UI.ShowSpawnMenu = false;
            }

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
            case 1: // Item
                RenderItemContent();
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
        
        SpawnModel(characterName);
    }

    private void RenderItemContent()
    {
        // Load textures for the selected sheet if not already loaded
        if (_selectedTextureSheet == 0 && _itemTextures.Count == 0)
        {
            LoadItemTextures();
        }
        else if (_selectedTextureSheet == 1 && _terrainTextures.Count == 0)
        {
            LoadTerrainTextures();
        }

        // Texture sheet dropdown
        ImGui.Text("Texture Sheet");
        ImGui.SetNextItemWidth(-1);
        if (ImGui.Combo("##TextureSheet", ref _selectedTextureSheet, _textureSheets, _textureSheets.Length))
        {
            // Reset selection when switching sheets
            _selectedItemIndex = -1;
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Search bar
        ImGui.Text("Search Textures");
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputText("##ItemSearch", ref _searchText, 256))
        {
            // Search functionality would go here
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Item grid
        ImGui.BeginChild("##ItemGrid", new Vector2(-1, ImGui.GetContentRegionAvail().Y - 100), ImGuiChildFlags.None, ImGuiWindowFlags.None);
        
        // Get the current texture dictionary based on selected sheet
        var currentTextures = _selectedTextureSheet == 0 ? _itemTextures : _terrainTextures;
        var textureNames = currentTextures.Keys.ToArray();
        
        // Filter items based on search text
        var filteredItems = string.IsNullOrEmpty(_searchText)
            ? textureNames
            : Array.FindAll(textureNames, item => item.ToLower().Contains(_searchText.ToLower()));
        
        // Calculate grid layout
        var availableWidth = ImGui.GetContentRegionAvail().X;
        var buttonSize = new Vector2(64, 64);
        var buttonSpacing = new Vector2(10, 10);
        var itemsPerRow = (int)Math.Floor(availableWidth / (buttonSize.X + buttonSpacing.X));
        
        if (itemsPerRow < 1) itemsPerRow = 1;
        
        for (int i = 0; i < filteredItems.Length; i++)
        {
            if (i % itemsPerRow != 0)
            {
                ImGui.SameLine();
            }
            
            var itemName = filteredItems[i];
            var texture = currentTextures[itemName];
            var isSelected = _selectedItemIndex == Array.IndexOf(textureNames, itemName);
            
            // Use ImGuiGD.ImageButton with the texture
            if (ImGuiGD.ImageButton($"##{itemName}", texture, buttonSize))
            {
                _selectedItemIndex = Array.IndexOf(textureNames, itemName);
                // Handle item selection here
            }
            
            // Draw highlight border if selected
            if (isSelected)
            {
                var drawList = ImGui.GetWindowDrawList();
                var min = ImGui.GetItemRectMin();
                var max = ImGui.GetItemRectMax();
                drawList.AddRect(min, max, ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), 0.0f, 0, 2.0f);
            }
            
            // Tooltip with item name
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(itemName);
            }
        }

        ImGui.EndChild();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // 3D mode toggle
        ImGui.Checkbox("3D Mode", ref _use3DMode);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Enable 3D mode to use SpriteMesh instead of flat plane");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Create button at the bottom
        if (ImGui.Button("Create", new Vector2(-1, 30)))
        {
            Main.GetInstance().UI.ShowSpawnMenu = false;
            
            // Create functionality would go here
            if (_selectedItemIndex >= 0)
            {
                SpawnSelectedItem();
            }
        }
    }

    private void LoadItemTextures()
    {
        // Load all .png files from assets/sprite/item directory
        using var dir = DirAccess.Open("res://assets/sprite/item");
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(".png"))
                {
                    var textureName = fileName.Replace(".png", "");
                    var texturePath = "res://assets/sprite/item/" + fileName;
                    var texture = ResourceLoader.Load<Texture2D>(texturePath);
                    
                    if (texture != null)
                    {
                        // Store the Texture2D directly
                        _itemTextures[textureName] = texture;
                    }
                    else
                    {
                        GD.PrintErr("Failed to load texture: " + texturePath);
                    }
                }
                fileName = dir.GetNext();
            }
            dir.ListDirEnd();
        }
        else
        {
            GD.PrintErr("Failed to open assets/sprite/item directory");
        }
    }

    private void LoadTerrainTextures()
    {
        // Load all .png files from assets/sprite/terrain directory
        using var dir = DirAccess.Open("res://assets/sprite/terrain");
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(".png"))
                {
                    var textureName = fileName.Replace(".png", "");
                    var texturePath = "res://assets/sprite/terrain/" + fileName;
                    var texture = ResourceLoader.Load<Texture2D>(texturePath);
                    
                    if (texture != null)
                    {
                        // Store the Texture2D directly
                        _terrainTextures[textureName] = texture;
                    }
                    else
                    {
                        GD.PrintErr("Failed to load texture: " + texturePath);
                    }
                }
                fileName = dir.GetNext();
            }
            dir.ListDirEnd();
        }
        else
        {
            GD.PrintErr("Failed to open assets/sprite/terrain directory");
        }
    }

    private void SpawnSelectedItem()
    {
        // Get the current texture dictionary based on selected sheet
        var currentTextures = _selectedTextureSheet == 0 ? _itemTextures : _terrainTextures;
        var textureNames = currentTextures.Keys.ToArray();
        if (_selectedItemIndex < 0 || _selectedItemIndex >= textureNames.Length)
            return;

        var itemName = textureNames[_selectedItemIndex];
        var texture = currentTextures[itemName];
        GD.Print("Spawning item: " + itemName);

        // Get MainViewport
        var main = Main.GetInstance();
        if (main?.MainViewport == null)
        {
            GD.PrintErr("Main viewport is not available");
            return;
        }

        MeshInstance3D meshInstance;

        if (_use3DMode)
        {
            // 3D mode: Instantiate SpriteMesh from Main and set its texture
            if (main.SpriteMesh == null)
            {
                GD.PrintErr("SpriteMesh is not available");
                return;
            }

            var spriteMeshInstance = main.SpriteMesh.Instantiate() as SpriteMesh;
            if (spriteMeshInstance == null)
            {
                GD.PrintErr("Failed to instantiate SpriteMesh");
                return;
            }

            // Create reference material with the same properties as the flat plane would have
            var referenceMaterial = new StandardMaterial3D();
            referenceMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
            referenceMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            referenceMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Always;

            // Set the texture and reference material on the SpriteMesh
            spriteMeshInstance.Texture = texture;
            spriteMeshInstance.ReferenceMaterial = referenceMaterial;

            meshInstance = spriteMeshInstance;
        }
        else
        {
            // 2D mode: Create a flat plane
            var quadMesh = new QuadMesh();
            quadMesh.Size = new Godot.Vector2(1, 1);

            var material = new StandardMaterial3D();
            material.AlbedoTexture = texture;
            material.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            material.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Always;

            quadMesh.SurfaceSetMaterial(0, material);

            meshInstance = new MeshInstance3D();
            meshInstance.Mesh = quadMesh;
        }

        // Use MainViewport to create SceneObject of type Item with name "Item"
        main.MainViewport.CreateSceneObject(SceneObject.Type.Item, meshInstance, "Item");
        
        // Update picking system to include the new object
        main.MainViewport.UpdatePicking();
    }

    private void SpawnModel(string modelName, bool packed = false)
    {
        var main = Main.GetInstance();
        if (main?.MainViewport?.World == null)
        {
            GD.PrintErr("Main viewport or world is not available");
            return;
        }

        // Load .glb from assets folder
        string path = "res://assets/mesh/" + modelName + ".glb";
        _modelLoader.LoadModel(path, main.MainViewport.World);
    }
}