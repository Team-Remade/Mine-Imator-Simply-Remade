using System.Collections.Generic;
using Godot;

namespace MineImatorSimplyRemade;

public class AtlasTexture
{
	public const int Size = 8192;
	
	public readonly string atlasId;
	private Dictionary<string, Rect2I> textures = new();
	private MaxRectsBinPack rectsBinPack = new(Size, Size, false);

	private const MaxRectsBinPack.FreeRectChoiceHeuristic Method =
		MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestShortSideFit;

	private Image atlasImage = Image.Create(Size, Size, true, Image.Format.Rgba8);
	private ImageTexture imageTexture;

	public AtlasTexture(string atlasId)
	{
		this.atlasId = atlasId;
	}

	public ImageTexture getImageTexture()
	{
		if (imageTexture == null)
			imageTexture = ImageTexture.CreateFromImage(atlasImage);
		else
			imageTexture.Update(atlasImage);
		
		return imageTexture;
	}

	public Rect2I getTexture(string id, string provider = "minecraft")
	{
		return textures[$"{provider}:{id}"];
	}

	public void registerTexture(string id, Image image)
	{
		var width = image.GetWidth();
		var height = image.GetHeight();

		var rect = rectsBinPack.Insert(width, height, Method);

		if (rect.Size == Vector2I.Zero)
			return;
		
		atlasImage.BlitRect(image, new Rect2I(0, 0, width, height), new Vector2I(rect.Position.X, rect.Position.Y));
		textures[id] = rect;
	}

	public void dumpAtlas()
	{
		foreach (var texture in textures)
		{
			GD.Print($"{texture.Key} - {texture.Value.Size.X}x{texture.Value.Size.Y} {texture.Value.Position.X},{texture.Value.Position.Y}");
		}

		var rootDir = DirAccess.Open("res://");
		if (!rootDir.DirExists("debug"))
			rootDir.MakeDir("debug");
		
		atlasImage.SavePng($"res://debug/atlas_{atlasId}.png");
	}

	public void loadTextures(string provider, DirAccess dir, string originalPath = null)
	{
		// Recursively traverse the dir
		foreach (var directory in dir.GetDirectories())
		{
			var next = DirAccess.Open($"{dir.GetCurrentDir()}/{directory}");
			loadTextures(provider, next, originalPath ?? dir.GetCurrentDir());
		}
		
		foreach (var file in dir.GetFiles())
		{
			// Not a supported image
			if (!file.EndsWith(".png"))
				continue;

			var image = ResourceLoader.Load<Texture2D>($"{dir.GetCurrentDir()}/{file}").GetImage();
			image.Convert(Image.Format.Rgba8);

			string id;
			if (originalPath != null)
			{
				var mainPath = dir.GetCurrentDir();
				id = $"{provider}:{mainPath.Replace(originalPath, "")}/{file}";
			}
			else
			{
				id = $"{provider}:{file.TrimSuffix(".png")}";
			}
			
			registerTexture(id, image);
		}
	}
}
