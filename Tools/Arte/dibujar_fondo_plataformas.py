# dibuja el fondo de la plaza para el plataformero, sin montaña
# 1920x1080 a 100 ppu = 19.2 x 10.8, justo lo que ve la camara
# el piso del dibujo arranca en el pixel 893, que es y=-3.53 en el mundo
# se puede cambiar la paleta y volver a correr

import random

from PIL import Image

ANCHO, ALTO = 1920, 1080
PISO_PIXEL = 893

PALETA = {
    "cielo_arriba": (150, 200, 225),
    "cielo_abajo": (216, 236, 240),
    "nube": (250, 250, 248),
    "nube_sombra": (226, 234, 238),
    "sol": (252, 236, 178),
    "casa": (200, 160, 120),
    "casa_oscura": (185, 145, 105),
    "techo": (178, 92, 62),
    "ventana": (70, 50, 35),
    "piso": (205, 170, 125),
    "piso_borde": (150, 120, 90),
    "piso_mancha": (188, 152, 110),
    "toldo_rojo": (198, 42, 38),
    "toldo_verde": (52, 140, 74),
    "toldo_amarillo": (244, 197, 66),
    "poste": (106, 68, 40),
    "muro": (222, 190, 150),
}


def con_canal(rgb):
    return (rgb[0], rgb[1], rgb[2], 255)


def rect(img, x0, y0, x1, y1, color):
    for y in range(max(0, y0), min(ALTO, y1 + 1)):
        for x in range(max(0, x0), min(ANCHO, x1 + 1)):
            img.putpixel((x, y), con_canal(PALETA[color]))


def cielo(img):
    for y in range(0, PISO_PIXEL):
        t = y / PISO_PIXEL
        color = tuple(
            int(PALETA["cielo_arriba"][i] * (1 - t) + PALETA["cielo_abajo"][i] * t)
            for i in range(3)
        )
        for x in range(ANCHO):
            img.putpixel((x, y), con_canal(color))


def nube(img, x, y, ancho, alto):
    # nube cuadrada estilo 8 bits
    rect(img, x, y + alto // 2, x + ancho, y + alto, "nube")
    rect(img, x + ancho // 4, y, x + ancho * 3 // 4, y + alto // 2, "nube")
    rect(img, x, y + alto, x + ancho, y + alto + 4, "nube_sombra")


def sol(img):
    cx, cy, r = 1660, 150, 52
    for y in range(cy - r, cy + r):
        for x in range(cx - r, cx + r):
            if (x - cx) ** 2 + (y - cy) ** 2 <= r ** 2:
                img.putpixel((x, y), con_canal(PALETA["sol"]))


def casas(img):
    # fila de casitas de adobe a los costados, el centro queda libre
    def casa(x, ancho, alto, tono):
        y_base = PISO_PIXEL - 14
        rect(img, x, y_base - alto, x + ancho, y_base, tono)
        rect(img, x - 6, y_base - alto - 10, x + ancho + 6, y_base - alto, "techo")
        # puerta y ventanas
        rect(img, x + ancho // 2 - 10, y_base - 52, x + ancho // 2 + 10, y_base, "ventana")
        rect(img, x + 14, y_base - alto + 26, x + 40, y_base - alto + 50, "ventana")
        rect(img, x + ancho - 40, y_base - alto + 26, x + ancho - 14, y_base - alto + 50, "ventana")

    for i, (x, ancho, alto) in enumerate([
        (40, 150, 170), (210, 190, 210), (420, 130, 150),
    ]):
        casa(x, ancho, alto, "casa" if i % 2 == 0 else "casa_oscura")

    for i, (x, ancho, alto) in enumerate([
        (1370, 130, 150), (1520, 190, 210), (1730, 150, 170),
    ]):
        casa(x, ancho, alto, "casa_oscura" if i % 2 == 0 else "casa")


def toldo(img, x, y, ancho, color):
    # toldo de feria con su poste
    rect(img, x, y, x + ancho, y + 22, "poste")
    rect(img, x - 8, y - 26, x + ancho + 8, y, color)
    rect(img, x - 8, y - 26, x + ancho + 8, y - 20, "nube")
    rect(img, x + 6, y + 22, x + 10, PISO_PIXEL - 14, "poste")
    rect(img, x + ancho - 10, y + 22, x + ancho - 6, PISO_PIXEL - 14, "poste")


def piso(img):
    rect(img, 0, PISO_PIXEL, ANCHO - 1, ALTO - 1, "piso")
    rect(img, 0, PISO_PIXEL, ANCHO - 1, PISO_PIXEL + 10, "piso_borde")

    # manchas de piedra, siempre las mismas para que no baile
    azar = random.Random(7)
    for _ in range(260):
        x = azar.randrange(0, ANCHO - 8)
        y = azar.randrange(PISO_PIXEL + 18, ALTO - 6)
        ancho = azar.choice([6, 10, 14])
        alto = azar.choice([4, 6])
        rect(img, x, y, x + ancho, y + alto, "piso_mancha")


def main():
    img = Image.new("RGBA", (ANCHO, ALTO), (0, 0, 0, 0))

    cielo(img)
    sol(img)
    nube(img, 240, 150, 190, 46)
    nube(img, 760, 90, 150, 38)
    nube(img, 1180, 210, 210, 50)
    casas(img)
    toldo(img, 300, 760, 150, "toldo_rojo")
    toldo(img, 1080, 780, 140, "toldo_verde")
    toldo(img, 1560, 760, 150, "toldo_amarillo")
    piso(img)

    ruta = "Assets/ArteBoliviano/fondo_plaza_plataformas.png"
    img.save(ruta)
    print("listo:", ruta, img.size)


if __name__ == "__main__":
    main()
