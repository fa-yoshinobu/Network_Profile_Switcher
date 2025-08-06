# Network Profile Switcher

## 概要

Network Profile Switcherは、Windows環境でネットワーク設定を簡単に切り替えることができるデスクトップアプリケーションです。複数のネットワーク設定（IPアドレス、サブネットマスク、ゲートウェイ、DNSサーバー）をプリセットとして保存し、ワンクリックで切り替えることができます。

## 主な機能

### 🔧 ネットワーク設定管理
- **静的IP設定**: 固定IPアドレス、サブネットマスク、ゲートウェイ、DNSサーバーの設定
- **DHCP設定**: 自動IP取得設定への切り替え
- **プリセット保存**: 複数のネットワーク設定を名前付きで保存
- **プリセット編集**: 保存された設定の編集・削除・複製

### 🖥️ ユーザーインターフェース
- **ネットワークアダプタ一覧**: システム上の全ネットワークアダプタを表示
- **プリセット一覧**: 保存された設定の一覧表示
- **現在の設定表示**: 選択されたアダプタの現在のIP設定を表示
- **視覚的フィードバック**: アダプタの状態（有効/無効）を色分け表示

### 🔒 セキュリティ
- **管理者権限**: ネットワーク設定変更のため管理者権限で実行
- **自動権限昇格**: 管理者権限なしで起動時に自動的に権限昇格を提案

## システム要件

### 必須要件
- **OS**: Windows 10/11
- **.NET**: .NET 6.0 Runtime
- **権限**: 管理者権限（ネットワーク設定変更のため）

### 推奨要件
- **メモリ**: 4GB以上
- **ディスク容量**: 100MB以上の空き容量

## ダウンロード・インストール

### 方法1: リリース版のダウンロード（推奨）

