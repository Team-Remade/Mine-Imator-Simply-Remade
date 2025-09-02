using System;
using System.Collections.Generic;
using System.IO;
using Godot;

public class TextureAtlas
{
    private int _width;
    private int _height;
    private List<Texture2D> _textures = new();
    private List<string> _texturePaths = new();
    private List<Rect2I> _uvRects = new();
    private MaxRectBinPack _binPacker;
    private Texture2D _atlasTexture;
    
    public TextureAtlas(int width, int height)
    {
        _width = width;
        _height = height;
        _binPacker = new MaxRectBinPack(width, height);
    }

    public bool AddTexture(string texturePath, Texture2D texture)
    {
        int width = texture.GetWidth();
        int height = texture.GetHeight();
        //GD.Print($"Attempting to add texture {texturePath} ({width}x{height}) to atlas");
        
        var rect = _binPacker.Insert(
            width,
            height,
            MaxRectBinPack.FreeRectChoiceHeuristic.RectBestShortSideFit);

        if (rect.HasValue)
        {
            //GD.Print($"Texture added at position: {rect.Value.Position}, size: {rect.Value.Size}");
            _textures.Add(texture);
            _texturePaths.Add(texturePath);
            _uvRects.Add(rect.Value);
            return true;
        }
        else
        {
            GD.PrintErr($"Failed to pack texture {texturePath} ({width}x{height}) - no space in atlas");
            return false;
        }
    }

    public void LoadTexturesFromPattern(string pattern)
    {
        // Extract the numeric pattern part (tile###.png -> ###)
        string numericPattern = pattern.Substring(pattern.IndexOf("tile") + 4);
        numericPattern = numericPattern.Substring(0, numericPattern.IndexOf(".png"));
        int digits = numericPattern.Length;
        
        //GD.Print($"Loading textures with pattern: {pattern}, digits: {digits}");
        
        // Load all matching textures using Godot's resource system
        for (int i = 0; i < 1000; i++) // Support up to 999 tiles
        {
            string paddedNumber = i.ToString().PadLeft(digits, '0');
            string fullPath = pattern.Replace("###", paddedNumber);
            
            if (ResourceLoader.Exists(fullPath))
            {
                //GD.Print($"Found texture: {fullPath}");
                Texture2D texture = ResourceLoader.Load<Texture2D>(fullPath);
                if (texture != null)
                {
                    //GD.Print($"Loaded texture: {fullPath}, size: {texture.GetWidth()}x{texture.GetHeight()}");
                    if (!AddTexture(fullPath, texture))
                    {
                        GD.PrintErr($"Failed to add texture to atlas: {fullPath}");
                    }
                    else
                    {
                        //GD.Print($"Successfully added texture to atlas: {fullPath}");
                    }
                }
                else
                {
                    GD.PrintErr($"Failed to load texture: {fullPath}");
                }
            }
            else
            {
                GD.Print($"Texture not found: {fullPath}, stopping sequence");
                // Stop when we find a gap in the sequence
                break;
            }
        }
    }

    public void GenerateAtlas()
    {
        if (_textures.Count == 0)
            return;

        // Create a new image for the atlas
        Image atlasImage = Image.Create(_width, _height, false, Image.Format.Rgba8);
        
        // Blit each texture into the atlas at its UV position
        for (int i = 0; i < _textures.Count; i++)
        {
            Texture2D texture = _textures[i];
            Rect2I uvRect = _uvRects[i];
            
            Image textureImage = texture.GetImage();
            
            // Convert texture image to the same format as atlas if needed
            if (textureImage.GetFormat() != atlasImage.GetFormat())
            {
                textureImage.Convert(atlasImage.GetFormat());
            }
            
            atlasImage.BlitRect(textureImage, new Rect2I(0, 0, textureImage.GetWidth(), textureImage.GetHeight()), uvRect.Position);
        }
        
        // Create texture from image
        _atlasTexture = ImageTexture.CreateFromImage(atlasImage);
    }

    public Texture2D GetAtlasTexture()
    {
        return _atlasTexture;
    }

    public Rect2I GetTextureUvRect(int index)
    {
        if (index >= 0 && index < _uvRects.Count)
            return _uvRects[index];
        return new Rect2I();
    }

    public Rect2I GetTextureUvRect(string texturePath)
    {
        int index = _texturePaths.IndexOf(texturePath);
        if (index >= 0)
            return _uvRects[index];
        return new Rect2I();
    }

    public int GetTextureCount()
    {
        return _textures.Count;
    }

    public class MaxRectBinPack
    {
        public enum FreeRectChoiceHeuristic
        {
            RectBestShortSideFit,
            RectBestLongSideFit,
            RectBestAreaFit
        }

        private List<Rect2I> _freeRects = new();

        public MaxRectBinPack(int width, int height)
        {
            _freeRects.Add(new Rect2I(0, 0, width, height));
        }

