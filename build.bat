@echo off
setlocal enabledelayedexpansion

set GAME_PATH=C:\Program Files (x86)\Steam\steamapps\common\LobotomyCorp
set MANAGED=%GAME_PATH%\LobotomyCorp_Data\Managed

mkdir bin 2>nul

csc ^
  -target:library ^
  -nostdlib ^
  -out:bin\AbnoSkeleton_MOD.dll ^
  -reference:"%MANAGED%\mscorlib.dll" ^
  -reference:"%MANAGED%\Assembly-CSharp.dll" ^
  -reference:"%MANAGED%\UnityEngine.dll" ^
  -reference:"%MANAGED%\UnityEngine.CoreModule.dll" ^
  src\*.cs

if %errorlevel% equ 0 (
    echo Build successful!
    copy bin\AbnoSkeleton_MOD.dll "%GAME_PATH%\LobotomyCorp_Data\BaseMods\AbnoSkeleton_MOD\" /Y
    echo DLL copied to game folder
) else (
    echo Build failed!
    pause
)