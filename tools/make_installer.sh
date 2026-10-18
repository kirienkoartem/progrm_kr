#!/bin/sh
# Сборка установщика Windows (setup.exe): Release-сборка программы + NSIS.
# Нужны: .NET SDK и NSIS (makensis). Результат: dist/Integral_Setup_1.0.exe
set -e
ROOT=$(cd "$(dirname "$0")/.." && pwd)
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
mkdir -p "$ROOT/dist"
dotnet build "$ROOT/src/Integral.App/Integral.App.csproj" -c Release -warnaserror -nologo -v q
makensis -V2 \
  -DAPPDIR="$ROOT/src/Integral.App/bin/Release/net48" \
  -DDATADIR="$ROOT/data" \
  -DICON="$ROOT/src/Integral.App/app.ico" \
  -DOUTFILE="$ROOT/dist/Integral_Setup_1.0.exe" \
  "$ROOT/installer/Integral.nsi"
ls -la "$ROOT/dist/Integral_Setup_1.0.exe"
