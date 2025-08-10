@echo off
echo ========================================
echo Network Profile Switcher Build Script
echo ========================================
echo.

:: 現在のディレクトリを確認
echo Current directory: %CD%
echo.

:: プロジェクトディレクトリに移動
cd NetworkProfileSwitcher

:: 依存関係の復元
echo [1/4] Restoring dependencies...
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo Error: Failed to restore dependencies.
    pause
    exit /b 1
)
echo Dependencies restored successfully.
echo.

:: クリーンビルド
echo [2/4] Cleaning build...
dotnet clean
if %ERRORLEVEL% neq 0 (
    echo Warning: Clean failed, but continuing.
)

:: リリースビルド
echo [3/4] Building release version...
dotnet build -c Release --no-restore
if %ERRORLEVEL% neq 0 (
    echo Error: Build failed.
    pause
    exit /b 1
)
echo Build completed successfully.
echo.

:: 単一ファイルでの公開
echo [4/4] Publishing single file...
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
if %ERRORLEVEL% neq 0 (
    echo Error: Single file publish failed.
    pause
    exit /b 1
)
echo Single file publish completed successfully.
echo.

:: 出力ファイルの確認
echo ========================================
echo Build Results
echo ========================================
echo.

:: 出力ディレクトリの内容を表示
set OUTPUT_DIR=bin\Release\net6.0-windows\win-x64\publish
if exist "%OUTPUT_DIR%" (
    echo Output directory: %OUTPUT_DIR%
    echo.
    echo Output files:
    dir /b "%OUTPUT_DIR%"
    echo.
    
    :: 実行ファイルのサイズを表示
    for %%f in ("%OUTPUT_DIR%\*.exe") do (
        echo Executable: %%~nxf
        for %%A in ("%%f") do echo Size: %%~zA bytes
    )
    echo.
    
    echo Build completed successfully!
    echo Executable: %OUTPUT_DIR%\NetworkProfileSwitcher.exe
) else (
    echo Error: Output directory not found.
    pause
    exit /b 1
)

echo.
echo ========================================
echo Build Complete
echo ========================================
pause
