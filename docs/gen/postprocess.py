"""Подстановка рамки и блок-схем (родные фигуры Word) вместо маркеров @@NAME@@ в готовом .docx."""
import re
import sys
import zipfile

sys.path.insert(0, __file__.rsplit('/', 1)[0] + '/lib')
from ooxml import frame_paragraph  # noqa: E402
from flows import flow_general, flow_integrate, flow_tree  # noqa: E402

PARA = r'<w:p(?:\s[^>]*)?>(?:(?!</w:p>).)*?@@%s@@(?:(?!</w:p>).)*?</w:p>'


def replace_marker(xml, name, make):
    count = [0]
    def repl(_m):
        count[0] += 1
        return make(count[0])
    new = re.sub(PARA % name, repl, xml, flags=re.S)
    return new, count[0]


def toc_xml(headings, pages):
    """Содержание: поле Word TOC с готовым (кэшированным) списком; Word обновит его по команде «Обновить поле»."""
    rf = '<w:rFonts w:ascii="Times New Roman" w:hAnsi="Times New Roman" w:cs="Times New Roman"/><w:sz w:val="28"/><w:szCs w:val="28"/>'
    out = []
    n = len(headings)
    for i, h in enumerate(headings):
        left = 0 if h['level'] == 1 else 340
        page = pages.get(h['text'], 0)
        text = h['text'].replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;')
        pre = ''
        if i == 0:
            pre = ('<w:r><w:fldChar w:fldCharType="begin"/></w:r><w:r><w:instrText xml:space="preserve"> TOC \\o "1-2" \\h \\z \\u </w:instrText></w:r>'
                   '<w:r><w:fldChar w:fldCharType="separate"/></w:r>')
        post = '<w:r><w:fldChar w:fldCharType="end"/></w:r>' if i == n - 1 else ''
        out.append(
            '<w:p><w:pPr><w:tabs><w:tab w:val="right" w:leader="dot" w:pos="9922"/></w:tabs>'
            '<w:spacing w:before="0" w:after="60" w:line="300" w:lineRule="auto"/><w:ind w:left="%d" w:right="567"/></w:pPr>%s'
            '<w:r><w:rPr>%s</w:rPr><w:t xml:space="preserve">%s</w:t></w:r><w:r><w:rPr>%s</w:rPr><w:tab/></w:r>'
            '<w:r><w:rPr>%s</w:rPr><w:t>%s</w:t></w:r>%s</w:p>' % (left, pre, rf, text, rf, rf, page, post))
    return ''.join(out)


def main(src, dst, toc=None, pages=None):
    zin = zipfile.ZipFile(src)
    zout = zipfile.ZipFile(dst, 'w', zipfile.ZIP_DEFLATED)
    frames = 0
    for item in zin.infolist():
        data = zin.read(item.filename)
        if item.filename.startswith('word/') and item.filename.endswith('.xml'):
            xml = data.decode('utf-8')
            xml, n = replace_marker(xml, 'FRAME', lambda i: frame_paragraph(9000 + frames + i))
            frames += n
            xml, _ = replace_marker(xml, 'FLOW1', lambda i: flow_general())
            xml, _ = replace_marker(xml, 'FLOW2', lambda i: flow_integrate())
            xml, _ = replace_marker(xml, 'TREE', lambda i: flow_tree())
            if toc:
                xml, _ = replace_marker(xml, 'TOC', lambda i: toc_xml(toc, pages or {}))
            xml = re.sub(r'<w:highlightCs[^>]*/>', '', xml)  # лишний тег docx-js: нарушает схему Word
            data = xml.encode('utf-8')
        zout.writestr(item, data)
    zout.close()
    print('frames placed:', frames)


if __name__ == '__main__':
    import json
    args = sys.argv[1:]
    toc = pages = None
    if '--toc' in args:
        toc = json.load(open(args[args.index('--toc') + 1], encoding='utf-8'))
    if '--pages' in args:
        p = args[args.index('--pages') + 1]
        pages = json.load(open(p, encoding='utf-8')) if __import__('os').path.exists(p) else {}
    main(args[0], args[1], toc, pages)
