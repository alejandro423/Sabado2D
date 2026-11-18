# Grupo5 — Escenario y meta visual

Abrir `Assets/Scenes/Grupo5.unity` en Unity. La escena contiene objetos editables agrupados bajo `Escenario Grupo5`: césped, carretera, líneas de carril, salida y meta visual.

## Distribución

- Vista superior 2D; avanzar desde abajo hacia arriba, sobre el eje Y positivo.
- Asfalto: X entre -4 y 4; Y entre -9 y 9. Mantener el auto dentro de las líneas de borde (X = ±3.8), descontando la mitad de su ancho.
- Tres carriles con centros aproximados en X = -2.54, 0 y 2.54.
- `InicioAuto`: posición (0, -8.3, 0); contiene el auto del jugador.
- `CentroMeta`: posición (0, 7.2, 0), referencia vacía para incorporar el detector de victoria. La franja de cuadros ocupa Y de 6.7 a 7.7.
- Cámara ortográfica fija en (0, 0, -10), tamaño 10.5. El recorrido completo cabe en 16:9, 4:3 y 9:16.
- Los sprites del escenario tienen `Order in Layer` entre -30 y -17. Autos y conos pueden usar la capa Default con orden 0 para verse por encima.

## Entregas del equipo

1. Escenario y cámara: esta entrega.
2. Auto, avance y controles: auto incluido en `InicioAuto`; avanza automáticamente y se mueve con A/D o flechas izquierda/derecha, sin cruzar los bordes de la carretera. Se detiene al llegar a la meta.
3. Conos y choques: colocar obstáculos entre la salida y la meta.
4. Victoria y reinicio: añadir un trigger en `CentroMeta` y la interfaz.
5. Integración: reunir las entregas, probar el recorrido y añadir la escena a la lista de escenas del build que utilice el equipo.

El escenario base usa un sprite cuadrado propio y un material URP 2D sin iluminación; no requiere recursos externos ni scripts para dibujarlo. El movimiento del auto usa el Input System ya incluido en el proyecto. No se incluyen obstáculos, colisiones ni lógica de victoria. Las escenas y la configuración de build existentes se conservan.