        public Rect2I? Insert(int width, int height, FreeRectChoiceHeuristic method)
        {
            //GD.Print($"Free rectangles count: {_freeRects.Count}");
            for (int i = 0; i < _freeRects.Count; i++)
            {
                //GD.Print($"Free rect {i}: {_freeRects[i]}");
            }

            int bestScore1 = int.MaxValue;
            int bestScore2 = int.MaxValue;
            Rect2I bestRect = new Rect2I();
            int bestIndex = -1;

            for (int i = 0; i < _freeRects.Count; i++)
            {
                Rect2I rect = _freeRects[i];
                if (rect.Size.X >= width && rect.Size.Y >= height)
                {
                    int score1 = Math.Abs(rect.Size.X - width) + Math.Abs(rect.Size.Y - height);
                    int score2 = Math.Min(rect.Size.X - width, rect.Size.Y - height);

                    if (score1 < bestScore1 || (score1 == bestScore1 && score2 < bestScore2))
                    {
                        bestScore1 = score1;
                        bestScore2 = score2;
                        bestRect = new Rect2I(rect.Position.X, rect.Position.Y, width, height);
                        bestIndex = i;
                    }
                }
            }

            if (bestIndex == -1)
            {
                GD.PrintErr("No suitable free rectangle found");
                return null;
            }

            //GD.Print($"Best free rect found at index {bestIndex}: {_freeRects[bestIndex]}");
            //GD.Print($"Chosen rect: {bestRect}");

            Rect2I chosen = bestRect;
            
            // First split the free rect we're using and remove it
            //GD.Print($"Splitting used free rect {bestIndex}: {_freeRects[bestIndex]} against chosen {chosen}");
            SplitFreeRect(_freeRects[bestIndex], chosen);
            _freeRects.RemoveAt(bestIndex);
            
            // Then process other free rects that intersect with the chosen rect
            int numToProcess = _freeRects.Count;
            for (int i = numToProcess - 1; i >= 0; i--)
            {
                if (_freeRects[i].Intersects(chosen))
                {
                    //GD.Print($"Splitting intersecting free rect {i}: {_freeRects[i]} against chosen {chosen}");
                    SplitFreeRect(_freeRects[i], chosen);
                    _freeRects.RemoveAt(i);
                }
            }
            
            //GD.Print($"After processing, free rectangles count: {_freeRects.Count}");
            for (int i = 0; i < _freeRects.Count; i++)
            {
                //GD.Print($"Remaining free rect {i}: {_freeRects[i]}");
            }
            
            return chosen;
        }

        private void SplitFreeRect(Rect2I freeRect, Rect2I usedRect)
        {
            //GD.Print($"Splitting freeRect: {freeRect} with usedRect: {usedRect}");
            
            // Left part
            if (usedRect.Position.X > freeRect.Position.X)
            {
                int width = usedRect.Position.X - freeRect.Position.X;
                if (width > 0)
                {
                    var leftRect = new Rect2I(
                        freeRect.Position.X,
                        freeRect.Position.Y,
                        width,
                        freeRect.Size.Y
                    );
                    _freeRects.Add(leftRect);
                    //GD.Print($"Added left free rect: {leftRect}");
                }
            }

            // Right part
            if (usedRect.Position.X + usedRect.Size.X < freeRect.Position.X + freeRect.Size.X)
            {
                int width = freeRect.Position.X + freeRect.Size.X - (usedRect.Position.X + usedRect.Size.X);
                if (width > 0)
                {
                    var rightRect = new Rect2I(
                        usedRect.Position.X + usedRect.Size.X,
                        freeRect.Position.Y,
                        width,
                        freeRect.Size.Y
                    );
                    _freeRects.Add(rightRect);
                    //GD.Print($"Added right free rect: {rightRect}");
                }
            }

            // Top part
            if (usedRect.Position.Y > freeRect.Position.Y)
            {
                int height = usedRect.Position.Y - freeRect.Position.Y;
                if (height > 0)
                {
                    var topRect = new Rect2I(
                        freeRect.Position.X,
                        freeRect.Position.Y,
                        freeRect.Size.X,
                        height
                    );
                    _freeRects.Add(topRect);
                    //GD.Print($"Added top free rect: {topRect}");
                }
            }

            // Bottom part
            if (usedRect.Position.Y + usedRect.Size.Y < freeRect.Position.Y + freeRect.Size.Y)
            {
                int height = freeRect.Position.Y + freeRect.Size.Y - (usedRect.Position.Y + usedRect.Size.Y);
                if (height > 0)
                {
                    var bottomRect = new Rect2I(
                        freeRect.Position.X,
                        usedRect.Position.Y + usedRect.Size.Y,
                        freeRect.Size.X,
                        height
                    );
                    _freeRects.Add(bottomRect);
                    //GD.Print($"Added bottom free rect: {bottomRect}");
                }
            }
        }
    }
}