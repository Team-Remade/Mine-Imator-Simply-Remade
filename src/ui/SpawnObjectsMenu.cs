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
    private string[] _categories = { "Character", "Item", "Block", "Camera" };
    private string[] _allCharacters = { "Steve", "Balloonicorn", "PyroBaby" };
    private Dictionary<string, Texture2D> _itemTextures = new Dictionary<string, Texture2D>();
    private Dictionary<string, Texture2D> _terrainTextures = new Dictionary<string, Texture2D>();
    private ModelLoader _modelLoader = new ModelLoader();
    
    // Available blocks with their texture definitions
    private Dictionary<string, object> _availableBlocks = new Dictionary<string, object>()
    {
        { "Stone Block", "tile001" },
        { "Dirt Block", "tile002" },
        { "Cobblestone Block", "tile016" },
        { "Grass Block", new Dictionary<string, string>() {
            { "top", "tile040" },
            { "bottom", "tile002" },
            { "front", "tile003" },
            { "back", "tile003" },
            { "left", "tile003" },
            { "right", "tile003" }
        }},
        { "Wood Planks", "tile004" },
        { "Sapling", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Cross.obj" },
            { "textures", new Dictionary<string, string>() {
                { "Oak", "tile015" },
                { "Spruce", "tile0063" },
                { "Birch", "tile079" },
                { "Jungle", "tile030" }
            }}
        }},
        { "Bedrock", "tile017" },
        { "Water", "tile190" },
        { "Lava", "tile191" },
        { "Sand", "tile018" },
        { "Gravel", "tile019" },
        { "Gold Ore", "tile032" },
        { "Iron Ore", "tile033" },
        { "Coal Ore", "tile034" },
        { "Log", new Dictionary<string, object>() {
            { "type", "multi_texture" },
            { "variants", new Dictionary<string, Dictionary<string, string>>() {
                { "Oak", new Dictionary<string, string>() {
                    { "top", "tile021" },
                    { "bottom", "tile021" },
                    { "front", "tile020" },
                    { "back", "tile020" },
                    { "left", "tile020" },
                    { "right", "tile020" }
                }},
                { "Spruce", new Dictionary<string, string>() {
                    { "top", "tile021" },
                    { "bottom", "tile021" },
                    { "front", "tile116" },
                    { "back", "tile116" },
                    { "left", "tile116" },
                    { "right", "tile116" }
                }},
                { "Birch", new Dictionary<string, string>() {
                    { "top", "tile021" },
                    { "bottom", "tile021" },
                    { "front", "tile117" },
                    { "back", "tile117" },
                    { "left", "tile117" },
                    { "right", "tile117" }
                }},
                { "Jungle", new Dictionary<string, string>() {
                    { "top", "tile021" },
                    { "bottom", "tile021" },
                    { "front", "tile153" },
                    { "back", "tile153" },
                    { "left", "tile153" },
                    { "right", "tile153" }
                }}
            }}
        }},
        { "Leaves", "tile053" },
        { "Sponge", "tile048" },
        { "Glass", "tile049" },
        { "Lapis Ore", "tile160" },
        { "Lapis Block", "tile144" },
        { "Dispenser", new Dictionary<string, string>() {
            { "top", "tile062" },
            { "bottom", "tile062" },
            { "front", "tile046" },
            { "back", "tile045" },
            { "left", "tile045" },
            { "right", "tile045" }
        }},
        { "Sandstone", new Dictionary<string, string>() {
            { "top", "tile176" },
            { "bottom", "tile208" },
            { "front", "tile192" },
            { "back", "tile192" },
            { "left", "tile192" },
            { "right", "tile192" }
        }},
        { "Note Block", "tile074" },
        { "Bed", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Bed.glb" }
        }},
        { "Powered Rail", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Flat.obj" },
            { "texture", "tile163" }
        }},
        { "Detector Rail", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Flat.obj" },
            { "texture", "tile195" }
        }},
        { "Sticky Piston", new Dictionary<string, string>() {
            { "top", "tile062" },
            { "bottom", "tile188" },
            { "front", "tile170" },
            { "back", "tile170" },
            { "left", "tile170" },
            { "right", "tile170" }
        }},
        { "Piston", new Dictionary<string, string>() {
            { "top", "tile062" },
            { "bottom", "tile172" },
            { "front", "tile170" },
            { "back", "tile170" },
            { "left", "tile170" },
            { "right", "tile170" }
        }},
        { "Cobweb", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Cross.obj" },
            { "texture", "tile011" }
        }},
        { "Tall Grass", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Cross.obj" },
            { "textures", new Dictionary<string, string>() {
                { "Dead Bush", "tile055" },
                { "Grass", "tile039" },
                { "Fern", "tile056" }
            }}
        }},
        { "Wool", new Dictionary<string, object>() {
            { "type", "textured_block" },
            { "textures", new Dictionary<string, string>() {
                { "White", "tile064" },
                { "Orange", "tile210" },
                { "Magenta", "tile194" },
                { "Light Blue", "tile178" },
                { "Yellow", "tile162" },
                { "Lime", "tile146" },
                { "Pink", "tile130" },
                { "Gray", "tile114" },
                { "Light Gray", "tile225" },
                { "Cyan", "tile209" },
                { "Purple", "tile193" },
                { "Blue", "tile177" },
                { "Brown", "tile161" },
                { "Green", "tile145" },
                { "Red", "tile129" },
                { "Black", "tile113" }
            }}
        }},
        { "Small Flower", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Cross.obj" },
            { "textures", new Dictionary<string, string>() {
                { "Dandelion", "tile013" },
                { "Rose", "tile012" }
            }}
        }},
        { "Small Mushroom", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Cross.obj" },
            { "textures", new Dictionary<string, string>() {
                { "Red", "tile028" },
                { "Brown", "tile029" }
            }}
        }},
        { "Large Mushroom", new Dictionary<string, object>() {
            { "type", "textured_block" },
            { "textures", new Dictionary<string, string>() {
                { "Red", "tile125" },
                { "Brown", "tile126" }
            }}
        }},
        { "Gold Block", "tile023" },
        { "Iron Block", "tile022" },
        { "Double slab", new Dictionary<string, object>() {
            { "type", "multi_texture" },
            { "variants", new Dictionary<string, Dictionary<string, string>>() {
                { "Smooth Stone", new Dictionary<string, string>() {
                    { "top", "tile006" },
                    { "bottom", "tile006" },
                    { "front", "tile005" },
                    { "back", "tile005" },
                    { "left", "tile005" },
                    { "right", "tile005" }
                }},
                { "Sandstone", new Dictionary<string, string>() {
                    { "top", "tile176" },
                    { "bottom", "tile208" },
                    { "front", "tile240" },
                    { "back", "tile240" },
                    { "left", "tile240" },
                    { "right", "tile240" }
                }},
                { "Planks", new Dictionary<string, string>() {
                    { "top", "tile004" },
                    { "bottom", "tile004" },
                    { "front", "tile004" },
                    { "back", "tile004" },
                    { "left", "tile004" },
                    { "right", "tile004" }
                }},
                { "Cobblestone", new Dictionary<string, string>() {
                    { "top", "tile016" },
                    { "bottom", "tile016" },
                    { "front", "tile016" },
                    { "back", "tile016" },
                    { "left", "tile016" },
                    { "right", "tile016" }
                }},
                { "Bricks", new Dictionary<string, string>() {
                    { "top", "tile007" },
                    { "bottom", "tile007" },
                    { "front", "tile007" },
                    { "back", "tile007" },
                    { "left", "tile007" },
                    { "right", "tile007" }
                }}
            }},
            { "half_slab", true }
        }},
        { "Bricks", "tile007" },
        { "TNT", new Dictionary<string, string>() {
            { "top", "tile009" },
            { "bottom", "tile010" },
            { "front", "tile008" },
            { "back", "tile008" },
            { "left", "tile008" },
            { "right", "tile008" }
        }},
        { "Bookshelf", new Dictionary<string, string>() {
            { "top", "tile004" },
            { "bottom", "tile004" },
            { "front", "tile035" },
            { "back", "tile035" },
            { "left", "tile035" },
            { "right", "tile035" }
        }},
        { "Moss Stone", "tile036" },
        { "Obsidian", "tile037" },
        { "Torch", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "TorchMesh.glb" },
            { "wall_mesh", "TorchWallMesh.glb" }
        }},
        { "Redstone Torch", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "TorchMesh.glb" },
            { "wall_mesh", "TorchWallMesh.glb" },
            { "texture", "tile099" },
            { "unpowered_texture", "tile115" }
        }},
        { "Spawner", "tile065" },
        { "Stair", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Stair.glb" },
            { "textures", new Dictionary<string, string>() {
                { "Wooden", "tile004" },
                { "Cobblestone", "tile016" },
                { "Brick", "tile007" },
                { "Stone Brick", "tile054" },
                { "Netherbrick", "tile226" }
            }}
        }},
        { "Diamond Ore", "tile050" },
        { "Diamond Block", "tile024" },
        { "Crafting Table", new Dictionary<string, string>() {
            { "top", "tile043" },
            { "bottom", "tile004" },
            { "front", "tile059" },
            { "back", "tile060" },
            { "left", "tile059" },
            { "right", "tile060" }
        }},
        { "Farmland", new Dictionary<string, string>() {
            { "top", "tile087" },
            { "bottom", "tile002" },
            { "front", "tile002" },
            { "back", "tile002" },
            { "left", "tile002" },
            { "right", "tile002" }
        }},
        { "Furnace", new Dictionary<string, object>() {
            { "type", "multi_texture" },
            { "textures", new Dictionary<string, string>() {
                { "top", "tile062" },
                { "bottom", "tile062" },
                { "front", "tile044" },
                { "back", "tile045" },
                { "left", "tile045" },
                { "right", "tile045" }
            }},
            { "burning_textures", new Dictionary<string, string>() {
                { "front", "tile061" }
            }},
            { "burning", false }
        }},
        { "Door", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "OakDoor.glb" },
            { "variants", new Dictionary<string, string>() {
                { "Oak", "OakDoor.glb" },
                { "Iron", "IronDoor.glb" },
                { "Trapdoor", "Trapdoor.glb" },
            }}
        }},
        { "Rail", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Flat.obj" },
            { "texture", "tile216" }
        }},
        { "Pressure Plate", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Plate.obj" },
            { "textures", new Dictionary<string, string>() {
                { "Stone", "tile001" },
                { "Wood", "tile004" }
            }}
        }},
        { "Redstone Ore", "tile051" },
        { "Ice", "tile067" },
        { "Snow Block", "tile066" },
        { "Cactus", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Cactus.glb" }
        }},
        { "Clay", "tile072" },
        { "Sugar Cane", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Cross.obj" },
            { "texture", "tile073" }
        }},
        { "Jukebox", new Dictionary<string, string>() {
            { "top", "tile075" },
            { "bottom", "tile074" },
            { "front", "tile074" },
            { "back", "tile074" },
            { "left", "tile074" },
            { "right", "tile074" }
        }},
        { "Fence", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Post.obj" },
            { "textures", new Dictionary<string, string>() {
                { "Wood", "tile004" },
                { "Netherbrick", "tile226" }
            }}
        }},
        { "Pumpkin", new Dictionary<string, string>() {
            { "top", "tile102" },
            { "bottom", "tile102" },
            { "front", "tile119" },
            { "back", "tile118" },
            { "left", "tile118" },
            { "right", "tile118" }
        }},
        { "Jack-O-Lantern", new Dictionary<string, string>() {
            { "top", "tile102" },
            { "bottom", "tile102" },
            { "front", "tile120" },
            { "back", "tile118" },
            { "left", "tile118" },
            { "right", "tile118" }
        }},
        { "Netherrack", "tile103" },
        { "Soul Sand", "tile104" },
        { "Glowstone", "tile105" },
        { "Cake", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Cake.glb" }
        }},
        { "Stone Bricks", "tile054" },
        { "Melon", new Dictionary<string, string>() {
            { "top", "tile137" },
            { "bottom", "tile137" },
            { "front", "tile136" },
            { "back", "tile136" },
            { "left", "tile136" },
            { "right", "tile136" }
        }},
        { "Mycelium", new Dictionary<string, string>() {
            { "top", "tile078" },
            { "bottom", "tile002" },
            { "front", "tile077" },
            { "back", "tile077" },
            { "left", "tile077" },
            { "right", "tile077" }
        }},
        { "Lily Pad", new Dictionary<string, object>() {
            { "type", "model" },
            { "mesh", "Flat.obj" },
            { "texture", "tile076" }
        }},
        { "Nether Bricks", "tile226" },
        { "End Stone", "tile175" },
        { "Redstone Lamp", new Dictionary<string, object>() {
            { "type", "textured_block" },
            { "textures", new Dictionary<string, string>() {
                { "Unpowered", "tile242" },
                { "Powered", "tile241" }
            }},
            { "powered", false }
        }},
    };
    private int _selectedBlockIndex = -1;
    private int _selectedSaplingTypeIndex = 0;
    private string[] _saplingTypes = { "Oak", "Spruce", "Birch", "Jungle" };
    private int _selectedLogTypeIndex = 0;
    private string[] _logTypes = { "Oak", "Spruce", "Birch", "Jungle" };
    private int _selectedSlabTypeIndex = 0;
    private string[] _slabTypes = { "Smooth Stone", "Sandstone", "Planks", "Cobblestone", "Bricks" };
    private bool _halfSlab = true;
    private bool _useWallTorch = false;
    private bool _redstoneTorchPowered = true;
    private bool _redstoneLampPowered = false;
    private int _selectedGrassTypeIndex = 0;
    private string[] _grassTypes = { "Dead Bush", "Grass", "Fern" };
    private int _selectedWoolColorIndex = 0;
    private string[] _woolColors = { "White", "Orange", "Magenta", "Light Blue", "Yellow", "Lime", "Pink", "Gray", "Light Gray", "Cyan", "Purple", "Blue", "Brown", "Green", "Red", "Black" };
    private int _selectedFlowerTypeIndex = 0;
    private string[] _flowerTypes = { "Dandelion", "Rose" };
    private int _selectedMushroomTypeIndex = 0;
    private string[] _mushroomTypes = { "Red", "Brown" };
    private int _selectedLargeMushroomTypeIndex = 0;
    private string[] _largeMushroomTypes = { "Red", "Brown" };
    private int _selectedStairTypeIndex = 0;
    private string[] _stairTypes = { "Wooden", "Cobblestone", "Brick", "Stone Brick", "Netherbrick" };
    private bool _farmlandHydrated = false;
    private bool _furnaceBurning = false;
    private int _selectedDoorTypeIndex = 0;
    private string[] _doorTypes = { "Oak", "Iron", "Trapdoor" };
    private int _selectedPressurePlateTypeIndex = 0;
    private string[] _pressurePlateTypes = { "Stone", "Wood" };
    private int _selectedFenceTypeIndex = 0;
    private string[] _fenceTypes = { "Wood", "Netherbrick" };

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
            case 2: // Block
                RenderBlockContent();
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
            referenceMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
            referenceMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;

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
            material.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
            material.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;

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

    private void RenderBlockContent()
    {
        ImGui.Text("Block Types");
        ImGui.Separator();
        
        // Search bar
        ImGui.Text("Search Blocks");
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputText("##BlockSearch", ref _searchText, 256))
        {
            // Search functionality would go here
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        
        // Block list
        ImGui.BeginChild("##BlockList", new Vector2(-1, ImGui.GetContentRegionAvail().Y - 100), ImGuiChildFlags.None, ImGuiWindowFlags.None);
        
        // Get block names
        var blockNames = _availableBlocks.Keys.ToArray();
        
        // Filter blocks based on search text
        var filteredBlocks = string.IsNullOrEmpty(_searchText)
            ? blockNames
            : Array.FindAll(blockNames, block => block.ToLower().Contains(_searchText.ToLower()));
        
        for (int i = 0; i < filteredBlocks.Length; i++)
        {
            if (ImGui.Selectable(filteredBlocks[i], _selectedBlockIndex == Array.IndexOf(blockNames, filteredBlocks[i])))
            {
                _selectedBlockIndex = Array.IndexOf(blockNames, filteredBlocks[i]);
            }
        }
        
        ImGui.EndChild();
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        
        // Show sapling type selection if Sapling is selected
        if (_selectedBlockIndex >= 0)
        {
            var availableBlockNames = _availableBlocks.Keys.ToArray();
            var selectedBlockName = availableBlockNames[_selectedBlockIndex];
            if (selectedBlockName == "Sapling")
            {
                ImGui.Text("Sapling Type");
                ImGui.SetNextItemWidth(-1);
                ImGui.Combo("##SaplingType", ref _selectedSaplingTypeIndex, _saplingTypes, _saplingTypes.Length);
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Log")
            {
                ImGui.Text("Log Type");
                ImGui.SetNextItemWidth(-1);
                ImGui.Combo("##LogType", ref _selectedLogTypeIndex, _logTypes, _logTypes.Length);
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Double slab")
            {
                ImGui.Text("Slab Type");
                ImGui.SetNextItemWidth(-1);
                ImGui.Combo("##SlabType", ref _selectedSlabTypeIndex, _slabTypes, _slabTypes.Length);
                
                ImGui.Spacing();
                ImGui.Checkbox("Half Slab", ref _halfSlab);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Generate slab with half height and adjusted UVs");
                }
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Tall Grass")
            {
                ImGui.Text("Grass Type");
                ImGui.SetNextItemWidth(-1);
                ImGui.Combo("##GrassType", ref _selectedGrassTypeIndex, _grassTypes, _grassTypes.Length);
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Wool")
            {
                ImGui.Text("Wool Color");
                ImGui.SetNextItemWidth(-1);
                ImGui.Combo("##WoolColor", ref _selectedWoolColorIndex, _woolColors, _woolColors.Length);
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Small Flower")
            {
                ImGui.Text("Flower Type");
                ImGui.SetNextItemWidth(-1);
                ImGui.Combo("##FlowerType", ref _selectedFlowerTypeIndex, _flowerTypes, _flowerTypes.Length);
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Small Mushroom")
            {
                ImGui.Text("Mushroom Type");
                ImGui.SetNextItemWidth(-1);
                ImGui.Combo("##MushroomType", ref _selectedMushroomTypeIndex, _mushroomTypes, _mushroomTypes.Length);
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Large Mushroom")
            {
                ImGui.Text("Mushroom Type");
                ImGui.SetNextItemWidth(-1);
                ImGui.Combo("##LargeMushroomType", ref _selectedLargeMushroomTypeIndex, _largeMushroomTypes, _largeMushroomTypes.Length);
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Torch" || selectedBlockName == "Redstone Torch")
            {
                ImGui.Checkbox("Wall Torch", ref _useWallTorch);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Use wall-mounted torch instead of floor torch");
                }
                
                if (selectedBlockName == "Redstone Torch")
                {
                    ImGui.Checkbox("Powered", ref _redstoneTorchPowered);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Whether the redstone torch is powered or unpowered");
                    }
                }
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Stair")
            {
                ImGui.Text("Stair Type");
                ImGui.SetNextItemWidth(-1);
                ImGui.Combo("##StairType", ref _selectedStairTypeIndex, _stairTypes, _stairTypes.Length);
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Farmland")
            {
                ImGui.Checkbox("Hydrated", ref _farmlandHydrated);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Toggle farmland hydration (uses tile086 when hydrated)");
                }
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Furnace")
            {
                ImGui.Checkbox("Burning", ref _furnaceBurning);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Toggle furnace burning state (uses tile061 for front when burning)");
                }
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Door")
            {
                ImGui.Text("Door Type");
                ImGui.SetNextItemWidth(-1);
                ImGui.Combo("##DoorType", ref _selectedDoorTypeIndex, _doorTypes, _doorTypes.Length);
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Pressure Plate")
            {
                ImGui.Text("Pressure Plate Type");
                ImGui.SetNextItemWidth(-1);
                ImGui.Combo("##PressurePlateType", ref _selectedPressurePlateTypeIndex, _pressurePlateTypes, _pressurePlateTypes.Length);
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Fence")
            {
                ImGui.Text("Fence Type");
                ImGui.SetNextItemWidth(-1);
                ImGui.Combo("##FenceType", ref _selectedFenceTypeIndex, _fenceTypes, _fenceTypes.Length);
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
            else if (selectedBlockName == "Redstone Lamp")
            {
                ImGui.Checkbox("Powered", ref _redstoneLampPowered);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Whether the redstone lamp is powered or unpowered");
                }
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
        }
        
        // Create button at the bottom
        if (ImGui.Button("Create", new Vector2(-1, 30)))
        {
            Main.GetInstance().UI.ShowSpawnMenu = false;
            
            // Create functionality would go here
            if (_selectedBlockIndex >= 0)
            {
                SpawnSelectedBlock();
            }
        }
    }

    private void SpawnSelectedBlock()
    {
        if (_selectedBlockIndex < 0)
            return;
            
        var blockNames = _availableBlocks.Keys.ToArray();
        var blockName = blockNames[_selectedBlockIndex];
        SpawnBlock(blockName);
    }
    
    private void SpawnBlock(string blockName)
    {
        var main = Main.GetInstance();
        if (main?.MainViewport == null)
        {
            GD.PrintErr("Main viewport is not available");
            return;
        }
        
        // Load terrain textures if not already loaded
        if (_terrainTextures.Count == 0)
        {
            LoadTerrainTextures();
        }
        
        // Get the block texture definition
        var textureDef = _availableBlocks[blockName];
        
        MeshInstance3D meshInstance;
        
        // Handle different texture types
        if (textureDef is string singleTextureName)
        {
            // Single texture for all faces
            if (!_terrainTextures.TryGetValue(singleTextureName, out var tileTexture))
            {
                GD.PrintErr($"Texture {singleTextureName} not found in terrain textures");
                return;
            }
            
            // Create custom cube mesh with the texture
            var mesh = CreateCustomCubeMesh(tileTexture);
            meshInstance = new MeshInstance3D();
            meshInstance.Mesh = mesh;
        }
        else if (textureDef is Dictionary<string, string> multiTextures)
        {
            // Multi-texture block (like Grass)
            if (!_terrainTextures.TryGetValue(multiTextures["top"], out var topTexture) ||
                !_terrainTextures.TryGetValue(multiTextures["bottom"], out var bottomTexture) ||
                !_terrainTextures.TryGetValue(multiTextures["front"], out var frontTexture) ||
                !_terrainTextures.TryGetValue(multiTextures["back"], out var backTexture) ||
                !_terrainTextures.TryGetValue(multiTextures["left"], out var leftTexture) ||
                !_terrainTextures.TryGetValue(multiTextures["right"], out var rightTexture))
            {
                GD.PrintErr("One or more grass textures not found in terrain textures");
                return;
            }
            
            // Create appropriate mesh based on block type
            ArrayMesh blockMesh;
            
            // For Farmland, check hydration status and use tile086 for top if hydrated
            if (blockName == "Farmland" && _farmlandHydrated)
            {
                if (_terrainTextures.TryGetValue("tile086", out var hydratedTopTexture))
                {
                    topTexture = hydratedTopTexture;
                }
                else
                {
                    GD.PrintErr("Hydrated farmland texture tile086 not found");
                }
            }
            
            if (blockName == "Farmland")
            {
                blockMesh = CreateFarmlandMesh(topTexture, bottomTexture, frontTexture, backTexture, leftTexture, rightTexture);
            }
            else
            {
                blockMesh = CreateGrassBlockMesh(topTexture, bottomTexture, frontTexture, backTexture, leftTexture, rightTexture);
            }
            meshInstance = new MeshInstance3D();
            meshInstance.Mesh = blockMesh;
        }
        else if (textureDef is Dictionary<string, object> furnaceDef && blockName == "Furnace")
        {
            // Handle Furnace block with burning state
            if (!furnaceDef.ContainsKey("textures") || furnaceDef["textures"] is not Dictionary<string, string> textures)
            {
                GD.PrintErr("Textures not found or invalid format for furnace");
                return;
            }
            
            if (!_terrainTextures.TryGetValue(textures["top"], out var topTexture) ||
                !_terrainTextures.TryGetValue(textures["bottom"], out var bottomTexture) ||
                !_terrainTextures.TryGetValue(textures["front"], out var frontTexture) ||
                !_terrainTextures.TryGetValue(textures["back"], out var backTexture) ||
                !_terrainTextures.TryGetValue(textures["left"], out var leftTexture) ||
                !_terrainTextures.TryGetValue(textures["right"], out var rightTexture))
            {
                GD.PrintErr("One or more furnace textures not found in terrain textures");
                return;
            }
            
            // Check if burning is enabled and use burning texture for front face
            if (_furnaceBurning && furnaceDef.ContainsKey("burning_textures") && furnaceDef["burning_textures"] is Dictionary<string, string> burningTextures)
            {
                if (burningTextures.ContainsKey("front") && _terrainTextures.TryGetValue(burningTextures["front"], out var burningFrontTexture))
                {
                    frontTexture = burningFrontTexture;
                }
            }
            
            // Create grass block mesh with the appropriate textures
            var blockMesh = CreateGrassBlockMesh(topTexture, bottomTexture, frontTexture, backTexture, leftTexture, rightTexture);
            meshInstance = new MeshInstance3D();
            meshInstance.Mesh = blockMesh;
        }
        else if (textureDef is Dictionary<string, object> multiTextureDef && multiTextureDef.ContainsKey("type") && multiTextureDef["type"] as string == "multi_texture")
        {
            // Multi-texture block with variants (like Log or Double slab)
            if (!multiTextureDef.ContainsKey("variants") || multiTextureDef["variants"] is not Dictionary<string, Dictionary<string, string>> variants)
            {
                GD.PrintErr("Variants not found or invalid format for multi-texture block");
                return;
            }

            string selectedType;
            if (blockName == "Log")
            {
                selectedType = _logTypes[_selectedLogTypeIndex];
            }
            else if (blockName == "Double slab")
            {
                selectedType = _slabTypes[_selectedSlabTypeIndex];
            }
            else
            {
                GD.PrintErr($"Unknown multi-texture block: {blockName}");
                return;
            }

            if (!variants.TryGetValue(selectedType, out var textures))
            {
                GD.PrintErr($"{blockName} type {selectedType} not found in variants");
                return;
            }

            if (!_terrainTextures.TryGetValue(textures["top"], out var topTexture) ||
                !_terrainTextures.TryGetValue(textures["bottom"], out var bottomTexture) ||
                !_terrainTextures.TryGetValue(textures["front"], out var frontTexture) ||
                !_terrainTextures.TryGetValue(textures["back"], out var backTexture) ||
                !_terrainTextures.TryGetValue(textures["left"], out var leftTexture) ||
                !_terrainTextures.TryGetValue(textures["right"], out var rightTexture))
            {
                GD.PrintErr($"One or more {blockName.ToLower()} textures not found in terrain textures");
                return;
            }

            // Create a multi-surface mesh for the block
            ArrayMesh blockMesh;
            if (blockName == "Double slab" && _halfSlab)
            {
                blockMesh = CreateHalfSlabMesh(topTexture, bottomTexture, frontTexture, backTexture, leftTexture, rightTexture);
            }
            else
            {
                blockMesh = CreateGrassBlockMesh(topTexture, bottomTexture, frontTexture, backTexture, leftTexture, rightTexture);
            }
            meshInstance = new MeshInstance3D();
            meshInstance.Mesh = blockMesh;
        }
        else if (textureDef is Dictionary<string, object> texturedBlockDef && texturedBlockDef.ContainsKey("type") && texturedBlockDef["type"] as string == "textured_block")
        {
            // Textured block with variants (like Wool or Redstone Lamp)
            if (!texturedBlockDef.ContainsKey("textures") || texturedBlockDef["textures"] is not Dictionary<string, string> textures)
            {
                GD.PrintErr("Textures not found or invalid format for textured block");
                return;
            }

            string selectedTextureName;
            if (blockName == "Wool")
            {
                selectedTextureName = _woolColors[_selectedWoolColorIndex];
            }
            else if (blockName == "Large Mushroom")
            {
                selectedTextureName = _largeMushroomTypes[_selectedLargeMushroomTypeIndex];
            }
            else if (blockName == "Redstone Lamp")
            {
                // Handle Redstone Lamp powered state
                if (_redstoneLampPowered)
                {
                    selectedTextureName = "Powered";
                }
                else
                {
                    selectedTextureName = "Unpowered";
                }
            }
            else
            {
                GD.PrintErr($"Unknown textured_block: {blockName}");
                return;
            }
            
            if (!textures.TryGetValue(selectedTextureName, out var textureName))
            {
                GD.PrintErr($"Texture variant {selectedTextureName} not found in textures for {blockName}");
                return;
            }

            if (!_terrainTextures.TryGetValue(textureName, out var texture))
            {
                GD.PrintErr($"Texture {textureName} not found in terrain textures");
                return;
            }

            // Create custom cube mesh with the selected texture
            var mesh = CreateCustomCubeMesh(texture);
            meshInstance = new MeshInstance3D();
            meshInstance.Mesh = mesh;
        }
        else if (textureDef is Dictionary<string, object> modelDef && modelDef.ContainsKey("type") && modelDef["type"] as string == "model")
        {
            // Model-based block (like Sapling or Bed)
            string meshPath;
            
            // Handle door variants
            if (blockName == "Door" && modelDef.ContainsKey("variants") && modelDef["variants"] is Dictionary<string, string> variants)
            {
                string selectedDoorType = _doorTypes[_selectedDoorTypeIndex];
                if (variants.TryGetValue(selectedDoorType, out var variantMeshPath))
                {
                    meshPath = variantMeshPath;
                }
                else
                {
                    meshPath = modelDef["mesh"] as string;
                }
            }
            else if ((blockName == "Torch" || blockName == "Redstone Torch") && _useWallTorch && modelDef.ContainsKey("wall_mesh"))
            {
                meshPath = modelDef["wall_mesh"] as string;
            }
            else
            {
                meshPath = modelDef["mesh"] as string;
            }
            
            if (string.IsNullOrEmpty(meshPath))
            {
                GD.PrintErr("Mesh path not specified for model block");
                return;
            }
            
            // Handle different file types
            if (meshPath.EndsWith(".obj"))
            {
                // Load the OBJ model as a mesh
                var meshResource = ResourceLoader.Load<Mesh>($"res://assets/mesh/{meshPath}");
                if (meshResource == null)
                {
                    GD.PrintErr($"Failed to load mesh: {meshPath}");
                    return;
                }
                
                // Duplicate the mesh to avoid modifying the original resource
                var duplicatedMesh = meshResource.Duplicate() as Mesh;
                meshInstance = new MeshInstance3D();
                meshInstance.Mesh = duplicatedMesh;
                
                // Apply texture based on block type
                if (modelDef.ContainsKey("textures") && modelDef["textures"] is Dictionary<string, string> textures)
                {
                    string selectedTextureName = "";
                    
                    // Check if this is a sapling and use sapling type
                    if (blockName == "Sapling")
                    {
                        selectedTextureName = _saplingTypes[_selectedSaplingTypeIndex];
                    }
                    // Check if this is tall grass and use grass type
                    else if (blockName == "Tall Grass")
                    {
                        selectedTextureName = _grassTypes[_selectedGrassTypeIndex];
                    }
                    // Check if this is a small flower and use flower type
                    else if (blockName == "Small Flower")
                    {
                        selectedTextureName = _flowerTypes[_selectedFlowerTypeIndex];
                    }
                    // Check if this is a small mushroom and use mushroom type
                    else if (blockName == "Small Mushroom")
                    {
                        selectedTextureName = _mushroomTypes[_selectedMushroomTypeIndex];
                    }
                    // Check if this is a stair and use stair type
                    else if (blockName == "Stair")
                    {
                        selectedTextureName = _stairTypes[_selectedStairTypeIndex];
                    }
                    // Check if this is a pressure plate and use pressure plate type
                    else if (blockName == "Pressure Plate")
                    {
                        selectedTextureName = _pressurePlateTypes[_selectedPressurePlateTypeIndex];
                    }
                    // Check if this is a fence and use fence type
                    else if (blockName == "Fence")
                    {
                        selectedTextureName = _fenceTypes[_selectedFenceTypeIndex];
                    }
                    
                    if (!string.IsNullOrEmpty(selectedTextureName) && textures.TryGetValue(selectedTextureName, out var textureName))
                    {
                        GD.Print($"Selected texture name: {textureName} for block: {blockName}");
                        if (_terrainTextures.TryGetValue(textureName, out var texture))
                        {
                            GD.Print($"Successfully loaded texture: {textureName}");
                            var material = new StandardMaterial3D();
                            material.AlbedoTexture = texture;
                            material.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
                            material.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
                            material.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
                            material.CullMode = BaseMaterial3D.CullModeEnum.Disabled; // Disable front and backface culling for cross models
                            
                            // Apply the material directly to all surfaces of the mesh
                            for (int i = 0; i < duplicatedMesh.GetSurfaceCount(); i++)
                            {
                                duplicatedMesh.SurfaceSetMaterial(i, material);
                            }
                        }
                        else
                        {
                            GD.PrintErr($"Texture {textureName} not found in terrain textures");
                            GD.Print($"Available terrain textures: {string.Join(", ", _terrainTextures.Keys)}");
                        }
                    }
                    else
                    {
                        GD.PrintErr($"Could not find texture for selected type: {selectedTextureName} in block: {blockName}");
                        GD.Print($"Available texture types: {string.Join(", ", textures.Keys)}");
                    }
                }
                // Apply single texture if specified
                else if (modelDef.ContainsKey("texture") && modelDef["texture"] is string textureName)
                {
                    if (_terrainTextures.TryGetValue(textureName, out var texture))
                    {
                        var material = new StandardMaterial3D();
                        material.AlbedoTexture = texture;
                        material.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
                        material.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
                        material.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
                        material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
                        
                        // Apply the material directly to all surfaces of the mesh
                        for (int i = 0; i < duplicatedMesh.GetSurfaceCount(); i++)
                        {
                            duplicatedMesh.SurfaceSetMaterial(i, material);
                        }
                    }
                }
                
                // Handle Redstone Torch texture replacement after any other texture application
                if (blockName == "Redstone Torch")
                {
                    string redstoneTextureName;
                    if (_redstoneTorchPowered)
                    {
                        redstoneTextureName = modelDef["texture"] as string;
                    }
                    else
                    {
                        redstoneTextureName = modelDef["unpowered_texture"] as string;
                    }
                    
                    if (!string.IsNullOrEmpty(redstoneTextureName) && _terrainTextures.TryGetValue(redstoneTextureName, out var redstoneTexture))
                    {
                        var redstoneMaterial = new StandardMaterial3D();
                        redstoneMaterial.AlbedoTexture = redstoneTexture;
                        redstoneMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
                        redstoneMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
                        redstoneMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
                        redstoneMaterial.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
                        
                        for (int i = 0; i < duplicatedMesh.GetSurfaceCount(); i++)
                        {
                            duplicatedMesh.SurfaceSetMaterial(i, redstoneMaterial);
                        }
                    }
                }
            }
            else if (meshPath.EndsWith(".glb"))
            {
                // Load the GLB model as a scene and instantiate it directly
                var sceneResource = ResourceLoader.Load<PackedScene>($"res://assets/mesh/{meshPath}");
                if (sceneResource == null)
                {
                    GD.PrintErr($"Failed to load scene: {meshPath}");
                    return;
                }
                
                // Instantiate the scene
                var instance = sceneResource.Instantiate();
                
                // Collect all MeshInstance3D nodes from the scene
                var meshInstances = new List<MeshInstance3D>();
                FindAllMeshInstances(instance, meshInstances);
                
                if (meshInstances.Count == 0)
                {
                    GD.PrintErr($"GLB scene {meshPath} does not contain any MeshInstance3D");
                    instance.QueueFree();
                    return;
                }
                
                // Create a parent Node3D to hold all mesh instances
                var parentNode = new Node3D();
                parentNode.Name = $"{System.IO.Path.GetFileNameWithoutExtension(meshPath)}_Container";
                
                // Add all mesh instances to the parent node
                foreach (var meshInst in meshInstances)
                {
                    // Reparent each mesh instance to the parent node
                    meshInst.GetParent()?.RemoveChild(meshInst);
                    parentNode.AddChild(meshInst);
                }
                
                // Free the original scene instance
                instance.QueueFree();
                
                // Handle Redstone Torch texture replacement for GLB models
                if (blockName == "Redstone Torch")
                {
                    string redstoneTextureName;
                    if (_redstoneTorchPowered)
                    {
                        redstoneTextureName = modelDef["texture"] as string;
                    }
                    else
                    {
                        redstoneTextureName = modelDef["unpowered_texture"] as string;
                    }
                    
                    if (!string.IsNullOrEmpty(redstoneTextureName) && _terrainTextures.TryGetValue(redstoneTextureName, out var redstoneTexture))
                    {
                        var redstoneMaterial = new StandardMaterial3D();
                        redstoneMaterial.AlbedoTexture = redstoneTexture;
                        redstoneMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
                        redstoneMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
                        redstoneMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
                        redstoneMaterial.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
                        
                        // Apply the material to all mesh instances in the parent node
                        foreach (var meshInst in meshInstances)
                        {
                            if (meshInst.Mesh != null)
                            {
                                // Duplicate the mesh to avoid modifying the original resource
                                var duplicatedMesh = meshInst.Mesh.Duplicate() as Mesh;
                                meshInst.Mesh = duplicatedMesh;
                                
                                // Apply the material to all surfaces
                                for (int i = 0; i < duplicatedMesh.GetSurfaceCount(); i++)
                                {
                                    duplicatedMesh.SurfaceSetMaterial(i, redstoneMaterial);
                                }
                            }
                        }
                    }
                }
                
                // Handle Stair texture replacement for GLB models
                else if (blockName == "Stair" && modelDef.ContainsKey("textures") && modelDef["textures"] is Dictionary<string, string> stairTextures)
                {
                    string selectedStairType = _stairTypes[_selectedStairTypeIndex];
                    if (stairTextures.TryGetValue(selectedStairType, out var stairTextureName) && _terrainTextures.TryGetValue(stairTextureName, out var stairTexture))
                    {
                        var stairMaterial = new StandardMaterial3D();
                        stairMaterial.AlbedoTexture = stairTexture;
                        stairMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
                        stairMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
                        stairMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
                        stairMaterial.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
                        
                        // Apply the material to all mesh instances in the parent node
                        foreach (var meshInst in meshInstances)
                        {
                            if (meshInst.Mesh != null)
                            {
                                // Duplicate the mesh to avoid modifying the original resource
                                var duplicatedMesh = meshInst.Mesh.Duplicate() as Mesh;
                                meshInst.Mesh = duplicatedMesh;
                                
                                // Apply the material to all surfaces
                                for (int i = 0; i < duplicatedMesh.GetSurfaceCount(); i++)
                                {
                                    duplicatedMesh.SurfaceSetMaterial(i, stairMaterial);
                                }
                            }
                        }
                    }
                }
                
                // Create a MeshInstance3D wrapper for the parent node
                // This is needed because CreateSceneObject expects a MeshInstance3D
                meshInstance = new MeshInstance3D();
                meshInstance.Name = $"{System.IO.Path.GetFileNameWithoutExtension(meshPath)}_Wrapper";
                
                // Add the parent node with all meshes as a child of the wrapper
                meshInstance.AddChild(parentNode);
            }
            else
            {
                GD.PrintErr($"Unsupported model format: {meshPath}");
                return;
            }
        }
        else
        {
            // Fallback: create a cube without texture
            var mesh = CreateCustomCubeMesh();
            meshInstance = new MeshInstance3D();
            meshInstance.Mesh = mesh;
        }
        
        // Create scene object of type Block
        main.MainViewport.CreateSceneObject(SceneObject.Type.Block, meshInstance, blockName);
        
        // Update picking system
        main.MainViewport.UpdatePicking();
    }

    private ArrayMesh CreateCustomCubeMesh(Texture2D texture = null)
    {
        var arrayMesh = new ArrayMesh();
        
        // Create material if texture is provided
        StandardMaterial3D material = null;
        if (texture != null)
        {
            material = new StandardMaterial3D();
            material.AlbedoTexture = texture;
            material.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
            material.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
            material.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
            material.Roughness = 0.8f;
            material.Metallic = 0.1f;
        }
        
        // Define common UVs and indices for all faces
        var uvs = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        var indices = new int[] { 0, 1, 2, 0, 2, 3 }; // Counter-clockwise winding
        var indices2 = new int[] { 2, 1, 0, 3, 2, 0 }; // Clockwise winding
        
        // Create each face using CreateFace method without material
        // Front face: vertices at Z=0.5, normals Forward
        var frontNormals = new Vector3[] { Vector3.Forward, Vector3.Forward, Vector3.Forward, Vector3.Forward };
        var frontFaceMesh = CreateFace(Vector3.Back, frontNormals, uvs, indices2);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, frontFaceMesh.SurfaceGetArrays(0));
        if (material != null)
        {
            arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, material);
        }
        
        // Back face: vertices at Z=-0.5, normals Back
        var backNormals = new Vector3[] { Vector3.Back, Vector3.Back, Vector3.Back, Vector3.Back };
        var backFaceMesh = CreateFace(Vector3.Forward, backNormals, uvs, indices2);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, backFaceMesh.SurfaceGetArrays(0));
        if (material != null)
        {
            arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, material);
        }
        
        // Top face: vertices at Y=0.5, normals Up
        var topNormals = new Vector3[] { Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up };
        var topFaceMesh = CreateFace(Vector3.Up, topNormals, uvs, indices);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, topFaceMesh.SurfaceGetArrays(0));
        if (material != null)
        {
            arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, material);
        }
        
        // Bottom face: vertices at Y=-0.5, normals Down
        var bottomNormals = new Vector3[] { Vector3.Down, Vector3.Down, Vector3.Down, Vector3.Down };
        var bottomFaceMesh = CreateFace(Vector3.Down, bottomNormals, uvs, indices);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, bottomFaceMesh.SurfaceGetArrays(0));
        if (material != null)
        {
            arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, material);
        }
        
        // Right face: vertices at X=0.5, normals Right
        var rightNormals = new Vector3[] { Vector3.Right, Vector3.Right, Vector3.Right, Vector3.Right };
        var rightFaceMesh = CreateFace(Vector3.Right, rightNormals, uvs, indices2);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, rightFaceMesh.SurfaceGetArrays(0));
        if (material != null)
        {
            arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, material);
        }
        
        // Left face: vertices at X=-0.5, normals Left
        var leftNormals = new Vector3[] { Vector3.Left, Vector3.Left, Vector3.Left, Vector3.Left };
        var leftFaceMesh = CreateFace(Vector3.Left, leftNormals, uvs, indices2);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, leftFaceMesh.SurfaceGetArrays(0));
        if (material != null)
        {
            arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, material);
        }
        
        return arrayMesh;
    }

    private ArrayMesh CreateFace(Vector3 direction, Vector3[] normals, Vector2[] uvs, int[] indices, StandardMaterial3D material = null)
    {
        Vector3[] vertices = CalculateFaceVertices(direction);
        
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        arrays[(int)Mesh.ArrayType.Normal] = normals;
        arrays[(int)Mesh.ArrayType.TexUV] = uvs.Select(uv => new Godot.Vector2(uv.X, uv.Y)).ToArray();
        arrays[(int)Mesh.ArrayType.Index] = indices;

        var mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        
        if (material != null)
        {
            mesh.SurfaceSetMaterial(0, material);
        }
        
        return mesh;
    }

    private Vector3[] CalculateFaceVertices(Vector3 direction)
    {
        if (direction == Vector3.Forward) // Back face (Z=0)
        {
            return new Vector3[] {
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f)
            };
        }
        else if (direction == Vector3.Back) // Front face (Z=0.5f)
        {
            return new Vector3[] {
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f)
            };
        }
        else if (direction == Vector3.Left) // Left face (X=-0.5f)
        {
            return new Vector3[] {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f)
            };
        }
        else if (direction == Vector3.Right) // Right face (X=0.5f)
        {
            return new Vector3[] {
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f)
            };
        }
        else if (direction == Vector3.Down) // Bottom face (Y=-0.5f)
        {
            return new Vector3[] {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            };
        }
        else if (direction == Vector3.Up) // Top face (Y=0.5f)
        {
            return new Vector3[] {
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f)
            };
        }

        // Default case (should never happen with the defined directions)
        return new Vector3[4];
    }

    private ArrayMesh CreateGrassBlockMesh(Texture2D topTexture, Texture2D bottomTexture, Texture2D frontTexture, Texture2D backTexture, Texture2D leftTexture, Texture2D rightTexture)
    {
        var arrayMesh = new ArrayMesh();
        
        // Create materials for each face type
        var topMaterial = new StandardMaterial3D();
        topMaterial.AlbedoTexture = topTexture;
        topMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        topMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        topMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var bottomMaterial = new StandardMaterial3D();
        bottomMaterial.AlbedoTexture = bottomTexture;
        bottomMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        bottomMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        bottomMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var frontMaterial = new StandardMaterial3D();
        frontMaterial.AlbedoTexture = frontTexture;
        frontMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        frontMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        frontMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var backMaterial = new StandardMaterial3D();
        backMaterial.AlbedoTexture = backTexture;
        backMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        backMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        backMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var leftMaterial = new StandardMaterial3D();
        leftMaterial.AlbedoTexture = leftTexture;
        leftMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        leftMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        leftMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var rightMaterial = new StandardMaterial3D();
        rightMaterial.AlbedoTexture = rightTexture;
        rightMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        rightMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        rightMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var topNormals = new Vector3[] { Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up };
        var topUVs = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        var topIndices = new int[] { 0, 1, 2, 0, 2, 3 }; // Counter-clockwise winding
        
        var topFaceMesh = CreateFace(Vector3.Up, topNormals, topUVs, topIndices, topMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, topFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, topMaterial);
        
        var bottomNormals = new Vector3[] { Vector3.Down, Vector3.Down, Vector3.Down, Vector3.Down };
        var bottomUVs = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        var bottomIndices = new int[] { 0, 1, 2, 0, 2, 3 }; // Counter-clockwise when viewed from below
        
        var bottomFaceMesh = CreateFace(Vector3.Down, bottomNormals, bottomUVs, bottomIndices, bottomMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, bottomFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, bottomMaterial);
        
        var frontNormals = new Vector3[] { Vector3.Forward, Vector3.Forward, Vector3.Forward, Vector3.Forward };
        var frontUVs = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        var frontIndices = new int[] { 2, 1, 0, 3, 2, 0 }; // Clockwise
        
        var frontFaceMesh = CreateFace(Vector3.Forward, frontNormals, frontUVs, frontIndices, frontMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, frontFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, frontMaterial);
        
        var backNormals = new Vector3[] { Vector3.Back, Vector3.Back, Vector3.Back, Vector3.Back };
        var backUVs = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        var backIndices = new int[] { 2, 1, 0, 3, 2, 0 }; // Clockwise
        
        var backFaceMesh = CreateFace(Vector3.Back, backNormals, backUVs, backIndices, backMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, backFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, backMaterial);
        
        var rightNormals = new Vector3[] { Vector3.Right, Vector3.Right, Vector3.Right, Vector3.Right };
        var rightUVs = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        var rightIndices = new int[] { 2, 1, 0, 3, 2, 0 }; // Clockwise
        
        var rightFaceMesh = CreateFace(Vector3.Right, rightNormals, rightUVs, rightIndices, rightMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, rightFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, rightMaterial);
        
        var leftNormals = new Vector3[] { Vector3.Left, Vector3.Left, Vector3.Left, Vector3.Left };
        var leftUVs = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        var leftIndices = new int[] { 2, 1, 0, 3, 2, 0 }; // Clockwise
        
        var leftFaceMesh = CreateFace(Vector3.Left, leftNormals, leftUVs, leftIndices, leftMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, leftFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, leftMaterial);
        
        return arrayMesh;
    }

    private void FindAllMeshInstances(Node node, List<MeshInstance3D> meshInstances)
    {
        if (node is MeshInstance3D meshInstance)
        {
            meshInstances.Add(meshInstance);
        }

        foreach (Node child in node.GetChildren())
        {
            FindAllMeshInstances(child, meshInstances);
        }
    }

    private ArrayMesh CreateHalfSlabMesh(Texture2D topTexture, Texture2D bottomTexture, Texture2D frontTexture, Texture2D backTexture, Texture2D leftTexture, Texture2D rightTexture)
    {
        var arrayMesh = new ArrayMesh();
        
        // Create materials for each face type
        var topMaterial = new StandardMaterial3D();
        topMaterial.AlbedoTexture = topTexture;
        topMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        topMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        topMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var bottomMaterial = new StandardMaterial3D();
        bottomMaterial.AlbedoTexture = bottomTexture;
        bottomMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        bottomMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        bottomMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var frontMaterial = new StandardMaterial3D();
        frontMaterial.AlbedoTexture = frontTexture;
        frontMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        frontMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        frontMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var backMaterial = new StandardMaterial3D();
        backMaterial.AlbedoTexture = backTexture;
        backMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        backMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        backMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var leftMaterial = new StandardMaterial3D();
        leftMaterial.AlbedoTexture = leftTexture;
        leftMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        leftMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        leftMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var rightMaterial = new StandardMaterial3D();
        rightMaterial.AlbedoTexture = rightTexture;
        rightMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        rightMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        rightMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        // Top face: vertices at Y=0, normals Up
        var topNormals = new Vector3[] { Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up };
        var topUVs = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        var topIndices = new int[] { 0, 1, 2, 0, 2, 3 }; // Counter-clockwise winding
        var topVertices = new Vector3[] {
            new Vector3(-0.5f, 0.0f, -0.5f),
            new Vector3(0.5f, 0.0f, -0.5f),
            new Vector3(0.5f, 0.0f, 0.5f),
            new Vector3(-0.5f, 0.0f, 0.5f)
        };
        var topFaceMesh = CreateFaceWithVertices(topVertices, topNormals, topUVs, topIndices, topMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, topFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, topMaterial);
        
        // Bottom face: vertices at Y=-0.5, normals Down
        var bottomNormals = new Vector3[] { Vector3.Down, Vector3.Down, Vector3.Down, Vector3.Down };
        var bottomUVs = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        var bottomIndices = new int[] { 0, 1, 2, 0, 2, 3 }; // Counter-clockwise when viewed from below
        var bottomVertices = new Vector3[] {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, -0.5f)
        };
        var bottomFaceMesh = CreateFaceWithVertices(bottomVertices, bottomNormals, bottomUVs, bottomIndices, bottomMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, bottomFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, bottomMaterial);
        
        // Front face: vertices at Z=0.5, normals Forward, UVs using top half of texture
        var frontNormals = new Vector3[] { Vector3.Forward, Vector3.Forward, Vector3.Forward, Vector3.Forward };
        var frontUVs = new Vector2[] { new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0), new Vector2(0, 0) };
        var frontIndices = new int[] { 2, 1, 0, 3, 2, 0 }; // Clockwise
        var frontVertices = new Vector3[] {
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.0f, 0.5f),
            new Vector3(-0.5f, 0.0f, 0.5f)
        };
        var frontFaceMesh = CreateFaceWithVertices(frontVertices, frontNormals, frontUVs, frontIndices, frontMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, frontFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, frontMaterial);
        
        // Back face: vertices at Z=-0.5, normals Back, UVs using top half of texture
        var backNormals = new Vector3[] { Vector3.Back, Vector3.Back, Vector3.Back, Vector3.Back };
        var backUVs = new Vector2[] { new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0), new Vector2(0, 0) };
        var backIndices = new int[] { 2, 1, 0, 3, 2, 0 }; // Clockwise
        var backVertices = new Vector3[] {
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, 0.0f, -0.5f),
            new Vector3(0.5f, 0.0f, -0.5f)
        };
        var backFaceMesh = CreateFaceWithVertices(backVertices, backNormals, backUVs, backIndices, backMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, backFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, backMaterial);
        
        // Right face: vertices at X=0.5, normals Right, UVs using top half of texture
        var rightNormals = new Vector3[] { Vector3.Right, Vector3.Right, Vector3.Right, Vector3.Right };
        var rightUVs = new Vector2[] { new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0), new Vector2(0, 0) };
        var rightIndices = new int[] { 2, 1, 0, 3, 2, 0 }; // Clockwise
        var rightVertices = new Vector3[] {
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, 0.0f, -0.5f),
            new Vector3(0.5f, 0.0f, 0.5f)
        };
        var rightFaceMesh = CreateFaceWithVertices(rightVertices, rightNormals, rightUVs, rightIndices, rightMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, rightFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, rightMaterial);
        
        // Left face: vertices at X=-0.5, normals Left, UVs using top half of texture
        var leftNormals = new Vector3[] { Vector3.Left, Vector3.Left, Vector3.Left, Vector3.Left };
        var leftUVs = new Vector2[] { new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0), new Vector2(0, 0) };
        var leftIndices = new int[] { 2, 1, 0, 3, 2, 0 }; // Clockwise
        var leftVertices = new Vector3[] {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, 0.0f, 0.5f),
            new Vector3(-0.5f, 0.0f, -0.5f)
        };
        var leftFaceMesh = CreateFaceWithVertices(leftVertices, leftNormals, leftUVs, leftIndices, leftMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, leftFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, leftMaterial);
        
        return arrayMesh;
    }

    private ArrayMesh CreateFaceWithVertices(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int[] indices, StandardMaterial3D material = null)
    {
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        arrays[(int)Mesh.ArrayType.Normal] = normals;
        arrays[(int)Mesh.ArrayType.TexUV] = uvs.Select(uv => new Godot.Vector2(uv.X, uv.Y)).ToArray();
        arrays[(int)Mesh.ArrayType.Index] = indices;

        var mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        
        if (material != null)
        {
            mesh.SurfaceSetMaterial(0, material);
        }
        
        return mesh;
    }

    private ArrayMesh CreateFarmlandMesh(Texture2D topTexture, Texture2D bottomTexture, Texture2D frontTexture, Texture2D backTexture, Texture2D leftTexture, Texture2D rightTexture)
    {
        var arrayMesh = new ArrayMesh();
        
        // Create materials for each face type
        var topMaterial = new StandardMaterial3D();
        topMaterial.AlbedoTexture = topTexture;
        topMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        topMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        topMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var bottomMaterial = new StandardMaterial3D();
        bottomMaterial.AlbedoTexture = bottomTexture;
        bottomMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        bottomMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        bottomMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var frontMaterial = new StandardMaterial3D();
        frontMaterial.AlbedoTexture = frontTexture;
        frontMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        frontMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        frontMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var backMaterial = new StandardMaterial3D();
        backMaterial.AlbedoTexture = backTexture;
        backMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        backMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        backMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var leftMaterial = new StandardMaterial3D();
        leftMaterial.AlbedoTexture = leftTexture;
        leftMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        leftMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        leftMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        var rightMaterial = new StandardMaterial3D();
        rightMaterial.AlbedoTexture = rightTexture;
        rightMaterial.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        rightMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        rightMaterial.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        // Top face: vertices at Y=0.4375 (lowered by 0.0625), normals Up
        var topNormals = new Vector3[] { Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up };
        var topUVs = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        var topIndices = new int[] { 0, 1, 2, 0, 2, 3 }; // Counter-clockwise winding
        var topVertices = new Vector3[] {
            new Vector3(-0.5f, 0.4375f, -0.5f),  // Lowered by 0.0625
            new Vector3(0.5f, 0.4375f, -0.5f),   // Lowered by 0.0625
            new Vector3(0.5f, 0.4375f, 0.5f),    // Lowered by 0.0625
            new Vector3(-0.5f, 0.4375f, 0.5f)    // Lowered by 0.0625
        };
        var topFaceMesh = CreateFaceWithVertices(topVertices, topNormals, topUVs, topIndices, topMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, topFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, topMaterial);
        
        // Bottom face: vertices at Y=-0.5, normals Down
        var bottomNormals = new Vector3[] { Vector3.Down, Vector3.Down, Vector3.Down, Vector3.Down };
        var bottomUVs = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        var bottomIndices = new int[] { 0, 1, 2, 0, 2, 3 }; // Counter-clockwise when viewed from below
        var bottomVertices = new Vector3[] {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, -0.5f)
        };
        var bottomFaceMesh = CreateFaceWithVertices(bottomVertices, bottomNormals, bottomUVs, bottomIndices, bottomMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, bottomFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, bottomMaterial);
        
        // Front face: vertices at Z=0.5, normals Forward
        var frontNormals = new Vector3[] { Vector3.Forward, Vector3.Forward, Vector3.Forward, Vector3.Forward };
        var frontUVs = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0.0625f), new Vector2(0, 0.0625f) }; // Top UVs moved down to 0.0625
        var frontIndices = new int[] { 2, 1, 0, 3, 2, 0 }; // Clockwise
        var frontVertices = new Vector3[] {
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.4375f, 0.5f),  // Top vertex lowered by 0.0625
            new Vector3(-0.5f, 0.4375f, 0.5f)   // Top vertex lowered by 0.0625
        };
        var frontFaceMesh = CreateFaceWithVertices(frontVertices, frontNormals, frontUVs, frontIndices, frontMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, frontFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, frontMaterial);
        
        // Back face: vertices at Z=-0.5, normals Back
        var backNormals = new Vector3[] { Vector3.Back, Vector3.Back, Vector3.Back, Vector3.Back };
        var backUVs = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0.0625f), new Vector2(0, 0.0625f) }; // Top UVs moved down to 0.0625
        var backIndices = new int[] { 2, 1, 0, 3, 2, 0 }; // Clockwise
        var backVertices = new Vector3[] {
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, 0.4375f, -0.5f),  // Top vertex lowered by 0.0625
            new Vector3(0.5f, 0.4375f, -0.5f)    // Top vertex lowered by 0.0625
        };
        var backFaceMesh = CreateFaceWithVertices(backVertices, backNormals, backUVs, backIndices, backMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, backFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, backMaterial);
        
        // Right face: vertices at X=0.5, normals Right
        var rightNormals = new Vector3[] { Vector3.Right, Vector3.Right, Vector3.Right, Vector3.Right };
        var rightUVs = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0.0625f), new Vector2(0, 0.0625f) }; // Top UVs moved down to 0.0625
        var rightIndices = new int[] { 2, 1, 0, 3, 2, 0 }; // Clockwise
        var rightVertices = new Vector3[] {
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, 0.4375f, -0.5f),  // Top vertex lowered by 0.0625
            new Vector3(0.5f, 0.4375f, 0.5f)     // Top vertex lowered by 0.0625
        };
        var rightFaceMesh = CreateFaceWithVertices(rightVertices, rightNormals, rightUVs, rightIndices, rightMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, rightFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, rightMaterial);
        
        // Left face: vertices at X=-0.5, normals Left
        var leftNormals = new Vector3[] { Vector3.Left, Vector3.Left, Vector3.Left, Vector3.Left };
        var leftUVs = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0.0625f), new Vector2(0, 0.0625f) }; // Top UVs moved down to 0.0625
        var leftIndices = new int[] { 2, 1, 0, 3, 2, 0 }; // Clockwise
        var leftVertices = new Vector3[] {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, 0.4375f, 0.5f),  // Top vertex lowered by 0.0625
            new Vector3(-0.5f, 0.4375f, -0.5f)   // Top vertex lowered by 0.0625
        };
        var leftFaceMesh = CreateFaceWithVertices(leftVertices, leftNormals, leftUVs, leftIndices, leftMaterial);
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, leftFaceMesh.SurfaceGetArrays(0));
        arrayMesh.SurfaceSetMaterial(arrayMesh.GetSurfaceCount() - 1, leftMaterial);
        
        return arrayMesh;
    }
}