# saca el sol y la montaña del fondo de la plaza para los laterales del nivel
# el cielo se rellena con el mismo degradé del original
# el centro del nivel usa el fondo original, asi queda un solo sol y una montaña

from PIL import Image

ORIGINAL = "Assets/ArteBoliviano/fondo_plaza.png"
SALIDA = "Assets/ArteBoliviano/fondo_plaza_lateral.png"

# arriba de esta fila es cielo (sol y montaña), abajo queda la plaza pintada
CORTE_CIELO = 545


def main():
    original = Image.open(ORIGINAL).convert("RGBA")
    ancho, alto = original.size
    lateral = original.copy()

    # el degradé se toma de la franja izquierda, que es cielo puro
    for y in range(0, CORTE_CIELO):
        muestras = [original.getpixel((x, y)) for x in range(0, 80)]
        promedio = tuple(sum(s[i] for s in muestras) // len(muestras) for i in range(3))
        color = (promedio[0], promedio[1], promedio[2], 255)
        for x in range(ancho):
            lateral.putpixel((x, y), color)

    lateral.save(SALIDA)
    print("listo:", SALIDA, lateral.size)


if __name__ == "__main__":
    main()
