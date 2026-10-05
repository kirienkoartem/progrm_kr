"""Генерация значка приложения src/Integral.App/app.ico (несколько размеров, чёткая отрисовка под каждый).

Запуск: python3 tools/make_icon.py   (нужен Pillow)
Эскиз: скруглённый синий квадрат и белый знак интеграла, нарисованный кривой Безье.
"""
import io
import os
import struct
from PIL import Image, ImageDraw

SIZES = [16, 20, 24, 32, 40, 48, 64, 128, 256]
STROKE = {16: 0.17, 20: 0.15, 24: 0.14, 32: 0.13, 40: 0.125, 48: 0.12, 64: 0.115, 128: 0.105, 256: 0.10}
TOP, BOTTOM = (59, 130, 246), (29, 78, 216)   # градиент фона (акцентный синий интерфейса)
SS = 8                                          # суперсэмплинг


def bezier(p0, p1, p2, p3, n=60):
    pts = []
    for i in range(n + 1):
        t = i / n
        u = 1 - t
        pts.append((u**3 * p0[0] + 3 * u**2 * t * p1[0] + 3 * u * t**2 * p2[0] + t**3 * p3[0],
                    u**3 * p0[1] + 3 * u**2 * t * p1[1] + 3 * u * t**2 * p2[1] + t**3 * p3[1]))
    return pts


def integral_path():
    """Знак ∫ в единичном квадрате: нижний крючок, наклонный стержень, верхний крючок."""
    pts = bezier((0.30, 0.79), (0.35, 0.87), (0.45, 0.85), (0.47, 0.68))
    pts += [(0.47, 0.68), (0.53, 0.32)]
    pts += bezier((0.53, 0.32), (0.55, 0.15), (0.65, 0.13), (0.70, 0.21))
    return pts


def render(size):
    big = size * SS
    img = Image.new('RGBA', (big, big), (0, 0, 0, 0))
    # фон: вертикальный градиент в скруглённом квадрате
    grad = Image.new('RGBA', (big, big))
    gd = ImageDraw.Draw(grad)
    for y in range(big):
        k = y / (big - 1)
        gd.line([(0, y), (big, y)], fill=tuple(int(TOP[i] + (BOTTOM[i] - TOP[i]) * k) for i in range(3)) + (255,))
    mask = Image.new('L', (big, big), 0)
    pad = int(big * 0.02)
    ImageDraw.Draw(mask).rounded_rectangle([pad, pad, big - pad - 1, big - pad - 1], radius=int(big * 0.22), fill=255)
    img.paste(grad, (0, 0), mask)
    # знак интеграла: штрих круглой «кистью» по плотным точкам (без артефактов утолщённой линии)
    glyph = Image.new('L', (big, big), 0)
    gd2 = ImageDraw.Draw(glyph)
    r = max(1.0, big * STROKE[size] / 2)
    pts = [(x * big, y * big) for x, y in integral_path()]
    for (x1, y1), (x2, y2) in zip(pts, pts[1:]):
        steps = max(1, int(((x2 - x1) ** 2 + (y2 - y1) ** 2) ** 0.5 / (r / 4)))
        for i in range(steps + 1):
            x = x1 + (x2 - x1) * i / steps
            y = y1 + (y2 - y1) * i / steps
            gd2.ellipse([x - r, y - r, x + r, y + r], fill=255)
    white = Image.new('RGBA', (big, big), (255, 255, 255, 255))
    img.paste(white, (0, 0), glyph)
    return img.resize((size, size), Image.LANCZOS)


def bmp_entry(img):
    """Кадр ICO в классическом формате BMP (32 бита BGRA, строки снизу вверх, маска AND)."""
    w, h = img.size
    px = img.load()
    header = struct.pack('<IiiHHIIiiII', 40, w, h * 2, 1, 32, 0, 0, 0, 0, 0, 0)
    rows = bytearray()
    for y in range(h - 1, -1, -1):
        for x in range(w):
            r, g, b, a = px[x, y]
            rows += bytes((b, g, r, a))
    mask_row = ((w + 31) // 32) * 4
    mask = bytes(mask_row * h)          # прозрачность берётся из альфа-канала
    return header + bytes(rows) + mask


def write_ico(path, images):
    """ICO: размеры до 128 px — BMP (понимают все версии Windows и .NET Framework), 256 px — PNG (стандарт Windows Vista+)."""
    blobs = []
    for im in images:
        if im.width >= 256:
            buf = io.BytesIO()
            im.save(buf, format='PNG')
            blobs.append(buf.getvalue())
        else:
            blobs.append(bmp_entry(im))
    out = struct.pack('<HHH', 0, 1, len(images))
    offset = 6 + 16 * len(images)
    for im, blob in zip(images, blobs):
        dim = 0 if im.width >= 256 else im.width
        out += struct.pack('<BBBBHHII', dim, dim, 0, 0, 1, 32, len(blob), offset)
        offset += len(blob)
    with open(path, 'wb') as f:
        f.write(out + b''.join(blobs))


def main():
    root = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..')
    out = os.path.join(root, 'src', 'Integral.App', 'app.ico')
    images = [render(s) for s in SIZES]
    write_ico(out, images)
    # предпросмотр для проверки глазами
    prev = Image.new('RGBA', (sum(SIZES) + 20 * len(SIZES), 280), (240, 240, 240, 255))
    x = 10
    for im in images:
        prev.paste(im, (x, 10), im)
        x += im.width + 20
    prev.save(os.path.join(root, 'docs', 'icon_preview.png'))
    print('ok:', out, os.path.getsize(out), 'bytes')


if __name__ == '__main__':
    main()
