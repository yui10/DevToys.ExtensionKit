# 実装上の注意点

GUIを作成する際は、DevToys APIを使用して開発を行います。
実装する上で、DevToys API固有の書式があるため、他ツールでの実装を参考にしてから実装を行ってください。
不明な点があれば、推測で実装せず、必ずDevToysのドキュメントを参照を行ってください。
それでも不明な点があれば、ユーザーに質問を行い、実装を進めてください。

# プロジェクト概要

DevToys.ExtensionKit は、DevToys アプリケーションの拡張機能を提供するライブラリです。

## 技術スタック

- **言語**: C# (.NET 8)
- **テストフレームワーク**: xUnit
- **GUI**: DevToys API（Blazor Hybrid技術使用、WebView2/WebKit）
- **UI レンダリング**: 
  - Windows: Microsoft Edge WebView2（Chromium ベース）
  - macOS/Linux: WebKit
- **依存関係**: DevToys.Api (2.0.8-preview), CommunityToolkit.Diagnostics, Microsoft.Extensions.Logging

## 重要なリンク
- [DevToys API 入門](https://devtoys.app/doc/articles/introduction.html)
- [DevToys API リファレンス](https://devtoys.app/doc/api/DevToys.Api.html)
- [GUI ツール作成ガイド](https://devtoys.app/doc/articles/extension-development/guidelines/UX/create-a-tool-with-a-gui.html)
- [UI コンポーネント一覧](https://devtoys.app/doc/articles/extension-development/guidelines/UX/)
- [設定使用ガイドライン](https://devtoys.app/doc/articles/extension-development/guidelines/use-settings.html)
- [Do & Don't](https://devtoys.app/doc/articles/extension-development/guidelines/do-and-dont.html)

## ディレクトリ構成
```
DevToys.ExtensionKit/
├── DevToys.ExtensionKit/
│   ├── Helpers/機能カテゴリ/       # 機能ロジック（命名: 機能名Helper）
│   ├── Models/機能カテゴリ/        # データ型・モデル
│   ├── Tools/機能カテゴリ/機能名/  # ツール実装
│   │   ├── 機能名GuiTool.cs
│   │   ├── 機能名CommandLineTool.cs
│   │   ├── 機能名.resx             # リソースファイル
│   │   └── 機能名.Designer.cs      # リソースDesignerファイル
│   └── Properties/
│       ├── GlobalUsings.cs        # グローバルusing文
│       └── AssemblyInfo.cs
│
└── DevToys.ExtensionKit.Tests/    # テストプロジェクト
    ├── TestData/機能カテゴリ/      # テストデータ
    ├── Helpers/機能カテゴリ/       # ヘルパーテスト
    ├── Models/機能カテゴリ/        # モデルテスト
    ├── Tools/機能カテゴリ/         # ツールテスト
    └── TestData/                  # 共通テストデータ
```

### ディレクトリ構成例

IPAddress関連:
- `DevToys.ExtensionKit/Tools/Converters/IPAddress/IPAddressGuiTool.cs`
- `DevToys.ExtensionKit/Helpers/Converters/IPAddressHelper.cs`
- `DevToys.ExtensionKit.Tests/Helpers/Converters/IPAddressHelperTests.cs`

UUID関連:
- `DevToys.ExtensionKit/Tools/Generators/UUID/UUIDGeneratorGuiTool.cs`
- `DevToys.ExtensionKit/Helpers/Generators/UuidHelper.cs`
- `DevToys.ExtensionKit/Models/Generators/UuidVersion.cs`

# DevToys API 実装パターン

## GUIツール（必須）

### 基本実装パターン
```csharp
[Export(typeof(IGuiTool))]
[Name("ToolName")] // 一意の内部名（必須）
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons", // アイコンフォント名
    IconGlyph = '\uXXXX', // アイコンのグリフ文字（必須）
    GroupName = PredefinedCommonToolGroupNames.Converters, // ツールグループ
    ResourceManagerAssemblyIdentifier = nameof(DevToysExtensionKitResourceManagerAssemblyIdentifier), // リソースアセンブリ識別子（必須）
    ResourceManagerBaseName = "リソースファイルのベース名", // .resxファイルの完全名（必須）
    ShortDisplayTitleResourceName = nameof(ResourceClass.ShortDisplayTitle), // ナビゲーションバー表示名（必須）
    LongDisplayTitleResourceName = nameof(ResourceClass.LongDisplayTitle), // 検索結果・ツール上部表示名
    DescriptionResourceName = nameof(ResourceClass.Description), // ツール説明文
    AccessibleNameResourceName = nameof(ResourceClass.AccessibleName), // スクリーンリーダー用説明名
    SearchKeywordsResourceName = nameof(ResourceClass.SearchKeywords))] // 検索キーワード（空白区切り）
[AcceptedDataTypeName("DataTypeName")] // Smart Detection対応データ型指定（複数指定可能）
internal sealed partial class ToolNameGuiTool : IGuiTool
{
    [ImportingConstructor]
    public ToolNameGuiTool(ISettingsProvider settingsProvider) { }
    
    public UIToolView View => new(isScrollable: true, /* UI要素 */);
    
    /// <summary>
    /// Smart Detection機能でデータを受信した際の処理
    /// </summary>
    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}
```

### UI要素パターン
- **グリッドレイアウト**: 
`Grid().Rows().Columns().Cells()`
- **入力フィールド**: 
  - `SingleLineTextInput()`: 単一行テキスト入力
  - `MultiLineTextInput()`: 複数行テキスト入力（構文ハイライト、自動補完対応）
  - `NumberInput()`: 数値入力
  - `PasswordInput()`: パスワード入力
  - `SelectDropDownList()`: ドロップダウンリスト選択
- **出力・表示要素**:
  - `.ReadOnly()`: 読み取り専用フィールド
  - `Label()`: テキスト表示
  - `Icon()`: アイコン表示
  - `ImageViewer()`: 画像表示（BMP, GIF, JPEG, PNG, SVG等対応）
  - `DataGrid()`: データグリッド表示
  - `DiffTextInput()`: テキスト差分表示
- **レイアウト要素**: 
  - `Card()`: セクション分離
  - `Stack()`: 水平・垂直スタック
  - `SplitGrid()`: 分割可能なペイン
  - `Wrap()`: 折り返しレイアウト
- **インタラクション要素**:
  - `Button()`: ボタン
  - `DropDownButton()`: ドロップダウンボタン
  - `Switch()`: スイッチ（トグル）
  - `FileSelector()`: ファイル選択・ドロップゾーン
- **フィードバック要素**:
  - `InfoBar()`: 情報・エラー表示（`.Error()`, `.Warning()`, `.Success()`）
  - `ProgressBar()`: プログレスバー
  - `ProgressRing()`: プログレスリング
- **設定要素**:
  - `Setting()`: 設定項目
  - `SettingGroup()`: 設定グループ
- **その他**:
  - `WebView()`: Webビュー
  - `Dialog()`: モーダルダイアログ


## UI設計原則

### DevToys UI設計理念
- **一貫性**: DevToys全体で統一されたデザインを維持
- **シンプルさ**: Webの知識不要で視覚的に魅力的なUIを作成
- **アクセシビリティ**: スクリーンリーダーなど支援技術への対応
- **クロスプラットフォーム**: Windows、macOS、Linuxで一貫した動作

### UI設計方針
- **事前構築済みコンポーネント**: DevToys APIの事前構築済みUI要素を活用
- **最小限のカスタマイズ**: 一貫したデザインを保つため、カスタマイズオプションは最小限
- **レスポンシブ対応**: 異なる画面サイズやCompact Overlayモードに対応

## UI実装のベストプラクティス

### レイアウト構造
1. **メインレイアウト**: `Grid()` を使用して全体構造を定義
2. **セクション分離**: `Card()` を使用して入力、出力、設定エリアを分離
3. **グリッド構造**: enum を使用して行と列を定義（例: `GridRow`, `GridColumn`）

### UI要素の命名規則
- プライベートフィールドは readonly で定義
- UI要素のIDは kebab-case を使用（例: `"ip-address"`, `"subnet-mask"`）

### エラーハンドリング
- `InfoBar().Error()` を使用してエラーメッセージを表示
- 正常時は `.Close()` で非表示、エラー時は `.Open()` で表示

## 設定（Settings）の使用

### Settings Provider
`ISettingsProvider` は設定の読み書きを行うMEFサービスです。`SettingDefinition<T>` を使用して設定を定義し、MEFの `[Import]` 属性でクラスに注入できます。

#### 基本的な設定使用パターン
```csharp
[Export(typeof(IGuiTool))]
[Name("MyTool")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uE670',
    ResourceManagerAssemblyIdentifier = nameof(DevToysExtensionKitResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "MyProject.MyTool",
    ShortDisplayTitleResourceName = nameof(MyTool.ShortDisplayTitle),
    DescriptionResourceName = nameof(MyTool.Description),
    GroupName = PredefinedCommonToolGroupNames.Converters)]
internal sealed class MyGuiTool : IGuiTool
{
    // 設定の定義
    private static readonly SettingDefinition<bool> _enableAutoProcessing
        = new(
            name: $"{nameof(MyGuiTool)}.{nameof(_enableAutoProcessing)}", // 一意の名前
            defaultValue: true); // デフォルト値

    private static readonly SettingDefinition<string> _outputFormat
        = new(
            name: $"{nameof(MyGuiTool)}.{nameof(_outputFormat)}",
            defaultValue: "json");

    [Import] // Settings Providerの注入
    private ISettingsProvider _settingsProvider = null!;

    [ImportingConstructor]
    public MyGuiTool() { }

    public UIToolView View
        => new(isScrollable: true,
            Grid()
                .Rows(
                    GridRow.Auto,
                    GridRow.Auto,
                    GridRow.Fill)
                .Columns(
                    GridColumn.Fill)
                .Cells(
                    // 設定UI
                    Card("設定")
                        .Row(0)
                        .Column(0)
                        .Children(
                            Setting("enableAutoProcessing")
                                .Title("自動処理を有効にする")
                                .Description("入力時に自動的に処理を実行します")
                                .Handle(
                                    _settingsProvider,
                                    _enableAutoProcessing),
                            
                            Setting("outputFormat")
                                .Title("出力形式")
                                .Description("結果の出力形式を選択してください")
                                .Handle(
                                    _settingsProvider,
                                    _outputFormat,
                                    Item("json", "JSON"),
                                    Item("xml", "XML"),
                                    Item("csv", "CSV")),

                    // その他のUI要素
                    // ...
                ));

    public void OnDataReceived(string dataTypeName, object? parsedData) { }

    // 設定値の取得・設定
    private void OnProcessButtonClick()
    {
        bool autoProcessing = _settingsProvider.GetSetting(_enableAutoProcessing);
        string format = _settingsProvider.GetSetting(_outputFormat);

        // 設定値を使用した処理
        ProcessData(autoProcessing, format);
    }

    private void UpdateSettings()
    {
        // 設定値の変更
        _settingsProvider.SetSetting(_enableAutoProcessing, false);
        _settingsProvider.SetSetting(_outputFormat, "xml");
    }
}
```

### サポートされるデータ型
`SettingDefinition<T>` は以下のデータ型をサポートしています：

#### 基本型
- `bool`: true/false値
- `string`: 文字列値
- `int`: 整数値
- `double`: 浮動小数点数
- `DateTimeOffset`: 日時値
- `Enum`: 列挙型

#### 配列型
- 上記の基本型の `Array`（例：`int[]`, `string[]`）

#### 複雑なオブジェクト
複雑なオブジェクトの場合は、シリアライゼーション関数を提供できます：

```csharp
private static readonly SettingDefinition<MyCustomObject> _customSetting
    = new(
        name: $"{nameof(MyGuiTool)}.{nameof(_customSetting)}",
        defaultValue: new MyCustomObject(),
        serialize: obj => JsonSerializer.Serialize(obj),
        deserialize: json => JsonSerializer.Deserialize<MyCustomObject>(json) ?? new MyCustomObject());
```

### 設定の永続化場所

#### DevToys GUI
- **Windows**: `%LocalAppData%/DevToys/settings.ini`
- **macOS**: ユーザーのローカル設定ディレクトリ
- **Linux**: ユーザーのローカル設定ディレクトリ

#### DevToys CLI
- DevToys CLIがインストールされているフォルダの `Cache/settings.ini`
- Program Filesなど管理者権限が必要なフォルダの場合は、管理者権限で実行する必要があります

### Do & Don't

#### 推奨事項
- `IUISetting` と `IUISettingGroup` を使用する場合は設定機能を活用する
- 設定名は一意になるよう、クラス名をプレフィックスとして使用する
- 適切なデフォルト値を設定する

#### 避けるべき事項
- ユーザーのテキスト入力（`IUISingleLineTextInput`、`IUIMultiLineTextInput`、`IUIPasswordInput`など）に設定を使用しない
  - 設定はコンピューター上にクリアテキストで保存されるため、機密情報が含まれる可能性があります
- `Serialize` を使用して手動でシリアライゼーションする際、複数行にわたる設定を永続化しない


## コマンドラインツール（オプション）

### コマンドラインツール実装の基本パターン
```csharp
[Export(typeof(ICommandLineTool))]
[Name("ToolName")] // 一意の内部名（必須）
[CommandName(
    Name = "tool-name", // CLIコマンド名（必須）
    Alias = "tn", // コマンドエイリアス（短縮形）
    ResourceManagerBaseName = "リソースファイルのベース名", // .resxファイルの完全名（必須）
    DescriptionResourceName = nameof(ResourceClass.Description))] // --helpで表示される説明文（必須）
internal sealed class ToolNameCommandLineTool : ICommandLineTool
{
    // パラメータ定義例
    [CommandLineOption(
        Name = "input", 
        Alias = "i", 
        IsRequired = true, // 必須パラメータ指定
        DescriptionResourceName = nameof(ResourceClass.InputOptionDescription))]
    internal string Input { get; set; }
    
    [CommandLineOption(
        Name = "output", 
        Alias = "o", 
        IsRequired = false, // オプションパラメータ
        DescriptionResourceName = nameof(ResourceClass.OutputOptionDescription))]
    internal string Output { get; set; } = "default-value"; // デフォルト値設定
    
    // 複数型対応パラメータ（ファイルパスまたは文字列）
    [CommandLineOption(
        Name = "data", 
        Alias = "d", 
        DescriptionResourceName = nameof(ResourceClass.DataOptionDescription))]
    internal OneOf<FileInfo, string> Data { get; set; }
    
    // 配列パラメータ
    [CommandLineOption(
        Name = "numbers", 
        Alias = "n", 
        DescriptionResourceName = nameof(ResourceClass.NumbersOptionDescription))]
    internal int[] Numbers { get; set; }
    
    // Enum パラメータ
    [CommandLineOption(
        Name = "format", 
        Alias = "f", 
        DescriptionResourceName = nameof(ResourceClass.FormatOptionDescription))]
    internal OutputFormat Format { get; set; }
    
    // サービス注入（オプション）
    [Import]
    private IFileStorage _fileStorage = null!;
    
    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            // プログレスバー表示例
            using var progressBar = new ConsoleProgressBar();
            progressBar.Report(0);
            
            // OneOf型の処理例（詳細なエラーハンドリング付き）
            var result = await Data.Match(
                async file => await ProcessFileAsync(file, progressBar, cancellationToken),
                async text => await ProcessTextAsync(text, progressBar, cancellationToken)
            );
            
            progressBar.Report(100);
            
            // 結果出力
            Console.WriteLine(result);
            
            return 0; // 成功時は0を返す
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("処理がキャンセルされました");
            return 1; // キャンセル時は1を返す
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "処理中にエラーが発生しました");
            return -1; // エラー時は-1を返す
        }
    }
    
    private async Task<string> ProcessFileAsync(FileInfo file, ConsoleProgressBar progressBar, CancellationToken cancellationToken)
    {
        // ファイル存在確認とバリデーション
        if (!file.Exists)
        {
            throw new FileNotFoundException($"ファイルが見つかりません: {file.FullName}");
        }
        
        progressBar.Report(25);
        
        // ファイル処理ロジック（例：ファイル読み込み）
        using var stream = file.OpenRead();
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync(cancellationToken);
        
        progressBar.Report(75);
        
        // 処理結果を返す
        return $"ファイル処理完了: {file.Name} ({content.Length} 文字)";
    }
    
    private async Task<string> ProcessTextAsync(string text, ConsoleProgressBar progressBar, CancellationToken cancellationToken)
    {
        progressBar.Report(50);
        
        // テキスト処理ロジック（例：文字列変換）
        var processedText = text.ToUpperInvariant();
        
        // キャンセレーション対応の非同期処理
        await Task.Delay(100, cancellationToken);
        
        return $"テキスト処理完了: {processedText}";
    }
}

// Enumの定義例
internal enum OutputFormat
{
    Json,
    Xml,
    Csv
}
```

### サポートされるパラメータ型
- **基本型**: `bool`, `string`, `int`, `double`, `decimal`, `Guid`
- **日時型**: `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`
- **数値型**: `Int16`, `Int32`, `Int64`, `UInt16`, `UInt32`, `UInt64`, `Byte`, `SByte`, `Single`
- **ファイル型**: `FileInfo`, `DirectoryInfo`, `FileSystemInfo`
- **その他**: `Enum`, 上記型の`Array`
- **複合型**: `OneOf<T1, T2>` - 複数型を受け入れるパラメータ

### コマンド例
```bash
# 基本的な実行
DevToys.CLI.exe tool-name --input "hello world" --output result.txt

# エイリアス使用
DevToys.CLI.exe tn -i "hello world" -o result.txt

# 配列パラメータ
DevToys.CLI.exe tool-name --numbers 1 2 3 4 5

# Enumパラメータ
DevToys.CLI.exe tool-name --format Json

# ファイルまたは文字列
DevToys.CLI.exe tool-name --data "C:\path\to\file.txt"
DevToys.CLI.exe tool-name --data "direct text input"

# ヘルプ表示
DevToys.CLI.exe tool-name --help
```

### プログレス表示
```csharp
using var progressBar = new ConsoleProgressBar();
progressBar.Report(0);   // 0%開始

// 段階的な進捗更新
await SomeWorkAsync();
progressBar.Report(20);  // 20%完了

await MoreWorkAsync();
progressBar.Report(50);  // 50%完了

await FinalWorkAsync();
progressBar.Report(100); // 100%完了

// using文でプログレスバーが自動的に非表示になる
// Console出力: [########--] 80% のような表示
```

### OneOf型の詳細な使用方法
```csharp
// OneOf型ヘルパーメソッドの使用例
string content = await Input.Match(
    async file =>
    {
        // ファイルパスが指定された場合
        if (!file.Exists)
        {
            throw new FileNotFoundException($"ファイルが見つかりません: {file.FullName}");
        }
        return await File.ReadAllTextAsync(file.FullName, cancellationToken);
    },
    async text =>
    {
        // 直接テキストが指定された場合
        await Task.CompletedTask; // 非同期処理の統一のため
        return text;
    }
);

// OneOfExtensions の便利メソッド（DevToys.Api提供）
bool isFile = Input.IsT0;        // FileInfo型かどうか
bool isString = Input.IsT1;      // string型かどうか
FileInfo? file = Input.AsT0;     // FileInfo型として取得（型安全でない場合は例外）
string? text = Input.AsT1OrDefault(); // string型として取得（失敗時はnull）
```

### CLI制限
- `IClipboard`, `IThemeListener`, `IFontProvider` は利用不可
- ファイル選択ダイアログは利用不可（パラメータで直接指定）

# コーディングガイドライン

## 基本方針
- **コメント**: 必要に応じて記述
- **命名規則**: [Microsoft の命名規則](https://learn.microsoft.com/ja-jp/dotnet/csharp/fundamentals/coding-style/coding-conventions)に従う
- **国際化対応**: UIに表示される文字列は、リソースファイルに定義し、デフォルト言語の英語(en-US)と日本語(ja-JP)の両方を用意

## 機能追加の手順

1. **ヘルパークラスを実装する**
   - `DevToys.ExtensionKit/Helpers/機能カテゴリ/` ディレクトリに配置
   - 命名規則: `機能名Helper` (例: `UuidHelper.cs`)

2. **ヘルパークラスのテストコードを実装する**
   - `DevToys.ExtensionKit.Tests/Helpers/機能カテゴリ/` ディレクトリに配置
   - 命名規則: `機能名HelperTests` (例: `UuidHelperTests.cs`)

3. **モデルクラスを実装する**
   - モデルクラスは、必要な場合のみ実装
   - `DevToys.ExtensionKit/Models/機能カテゴリ/` ディレクトリに配置
   - モデルクラスのテストコードは、必要な場合のみ実装

4. **ツールのリソースファイル（ToolDisplayInformationで使用する文字列）を作成する**
   - ツールディレクトリ内に `.resx` ファイルを配置
   - 対応する `.Designer.cs` ファイルも生成される

5. **ツールの UI を実装する**
   - GUIツールクラスを実装
   - DevToys API実装パターンに従う

6. **ツールのリソースファイルを使用して、UI の文字列を定義する**
   - リソースファイルからの文字列参照を設定

7. **ツールのコマンドライン実装を作成する（コマンドラインツールが必要であると指示された場合）**
   - コマンドラインツールクラスを実装

8. **ツールのテストコードを実装する**
   - `DevToys.ExtensionKit.Tests/Tools/機能カテゴリ/` ディレクトリに配置
   - GUIツールとコマンドラインツールのテストを作成

9. **ツールを追加した場合は、README.md にツールの説明を追加する**
   - プロジェクトのドキュメントを更新

## 重要なファイル

- **DevToysExtensionKitResourceManagerAssemblyIdentifier.cs**: リソースファイルへのアクセスを提供するアセンブリ識別子
- **Properties/GlobalUsings.cs**: プロジェクト全体で使用されるグローバルなusing文の定義
- **Properties/AssemblyInfo.cs**: アセンブリメタデータとテストアセンブリへの内部アクセス許可

### グローバルUsing設定
```csharp
global using CommunityToolkit.Diagnostics;
global using System;
global using System.Collections.Generic;
global using System.ComponentModel.Composition;
global using System.Diagnostics;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using DevToys.Api;
global using ExportAttribute = System.ComponentModel.Composition.ExportAttribute;
global using static DevToys.Api.GUI;
```

## ログ設定
- 各ツールクラスでは `ILogger` を使用してログを出力する
- ログは `this.Log()` 拡張メソッドを使用して取得する

### ログ出力ガイドライン
- **報告すべき内容**:
  - エラー情報
  - パフォーマンスに関する情報（関連する場合）
  - システム情報（パフォーマンスや互換性調査に有用な場合のみ）
- **報告してはいけない内容**:
  - ユーザーの入力データ（個人情報保護）

## 国際化（i18n）対応ガイドライン

### 必須言語
- **英語**（デフォルト）
- **日本語**（必須）

### 文字列のローカライゼーション指針
```csharp
// 良い例: 文化に依存しない形式
"Enter your text here"          // 英語
"ここにテキストを入力してください"      // 日本語

// 悪い例: 文化固有の表現を避ける
"Enter your text here, buddy!"  // カジュアルすぎ
"Please kindly enter text"      // 過度に丁寧

// パラメータを含む文字列
"Processing {0} of {1} items"   // 英語
"{1} 項目中 {0} 項目を処理中"    // 日本語（語順に注意）

// 複数形対応が必要な場合
"{0} item"      // 1つの場合
"{0} items"     // 複数の場合（英語）
"{0} 個のアイテム" // 日本語は複数形なし
```

### 数値・日付のローカライゼーション
```csharp
// 数値フォーマット（カルチャ対応）
decimal value = 1234.56m;
string formatted = value.ToString("N2", CultureInfo.CurrentCulture);

// 日付フォーマット（カルチャ対応）
DateTime date = DateTime.Now;
string dateFormatted = date.ToString("d", CultureInfo.CurrentCulture);

// ファイルサイズ表示（バイト単位）
long bytes = 1048576;
string sizeFormatted = $"{bytes / 1024.0 / 1024.0:F1} MB";  // "1.0 MB"
```

## パフォーマンス・メモリ効率ガイドライン

### 大容量データ対応（数百MB想定）
- **大容量データ対応**: ユーザーが大きなテキストファイル（数百MB）を扱う可能性を想定
- **ファイル読み込み**: 大容量ファイルは全体をメモリに読み込まず、ストリームで処理
- **バッファサイズ**: 適切なバッファサイズ（例：8192バイト）を使用
- **効率的な文字列処理**: `Memory<T>`/`Span<T>` を使用、`String.Split()`/`String.Substring()` などのメモリ割り当てを伴う操作は避ける
- **非同期処理**: ファイル読み込みや長時間処理は非同期で行い、UIの応答性を保つ

### コード例
```csharp
// 推奨: Memory<T>とSpan<T>を使用
public static void ProcessTextEfficiently(ReadOnlyMemory<char> input)
{
    ReadOnlySpan<char> span = input.Span;
    
    // 文字列の分割処理（メモリ割り当てなし）
    while (!span.IsEmpty)
    {
        int lineEnd = span.IndexOf('\n');
        if (lineEnd == -1) lineEnd = span.Length;
        
        ReadOnlySpan<char> line = span.Slice(0, lineEnd);
        ProcessLine(line); // Spanを直接処理
        
        span = span.Slice(Math.Min(lineEnd + 1, span.Length));
    }
}

// 避けるべきコード: String.Split()やString.Substring()
public static void ProcessTextInefficiently(string input)
{
    string[] lines = input.Split('\n'); // 大量のメモリ割り当て
    foreach (string line in lines)
    {
        ProcessLine(line); // さらなる文字列コピー
    }
}

// 大容量ファイル読み込み
public static async Task ProcessLargeFileAsync(FileInfo file, CancellationToken cancellationToken)
{
    using var stream = file.OpenRead();
    using var reader = new StreamReader(stream, bufferSize: 8192);
    
    char[] buffer = new char[4096];
    var memory = new Memory<char>(buffer);
    
    int charsRead;
    while ((charsRead = await reader.ReadAsync(memory, cancellationToken)) > 0)
    {
        // 読み込んだデータを直接処理（コピー不要）
        ProcessChunk(memory.Slice(0, charsRead).Span);
    }
}

```

## エラーハンドリングのベストプラクティス
```csharp
public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
{
    try
    {
        // パラメータバリデーション
        if (string.IsNullOrWhiteSpace(Input))
        {
            logger.LogError("入力が空です");
            Console.WriteLine("エラー: 入力パラメータが必要です");
            return -1;
        }

        // 処理実行
        var result = await ProcessAsync(cancellationToken);
        
        // 成功出力
        Console.WriteLine($"処理完了: {result}");
        return 0;
    }
    catch (OperationCanceledException)
    {
        logger.LogInformation("ユーザーによって処理がキャンセルされました");
        Console.WriteLine("処理がキャンセルされました");
        return 1; // キャンセル時の終了コード
    }
    catch (FileNotFoundException ex)
    {
        logger.LogError(ex, "ファイルが見つかりません: {FilePath}", ex.FileName);
        Console.WriteLine($"エラー: ファイルが見つかりません - {ex.FileName}");
        return -1;
    }
    catch (ArgumentException ex)
    {
        logger.LogError(ex, "パラメータエラー: {Message}", ex.Message);
        Console.WriteLine($"エラー: 無効なパラメータ - {ex.Message}");
        return -1;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "予期しないエラーが発生しました");
        Console.WriteLine($"エラー: 処理中に問題が発生しました - {ex.Message}");
        return -1;
    }
}
```

# Smart Detection機能

## 概要
Smart Detection は、DevToys が外部ソース（クリップボードなど）からのデータタイプを自動検出し、適切なツールを推奨する機能です。

## 実装方法
1. **データタイプ検出器の実装**: `IDataTypeDetector` を実装してデータタイプを検出
2. **ツールでの受信設定**: `[AcceptedDataTypeName("DataTypeName")]` 属性でデータタイプを指定
3. **データ受信処理**: `OnDataReceived(string dataTypeName, object? parsedData)` メソッドで処理

## 事前定義されたデータタイプ
`PredefinedCommonDataTypeNames` クラスには、よく使用されるデータタイプが定義されている：
- テキスト形式（JSON、XML、Base64など）
- ネットワーク関連（URL、IPアドレスなど）
- 画像形式（PNG、JPEG、SVGなど）
- その他の一般的な形式

# ファイル操作
```csharp
[Import]
private IFileStorage _fileStorage = null!;

// 一時ファイル作成（自動削除される）
using var tempFile = await _fileStorage.CreateTempFileAsync("prefix", ".json");
await File.WriteAllTextAsync(tempFile.FullName, jsonContent, cancellationToken);

// アプリキャッシュディレクトリの取得
var cacheDir = _fileStorage.AppCacheDirectory;
var cacheFile = Path.Combine(cacheDir.FullName, "data.cache");

// ファイル選択プロンプト（GUIツールのみ、CLIでは利用不可）
// var file = await _fileStorage.PickOpenFileAsync(new[] { ".json", ".txt" });

// ファイル保存プロンプト（GUIツールのみ、CLIでは利用不可）
// var saveFile = await _fileStorage.PickSaveFileAsync("output", ".json");
```
