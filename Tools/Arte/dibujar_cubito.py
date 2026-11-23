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
    "rojo": (240, 59, 48),
    "naranja": (255, 140, 26),
    "amarillo": (255, 210, 63),
    "verde": (31, 163, 107),
    "azul": (30, 155, 215),
    "morado": (122, 77, 191),
    "blanco": (245, 239, 230),
    "negro": (43, 26, 12),
    "mejilla": (217, 138, 106),
}

# colores del aguayo, de arriba hacia abajo
RAYAS = ["rojo", "naranja", "amarillo", "verde", "azul", "morado"]


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


def gorrito(img):
    # pompom arriba
    circulo(img, 80, 16, 12, "amarillo")

    # capucha con rayas de aguayo, arrriba angosta abajo ancha
    y0, y1 = 30, 84
    for y in range(y0, y1):
        progreso = (y - y0) / (y1 - y0)
        medio = 80
        ancho = 46 + int(progreso * 20)
        franja = RAYAS[(y - y0) // 9 % len(RAYAS)]
        for x in range(medio - ancho, medio + ancho + 1):
            punto(img, x, y, franja)

        # contorno del gorrito
        punto(img, medio - ancho, y, "contorno")
        punto(img, medio - ancho - 1, y, "contorno")
        punto(img, medio + ancho, y, "contorno")
        punto(img, medio + ancho + 1, y, "contorno")

    # vuelta del gorro
    rectangulo(img, 14, 82, 146, 94, "marron_oscuro")
    rectangulo(img, 14, 82, 146, 84, "contorno")
    rectangulo(img, 14, 92, 146, 94, "contorno")


def cubito(img):
    # contorno del cuerpo
    cuadrado_redondo(img, 8, 96, 152, 196, "contorno", corte=10)

    # relleno marron, tres pixeles adentro
    cuadrado_redondo(img, 11, 99, 149, 193, "marron", corte=8)

    # toque de luz arriba
    rectangulo(img, 22, 104, 138, 110, "marron_claro")

    # sombra abajo
    rectangulo(img, 22, 184, 138, 190, "marron_oscuro")


def cara(img):
    # ojos con brillo
    for ex in (48, 98):
        rectangulo(img, ex, 126, ex + 13, 143, "negro")
        rectangulo(img, ex + 8, 129, ex + 11, 132, "blanco")

    # mejillas
    rectangulo(img, 30, 146, 43, 153, "mejilla")
    rectangulo(img, 116, 146, 129, 153, "mejilla")

    # sonrisa
    rectangulo(img, 70, 156, 89, 159, "negro")
    punto(img, 66, 155, "negro")
    punto(img, 67, 155, "negro")
    punto(img, 67, 154, "negro")
    punto(img, 93, 155, "negro")
    punto(img, 92, 155, "negro")
    punto(img, 92, 154, "negro")


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
