using UnityEditor;
using UnityEngine;

/// <summary>
/// La cancion de la plaza se reproduce en streaming: es un mp3 largo
/// y no conviene descomprimirlo entero en memoria.
/// Los wav cortos (moneda) quedan con la configuracion normal.
/// </summary>
public class MusicaPlazaImportador : AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        if (!assetPath.StartsWith("Assets/SonidosPlaza/")) return;
        if (!assetPath.EndsWith(".mp3")) return;

        var importador = (AudioImporter)assetImporter;
        var ajustes = importador.defaultSampleSettings;
        ajustes.loadType = AudioClipLoadType.Streaming;
        ajustes.compressionFormat = AudioCompressionFormat.Vorbis;
        ajustes.quality = 0.7f;
        ajustes.preloadAudioData = false;
        importador.defaultSampleSettings = ajustes;
    }
}
