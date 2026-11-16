using UnityEditor;
using UnityEngine;

/// <summary>
/// Configura automaticamente los PNG de Assets/Resources/GatoCafe/ como Sprites
/// (pixel art: filtro Point, sin compresion, pivote en los pies).
/// </summary>
public class GatoCafeSpriteImporter : AssetPostprocessor
{
    // Si los PNG se importaron antes de que compilara este script, los reimporta una vez.
    [InitializeOnLoadMethod]
    static void ArreglarImportacion()
    {
        EditorApplication.delayCall += () =>
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/GatoCafe" });
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti != null && (ti.textureType != TextureImporterType.Sprite || ti.filterMode != FilterMode.Point))
                    ti.SaveAndReimport();
            }
        };
    }

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
