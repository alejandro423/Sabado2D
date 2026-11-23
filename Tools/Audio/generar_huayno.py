# genera la musica de fondo de la plaza, un huayno con musicgen
# deja el wav en Assets/SonidosPlaza/huayno_plaza.wav
# se puede cambiar DESCRIPCION o DURACION y volver a correr

import wave

import numpy as np
from transformers import AutoProcessor, MusicgenForConditionalGeneration

DESCRIPCION = (
    "upbeat andean huayno folk music, charango and quena flute melodies, "
    "bombo drum keeping a lively beat, joyful bolivian plaza festival mood"
)

DURACION = 15.0
TOKENS_POR_SEGUNDO = 50
ARCHIVO = "Assets/SonidosPlaza/huayno_plaza.wav"


def main():
    procesador = AutoProcessor.from_pretrained("facebook/musicgen-small")
    modelo = MusicgenForConditionalGeneration.from_pretrained("facebook/musicgen-small")

    entrada = procesador(text=[DESCRIPCION], padding=True, return_tensors="pt")

    audio = modelo.generate(
        **entrada,
        do_sample=True,
        guidance_scale=3.0,
        max_new_tokens=int(DURACION * TOKENS_POR_SEGUNDO),
    )

    frecuencia = modelo.config.audio_encoder.sampling_rate

    # a 16 bits, wav mono con la libreria estandar
    datos = audio[0, 0].cpu().numpy()
    datos = datos / max(1e-9, float(np.abs(datos).max())) * 0.9
    enteros = (datos * 32767).astype(np.int16)

    with wave.open(ARCHIVO, "wb") as archivo:
        archivo.setnchannels(1)
        archivo.setsampwidth(2)
        archivo.setframerate(frecuencia)
        archivo.writeframes(enteros.tobytes())

    print("listo:", ARCHIVO, len(enteros) / frecuencia, "segundos a", frecuencia, "hz")


if __name__ == "__main__":
    main()
