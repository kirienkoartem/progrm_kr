"""Генерация OOXML для рамки листа и блок-схем (родные фигуры Word, редактируются в Word).

Блок-схемы строятся по ГОСТ 19.701-90: терминатор, процесс, решение, ввод-вывод,
предопределённый процесс; линии связи со стрелками.
"""
from xml.sax.saxutils import escape

EMU_MM = 36000
NS_W = 'xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"'
NS_WP = 'xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"'
NS_A = 'xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"'
NS_MC = 'xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"'
NS_WPS = 'xmlns:wps="http://schemas.microsoft.com/office/word/2010/wordprocessingShape"'
NS_WPG = 'xmlns:wpg="http://schemas.microsoft.com/office/word/2010/wordprocessingGroup"'

_next_id = [100]


def _id():
    _next_id[0] += 1
    return _next_id[0]


def emu(v):
    return int(round(v * EMU_MM))


def frame_paragraph(doc_pr_id):
    """Рамка листа: прямоугольник 185x287 мм на расстоянии 20/5/5/5 мм от краёв страницы."""
    return (
        '<w:p %s %s %s %s %s><w:pPr><w:spacing w:before="0" w:after="0" w:line="240" w:lineRule="auto"/></w:pPr><w:r>'
        '<mc:AlternateContent><mc:Choice Requires="wps"><w:drawing>'
        '<wp:anchor distT="0" distB="0" distL="0" distR="0" simplePos="0" relativeHeight="0" behindDoc="1" '
        'locked="1" layoutInCell="1" allowOverlap="1"><wp:simplePos x="0" y="0"/>'
        '<wp:positionH relativeFrom="page"><wp:posOffset>%d</wp:posOffset></wp:positionH>'
        '<wp:positionV relativeFrom="page"><wp:posOffset>%d</wp:posOffset></wp:positionV>'
        '<wp:extent cx="%d" cy="%d"/><wp:effectExtent l="0" t="0" r="0" b="0"/><wp:wrapNone/>'
        '<wp:docPr id="%d" name="Рамка листа"/><wp:cNvGraphicFramePr/>'
        '<a:graphic><a:graphicData uri="http://schemas.microsoft.com/office/word/2010/wordprocessingShape">'
        '<wps:wsp><wps:cNvSpPr/><wps:spPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="%d" cy="%d"/></a:xfrm>'
        '<a:prstGeom prst="rect"><a:avLst/></a:prstGeom><a:noFill/>'
        '<a:ln w="19050"><a:solidFill><a:srgbClr val="000000"/></a:solidFill></a:ln></wps:spPr>'
        '<wps:bodyPr/></wps:wsp></a:graphicData></a:graphic></wp:anchor></w:drawing></mc:Choice></mc:AlternateContent>'
        '</w:r></w:p>'
    ) % (NS_W, NS_WP, NS_A, NS_MC, NS_WPS, emu(20), emu(5), emu(185), emu(287), doc_pr_id, emu(185), emu(287))


# ---------- блок-схемы ----------

PRST = {
    'terminator': 'flowChartTerminator',
    'process': 'flowChartProcess',
    'decision': 'flowChartDecision',
    'io': 'flowChartInputOutput',
    'subprocess': 'flowChartPredefinedProcess',
}


def _text_body(text, size_pt):
    paras = ''
    for line in text.split('\n'):
        paras += (
            '<w:p><w:pPr><w:spacing w:before="0" w:after="0" w:line="240" w:lineRule="auto"/><w:jc w:val="center"/></w:pPr>'
            '<w:r><w:rPr><w:rFonts w:ascii="Times New Roman" w:hAnsi="Times New Roman" w:cs="Times New Roman"/>'
            '<w:sz w:val="%d"/><w:szCs w:val="%d"/></w:rPr><w:t xml:space="preserve">%s</w:t></w:r></w:p>'
        ) % (size_pt * 2, size_pt * 2, escape(line))
    return paras


def shape(kind, text, x, y, w, h, size_pt=11):
    inset = emu(0.6) if kind != 'decision' else emu(0.2)
    return (
        '<wps:wsp><wps:cNvPr id="%d" name="Блок %d"/><wps:cNvSpPr/><wps:spPr>'
        '<a:xfrm><a:off x="%d" y="%d"/><a:ext cx="%d" cy="%d"/></a:xfrm>'
        '<a:prstGeom prst="%s"><a:avLst/></a:prstGeom><a:solidFill><a:srgbClr val="FFFFFF"/></a:solidFill>'
        '<a:ln w="12700"><a:solidFill><a:srgbClr val="000000"/></a:solidFill></a:ln></wps:spPr>'
        '<wps:txbx><w:txbxContent>%s</w:txbxContent></wps:txbx>'
        '<wps:bodyPr lIns="%d" tIns="0" rIns="%d" bIns="0" anchor="ctr" wrap="square"><a:noAutofit/></wps:bodyPr></wps:wsp>'
    ) % (_id(), _id(), emu(x), emu(y), emu(w), emu(h), PRST[kind], _text_body(text, size_pt), inset, inset)


