using Godot;

namespace SimplyRemadeMI.renderer;

public partial class Floor : MeshInstance3D
{
    private bool _meshCreated = false;

    public override void _Ready()
    {
        // Try to create the mesh immediately, but defer if Main isn't ready yet
        TryCreateMesh();
    }

    public override void _Process(double delta)
    {
        // If mesh hasn't been created yet, try again each frame until successful
        if (!_meshCreated)
        {
            TryCreateMesh();
        }
    }

    private void TryCreateMesh()
    {
        if (_meshCreated) return;

        // Get the main instance and use the existing terrain atlas
        var main = GetNode<Main>("/root/Main");
        if (main == null)
        {
            // Main node not ready yet, try again later
            return;
        }

        var terrainAtlas = main.TerrainAtlas;
        if (terrainAtlas == null)
        {
            // Atlas not initialized yet, try again later
            return;
        }

        // Get UV coordinates for tile040 from the atlas
        var uvRect = terrainAtlas.GetTextureUvRect("res://assets/sprite/terrain/tile040.png");
        if (uvRect.Size == Vector2I.Zero)
        {
            return;
        }

        // Debug output to check UV rect and atlas dimensions
        //GD.Print($"Tile040 UV rect: Position={uvRect.Position}, Size={uvRect.Size}");

        // Get atlas texture dimensions for normalization
        var atlasTex = terrainAtlas.GetAtlasTexture();
        float atlasWidth = atlasTex.GetWidth();
        float atlasHeight = atlasTex.GetHeight();
        //GD.Print($"Atlas dimensions: {atlasWidth}x{atlasHeight}");

        // Calculate normalized UV coordinates for the tile
        float minU = uvRect.Position.X / atlasWidth + 0.0001f;
        float maxU = (uvRect.Position.X + uvRect.Size.X) / atlasWidth - 0.0001f;
        float minV = uvRect.Position.Y / atlasHeight + 0.0001f;
        float maxV = (uvRect.Position.Y + uvRect.Size.Y) / atlasHeight - 0.0001f;
        //GD.Print($"Normalized UV: minU={minU}, maxU={maxU}, minV={minV}, maxV={maxV}");
        
        // Create lists for vertices, UVs, normals, and indices for the grid
        var verticesList = new System.Collections.Generic.List<Vector3>();
        var uvsList = new System.Collections.Generic.List<Vector2>();
        var normalsList = new System.Collections.Generic.List<Vector3>();
        var indicesList = new System.Collections.Generic.List<int>();

        // Number of tiles in each direction: 64x64 for 64x64 meter floor
        int numTilesX = 64;
        int numTilesZ = 64;

        // Loop through each tile position
        for (int i = 0; i < numTilesX; i++)
        {
            for (int j = 0; j < numTilesZ; j++)
            {
                // Calculate the four corners of the current quad (1x1 meter)
                float x0 = -32 + i;
                float z0 = -32 + j;
                float x1 = x0 + 1;
                float z1 = z0 + 1;

                // Add vertices for the quad
                verticesList.Add(new Vector3(x0, 0, z0)); // Bottom-left
                verticesList.Add(new Vector3(x1, 0, z0)); // Bottom-right
                verticesList.Add(new Vector3(x1, 0, z1)); // Top-right
                verticesList.Add(new Vector3(x0, 0, z1)); // Top-left

                // Add UVs for the quad - each vertex gets the full tile040 UV area
                uvsList.Add(new Vector2(minU, maxV)); // Bottom-left
                uvsList.Add(new Vector2(maxU, maxV)); // Bottom-right
                uvsList.Add(new Vector2(maxU, minV)); // Top-right
                uvsList.Add(new Vector2(minU, minV)); // Top-left

                // Add normals for the quad - all pointing up (Y+)
                normalsList.Add(Vector3.Up); // Bottom-left
                normalsList.Add(Vector3.Up); // Bottom-right
                normalsList.Add(Vector3.Up); // Top-right
                normalsList.Add(Vector3.Up); // Top-left

                // Add indices for the two triangles of the quad
                int baseIndex = verticesList.Count - 4;
                indicesList.Add(baseIndex + 0); // Triangle 1
                indicesList.Add(baseIndex + 1);
                indicesList.Add(baseIndex + 2);
                indicesList.Add(baseIndex + 2); // Triangle 2
                indicesList.Add(baseIndex + 3);
                indicesList.Add(baseIndex + 0);
            }
        }

        // Convert lists to arrays
        var vertices = verticesList.ToArray();
        var uvs = uvsList.ToArray();
        var normals = normalsList.ToArray();
        var indices = indicesList.ToArray();

        // Create mesh arrays
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        arrays[(int)Mesh.ArrayType.TexUV] = uvs;
        arrays[(int)Mesh.ArrayType.Normal] = normals;
        arrays[(int)Mesh.ArrayType.Index] = indices;

        // Create and assign ArrayMesh
        var arrayMesh = new ArrayMesh();
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        Mesh = arrayMesh;

        // Create material with atlas texture
        if (atlasTex == null)
        {
            GD.PrintErr("Atlas texture is null");
            return;
        }

        // Create a new material with the atlas texture
        var material = (StandardMaterial3D)MaterialOverride;
        material.AlbedoTexture = atlasTex;
        // Set texture filtering to avoid pixelation when stretched
        material.TextureFilter = BaseMaterial3D.TextureFilterEnum.NearestWithMipmapsAnisotropic;
        MaterialOverride = material;

        _meshCreated = true;
        SetProcess(false); // Stop processing once mesh is created
    }
}
