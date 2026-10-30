using Godot;

namespace ProjetoRpg.Client.Scripts;

/// <summary>
/// Applies the Coast GLB's authored material palette explicitly so its visible
/// mesh colors do not depend on glTF material propagation during import. The
/// source GLB contains color-only PBR materials, with no image/texture payloads.
/// </summary>
internal static class CoastMaterialFallback
{
    private static readonly Color[] BaseColors =
    {
        new(0.3371636569f, 0.3419145346f, 0.3185468614f), // 0 base stone
        new(0.1094617397f, 0.1094617546f, 0.1094617322f), // 1 cobble
        new(0.1094617397f, 0.1094617546f, 0.1094617322f), // 2 stone wall
        new(0.0761854053f, 0.0612460710f, 0.0451862291f), // 3 dark wood
        new(0.6444799304f, 0.4564110637f, 0.2622507215f), // 4 thatch
        new(0.0761854053f, 0.0612460710f, 0.0451862291f), // 5 wood
        new(0.0765796676f, 0.0615984164f, 0.0451840051f), // 6 barn body
        new(0.3613067865f, 0.2581830025f, 0.1746475101f), // 7 roof
        new(0.0231533647f, 0.0284260418f, 0.0168073773f), // 8 pine foliage
        new(0.1412633508f, 0.0703601167f, 0.0382043794f), // 9 pine trunk
        new(0.3613067865f, 0.2581830025f, 0.1746475101f), // 10 roof
        new(0.0765796676f, 0.0615984164f, 0.0451840051f), // 11 body
        new(0.0761854053f, 0.0612460710f, 0.0451862291f), // 12 body
        new(0.3613067865f, 0.2581830025f, 0.1746475101f), // 13 roof
        new(0.1620294452f, 0.1620294452f, 0.1589608490f), // 14 metal
        new(0.3613067865f, 0.2581830025f, 0.1746475101f), // 15 roof
        new(0.0765796676f, 0.0615984164f, 0.0451840051f), // 16 body
        new(0.3613067865f, 0.2581830025f, 0.1746475101f), // 17 roof
        new(0.0765796676f, 0.0615984164f, 0.0451840051f), // 18 body
        new(0.0000000000f, 0.1024740487f, 0.0022455628f), // 19 grass
        new(0.5394796133f, 0.4452012479f, 0.2158605456f), // 20 sand
        new(0.1470273435f, 0.0886555985f, 0.0212190169f), // 21 dirt road
        new(0.1733554155f, 0.1439517736f, 0.0725015923f)  // 22 cliff sand
    };

    private static readonly float[] Roughness =
    {
        0.4f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.4f, 0.5f, 0.7f, 1.0f,
        0.5f, 0.4f, 0.4f, 0.5f, 0.5f, 0.5f, 0.4f, 0.5f, 0.4f, 1.0f,
        1.0f, 1.0f, 1.0f
    };

    private static readonly StandardMaterial3D[] Materials = CreateMaterials();

    public static void Apply(Node root)
    {
        var overriddenSurfaces = ApplyRecursively(root);
        if (overriddenSurfaces == 0)
        {
            GD.PushWarning("Coast GLB palette fallback found no named mesh surfaces; verify CoastModel.glb import names.");
        }
    }

    private static int ApplyRecursively(Node node)
    {
        var applied = 0;
        if (node is MeshInstance3D meshInstance && meshInstance.Mesh is { } mesh &&
            TryGetMaterialIndices(node.Name.ToString(), out var materialIndices))
        {
            var count = Math.Min(mesh.GetSurfaceCount(), materialIndices.Length);
            for (var surface = 0; surface < count; surface++)
            {
                meshInstance.SetSurfaceOverrideMaterial(surface, Materials[materialIndices[surface]]);
                applied++;
            }
        }

        foreach (Node child in node.GetChildren())
        {
            applied += ApplyRecursively(child);
        }

        return applied;
    }

    private static bool TryGetMaterialIndices(string nodeName, out int[] materialIndices)
    {
        materialIndices = nodeName switch
        {
            "tavern" => new[] { 0, 1, 2, 3, 4 },
            "pier" or "pier.001" or "pier.002" or "pier_001" or "pier_002" => new[] { 5 },
            "barn" => new[] { 6, 7 },
            "House.002" or "House_002" => new[] { 10, 11 },
            "Blacksmith" => new[] { 12, 13, 14 },
            "House" => new[] { 15, 16 },
            "House.001" or "House_001" => new[] { 17, 18 },
            "Plane" => new[] { 19, 20, 21, 22 },
            _ when nodeName.StartsWith("pinetree", StringComparison.Ordinal) => new[] { 8, 9 },
            _ => System.Array.Empty<int>()
        };

        return materialIndices.Length > 0;
    }

    private static StandardMaterial3D[] CreateMaterials()
    {
        var materials = new StandardMaterial3D[BaseColors.Length];
        for (var index = 0; index < materials.Length; index++)
        {
            materials[index] = new StandardMaterial3D
            {
                AlbedoColor = BaseColors[index],
                Metallic = 0.0f,
                Roughness = Roughness[index]
            };
        }

        return materials;
    }
}
