# Práctica N.° 5: Unity 6 + XR

Escena inmersiva con **iluminación pre-calculada (lightmaps)** y **audio espacial 3D**, para el Laboratorio XR de la Universidad Autónoma del Perú. Se prueba en PC con el **XR Device Simulator**; no requiere visor.

## Resumen

| Dato | Valor |
|---|---|
| Proyecto | `Practica05_XR_Inmersion` |
| Editor | Unity 6.4 (6000.4.2f1), Universal Render Pipeline |
| XR | XR Interaction Toolkit 3.2.1 + XR Device Simulator |
| Escena | `Assets/Scenes/Practica05.unity` |
| Luces | Baked: 1 Directional (sol) + 3 Point Lights |
| Lightmapper | Progressive GPU (NVIDIA GeForce RTX 4050 Laptop GPU) |
| Parámetros del bake | 40 texels/unidad, tamaño máx. 1024, muestras directas 64 / indirectas 256 / entorno 128, 3 rebotes |
| Lightmaps generados | 5 (2 de 1024×1024 y 3 de 512×512) |
| Tiempo de bake | 00:00:33.09 (230,45 mrays/sec) |
| FPS en Play Mode (PC) | promedio 204,4 · 218,4 FPS en Stats (4,6 ms por frame) |
| Audio 3D | Spatial Blend 1.0 + Logarithmic Rolloff (Min 1 m, Max 15 m) |

## La escena

Sala de estar de 10 × 8 m con una ventana en la pared norte: parquet con textura generada por código, zócalo y friso, alfombra, sofá, sillón, mesa de centro, aparador, estantería con libros, lámpara de pie, lámpara colgante, plantas, cuadros y un jardín visible desde la ventana.

- **Static:** toda la geometría arquitectónica y el mobiliario fijo, para que participen en el bake.
- **Baked:** el sol entra por la ventana y proyecta una mancha de luz sobre el piso; las lámparas aportan luz cálida y sus pantallas emiten luz horneada (Baked Emissive).
- **`Audio_Object`:** una radio sobre el aparador (pared este). **No es estática**, porque es la fuente de sonido; recibe la luz horneada mediante Light Probes.
- **XR:** `XR Origin (XR Rig)` con el único Audio Listener, más el `XR Device Simulator` para moverse con teclado y mouse.
- **`FPS_Counter`:** muestra los FPS actuales y el promedio en pantalla.

## Iluminación y lightmaps

![Ventana Lighting](Evidencias/Captura_Lighting.png)

![Baked Lightmaps](Evidencias/Captura_Lightmaps.png)

Las capturas están en la carpeta [`Evidencias`](Evidencias).

## Audio espacial

El Audio Source de la radio usa Loop, Play On Awake, Spatial Blend 1.0 y atenuación logarítmica con Min Distance 1 y Max Distance 15, ajustados al tamaño de la sala. Se añadió una Reverb Zone con el preset Room.

> **Limitación:** el proyecto no tiene instalado ningún plugin de spatializer HRTF (por ejemplo Meta XR Audio), así que la dirección se percibe con el panning 3D estándar de Unity. El Audio Source ya tiene `Spatialize` activado para cuando se instale uno.

## Rendimiento

Medido en Play Mode recorriendo la sala: **204,4 FPS** de promedio en el contador de la escena. La ventana Statistics marca 218,4 FPS, 4,6 ms por frame, 1,6 ms de GPU, 25,0 mil triángulos y 141 draw calls.

![Contador de FPS](Evidencias/Captura_FPS.png)

![Statistics](Evidencias/Captura_Stats.png)

![Profiler](Evidencias/Captura_Profiler.png)

> **Alcance:** la medición se hizo en el Editor de una PC con RTX 4050 Laptop, sin visor. No es representativa de un Meta Quest; sirve como referencia relativa.

## Cómo probar la escena

1. Abre el proyecto con **Unity 6000.4.2f1** (Unity Hub > Add project from disk).
2. Abre `Assets/Scenes/Practica05.unity`.
3. Pulsa **Play**. Muévete con el XR Device Simulator (las teclas se muestran en un panel en la ventana Game).
4. Acércate y aléjate de la radio y gira para dejarla a izquierda, derecha y detrás: el volumen y la dirección del sonido cambian.

## Estructura

```
Assets/
  Audio/          campana.wav (generado por código)
  Editor/         scripts que construyen la escena y hornean la luz
  Materials/      materiales URP y textura de parquet
  Samples/        Starter Assets y XR Device Simulator (XRI 3.2.1)
  Scenes/         Practica05.unity + datos de lightmaps
  Scripts/        FPSCounter.cs
Evidencias/       capturas de la práctica
Reporte_Practica05.docx / .pdf
```

## Entregables

- [x] Repositorio en GitHub (este).
- [x] Capturas de lightmaps y FPS ([`Evidencias`](Evidencias)).
- [x] Reporte con tiempos de bake y FPS: [`Reporte_Practica05.pdf`](Reporte_Practica05.pdf).
- [ ] Video de navegación con el cambio de volumen y dirección del audio (se entrega por separado).
