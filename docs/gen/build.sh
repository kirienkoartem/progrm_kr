#!/bin/sh
# Сборка материалов КТ: node (текст, таблицы, штампы) -> python (рамка, блок-схемы) -> docs/kt/*.docx
set -e
cd "$(dirname "$0")"
node verify_names.js
TMP=$(mktemp -d)
node build_kt.js "$TMP/raw.docx"
python3 postprocess.py "$TMP/raw.docx" ../kt/KT_materialy_2026-10-19.docx
rm -rf "$TMP"
