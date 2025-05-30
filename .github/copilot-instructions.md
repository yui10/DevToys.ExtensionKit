# ディレクトリ構成

./
├── DevToys.ExtensionKit.sln
├── LICENSE
├── README.md
├── DevToys.ExtensionKit
│ ├── Properties
│ │ └── GlobalUsings.cs : "グローバルな using ディレクティブが定義されています"
│ ├── Helpers : "機能ごとのロジックを記述したヘルパークラスが格納されています"
│ ├── Models : "アプリケーションで使用されるデータ型やモデルクラスが格納されています"
│ ├── Tools : "ツールの UI やリソースが格納されています"
├── DevToys.ExtensionKit.Tests
│ ├── TestData
│ │ └── "テスト用のデータが格納されています"
│ ├── Properties
│ │ └── GlobalUsings.cs : "テストプロジェクトで使用されるグローバルな using ディレクティブが定義されています"
│ ├── Helpers : "ヘルパークラスへのテストコードが格納されています"
│ ├── Models : "モデルクラスへのテストコードが格納されています.但し、ロジックがある場合のみ格納されるため、空の場合があります"
│ ├── Tools : "ツールの UI やリソースへのテストコードが格納されています"

## ディレクトリ構成の詳細

- `DevToys.ExtensionKit/Helpers`, `DevToys.ExtensionKit/Models`, `DevToys.ExtensionKit/Tools`
  - 各ディレクトリは機能ごとに分類されたサブディレクトリを持ち、関連するロジック、データ型、ツールのコードが格納されています。
  - 命名規則:
    - Helpers: 機能名 + `Helper` (例: `Converters/IPAddressHelper`, `Generators/PasswordGeneratorHelper`)
    - Models: 機能名 + `Model` (例: `Converters/DateFormat.cs`)
    - Tools: 機能名 + `GuiTool` または `CommandLineTool` (例: `Converters/IPAddress/IPAddressGuiTool.cs`, `Generators/UUID/UUIDGeneratorCommandLineTool.cs`)

### サンプル

- IPAddress 関連のツール

  - `Converters/IPAddress/IPAddressGuiTool.cs`
  - `Converters/IPAddress/IPAddressCommandLineTool.cs`
  - `Converters/IPAddress/IPAddressParser.Designer.cs`
  - `Converters/IPAddress/IPAddressParser.resx`
  - `Helpers/Converters/IPAddressHelper.cs`

- UUID 関連のツール

  - `Generators/UUID/UUIDGeneratorGuiTool.cs`
  - `Generators/UUID/UUIDGeneratorCommandLineTool.cs`
  - `Generators/UUID/UUIDGenerator.Designer.cs`
  - `Generators/UUID/UUIDGenerator.resx`
  - `Helpers/Generators/UuidHelper.cs`
  - `Models/Generators/UuidVersion.cs`

- IPAddress 関連のテスト

  - `DevToys.ExtensionKit.Tests/Helpers/Converters/IPAddressHelperTests.cs`
  - `DevToys.ExtensionKit.Tests/Tools/Converters/IPAddressGuiToolTests.cs`

# プロジェクトの概要

- DevToys.ExtensionKit は、DevToys アプリケーションの拡張機能を提供するためのライブラリです。
- このプロジェクトは、開発者が求める便利機能を拡張追加する、さまざまなユーティリティを含んでいます。

# 技術スタック

- **言語**: C#
- **フレームワーク**: .NET 8
- **テストフレームワーク**: xUnit
- **GUI**: DevToys API (https://devtoys.app/doc/articles/introduction.html , https://devtoys.app/doc/api/DevToys.Api.html)

# コーディング規則

- コメントは必要に応じて可能な限り記述すること
- 命名規則は[Microsoft の命名規則](https://learn.microsoft.com/ja-jp/dotnet/csharp/fundamentals/coding-style/coding-conventions)に従うこと
- UI に表示される文字列は、リソースファイルに定義し、必ず最低でも英語と日本語の両方を用意すること
- ツールの UI は、DevToys API を使用して実装すること
- ツールのコマンドライン実装は、DevToys API を使用して実装すること
  - https://devtoys.app/doc/articles/introduction.html
  - https://devtoys.app/doc/api/DevToys.Api.html
- 機能を追加する場合は、以下の手順に従うこと
  1. ヘルパークラスを実装する
  2. ヘルパークラスのテストコードを実装する
  3. モデルクラスを実装する
    - モデルクラスは、必要な場合のみ実装すること
    - モデルクラスのテストコードは、必要な場合のみ実装すること
  5. ツールのリソースファイル(ToolDisplayInformationで使用する文字列)を作成する
  6. ツールの UI を実装する
  7. ツールのリソースファイルを使用して、UI の文字列を定義する
  8. ツールのコマンドライン実装を作成する(コマンドラインツールが必要であると指示された場合)
  9. ツールのテストコードを実装する
  10. ツールを追加した場合は、README.md にツールの説明を追加する