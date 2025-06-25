# SBOMシステム

製造業向けの部品表（BOM: Bill of Materials）管理システムです。機械の製造番号ごとに部品構成を管理し、部品の追跡や変更履歴を記録します。

## プロジェクト構成

```
SBOM/
├── SBOMSystemAPI/          # Web APIプロジェクト（ASP.NET Core）
├── SBOMSystemMockup/       # フロントエンドプロジェクト（Blazor WebAssembly）
└── SBOMSystemDatabase/     # データベースプロジェクト（SQL Server）
```

## 主な機能

- **機械管理**: 製造番号による機械の個体管理
- **部品構成管理**: 階層構造での部品表示（通常/階層/展開表示）
- **部品変更履歴**: 部品の変更理由と履歴の記録
- **セット品管理**: 複数部品をまとめたセット品の登録・編集
- **オプション管理**: 機械に追加可能なオプション部品の選択
- **部品情報取得**: 外部システムからの部品情報インポート

## 技術スタック

- **フロントエンド**: Blazor WebAssembly (.NET 9)
- **バックエンド**: ASP.NET Core Web API (.NET 9)
- **データベース**: SQL Server
- **CSS フレームワーク**: Bootstrap 5

## 開発環境のセットアップ

### 必要なツール

- .NET 9 SDK
- Visual Studio 2022 または Visual Studio Code
- SQL Server（LocalDB可）

### 実行方法

1. リポジトリをクローン
```bash
git clone [repository-url]
cd SBOM
```

2. データベースの準備
```bash
cd SBOMSystemDatabase
# SQL Serverにデータベースを作成
```

3. APIプロジェクトの実行
```bash
cd SBOMSystemAPI
dotnet run
```

4. フロントエンドプロジェクトの実行
```bash
cd SBOMSystemMockup
dotnet run
```

5. ブラウザで `https://localhost:5001` にアクセス

## 画面一覧

1. **ホーム画面**: 機械一覧の表示
2. **機械詳細画面**: 個別機械の詳細情報
3. **個体部品一覧画面**: 機械の部品構成表示
4. **部品情報取得画面**: 外部システムからの部品情報取得
5. **部品変更画面**: 部品の変更登録
6. **セット登録・編集画面**: セット品の管理
7. **オプション選択画面**: オプション部品の選択

## データベース構成

主要なテーブル：
- `T_機械`: 機械の基本情報
- `T_部品マスタ`: 部品のマスタ情報
- `T_個体部品`: 機械ごとの部品構成
- `T_部品変更履歴`: 部品変更の履歴管理
- `T_セット品`: セット品の定義

## 開発ガイドライン

- コーディング規約: C#標準規約に準拠
- コミットメッセージ: 日本語で簡潔に記述
- ブランチ戦略: main + feature/topic ブランチ

## ライセンス

[ライセンス情報を記載]