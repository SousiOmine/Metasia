# CLAUDE.md

このファイルは、Claude Code (claude.ai/code) がこのリポジトリでコードを操作する際のガイダンスを提供します。

## プロジェクト概要

Metasiaは、C# .NET 8.0とAvalonia UIで構築されたクロスプラットフォーム動画編集アプリケーションです。ReactiveUIを使用したMVVMアーキテクチャに従い、レイヤーとクリップを持つタイムラインベースの編集システムを実装しています。

## 一般的な開発コマンド

### ビルドコマンド
```bash
# ソリューション全体をビルド
dotnet build Metasia.sln

# 特定のプロジェクトをビルド
dotnet build Metasia.Editor/Metasia.Editor.csproj
dotnet build Metasia.Core/Metasia.Core.csproj
```

### テストコマンド
```bash
# 全てのテストを実行
dotnet test Metasia.sln

# 特定のテストプロジェクトを実行
dotnet test Metasia.Core.Tests/Metasia.Core.Tests.csproj
dotnet test Metasia.Editor.Tests/Metasia.Editor.Tests.csproj

# カバレッジ付きで実行
dotnet test --collect:"XPlat Code Coverage"
```

### アプリケーション実行
```bash
# メインエディターアプリケーションを実行
dotnet run --project Metasia.Editor/Metasia.Editor.csproj
```

### パッケージ管理
```bash
# 依存関係を復元
dotnet restore Metasia.sln

# ビルド成果物をクリーン
dotnet clean Metasia.sln
```

## アーキテクチャ概要

### プロジェクト構造
- **Metasia.Core**: タイムラインオブジェクト、レンダリングパイプライン、シリアライゼーションを含むコアビジネスロジックライブラリ
- **Metasia.Editor**: ReactiveUIを使用したMVVMパターンを実装するAvalonia UIアプリケーション
- **Metasia.Core.Tests**: コアライブラリの単体テスト（NUnit）
- **Metasia.Editor.Tests**: エディターアプリケーションの単体テスト（NUnit + Moq）

### 主要デザインパターン
- **MVVM**: ViewModelはReactiveUIプロパティ通知を持つViewModelBaseから継承
- **コマンドパターン**: 全ての編集操作はundo/redo機能のためIEditCommandを実装
- **オブザーバーパターン**: 変更通知のためのObservableCollectionとReactiveUIのWhenAnyValue

### コアドメインオブジェクト
- **MetasiaObject**: 全てのタイムラインオブジェクトのベースクラス
- **TimelineObject**: 複数レイヤーを持つタイムライン全体を管理
- **LayerObject**: クリップとオブジェクトを含む個別レイヤー
- **ClipViewModel**: ドラッグ&ドロップサポート付きタイムラインクリップのUI表現

## コーディング規約

### C# 命名規則
- **クラス**: PascalCase (TimelineObject, LayerObject)
- **インターフェース**: I + PascalCase (IMetaDrawable, IEditCommand)
- **メソッド/プロパティ**: PascalCase (DrawExpresser, StartFrame)
- **プライベートフィールド**: _camelCase (_timeline, _frame_per_DIP)
- **パラメータ/ローカル変数**: camelCase (frame, targetObject)

### MVVM実装
- ViewModelはプロパティ変更にReactiveUIのRaiseAndSetIfChangedを使用
- コマンドはReactiveCommand.Createで実装
- ビューはコンパイル時バインディング検証のためx:DataTypeを使用

### EditCommandパターン
全ての編集操作は、適切なundo/redoサポートのためExecute()とUndo()メソッドを持つIEditCommandを実装する必要があります。

### リソース管理
- SKBitmapやその他の破棄可能なグラフィックスリソースには`using`文を使用
- レンダリング操作後にDrawExpresserArgsを破棄
- 大きなオブジェクトには適切な破棄パターンを実装

## テストガイドライン

### テスト構造
- テストクラスは{ClassName}Tests命名規則に従う
- テストメソッドは{Method}_{Scenario}_{Expected}パターンを使用
- 一貫してAAA（Arrange-Act-Assert）パターンに従う

### フレームワークとツール
- **NUnit 3.14.0**: 主要テストフレームワーク
- **Moq 4.20.72**: 依存関係のモッキングフレームワーク
- **coverlet.collector**: コードカバレッジ収集

### 単一テスト実行
```bash
# 特定のテストクラスを実行
dotnet test --filter "FullyQualifiedName~MetasiaObjectTests"

# 特定のテストメソッドを実行
dotnet test --filter "Name=Constructor_WithId_SetsIdCorrectly"
```

## 主要技術

### UIフレームワーク
- **Avalonia UI 11.2.6**: クロスプラットフォームデスクトップUIフレームワーク
- **ReactiveUI**: リアクティブプログラミング用MVVMフレームワーク
- **SkiaSharp 2.88.8**: 動画処理用2Dグラフィックスレンダリング

### 追加ライブラリ
- **Jint 4.1.0**: プロパティアニメーション用JavaScriptエンジン
- **libsoundio-sharp**: オーディオI/O処理（カスタムDLL）
- **Microsoft.Extensions.DependencyInjection**: 依存関係注入コンテナ

## ファイル形式とシリアライゼーション

### プロジェクトファイル
- **.mtpj**: カスタムコンバーターを使用するJSONベースのプロジェクトファイル
- **.mttl**: タイムラインファイル

### 開発環境
- ターゲットフレームワーク: .NET 8.0
- nullable参照型が有効
- オーディオ処理用のunsafeブロックが許可
- クロスプラットフォームサポート（Windows、macOS、Linux）