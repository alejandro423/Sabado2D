using UnityEditor;
using UnityEngine;

/// <summary>
/// Configura automaticamente los PNG de Assets/Resources/GatoCafe/ como Sprites
/// (pixel art: filtro Point, sin compresion, pivote en los pies).
/// </summary>
public class GatoCafeSpriteImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("/Resources/GatoCafe/")) return;

        var ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 200;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;

        var s = new TextureImporterSettings();
        ti.ReadTextureSettings(s);
        s.spriteAlignment = (int)SpriteAlignment.Custom;
        s.spritePivot = new Vector2(0.435f, 0.023f);
        ti.SetTextureSettings(s);
    }
}
