@echo off
chcp 65001 >nul
echo ========================================
echo Network Profile Switcher デバッグビルド
echo ========================================
echo.

:: 現在のディレクトリを確認
echo 現在のディレクトリ: %CD%
echo.

:: プロジェクトディレクトリに移動
cd NetworkProfileSwitcher

:: 依存関係の復元
echo [1/3] 依存関係を復元中...
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo エラー: 依存関係の復元に失敗しました。
    pause
    exit /b 1
)
echo 依存関係の復元が完了しました。
echo.

:: クリーンビルド
echo [2/3] クリーンビルドを実行中...
dotnet clean
if %ERRORLEVEL% neq 0 (
    echo 警告: クリーンに失敗しましたが、続行します。
)

:: デバッグビルド
echo [3/3] デバッグビルドを実行中...
dotnet build -c Debug --no-restore
if %ERRORLEVEL% neq 0 (
    echo エラー: ビルドに失敗しました。
    pause
    exit /b 1
)
echo デバッグビルドが完了しました。
echo.

:: 出力ファイルの確認
echo ========================================
echo ビルド結果
echo ========================================
echo.

:: 出力ディレクトリの内容を表示
set OUTPUT_DIR=bin\Debug\net6.0-windows
if exist "%OUTPUT_DIR%" (
    echo 出力ディレクトリ: %OUTPUT_DIR%
    echo.
    echo 出力ファイル:
    dir /b "%OUTPUT_DIR%"
    echo.
    
    :: 実行ファイルのサイズを表示
    for %%f in ("%OUTPUT_DIR%\*.exe") do (
        echo 実行ファイル: %%~nxf
        for %%A in ("%%f") do echo サイズ: %%~zA バイト
    )
    echo.
    
    echo デバッグビルドが正常に完了しました！
    echo 実行ファイル: %OUTPUT_DIR%\NetworkProfileSwitcher.exe
) else (
    echo エラー: 出力ディレクトリが見つかりません。
    pause
    exit /b 1
)

echo.
echo ========================================
echo デバッグビルド完了
echo ========================================
pause