def line(x1, y1, x2, y2, arrow=False):
    """Отрезок от (x1,y1) к (x2,y2) в мм; стрелка на конце, если arrow."""
    flip = ''
    if x2 < x1:
        flip += ' flipH="1"'
    if y2 < y1:
        flip += ' flipV="1"'
    tail = '<a:tailEnd type="triangle" w="med" len="med"/>' if arrow else ''
    return (
        '<wps:wsp><wps:cNvPr id="%d" name="Линия %d"/><wps:cNvCnPr/><wps:spPr>'
        '<a:xfrm%s><a:off x="%d" y="%d"/><a:ext cx="%d" cy="%d"/></a:xfrm>'
        '<a:prstGeom prst="line"><a:avLst/></a:prstGeom>'
        '<a:ln w="12700"><a:solidFill><a:srgbClr val="000000"/></a:solidFill>%s</a:ln></wps:spPr>'
        '<wps:bodyPr/></wps:wsp>'
    ) % (_id(), _id(), flip, emu(min(x1, x2)), emu(min(y1, y2)), emu(abs(x2 - x1)), emu(abs(y2 - y1)), tail)


def label(text, x, y, w=8, h=4.5, size_pt=10):
    """Подпись «Да»/«Нет» возле линии (прозрачный текстовый блок)."""
    return (
        '<wps:wsp><wps:cNvPr id="%d" name="Подпись %d"/><wps:cNvSpPr txBox="1"/><wps:spPr>'
        '<a:xfrm><a:off x="%d" y="%d"/><a:ext cx="%d" cy="%d"/></a:xfrm>'
        '<a:prstGeom prst="rect"><a:avLst/></a:prstGeom><a:noFill/><a:ln><a:noFill/></a:ln></wps:spPr>'
        '<wps:txbx><w:txbxContent>%s</w:txbxContent></wps:txbx>'
        '<wps:bodyPr lIns="0" tIns="0" rIns="0" bIns="0" anchor="ctr"><a:noAutofit/></wps:bodyPr></wps:wsp>'
    ) % (_id(), _id(), emu(x), emu(y), emu(w), emu(h), _text_body(text, size_pt))


def polyline(points, arrow=True):
    """Ломаная из горизонтальных/вертикальных отрезков; стрелка на последнем."""
    out = ''
    for i in range(len(points) - 1):
        (x1, y1), (x2, y2) = points[i], points[i + 1]
        out += line(x1, y1, x2, y2, arrow=arrow and i == len(points) - 2)
    return out


def figure_paragraph(width_mm, height_mm, children, name):
    """Группа фигур в абзаце (inline), по центру."""
    cx, cy = emu(width_mm), emu(height_mm)
    return (
        '<w:p %s %s %s %s %s %s><w:pPr><w:keepNext/><w:spacing w:before="120" w:after="60" w:line="240" w:lineRule="auto"/>'
        '<w:jc w:val="center"/></w:pPr><w:r><mc:AlternateContent><mc:Choice Requires="wpg"><w:drawing>'
        '<wp:inline distT="0" distB="0" distL="0" distR="0"><wp:extent cx="%d" cy="%d"/>'
        '<wp:effectExtent l="0" t="0" r="0" b="0"/><wp:docPr id="%d" name="%s"/><wp:cNvGraphicFramePr/>'
        '<a:graphic><a:graphicData uri="http://schemas.microsoft.com/office/word/2010/wordprocessingGroup">'
        '<wpg:wgp><wpg:cNvGrpSpPr/><wpg:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="%d" cy="%d"/>'
        '<a:chOff x="0" y="0"/><a:chExt cx="%d" cy="%d"/></a:xfrm></wpg:grpSpPr>%s</wpg:wgp>'
        '</a:graphicData></a:graphic></wp:inline></w:drawing></mc:Choice></mc:AlternateContent></w:r></w:p>'
    ) % (NS_W, NS_WP, NS_A, NS_MC, NS_WPS, NS_WPG, cx, cy, _id(), escape(name), cx, cy, cx, cy, ''.join(children))
