@echo off
chcp 65001 >nul
rem Сборка программы на Windows (нужен .NET SDK — ставится вместе с Visual Studio 2022)
dotnet build "%~dp0src\Integral.App\Integral.App.csproj" -c Release
if errorlevel 1 (
  echo.
  echo Ошибка сборки.
  pause
  exit /b 1
)
echo.
echo Готово: src\Integral.App\bin\Release\net48\Integral.exe
pause
