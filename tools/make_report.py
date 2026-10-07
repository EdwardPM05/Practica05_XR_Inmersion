"""Genera Reporte_Practica05.pdf con los datos medidos y las evidencias de Evidencias/."""
import os
from reportlab.lib import colors
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import cm
from reportlab.platypus import Image, PageBreak, Paragraph, SimpleDocTemplate, Spacer, Table, TableStyle

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "Reporte_Practica05.pdf")

ss = getSampleStyleSheet()
H1 = ParagraphStyle("H1", parent=ss["Heading1"], fontSize=18, spaceAfter=6, textColor=colors.HexColor("#1f3a4d"))
H2 = ParagraphStyle("H2", parent=ss["Heading2"], fontSize=13, spaceBefore=12, spaceAfter=6, textColor=colors.HexColor("#1f3a4d"))
BODY = ParagraphStyle("B", parent=ss["BodyText"], fontSize=10, leading=14)
SMALL = ParagraphStyle("S", parent=BODY, fontSize=8.5, leading=11, textColor=colors.HexColor("#555555"))


def read_kv(path):
    txt = open(os.path.join(ROOT, path), encoding="utf-8").read().strip()
    return dict(p.split("=", 1) for p in txt.replace("\n", " ").split() if "=" in p)


bake = read_kv("bake_report.txt")
fps = read_kv("fps_report.txt")
bake_time = bake["tiempo"]  # hh:mm:ss.fffffff
h, m, s = bake_time.split(":")
bake_fmt = f"{int(h):02d}:{int(m):02d}:{float(s):04.1f}  ({int(h) * 3600 + int(m) * 60 + float(s):.1f} s)"


def table(rows, widths):
    rows = [[r[0], Paragraph(r[1], ParagraphStyle('c', parent=BODY, fontSize=9.5, leading=12))] for r in rows]
    t = Table(rows, colWidths=widths)
    t.setStyle(TableStyle([
        ("FONTNAME", (0, 0), (0, -1), "Helvetica-Bold"),
        ("FONTSIZE", (0, 0), (-1, -1), 9.5),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("GRID", (0, 0), (-1, -1), 0.4, colors.HexColor("#b8c4cc")),
        ("ROWBACKGROUNDS", (0, 0), (-1, -1), [colors.white, colors.HexColor("#eef3f6")]),
        ("LEFTPADDING", (0, 0), (-1, -1), 6), ("TOPPADDING", (0, 0), (-1, -1), 4), ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
    ]))
    return t


def P(x):
    return Paragraph(x, BODY)


story = [
    Paragraph("Práctica N.° 5: Unity 6 + XR", H1),
    Paragraph("Escena inmersiva con iluminación pre-calculada y audio espacial", ss["Heading3"]),
    Paragraph("Universidad Autónoma del Perú | Laboratorio XR", SMALL),
    Spacer(1, 10),

    Paragraph("1. Resumen de resultados", H2),
    table([
        ["Proyecto", "Practica05_XR_Inmersion"],
        ["Editor / pipeline", "Unity 6.4 (6000.4.2f1), Universal Render Pipeline (URP)"],
        ["XR", "XR Interaction Toolkit 3.2.1 + XR Device Simulator (prueba en PC, sin visor)"],
        ["Tipo de luz", "Baked: 1 Directional (sol) + 3 Point Lights"],
        ["Lightmapper", "Progressive GPU (NVIDIA GeForce RTX 4050 Laptop GPU)"],
        ["Parámetros del bake", "Resolución 40 texels/unidad, tamaño máx. 1024, muestras directas 64 / indirectas 256 / entorno 128, 3 rebotes"],
        ["Lightmaps generados", f"{bake['lightmaps']} (2 de 1024x1024 y 3 de 512x512)"],
        ["Tiempo de bake", bake_fmt],
        ["FPS en Play Mode (PC)", f"Promedio {fps['promedio']} | máx. {fps['max']} | mín. {fps['min']}"],
        ["Audio 3D", "Spatial Blend 1.0 + Logarithmic Rolloff (Min 1 m, Max 15 m)"],
    ], [4.3 * cm, 12.7 * cm]),

    Paragraph("2. Descripción de la escena", H2),
    P("Sala de estar de 10 x 8 m y 3.2 m de altura con una ventana en la pared norte. Incluye parquet con textura de tablones "
      "generada por código, zócalo y friso, alfombra, sofá, sillón, mesa de centro, aparador, estantería con libros, lámpara de pie, "
      "lámpara colgante, plantas, cuadros y un jardín visible desde la ventana. Toda la geometría arquitectónica y el mobiliario fijo están "
      "marcados como <b>Static</b>. El sol entra por la ventana y proyecta una mancha de luz sobre el piso; las lámparas aportan luz cálida "
      "y sus pantallas emiten luz horneada (Baked Emissive). La radio (<i>Audio_Object</i>) <b>no es estática</b>, porque es el objeto que "
      "emite el sonido, y recibe la iluminación mediante una malla de Light Probes."),

    Paragraph("3. Iluminación pre-calculada (Lightmaps)", H2),
    P(f"El bake terminó correctamente ({bake['lightmaps']} lightmaps) en {bake_fmt}. Con luces Baked y geometría estática, la iluminación se "
      "almacena en texturas y no se recalcula en cada cuadro, lo que reduce el costo de GPU, una ventaja clave en visores autónomos."),
    Spacer(1, 6),
]

