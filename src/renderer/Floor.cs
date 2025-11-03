using Godot;

namespace SimplyRemadeMI.renderer;

public partial class Floor : MeshInstance3D
{
    private bool _meshCreated = false;

    public void SetTexture(Texture2D texture)
    {
        var mat = (StandardMaterial3D)MaterialOverride;
        mat.AlbedoTexture = texture;
    }

    public override void _Process(double delta)
    {
        var main = GetNode<Main>("/root/Main");
        if (main == null)
        {
            // Main node not ready yet, try again later
            return;
        }

        if (!_meshCreated)
        {
            TrySetTexture(main.TerrainTextures["tile040"]);
        }
    }

    private void TrySetTexture(Texture2D texture)
    {
        var mat = (StandardMaterial3D)MaterialOverride;
        mat.AlbedoTexture = texture;
        _meshCreated = true;
    }
}
