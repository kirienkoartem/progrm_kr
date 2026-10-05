#!/bin/sh
# Проверка: ни один рукописный .cs-файл не длиннее 250 строк (требование методички 2.5).
LIMIT=250
BAD=0
for f in $(find src tests -name '*.cs' ! -name '*.Designer.cs' ! -path '*/obj/*' ! -path '*/bin/*'); do
  n=$(wc -l < "$f")
  if [ "$n" -gt "$LIMIT" ]; then echo "ПРЕВЫШЕНИЕ: $f — $n строк"; BAD=1; fi
done
[ "$BAD" -eq 0 ] && echo "OK: все файлы не длиннее $LIMIT строк"
exit $BAD