imgs = []
for i in range(int(bake["lightmaps"])):
    f = os.path.join(ROOT, "Evidencias", f"Lightmap-{i}.png")
    if os.path.exists(f):
        imgs.append([Image(f, 5.2 * cm, 5.2 * cm), Paragraph(f"Lightmap-{i}", SMALL)])
row, rows = [], []
for cell in imgs:
    row.append(cell)
    if len(row) == 3:
        rows.append(row)
        row = []
if row:
    rows.append(row + [""] * (3 - len(row)))
grid = Table([[c if c == "" else [c[0], c[1]] for c in r] for r in rows], colWidths=[5.6 * cm] * 3)
grid.setStyle(TableStyle([("ALIGN", (0, 0), (-1, -1), "CENTER"), ("VALIGN", (0, 0), (-1, -1), "TOP")]))
story += [grid, Spacer(1, 4), Paragraph("Figura 1. Lightmaps generados (exportados desde el Editor tras el bake).", SMALL)]

story += [
    Paragraph("4. Rendimiento (FPS)", H2),
    P(f"Se midieron {fps['frames']} cuadros durante {fps['duracion']} en Play Mode, tras 3 s de calentamiento, con el contador "
      f"<i>FPSCounter</i> incluido en la escena. Resultado: <b>{fps['promedio']} FPS</b> en promedio (máximo {fps['max']}). "
      "El valor mínimo de 0.2 FPS corresponde a un único cuadro detenido (tirón puntual del Editor), no al comportamiento sostenido."),
    P("<b>Alcance:</b> la medición se hizo en el Editor de una PC con RTX 4050 Laptop, sin visor. No es representativa de un Meta Quest; "
      "sirve como referencia relativa del costo de la escena."),

    Paragraph("5. Audio espacial", H2),
    P("El objeto <i>Audio_Object</i> (una radio sobre el aparador, junto a la pared este) tiene un Audio Source con Loop, Play On Awake, "
      "<b>Spatial Blend = 1.0</b> (3D) y <b>Logarithmic Rolloff</b> con Min Distance 1 y Max Distance 15, ajustado al tamaño de la sala. "
      "Se añadió una Reverb Zone con el preset Room. El XR Origin lleva el único Audio Listener activo."),
    P("<b>Limitación:</b> la guía menciona activar un plugin Spatializer (HRTF) en Project Settings. En este proyecto no hay ningún plugin "
      "de spatializer instalado (por ejemplo Meta XR Audio), por lo que la dirección se percibe con el panning 3D estándar de Unity y no con HRTF. "
      "El Audio Source ya tiene la opción Spatialize activada para cuando se instale uno."),

    Paragraph("6. Evidencias", H2),
    table([
        ["Capturas de lightmaps", "Carpeta Evidencias/ (Lightmap-0 a Lightmap-4) y Figura 1"],
        ["Video de navegación", "Grabación 2026-10-07 090749.mp4 (recorrido con XR Device Simulator)"],
        ["Datos del reporte", "bake_report.txt y fps_report.txt"],
        ["Repositorio GitHub", "Pendiente de publicar"],
    ], [4.3 * cm, 12.7 * cm]),

    Paragraph("7. Conclusión", H2),
    P("La combinación de geometría estática, luces Baked y lightmaps produjo una escena estable, sin parpadeos, con un costo de render bajo "
      f"({fps['promedio']} FPS en PC) y un bake de menos de un minuto con Progressive GPU. El audio 3D con atenuación logarítmica hace que el "
      "volumen y la dirección de la radio cambien con la posición del usuario, de modo que se puede ubicar la fuente aunque quede fuera del "
      "campo visual. Queda como mejora instalar un spatializer HRTF y repetir la medición de FPS en un visor autónomo."),
]

SimpleDocTemplate(OUT, pagesize=A4, leftMargin=2 * cm, rightMargin=2 * cm, topMargin=2 * cm, bottomMargin=2 * cm,
                  title="Práctica 05 - Unity 6 + XR").build(story)
print("OK", OUT)
