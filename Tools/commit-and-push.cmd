@echo off
setlocal enabledelayedexpansion

set REPO=C:\Projects\PDXSMS_V2
set BRANCH=Phase1_Build

cd /d "%REPO%" || goto :err

echo === Git Status ===
git status || goto :err
echo.

set /p MODE=Stage mode [A=all / S=selected files] (default A): 
if /I "%MODE%"=="" set MODE=A

if /I "%MODE%"=="S" (
	echo Enter file paths to stage (relative to repo root), one per line.
	echo Enter a blank line when done.
	:stageLoop
	set /p FILEPATH=File: 
	if "!FILEPATH!"=="" goto afterStage
	git add "!FILEPATH!" || goto :err
	goto stageLoop
) else (
	git add -A || goto :err
)

:afterStage
echo.
git diff --cached --name-only
echo.

set /p MSG=Enter commit message: 
if "%MSG%"=="" (
	echo Commit message is required.
	exit /b 1
)

set /p CONFIRM=Proceed with commit and push to origin + pop? [Y/N]: 
if /I not "%CONFIRM%"=="Y" (
	echo Canceled.
	exit /b 0
)

git commit -m "%MSG%" || goto :err
git push origin %BRANCH% || goto :err
git push pop %BRANCH% || goto :err

echo.
echo Done.
exit /b 0

:err
echo.
echo Script failed. Check output above.
exit /b 1