#### 1. 最新リリースのダウンロード
1. [Releases](https://github.com/your-username/Network_Profile_Switcher/releases)ページから最新版をダウンロード
2. `NetworkProfileSwitcher-Release.zip`を任意のフォルダに展開

#### 2. 初回実行
1. 展開したフォルダ内の`NetworkProfileSwitcher.exe`を右クリック
2. 「管理者として実行」を選択
3. 管理者権限の確認ダイアログで「はい」を選択

### 方法2: ソースコードからビルド

#### 1. 開発環境の準備
```bash
# 必要なツール
- Visual Studio 2022 または Visual Studio Code
- .NET 6.0 SDK
- Windows 10/11
```

#### 2. ソースコードの取得
```bash
git clone https://github.com/your-username/Network_Profile_Switcher.git
cd Network_Profile_Switcher
```

#### 3. ビルド
```bash
# デバッグビルド
dotnet build NetworkProfileSwitcher/NetworkProfileSwitcher.csproj

# リリースビルド（推奨）
dotnet build NetworkProfileSwitcher/NetworkProfileSwitcher.csproj --configuration Release
```

#### 4. 実行
```bash
# ビルドされたファイルを実行
NetworkProfileSwitcher/bin/Release/net6.0-windows/NetworkProfileSwitcher.exe
```

## 使用方法

### 基本的な使い方

#### 1. プリセットの作成
1. アプリケーションを起動
2. 「プリセット追加」ボタンをクリック
3. 以下の情報を入力：
   - **名前**: プリセットの識別名（例：「自宅」「会社」「DHCP」）
   - **IPアドレス**: 固定IPアドレス（例：192.168.1.100）
   - **サブネットマスク**: サブネットマスク（例：255.255.255.0）
   - **ゲートウェイ**: デフォルトゲートウェイ（例：192.168.1.1）
   - **DNS1**: プライマリDNSサーバー（例：8.8.8.8）
   - **DNS2**: セカンダリDNSサーバー（例：8.8.4.4）
   - **コメント**: 設定の説明（任意）

#### 2. DHCP設定の作成
1. プリセット追加画面で「IPアドレス」に「DHCP」と入力
2. その他の項目は空欄のまま
3. 名前を「DHCP」など分かりやすい名前に設定

#### 3. 設定の適用
1. ネットワークアダプタ一覧から設定を変更したいアダプタを選択
2. プリセット一覧から適用したい設定を選択
3. 「設定適用」ボタンをクリック
4. 確認ダイアログで「はい」を選択

### 高度な機能

#### プリセットの管理
- **編集**: プリセットを選択して「編集」ボタンをクリック
- **複製**: プリセットを選択して「複製」ボタンをクリック
- **削除**: プリセットを選択して「削除」ボタンをクリック

#### ネットワーク情報の確認
- **現在の設定**: 選択されたアダプタの現在のIP設定を表示
- **アダプタ情報**: アダプタの種類、速度、MACアドレスを表示
- **更新**: 「更新」ボタンでネットワーク情報を再取得

## ファイル構成

```
Network_Profile_Switcher/
├── NetworkProfileSwitcher/
│   ├── Forms/                    # Windows Forms UI
│   │   ├── MainForm.cs          # メインフォーム
│   │   ├── PresetEditorForm.cs  # プリセット編集フォーム
│   │   └── ErrorDialogForm.cs   # エラーダイアログ
│   ├── Models/                   # データモデル
│   │   ├── NetworkPreset.cs     # プリセットデータクラス
│   │   └── NetworkManager.cs    # ネットワーク設定管理
│   ├── Program.cs               # アプリケーションエントリーポイント
│   ├── app.manifest            # 管理者権限要求設定
│   └── NetworkProfileSwitcher.csproj
├── NetworkProfileSwitcher.sln   # Visual Studio ソリューションファイル
└── README.md                    # このファイル
```

## 設定ファイル

### presets.json
プリセット設定は`presets.json`ファイルに保存されます。

```json
[
  {
    "Name": "自宅",
    "IP": "192.168.1.100",
    "Subnet": "255.255.255.0",
    "Gateway": "192.168.1.1",
    "DNS1": "8.8.8.8",
    "DNS2": "8.8.4.4",
    "Comment": "自宅のWiFi設定"
  },
  {
    "Name": "DHCP",
    "IP": "DHCP",
    "Subnet": "",
    "Gateway": "",
    "DNS1": "",
    "DNS2": "",
    "Comment": "自動IP取得設定"
  }
]
```

### ログファイル
デバッグログは以下の場所に保存されます：
```
%LOCALAPPDATA%\NetworkProfileSwitcher\debug.log
```

## ビルド情報

### リリースビルド
- **実行ファイル**: `NetworkProfileSwitcher.exe` (151KB)
- **依存関係**: 約862KB（合計）
- **最適化**: デバッグ情報を除いた最適化されたコード
- **配布準備**: 自己完結型、ポータブル

### ビルドコマンド
```bash
# デバッグビルド
dotnet build NetworkProfileSwitcher/NetworkProfileSwitcher.csproj

# リリースビルド
dotnet build NetworkProfileSwitcher/NetworkProfileSwitcher.csproj --configuration Release

# クリーンアップ
dotnet clean NetworkProfileSwitcher/NetworkProfileSwitcher.csproj
```

## トラブルシューティング

### よくある問題

#### 1. 管理者権限エラー
**症状**: 「管理者権限が必要です」エラーが表示される
**解決方法**:
- アプリケーションを右クリックして「管理者として実行」を選択
- UAC（ユーザーアカウント制御）の設定を確認
- セキュリティソフトの設定を確認

#### 2. ネットワークアダプタが表示されない
**症状**: ネットワークアダプタ一覧が空
**解決方法**:
- 管理者権限で実行されているか確認
- 「更新」ボタンをクリックして再取得
- ネットワークアダプタが有効になっているか確認
- デバイスマネージャーでネットワークアダプタの状態を確認

#### 3. 設定適用に失敗
**症状**: 「設定適用」ボタンをクリックしても設定が変更されない
**解決方法**:
- 管理者権限で実行されているか確認
- 選択したアダプタが有効になっているか確認
- 入力したIPアドレスが正しい形式か確認
- ファイアウォールやセキュリティソフトの設定を確認
- ネットワークアダプタのドライバーを更新

#### 4. プリセットが保存されない
**症状**: プリセットを追加しても保存されない
**解決方法**:
- アプリケーションの実行ディレクトリに書き込み権限があるか確認
- `presets.json`ファイルが破損していないか確認
- アンチウイルスソフトがファイルの書き込みをブロックしていないか確認

#### 5. .NET Runtimeエラー
**症状**: 「.NET Runtimeが必要です」エラーが表示される
**解決方法**:
- [.NET 6.0 Runtime](https://dotnet.microsoft.com/download/dotnet/6.0)をダウンロード・インストール
- システムを再起動
- アプリケーションを再実行

#### 6. アプリケーションが起動しない
**症状**: アプリケーションが起動しない、または即座に終了する
**解決方法**:
- 管理者権限で実行しているか確認
- ログファイル（`%LOCALAPPDATA%\NetworkProfileSwitcher\debug.log`）を確認
- アンチウイルスソフトがアプリケーションをブロックしていないか確認
- Windows Defenderの設定を確認

#### 7. USB EthernetアダプタでDHCP設定が失敗する
**症状**: ASIX USB to Gigabit EthernetなどのUSBアダプタでDHCP設定が失敗する
**解決方法**:
- **アダプタの状態確認**: アダプタが有効になっているか確認
- **ドライバーの更新**: 最新のドライバーに更新
- **代替方法**: アプリケーションが自動的にWMIを使用してDHCP設定を適用
- **手動設定**: コントロールパネルから手動でDHCP設定を確認
- **アダプタの再接続**: USBアダプタを抜き差しして再接続

**詳細なエラー情報**:
- アプリケーションはnetshコマンドを試行し、失敗した場合はWMIを使用
- より詳細なエラー情報が表示されるようになりました
- アダプタの状態、種類、エラー詳細が含まれます

## 技術仕様

### 使用技術
- **言語**: C# (.NET 6.0)
- **UI**: Windows Forms
- **設定保存**: JSON (System.Text.Json 8.0.5)
- **ネットワーク操作**: netshコマンド
- **ビルド**: MSBuild

### アーキテクチャ
- **MVVMパターン**: モデルとビューの分離
- **静的クラス**: NetworkManagerでネットワーク操作を集約
- **イベント駆動**: UI操作による非同期処理
- **Null安全性**: C# 8.0のNullable Reference Typesを活用

### セキュリティ
- **管理者権限**: ネットワーク設定変更のため必要
- **UAC対応**: ユーザーアカウント制御との連携
- **エラーハンドリング**: 包括的な例外処理
- **ログ機能**: デバッグ情報の記録

## ライセンス

このプロジェクトはMITライセンスの下で公開されています。

## 貢献

バグ報告や機能要望は[Issues](https://github.com/your-username/Network_Profile_Switcher/issues)でお知らせください。

プルリクエストも歓迎します。貢献する前に以下をご確認ください：
1. コードスタイルの統一
2. 適切なテストの追加
3. ドキュメントの更新
4. Null安全性の確保

## サポート

### ヘルプが必要な場合
1. **ドキュメント**: このREADMEファイルを参照
2. **Issues**: [GitHub Issues](https://github.com/your-username/Network_Profile_Switcher/issues)で質問
3. **ログファイル**: `%LOCALAPPDATA%\NetworkProfileSwitcher\debug.log`を確認

### フィードバック
- バグ報告
- 機能要望
- 改善提案
- 使用体験の共有

すべてのフィードバックを歓迎いたします！
