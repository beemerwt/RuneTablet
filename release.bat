set "FILES=manifest.json icon.png README.md"
set "RELEASE_DIR=releases"
set "ASSETS_DIR=Assets"
set "DLL_PATH=bin\Debug\net462\RuneTablet.dll"

set "TEMP_DIR=RuneTablet"
set "DLL_TEMP=RuneTablet.dll"

set "ZIP_FILE=RuneTablet-1.0.0.zip"

:: Make necessary directories
if not exist "%TEMP_DIR%" mkdir "%TEMP_DIR%"
if not exist "%RELEASE_DIR%" mkdir "%RELEASE_DIR%"

:: Copy files to make the final Zip structure
if exist "%DLL_PATH%" copy "%DLL_PATH%" "%TEMP_DIR%\" >nul
xcopy /S /I "%ASSETS_DIR%" "%TEMP_DIR%\Assets" >nul

tar -a -cf %ZIP_FILE% "%TEMP_DIR%" %FILES%

move "%ZIP_FILE%" "%RELEASE_DIR%\" >nul
rmdir /S /Q "%TEMP_DIR%"