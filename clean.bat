@echo off
chcp 65001 >nul
echo ========================================
echo Network Profile Switcher クリーンアップ
echo ========================================
echo.

:: 現在のディレクトリを確認
echo 現在のディレクトリ: %CD%
echo.

:: プロジェクトディレクトリに移動
cd NetworkProfileSwitcher

:: 確認メッセージ
echo 警告: この操作により、すべてのビルド成果物が削除されます。
echo.
set /p confirm="続行しますか？ (y/N): "
if /i not "%confirm%"=="y" (
    echo クリーンアップをキャンセルしました。
    pause
    exit /b 0
)

echo.
echo クリーンアップを開始します...
echo.

:: dotnet cleanを実行
echo [1/2] dotnet cleanを実行中...
dotnet clean
if %ERRORLEVEL% neq 0 (
    echo 警告: dotnet cleanでエラーが発生しました。
)

:: 手動でbinとobjディレクトリを削除
echo [2/2] 手動でビルドディレクトリを削除中...
if exist "bin" (
    echo binディレクトリを削除中...
    rmdir /s /q "bin" 2>nul
    if exist "bin" (
        echo 警告: binディレクトリの削除に失敗しました。
    ) else (
        echo binディレクトリを削除しました。
    )
) else (
    echo binディレクトリは存在しません。
)

if exist "obj" (
    echo objディレクトリを削除中...
    rmdir /s /q "obj" 2>nul
    if exist "obj" (
        echo 警告: objディレクトリの削除に失敗しました。
    ) else (
        echo objディレクトリを削除しました。
    )
) else (
    echo objディレクトリは存在しません。
)

echo.
echo ========================================
echo クリーンアップ完了
echo ========================================
echo.
echo すべてのビルド成果物が削除されました。
echo 次回のビルド時には、依存関係の復元から開始されます。
echo.
pause
