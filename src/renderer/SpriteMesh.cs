using Godot;

namespace SimplyRemadeMI.renderer;

public partial class SpriteMesh : MeshInstance3D
{
    [Export] private MeshInstance3D MeshInstance { get; set; }
    
    public Texture2D Texture { get; set; }
    public StandardMaterial3D ReferenceMaterial { get; set; }

    public override void _Ready()
    {
        if (MeshInstance.HasMethod("init_tex"))
        {
            MeshInstance.Call("init_tex", Texture);
        }
    }

    public override void _Process(double delta)
    {
        var mat = (StandardMaterial3D)MeshInstance.GetSurfaceOverrideMaterial(0);
        if (Mesh == null) return;
        var mat2 = (StandardMaterial3D)Mesh.SurfaceGetMaterial(0);
        if (mat != null && mat2 != null)
        {
            mat.AlbedoColor = new Color(mat.AlbedoColor.R,  mat.AlbedoColor.G, mat.AlbedoColor.B, mat2.AlbedoColor.A);
        }
    }

    private void Init()
    {
        Mesh = MeshInstance.GetMesh();
        Mesh.SurfaceSetMaterial(0, ReferenceMaterial);
        
        var mat = (StandardMaterial3D)MeshInstance.GetSurfaceOverrideMaterial(0);
        mat.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        mat.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly;
        
        Main.GetInstance().MainViewport.UpdatePicking();
    }

    public Texture2D GetTexture()
    {
        return Texture;
    }
}