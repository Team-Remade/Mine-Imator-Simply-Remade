using System.Collections.Generic;
using Godot;

namespace MineImatorSimplyRemade.scripts;

public partial class Atlas : Resource
{
    public string Name = "";
    public string ResourceLocation = "";
    
    public List<Texture2D>  Textures = new();
    
    public void Init()
    {
        using var dir = DirAccess.Open(ResourceLocation);

        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();

            while (!string.IsNullOrEmpty(fileName))
            {
                if (fileName.EndsWith(".png"))
                {
                    var texture = ResourceLoader.Load<Texture2D>(ResourceLocation + fileName);
                    Textures.Add(texture);
                }
                fileName = dir.GetNext();
            }
        }
    }
}