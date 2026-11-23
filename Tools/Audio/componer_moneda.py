# compone el sonido de la moneda, dos notas tipo ding
# genera Assets/SonidosPlaza/moneda.wav
# se puede cambiar la frecuencia o la duracion y volver a correr


import wave
import numpy as np

FRECUENCIA = 44100

# si5 y mi6, como una moneda brillante
NOTA_CORTA = 987.77
NOTA_LARGA = 1318.51


def nota(frecuencia, duracion, volumen=0.4):
    muestras = int(FRECUENCIA * duracion)
    t = np.arange(muestras) / FRECUENCIA

    onda = (np.sin(2 * np.pi * frecuencia * t)
            + 0.3 * np.sin(4 * np.pi * frecuencia * t)
            + 0.1 * np.sin(6 * np.pi * frecuencia * t))

    # arranca rapido y se apaga suave
    envolvente = np.exp(-t / 0.12)
    arranque = np.clip(t / 0.002, 0, 1)

    return (onda * envolvente * arranque * volumen).astype(np.float32)


def main():
    import os
    os.makedirs("Assets/SonidosPlaza", exist_ok=True)

    corta = nota(NOTA_CORTA, 0.09)
    larga = nota(NOTA_LARGA, 0.30)

    # silencio minimo al final para que no corte feo
    cola = np.zeros(int(FRECUENCIA * 0.05), dtype=np.float32)
    audio = np.concatenate([corta, larga, cola])

    # a 16 bits
    enteros = (audio * 32767).astype(np.int16)

    ruta = "Assets/SonidosPlaza/moneda.wav"
    with wave.open(ruta, "wb") as archivo:
        archivo.setnchannels(1)
        archivo.setsampwidth(2)
        archivo.setframerate(FRECUENCIA)
        archivo.writeframes(enteros.tobytes())

    print("listo:", ruta, len(enteros) / FRECUENCIA, "segundos")


if __name__ == "__main__":
    main()
