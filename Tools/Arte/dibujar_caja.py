# dibuja la caja de madera de la plaza
# genera Assets/ArteBoliviano/caja_madera.png
# estilo 8 bits a juego con el cubito

from PIL import Image

ANCHO, ALTO = 100, 100

PALETA = {
    "contorno": (58, 36, 16),
    "madera": (172, 121, 78),
    "madera_clara": (196, 148, 100),
    "madera_oscura": (139, 94, 60),
    "clavo": (40, 40, 44),
}


def con_canal(rgb):
    return (rgb[0], rgb[1], rgb[2], 255)


def punto(img, x, y, color):
    if 0 <= x < ANCHO and 0 <= y < ALTO:
        img.putpixel((x, y), con_canal(PALETA[color]))


def rectangulo(img, x0, y0, x1, y1, color):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            punto(img, x, y, color)


def diagonal(img, x0, y0, x1, y1, grosor, color):
    pasos = max(abs(x1 - x0), abs(y1 - y0))
    for i in range(pasos + 1):
        x = x0 + (x1 - x0) * i // pasos
        y = y0 + (y1 - y0) * i // pasos
        for g in range(grosor):
            punto(img, x - g // 2 + g // 2, y, color)
            punto(img, x + g, y, color)
            punto(img, x - g, y, color)


def main():
    img = Image.new("RGBA", (ANCHO, ALTO), (0, 0, 0, 0))

    # tablones horizontales
    for i, y0 in enumerate(range(8, 92, 21)):
        color = "madera" if i % 2 == 0 else "madera_clara"
        rectangulo(img, 8, y0, 91, y0 + 20, color)

    # sombra abajo y luz arriba de cada tablon
    rectangulo(img, 8, 8, 91, 10, "madera_clara")
    rectangulo(img, 8, 89, 91, 91, "madera_oscura")

    # marco de la caja
    rectangulo(img, 0, 0, 99, 7, "contorno")
    rectangulo(img, 0, 92, 99, 99, "contorno")
    rectangulo(img, 0, 0, 7, 99, "contorno")
    rectangulo(img, 92, 0, 99, 99, "contorno")

    # cruces de refuerzo
    diagonal(img, 10, 10, 89, 89, 7, "madera_oscura")
    diagonal(img, 89, 10, 10, 89, 7, "madera_oscura")

    # clavos en las esquinas
    for cx, cy in [(14, 14), (85, 14), (14, 85), (85, 85)]:
        rectangulo(img, cx - 3, cy - 3, cx + 3, cy + 3, "clavo")

    ruta = "Assets/ArteBoliviano/caja_madera.png"
    img.save(ruta)
    print("listo:", ruta, img.size)


if __name__ == "__main__":
    main()
