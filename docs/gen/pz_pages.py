"""Определяет номера листов заголовков по PDF: python3 pz_pages.py file.pdf headings.json pages.json"""
import json, re, subprocess, sys

pdf, headings_path, out = sys.argv[1:4]
heads = json.load(open(headings_path, encoding='utf-8'))
n = int(re.search(r'Pages:\s+(\d+)', subprocess.run(['pdfinfo', pdf], capture_output=True, text=True).stdout).group(1))
txt = [subprocess.run(['pdftotext', '-f', str(i), '-l', str(i), '-layout', pdf, '-'], capture_output=True, text=True).stdout for i in range(1, n + 1)]
toc_pages = {i + 1 for i, t in enumerate(txt) if re.search(r'^\s*СОДЕРЖАНИЕ\s*$', t, re.M)}
norm = lambda s: re.sub(r'\s+', ' ', s).strip()
pages = {'total': n}
for h in heads:
    key = norm(h['find'])[:24]
    for i, t in enumerate(txt, 1):
        if i in toc_pages:
            continue
        if any(norm(line).startswith(key) for line in t.split('\n')):
            pages[h['text']] = i
            break
    else:
        print('НЕ НАЙДЕН заголовок в PDF:', h['text'])
json.dump(pages, open(out, 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
print('листов всего:', n, '| найдено заголовков:', len(pages) - 1, 'из', len(heads))
