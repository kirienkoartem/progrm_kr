"""Подстановка рамки и блок-схем (родные фигуры Word) вместо маркеров @@NAME@@ в готовом .docx."""
import re
import sys
import zipfile

sys.path.insert(0, __file__.rsplit('/', 1)[0] + '/lib')
from ooxml import frame_paragraph  # noqa: E402
from flows import flow_general, flow_integrate  # noqa: E402

PARA = r'<w:p(?:\s[^>]*)?>(?:(?!</w:p>).)*?@@%s@@(?:(?!</w:p>).)*?</w:p>'


def replace_marker(xml, name, make):
    count = [0]
    def repl(_m):
        count[0] += 1
        return make(count[0])
    new = re.sub(PARA % name, repl, xml, flags=re.S)
    return new, count[0]


def main(src, dst):
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
            data = xml.encode('utf-8')
        zout.writestr(item, data)
    zout.close()
    print('frames placed:', frames)


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2])
