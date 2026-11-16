using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SelvaImportador : AssetPostprocessor
{
    static readonly string[] Rutas={"Assets/Resources/Sprites","Assets/Resources/Imagenes"};
    static SelvaImportador() { EditorApplication.delayCall += Revisar; }
    static void Revisar()
    {
        foreach(var guid in AssetDatabase.FindAssets("t:Texture2D",Rutas))
        {
            var t=AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;
            if(t && (t.textureType!=TextureImporterType.Sprite || t.spriteImportMode!=SpriteImportMode.Single || t.spritePixelsPerUnit!=100 || t.textureCompression!=TextureImporterCompression.Uncompressed || (t.assetPath.EndsWith("InterfazOriginal.png") && !t.isReadable)))
            { Configurar(t); t.SaveAndReimport(); }
        }
    }
    void OnPreprocessTexture() { foreach(var ruta in Rutas) if(assetPath.StartsWith(ruta+"/")) { Configurar((TextureImporter)assetImporter); break; } }
    static void Configurar(TextureImporter t)
    {
        t.isReadable=t.assetPath.EndsWith("InterfazOriginal.png");
        t.textureType=TextureImporterType.Sprite;
        t.spriteImportMode=SpriteImportMode.Single;
        t.spritePixelsPerUnit=100;
        t.alphaSource=TextureImporterAlphaSource.FromInput;
        t.alphaIsTransparency=true;
        t.mipmapEnabled=false;
        t.wrapMode=TextureWrapMode.Clamp;
        t.filterMode=FilterMode.Bilinear;
        t.maxTextureSize=2048;
        t.textureCompression=TextureImporterCompression.Uncompressed;
        var settings=new TextureImporterSettings(); t.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;
        t.SetTextureSettings(settings);
    }
}
