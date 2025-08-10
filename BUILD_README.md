# Network Profile Switcher ビルドガイド

このディレクトリには、Network Profile Switcherのビルド用バッチファイルが含まれています。

## 利用可能なバッチファイル

### 1. `build.bat` - リリースビルド（単一ファイル）
- **用途**: 本番用の単一実行ファイルを作成
- **出力**: `NetworkProfileSwitcher/bin/Release/net6.0-windows/win-x64/publish/NetworkProfileSwitcher.exe`
- **特徴**: 
  - 単一の.exeファイルとして出力
  - .NET 6.0 Runtimeが必要
  - 最適化されたリリースビルド

### 2. `build-debug.bat` - デバッグビルド
- **用途**: 開発・デバッグ用のビルド
- **出力**: `NetworkProfileSwitcher/bin/Debug/net6.0-windows/NetworkProfileSwitcher.exe`
- **特徴**:
  - デバッグ情報付き
  - 開発時のテスト用

### 3. `clean.bat` - クリーンアップ
- **用途**: ビルド成果物の削除
- **対象**: bin, objディレクトリ
- **特徴**: 完全なクリーンアップ

## 使用方法

### リリースビルド（推奨）
```cmd
build.bat
```

### デバッグビルド
```cmd
build-debug.bat
```

### クリーンアップ
```cmd
clean.bat
```

## ビルド要件

- **.NET 6.0 SDK** がインストールされていること
- **Windows 10/11** で実行すること
- **管理者権限** は不要（通常のビルド時）

## 出力ファイル

### リリースビルド
- **場所**: `NetworkProfileSwitcher/bin/Release/net6.0-windows/win-x64/publish/`
- **ファイル**: `NetworkProfileSwitcher.exe` (単一ファイル)
- **サイズ**: 約2-3MB
- **依存関係**: .NET 6.0 Runtime

### デバッグビルド
- **場所**: `NetworkProfileSwitcher/bin/Debug/net6.0-windows/`
- **ファイル**: `NetworkProfileSwitcher.exe`, `NetworkProfileSwitcher.dll` など
- **サイズ**: 約1-2MB
- **依存関係**: .NET 6.0 Runtime

## トラブルシューティング

### ビルドエラーが発生した場合
1. `clean.bat` を実行してクリーンアップ
2. .NET 6.0 SDKが正しくインストールされているか確認
3. 再度 `build.bat` を実行

### 実行時にエラーが発生した場合
1. .NET 6.0 Runtimeがインストールされているか確認
2. 管理者権限で実行してみる
3. アンチウイルスソフトの除外設定を確認

## 配布について

### リリースビルドの配布
- `NetworkProfileSwitcher.exe` 単体で配布可能
- ユーザー側に.NET 6.0 Runtimeが必要
- インストーラー不要

### 自己完結型ビルド（オプション）
より大きなファイルサイズになりますが、.NET Runtimeが不要なバージョンも作成可能です：

```cmd
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 注意事項

- ビルドには数分かかる場合があります
- 初回ビルド時は依存関係のダウンロードに時間がかかります
- アンチウイルスソフトがビルドを妨げる場合があります
