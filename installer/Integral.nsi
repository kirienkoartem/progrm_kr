; Установщик программы «Интегрирование функций» (NSIS 3, Unicode).
; Сборка: sh tools/make_installer.sh   (на Linux и Windows с установленным NSIS)
; Параметры сборки передаются через /D: APPDIR (папка с Integral.exe), DATADIR (примеры данных), ICON, OUTFILE.
Unicode true
SetCompressor /SOLID lzma

!define APPNAME   "Интегрирование функций"
!define APPVER    "1.0.0"
!define PUBLISHER "Кириенко А.М., ДонГТУ, группа БИ-25"
!define UNINSTKEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\IntegralKR"

!ifndef OUTFILE
  !define OUTFILE "Integral_Setup_1.0.exe"
!endif

Name "${APPNAME} ${APPVER}"
OutFile "${OUTFILE}"
; Установка для текущего пользователя: права администратора не нужны (удобно на учебных компьютерах).
RequestExecutionLevel user
InstallDir "$LOCALAPPDATA\Programs\Integral"
InstallDirRegKey HKCU "${UNINSTKEY}" "InstallLocation"
BrandingText "${APPNAME} ${APPVER}"

!include "MUI2.nsh"
!include "x64.nsh"
!include "LogicLib.nsh"

!define MUI_ICON   "${ICON}"
!define MUI_UNICON "${ICON}"
!define MUI_ABORTWARNING
!define MUI_WELCOMEPAGE_TITLE "Установка программы «${APPNAME}»"
!define MUI_WELCOMEPAGE_TEXT "Мастер установит программу вычисления определённых интегралов методами трапеций и Симпсона.$\r$\n$\r$\nКурсовая работа по дисциплине «Основы программирования».$\r$\nАвтор: Кириенко Артём Максимович, группа БИ-25.$\r$\n$\r$\nУстановка не требует прав администратора."
!define MUI_FINISHPAGE_RUN "$INSTDIR\Integral.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Запустить программу"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "Russian"

VIProductVersion "1.0.0.0"
VIAddVersionKey /LANG=${LANG_RUSSIAN} "ProductName"     "${APPNAME}"
VIAddVersionKey /LANG=${LANG_RUSSIAN} "FileDescription" "Установщик программы «${APPNAME}»"
VIAddVersionKey /LANG=${LANG_RUSSIAN} "CompanyName"     "${PUBLISHER}"
VIAddVersionKey /LANG=${LANG_RUSSIAN} "LegalCopyright"  "© 2026 Кириенко А.М."
VIAddVersionKey /LANG=${LANG_RUSSIAN} "FileVersion"     "${APPVER}"
VIAddVersionKey /LANG=${LANG_RUSSIAN} "ProductVersion"  "${APPVER}"

; Программа требует .NET Framework 4.8 (на Windows 10/11 он уже установлен). Release >= 528040.
Function .onInit
  ${If} ${RunningX64}
    SetRegView 64
  ${EndIf}
  ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"
  ${If} $0 < 528040
    MessageBox MB_YESNO|MB_ICONEXCLAMATION "Для работы программы нужен Microsoft .NET Framework 4.8, он не найден на этом компьютере.$\r$\n$\r$\nСкачать его можно на сайте Microsoft (поиск: «.NET Framework 4.8 Runtime»). Продолжить установку?" /SD IDYES IDYES +2
    Abort
  ${EndIf}
  SetRegView 32
FunctionEnd

Section "Программа" SecMain
  SectionIn RO
  SetOutPath "$INSTDIR"
  File "${APPDIR}\Integral.exe"
  File "${APPDIR}\Integral.exe.config"
  File "${APPDIR}\Integral.Core.dll"
  SetOutPath "$INSTDIR\Примеры данных"
  File /r "${DATADIR}\*.txt"
  SetOutPath "$INSTDIR"

  WriteUninstaller "$INSTDIR\Uninstall.exe"

  CreateDirectory "$SMPROGRAMS\${APPNAME}"
  CreateShortCut "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk" "$INSTDIR\Integral.exe" "" "$INSTDIR\Integral.exe" 0
  CreateShortCut "$SMPROGRAMS\${APPNAME}\Удалить программу.lnk" "$INSTDIR\Uninstall.exe"
  CreateShortCut "$DESKTOP\${APPNAME}.lnk" "$INSTDIR\Integral.exe" "" "$INSTDIR\Integral.exe" 0

  ; Запись в «Приложения и возможности» (для текущего пользователя)
  WriteRegStr   HKCU "${UNINSTKEY}" "DisplayName"     "${APPNAME}"
  WriteRegStr   HKCU "${UNINSTKEY}" "DisplayVersion"  "${APPVER}"
  WriteRegStr   HKCU "${UNINSTKEY}" "Publisher"       "${PUBLISHER}"
  WriteRegStr   HKCU "${UNINSTKEY}" "DisplayIcon"     "$INSTDIR\Integral.exe"
  WriteRegStr   HKCU "${UNINSTKEY}" "InstallLocation" "$INSTDIR"
  WriteRegStr   HKCU "${UNINSTKEY}" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegDWORD HKCU "${UNINSTKEY}" "NoModify" 1
  WriteRegDWORD HKCU "${UNINSTKEY}" "NoRepair" 1
  WriteRegDWORD HKCU "${UNINSTKEY}" "EstimatedSize" 800
SectionEnd

Section "Uninstall"
  Delete "$DESKTOP\${APPNAME}.lnk"
  Delete "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk"
  Delete "$SMPROGRAMS\${APPNAME}\Удалить программу.lnk"
  RMDir  "$SMPROGRAMS\${APPNAME}"
  Delete "$INSTDIR\Integral.exe"
  Delete "$INSTDIR\Integral.exe.config"
  Delete "$INSTDIR\Integral.Core.dll"
  Delete "$INSTDIR\Примеры данных\*.txt"
  RMDir  "$INSTDIR\Примеры данных"
  Delete "$INSTDIR\Uninstall.exe"
  RMDir  "$INSTDIR"
  DeleteRegKey HKCU "${UNINSTKEY}"
SectionEnd
