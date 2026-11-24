# dibuja al cubito marron con gorrito de aguayo
# genera Assets/ArteBoliviano/cubito_gorrito.png
# se puede editar la paleta de abajo y volver a correr

from PIL import Image

ANCHO, ALTO = 160, 200

PALETA = {
    "contorno": (58, 36, 16),
    "marron": (139, 94, 60),
    "marron_claro": (172, 121, 78),
    "marron_oscuro": (106, 68, 40),
    "rojo": (198, 42, 38),
    "naranja": (233, 126, 38),
    "amarillo": (244, 197, 66),
    "verde": (52, 140, 74),
    "celeste": (63, 158, 212),
    "violeta": (108, 70, 158),
    "blanco": (242, 236, 222),
    "negro": (43, 26, 12),
    "mejilla": (217, 138, 106),
}

# colores del wipala, en orden
WIPALA = ["rojo", "naranja", "amarillo", "blanco", "verde", "celeste", "violeta"]


def con_canal(rgb):
    return (rgb[0], rgb[1], rgb[2], 255)


def punto(img, x, y, color):
    if 0 <= x < ANCHO and 0 <= y < ALTO:
        img.putpixel((x, y), con_canal(PALETA[color]))


def rectangulo(img, x0, y0, x1, y1, color):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            punto(img, x, y, color)


def cuadrado_redondo(img, x0, y0, x1, y1, color, corte=8):
    # rectangulo con esquinas chaflanadas estilo 8 bits
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            al_borde_x = min(x - x0, x1 - x)
            al_borde_y = min(y - y0, y1 - y)
            if al_borde_x + al_borde_y >= corte:
                punto(img, x, y, color)


def circulo(img, cx, cy, radio, color):
    for y in range(cy - radio, cy + radio + 1):
        for x in range(cx - radio, cx + radio + 1):
            if (x - cx) ** 2 + (y - cy) ** 2 <= radio ** 2:
                punto(img, x, y, color)


def wipala(img, x0, y0, x1, y1, celda=7):
    # cuadros en diagonal, como el wipala
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            indice = (x // celda + y // celda) % len(WIPALA)
            punto(img, x, y, WIPALA[indice])


def gorrito(img):
    # pompom arriba
    circulo(img, 80, 16, 12, "amarillo")

    # capucha con el wipala, arriba angosta abajo ancha
    y0, y1 = 28, 84
    for y in range(y0, y1):
        progreso = (y - y0) / (y1 - y0)
        medio = 80
        ancho = 48 + int(progreso * 18)
        for x in range(medio - ancho, medio + ancho + 1):
            indice = (x // 7 + y // 7) % len(WIPALA)
            punto(img, x, y, WIPALA[indice])

        # contorno del gorrito
        punto(img, medio - ancho, y, "contorno")
        punto(img, medio - ancho - 1, y, "contorno")
        punto(img, medio + ancho, y, "contorno")
        punto(img, medio + ancho + 1, y, "contorno")

    # vuelta del gorro, rojo con una linea blanca
    rectangulo(img, 12, 82, 148, 96, "rojo")
    rectangulo(img, 12, 82, 148, 84, "blanco")
    rectangulo(img, 12, 82, 148, 82, "contorno")
    rectangulo(img, 12, 96, 148, 96, "contorno")


def cubito(img):
    # contorno del cuerpo
    cuadrado_redondo(img, 8, 96, 152, 196, "contorno", corte=10)

    # relleno marron, tres pixeles adentro
    cuadrado_redondo(img, 11, 99, 149, 193, "marron", corte=8)

    # toque de luz arriba
    rectangulo(img, 22, 104, 138, 110, "marron_claro")

    # poncho wipala cruzando el cuerpo
    wipala(img, 13, 152, 147, 174)
    rectangulo(img, 13, 152, 147, 152, "contorno")
    rectangulo(img, 13, 174, 147, 174, "contorno")

    # sombra abajo
    rectangulo(img, 22, 184, 138, 190, "marron_oscuro")


def cara(img):
    # ojos con brillo
    for ex in (46, 96):
        rectangulo(img, ex, 112, ex + 13, 129, "negro")
        rectangulo(img, ex + 8, 115, ex + 11, 118, "blanco")

    # mejillas
    rectangulo(img, 28, 132, 41, 139, "mejilla")
    rectangulo(img, 114, 132, 127, 139, "mejilla")

    # sonrisa
    rectangulo(img, 68, 142, 87, 145, "negro")
    punto(img, 64, 141, "negro")
    punto(img, 65, 141, "negro")
    punto(img, 65, 140, "negro")
    punto(img, 91, 141, "negro")
    punto(img, 90, 141, "negro")
    punto(img, 90, 140, "negro")


def main():
    img = Image.new("RGBA", (ANCHO, ALTO), (0, 0, 0, 0))
    gorrito(img)
    cubito(img)
    cara(img)

    ruta = "Assets/ArteBoliviano/cubito_gorrito.png"
    img.save(ruta)
    print("listo:", ruta, img.size)


if __name__ == "__main__":
    main()
