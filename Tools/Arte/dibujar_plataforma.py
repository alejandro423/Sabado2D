# dibuja la plataforma de madera del nivel
# 96x48 px a 100 ppu = 0.96 x 0.48 unidades, se estira en X desde el armador
# se puede cambiar la paleta y volver a correr

from PIL import Image

ANCHO, ALTO = 96, 48

PALETA = {
    "contorno": (58, 36, 16),
    "tabla": (172, 121, 78),
    "tabla_clara": (196, 148, 100),
    "tabla_oscura": (139, 94, 60),
    "soporte": (106, 68, 40),
    "clavo": (40, 40, 44),
}


def con_canal(rgb):
    return (rgb[0], rgb[1], rgb[2], 255)


def punto(img, x, y, color):
    if 0 <= x < ANCHO and 0 <= y < ALTO:
        img.putpixel((x, y), con_canal(PALETA[color]))


def rect(img, x0, y0, x1, y1, color):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            punto(img, x, y, color)


def main():
    img = Image.new("RGBA", (ANCHO, ALTO), (0, 0, 0, 0))

    # tabla de arriba, con luz y borde oscuro
    rect(img, 0, 0, ANCHO - 1, 15, "tabla")
    rect(img, 0, 0, ANCHO - 1, 2, "contorno")
    rect(img, 0, 3, ANCHO - 1, 6, "tabla_clara")
    rect(img, 0, 15, ANCHO - 1, 15, "contorno")

    # viga de abajo
    rect(img, 0, 16, ANCHO - 1, 24, "tabla_oscura")
    rect(img, 0, 24, ANCHO - 1, 25, "contorno")

    # soportes en diagonal en los extremos
    for i in range(16):
        rect(img, 6 + i, 26 + i, 9 + i, 28 + i, "soporte")
        rect(img, ANCHO - 10 - i, 26 + i, ANCHO - 7 - i, 28 + i, "soporte")

    # clavos
    for x in (8, ANCHO - 12):
        rect(img, x, 8, x + 3, 11, "clavo")

    ruta = "Assets/ArteBoliviano/plataforma_madera.png"
    img.save(ruta)
    print("listo:", ruta, img.size)


if __name__ == "__main__":
    main()
