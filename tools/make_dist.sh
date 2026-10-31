#!/bin/sh
# Сборка содержимого CD/DVD для сдачи курсовой работы: dist/<имя>/ и архив dist/<имя>.zip.
#   Программа/           Integral.exe (Release, .NET Framework 4.8) + Integral.Core.dll
#   Примеры данных/      файлы *.txt из data/
#   Исходный код/        файлы проекта из последнего коммита (решение, src, tests, data)
#   Пояснительная записка/  сюда кладётся ПЗ (.docx) перед записью диска
#   ПРОЧТИ.txt           как запустить и собрать
set -e
ROOT=$(cd "$(dirname "$0")/.." && pwd)
NAME="КР_Интегрирование_Кириенко_БИ-25"
OUT="$ROOT/dist/$NAME"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

rm -rf "$ROOT/dist"
mkdir -p "$OUT/Программа" "$OUT/Примеры данных" "$OUT/Исходный код" "$OUT/Пояснительная записка"

dotnet build "$ROOT/src/Integral.App/Integral.App.csproj" -c Release -warnaserror -nologo -v q
BIN="$ROOT/src/Integral.App/bin/Release/net48"
cp "$BIN/Integral.exe" "$BIN/Integral.exe.config" "$BIN/Integral.Core.dll" "$OUT/Программа/"

cp "$ROOT"/data/*.txt "$OUT/Примеры данных/"

cd "$ROOT"
git archive --format=tar HEAD Integral.sln Directory.Build.props build.bat src tests data | tar -x -C "$OUT/Исходный код"

# Текстовые файлы для Блокнота Windows: UTF-8 с BOM и переводы строк CRLF
to_windows() { printf '\357\273\277' > "$2"; sed 's/$/\r/' "$1" >> "$2"; }
to_windows "$ROOT/tools/dist_readme.txt" "$OUT/ПРОЧТИ.txt"
to_windows "$ROOT/tools/dist_pz_note.txt" "$OUT/Пояснительная записка/Сюда положить ПЗ.txt"

cd "$ROOT/dist"
zip -qr "$NAME.zip" "$NAME"
echo "Готово: dist/$NAME/ и dist/$NAME.zip"
ls -la "$OUT/Программа"
