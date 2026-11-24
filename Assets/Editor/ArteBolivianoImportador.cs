using UnityEditor;
using UnityEngine;

/// <summary>
/// Fuerza el import correcto de los PNG de Assets/ArteBoliviano:
/// sprite unico (no hoja), pixel art nitido y sin compresion.
/// Sin esto, algunos PNG nuevos entran cortados en pedazos.
/// </summary>
public class ArteBolivianoImportador : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/ArteBoliviano/")) return;

        var importador = (TextureImporter)assetImporter;
        importador.textureType = TextureImporterType.Sprite;
        importador.spriteImportMode = SpriteImportMode.Single;
        importador.spritePixelsPerUnit = 100;
        importador.filterMode = FilterMode.Point;
        importador.textureCompression = TextureImporterCompression.Uncompressed;
        importador.mipmapEnabled = false;
        importador.alphaIsTransparency = true;
    }
}
