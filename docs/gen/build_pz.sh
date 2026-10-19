#!/bin/sh
# Сборка пояснительной записки: docs/pz/Пояснительная_записка.docx (+ .pdf для просмотра).
# Два прохода: первый даёт номера листов заголовков, второй вставляет их в содержание и реферат.
set -e
cd "$(dirname "$0")"
T=$(mktemp -d)
OUT=../pz/Пояснительная_записка.docx
build() { # $1 — pages.json
  node build_pz.js "$T/raw.docx" "$T/headings.json" "$1"
  python3 postprocess.py "$T/raw.docx" "$T/pz.docx" --toc "$T/headings.json" --pages "$1"
}
render() {
  soffice --headless --norestore -env:UserInstallation=file:///tmp/claude-0/lo_profile --convert-to pdf --outdir "$T/pdf" "$T/pz.docx" >/dev/null 2>&1
}
echo '{}' > "$T/pages0.json"
build "$T/pages0.json"; render
python3 pz_pages.py "$T/pdf/pz.pdf" "$T/headings.json" "$T/pages.json"
build "$T/pages.json"; render
python3 pz_pages.py "$T/pdf/pz.pdf" "$T/headings.json" "$T/pages2.json" | tail -1
cmp -s "$T/pages.json" "$T/pages2.json" || { cp "$T/pages2.json" "$T/pages.json"; build "$T/pages.json"; render; }
mkdir -p ../pz
cp "$T/pz.docx" "$OUT"
cp "$T/pdf/pz.pdf" "${OUT%.docx}.pdf"
echo "Готово: $OUT"; pdfinfo "${OUT%.docx}.pdf" | grep Pages
