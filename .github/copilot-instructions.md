# ディレクトリ構成

./
├── DevToys.ExtensionKit.sln
├── LICENSE
├── README.md
├── DevToys.ExtensionKit
│ ├── Properties
│ │ └── GlobalUsings.cs : "グローバルな using ディレクティブが定義されている"
│ ├── Helpers : "機能ごとのロジックを記述したヘルパークラスが格納されている"
│ ├── Models : "アプリケーションで使用されるデータ型やモデルクラスが格納されている"
│ ├── Tools : "ツールの UI やリソースが格納されている"
├── DevToys.ExtensionKit.Tests
│ ├── TestData
│ │ └── "テスト用のデータが格納されている"
│ ├── Properties
│ │ └── GlobalUsings.cs : "テストプロジェクトで使用されるグローバルな using ディレクティブが定義されている"
│ ├── Helpers : "ヘルパークラスへのテストコードが格納されている"
│ ├── Models : "モデルクラスへのテストコードが格納されている。ただし、ロジックがある場合のみ格納されるため、空の場合がある"
│ ├── Tools : "ツールの UI やリソースへのテストコードが格納されている"

## ディレクトリ構成の詳細

- `DevToys.ExtensionKit/Helpers`, `DevToys.ExtensionKit/Models`, `DevToys.ExtensionKit/Tools`
  - 各ディレクトリは機能ごとに分類されたサブディレクトリを持ち、関連するロジック、データ型、ツールのコードが格納されている。
  - 命名規則:
    - Helpers: 機能名 + `Helper` (例: `Converters/IPAddressHelper`, `Generators/PasswordGeneratorHelper`)
    - Models: 機能名 + `Model` (例: `Converters/DateFormat.cs`)
    - Tools: 機能名 + `GuiTool` または `CommandLineTool` (例: `Converters/IPAddress/IPAddressGuiTool.cs`, `Generators/UUID/UUIDGeneratorCommandLineTool.cs`)

### サンプル

- IPAddress 関連のツール

  - `Tools/Converters/IPAddress/IPAddressGuiTool.cs`
  - `Tools/Converters/IPAddress/IPAddressCommandLineTool.cs`
  - `Tools/Converters/IPAddress/IPAddressParser.Designer.cs`
  - `Tools/Converters/IPAddress/IPAddressParser.resx`
  - `Helpers/Converters/IPAddressHelper.cs`

- UUID 関連のツール

  - `Tools/Generators/UUID/UUIDGeneratorGuiTool.cs`
  - `Tools/Generators/UUID/UUIDGeneratorCommandLineTool.cs`
  - `Tools/Generators/UUID/UUIDGenerator.Designer.cs`
  - `Tools/Generators/UUID/UUIDGenerator.resx`
  - `Helpers/Generators/UuidHelper.cs`
  - `Models/Generators/UuidVersion.cs`

- IPAddress 関連のテスト

  - `DevToys.ExtensionKit.Tests/Helpers/Converters/IPAddressHelperTests.cs`
  - `DevToys.ExtensionKit.Tests/Tools/Converters/IPAddressGuiToolTests.cs`

# プロジェクトの概要

- DevToys.ExtensionKit は、DevToys アプリケーションの拡張機能を提供するためのライブラリである。
- このプロジェクトは、開発者が求める便利機能を拡張として追加する、さまざまなユーティリティを含んでいる。

# 技術スタック

- **言語**: C#
- **フレームワーク**: .NET 8
- **テストフレームワーク**: xUnit
- **GUI**: DevToys API（Blazor Hybrid技術を使用したWebベースUI）
- **UI レンダリング**: 
  - Windows: Microsoft Edge WebView2（Chromium ベース、OSに組み込み済み）
  - macOS/Linux: WebKit（OSに組み込み済み）
  - 特徴: Electronアプリのようなバンドル不要、OSの組み込みエンジンを利用
- **依存関係**: 
  - DevToys.Api (version 2.0.8-preview)
  - CommunityToolkit.Diagnostics
  - Microsoft.Extensions.Logging
- **参考資料**:
  - [DevToys API 入門](https://devtoys.app/doc/articles/introduction.html)
  - [DevToys API リファレンス](https://devtoys.app/doc/api/DevToys.Api.html)
  - [GUI ツール作成ガイド](https://devtoys.app/doc/articles/extension-development/guidelines/UX/create-a-tool-with-a-gui.html)
  - [拡張機能開発ガイドライン Do & Don't](https://devtoys.app/doc/articles/extension-development/guidelines/do-and-dont.html)

## プロジェクト固有の設定

- **ターゲットフレームワーク**: net8.0
- **Nullable**: 有効
- **ImplicitUsings**: 有効
- **AssemblyVersion**: 1.0.0

## DevToys API 実装パターン

### GUIツールの実装パターン
```csharp
[Export(typeof(IGuiTool))]
[Name("ToolName")] // 一意の内部名（必須）- デバッグログに表示される
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons", // アイコンフォント名（FluentSystemIconsが標準で利用可能）
    IconGlyph = '\uXXXX', // アイコンのグリフ文字（必須）
    GroupName = PredefinedCommonToolGroupNames.Converters, // ツールグループ（オプション）
    ResourceManagerAssemblyIdentifier = nameof(DevToysExtensionKitResourceManagerAssemblyIdentifier), // リソースアセンブリ識別子（必須）
    ResourceManagerBaseName = "リソースファイルのベース名（名前空間.クラス名）", // .resxファイルの完全名（必須）
    ShortDisplayTitleResourceName = nameof(ResourceClass.ShortDisplayTitle), // ナビゲーションバー表示名（必須）
    LongDisplayTitleResourceName = nameof(ResourceClass.LongDisplayTitle), // 検索結果・ツール上部表示名（オプション、未設定時はShortDisplayTitleを使用）
    DescriptionResourceName = nameof(ResourceClass.Description), // ツールグリッド・グループページでの説明文（オプション）
    AccessibleNameResourceName = nameof(ResourceClass.AccessibleName), // スクリーンリーダー用説明名（オプション）
    SearchKeywordsResourceName = nameof(ResourceClass.SearchKeywords))] // 検索キーワード（空白区切り、オプション）
[TargetPlatform(Platform.Windows)] // 対応プラットフォーム指定（オプション、未指定時は全プラットフォーム対応）
[TargetPlatform(Platform.MacOS)]
[Order(Before = "OtherToolName")] // ツールの表示順序（オプション、他ツールのNameAttributeを参照）
[NoCompactOverlaySupport] // Compact Overlay（Picture-in-Picture）モード非対応指定（オプション）
[NotSearchable] // 検索対象外指定（オプション）
[NotFavorable] // お気に入り追加不可指定（オプション）
[MenuPlacement(MenuPlacement.Header)] // メニュー配置指定（オプション）
[AcceptedDataTypeName("DataTypeName")] // Smart Detection対応データ型指定（オプション、複数指定可能）
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

### コマンドラインツールの実装パターン
```csharp
[Export(typeof(ICommandLineTool))]
[Name("ToolName")] // 一意の内部名（必須）- デバッグログに表示される
[CommandName(
    Name = "tool-name", // CLIコマンド名（必須）
    Alias = "tn", // コマンドエイリアス（オプション、短縮形）
    ResourceManagerBaseName = "リソースファイルのベース名（名前空間.クラス名）", // .resxファイルの完全名（必須）
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

### 共通のUI要素パターン
- **グリッドレイアウト**: `Grid().Rows().Columns().Cells()`
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

### DevToys API の特徴
- **WebベースUI**: Blazor Hybrid技術により、C#でWebベースUIを構築
- **事前構築済みコンポーネント**: DevToys APIが提供する一貫したUI要素を活用
- **最小限のカスタマイズ**: 統一されたデザインを保持するため、カスタマイズオプションは制限
- **Webエンジンバンドル不要**: OSに組み込み済みのWebエンジンを使用（ElectronとのDifference）

## 重要なファイル

- **DevToysExtensionKitResourceManagerAssemblyIdentifier.cs**: リソースファイルへのアクセスを提供するアセンブリ識別子
- **Properties/GlobalUsings.cs**: プロジェクト全体で使用されるグローバルなusing文の定義
- **Properties/AssemblyInfo.cs**: アセンブリメタデータとテストアセンブリへの内部アクセス許可

## グローバルUsing設定
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

## ファイル構造の補足説明

### リソースファイルの配置
- リソースファイル（`.resx`）と対応する Designer ファイル（`.Designer.cs`）は、各ツールのディレクトリ内に配置する
- リソースファイル名は `ToolName.resx` 形式とし、ResourceManagerBaseName と一致させる

### テストプロジェクトの構造詳細
- テストプロジェクトは、メインプロジェクトと同じ階層構造を持つ
- テストファイルは機能別に `Helpers/`, `Models/`, `Tools/` サブディレクトリに分類される
- テストデータは `TestData/` ディレクトリに格納される

### ログ設定
- 各ツールクラスでは `ILogger` を使用してログを出力する
- ログは `this.Log()` 拡張メソッドを使用して取得する

## フォルダ構造の規則

### メインプロジェクト
- `DevToys.ExtensionKit/Helpers`: 実際の構造では機能別サブディレクトリ無し（例: `IPAddressHelper.cs` は直接配置）
- `DevToys.ExtensionKit/Models`: 実際の構造では機能別サブディレクトリ無し（例: `NetworkInfo.cs` は直接配置）  
- `DevToys.ExtensionKit/Tools`: 機能別サブディレクトリあり（例: `Converters/IPAddress/`, `Converters/Chmod/`）

### テストプロジェクト
- 現在は機能別サブディレクトリが存在しない（改善が必要）
- テストファイルは `IPAddressHelperTests.cs`, `NetworkInfoTests.cs` のように直接配置されている

## UI設計原則

### DevToys UI設計理念
- **一貫性**: DevToys全体で統一されたデザインを維持すること
- **シンプルさ**: Webの知識不要で視覚的に魅力的なUIを作成できること
- **アクセシビリティ**: スクリーンリーダーなど支援技術への対応を考慮すること
- **クロスプラットフォーム**: Windows、macOS、Linuxで一貫した動作を保証すること

### UI設計方針
- **事前構築済みコンポーネント**: DevToys APIの事前構築済みUI要素を活用すること
- **最小限のカスタマイズ**: 一貫したデザインを保つため、カスタマイズオプションは最小限にとどめること
- **レスポンシブ対応**: 異なる画面サイズやCompact Overlayモードに対応すること

## UI実装のベストプラクティス

### レイアウト構造
1. **メインレイアウト**: `Grid()` を使用して全体構造を定義
2. **セクション分離**: `Card()` を使用して入力、出力、設定エリアを分離
3. **グリッド構造**: enum を使用して行と列を定義（例: `GridRow`, `GridColumn`）

### UI要素の命名規則
- プライベートフィールドは readonly で定義
- UI要素のIDは kebab-case を使用（例: `"ip-address"`, `"subnet-mask"`）
- 入力フィールドは `_inputName` 形式
- 出力フィールドは `_outputName` 形式

### エラーハンドリング
- `InfoBar().Error()` を使用してエラーメッセージを表示
- エラー時は `.Close()` で非表示、正常時は `.Open()` で表示

## コーディングガイドライン

### 基本方針
- **コメント**: 必要に応じて可能な限り記述すること
- **命名規則**: [Microsoft の命名規則](https://learn.microsoft.com/ja-jp/dotnet/csharp/fundamentals/coding-style/coding-conventions)に従うこと
- **国際化対応**: UIに表示される文字列は、リソースファイルに定義し、必ず最低でも英語と日本語の両方を用意すること

### 国際化（i18n）対応ガイドライン

### リソースファイル（.resx）の管理
```csharp
// リソースファイルの命名規則
// ToolName.resx      - デフォルト（英語）
// ToolName.ja.resx   - 日本語
// ToolName.fr.resx   - フランス語

// リソースキーの命名規則
public static class ToolNameStrings
{
    // ツール表示名（簡潔）
    public static string ShortDisplayTitle => GetString(nameof(ShortDisplayTitle));
    
    // ツール表示名（詳細、オプション）
    public static string LongDisplayTitle => GetString(nameof(LongDisplayTitle));
    
    // ツール説明文
    public static string Description => GetString(nameof(Description));
    
    // スクリーンリーダー用
    public static string AccessibleName => GetString(nameof(AccessibleName));
    
    // 検索キーワード（空白区切り）
    public static string SearchKeywords => GetString(nameof(SearchKeywords));
    
    // オプション説明（CLIツール用）
    public static string InputOptionDescription => GetString(nameof(InputOptionDescription));
    public static string OutputOptionDescription => GetString(nameof(OutputOptionDescription));
    
    // UI要素ラベル
    public static string InputLabel => GetString(nameof(InputLabel));
    public static string OutputLabel => GetString(nameof(OutputLabel));
    public static string ConvertButtonText => GetString(nameof(ConvertButtonText));
    
    // エラーメッセージ
    public static string InvalidInputError => GetString(nameof(InvalidInputError));
    public static string ProcessingError => GetString(nameof(ProcessingError));
    
    private static string GetString(string name) =>
        ResourceManager.GetString(name) ?? $"[Missing: {name}]";
    
    private static readonly ResourceManager ResourceManager = 
        new("DevToys.ExtensionKit.Tools.Category.ToolName.ToolName", 
            typeof(ToolNameStrings).Assembly);
}
```

### 必須言語サポート
**最低限必要**: 英語（デフォルト）と日本語
**推奨追加**: フランス語、ドイツ語、スペイン語、中国語（簡体字）

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

## パフォーマンスとメモリ効率
- **大容量データ対応**: ユーザーが大きなテキストファイル（数百MB）を扱う可能性を想定すること
- **文字列処理**: `Memory<T>` と `Span<T>` を活用してメモリ効率的な文字列解析を行うこと
- **避けるべき処理**: `String.Split()` や `String.Substring()` などのメモリ割り当てを伴う文字列操作は避ける
- **ファイル読み込み**: 大容量ファイルを想定し、バッファを使用したストリーム読み込みを行うこと
- **メモリ配分回避**: 新しい文字列インスタンスの生成を最小限に抑えること

## パフォーマンス・メモリ効率ガイドライン

### 大容量データ対応
- **想定サイズ**: ユーザーが数百MBのテキストファイルを扱う可能性を想定
- **ストリーム処理**: 大容量ファイルは全体をメモリに読み込まず、ストリームで処理
- **バッファサイズ**: 適切なバッファサイズ（例：8192バイト）を使用

### メモリ効率的な文字列処理
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

// 避けるべき: String.Split()やString.Substring()
public static void ProcessTextInefficiently(string input)
{
    string[] lines = input.Split('\n'); // 大量のメモリ割り当て
    foreach (string line in lines)
    {
        ProcessLine(line); // さらなる文字列コピー
    }
}
```

### ストリーム読み込みのベストプラクティス
```csharp
// 大容量ファイルの効率的な読み込み
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

// 文字列解析でのSpan活用例
public static bool TryParseIPAddress(ReadOnlySpan<char> input, out IPAddress? address)
{
    address = null;
    
    // Spanを使用してメモリ割り当てを回避
    int dotCount = 0;
    for (int i = 0; i < input.Length; i++)
    {
        if (input[i] == '.') dotCount++;
    }
    
    if (dotCount != 3) return false;
    
    // IPAddress.TryParseにSpanを直接渡す
    return IPAddress.TryParse(input, out address);
}
```

### ログ出力ガイドライン
- **報告すべき内容**:
  - エラー情報
  - パフォーマンスに関する情報（関連する場合）
  - システム情報（パフォーマンスや互換性調査に有用な場合のみ）
- **報告してはいけない内容**:
  - ユーザーの入力データ（個人情報保護のため）

### DevToys API 使用方針
- **ツールのUI**: DevToys API を使用して実装すること
- **コマンドライン実装**: DevToys API を使用して実装すること
- **参考資料**: 
  - [DevToys API 入門](https://devtoys.app/doc/articles/introduction.html)
  - [DevToys API リファレンス](https://devtoys.app/doc/api/DevToys.Api.html)

# 機能追加の手順

機能を追加する場合は、以下の手順に従うこと：

1. **ヘルパークラスを実装する**
   - `DevToys.ExtensionKit/Helpers/` ディレクトリに配置
   - 命名規則: `機能名Helper` (例: `UuidHelper.cs`)

2. **ヘルパークラスのテストコードを実装する**
   - `DevToys.ExtensionKit.Tests/` ディレクトリに配置
   - 命名規則: `機能名HelperTests` (例: `UuidHelperTests.cs`)

3. **モデルクラスを実装する**
   - モデルクラスは、必要な場合のみ実装すること
   - `DevToys.ExtensionKit/Models/` ディレクトリに配置
   - モデルクラスのテストコードは、必要な場合のみ実装すること

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
   - GUIツールとコマンドラインツールのテストを作成

9. **ツールを追加した場合は、README.md にツールの説明を追加する**
   - プロジェクトのドキュメントを更新

## Smart Detection機能

### 概要
Smart Detection は、DevToys が外部ソース（クリップボードなど）からのデータタイプを自動検出し、適切なツールを推奨する機能である。

### 実装方法
1. **データタイプ検出器の実装**: `IDataTypeDetector` を実装してデータタイプを検出
2. **ツールでの受信設定**: `[AcceptedDataTypeName("DataTypeName")]` 属性でデータタイプを指定
3. **データ受信処理**: `OnDataReceived(string dataTypeName, object? parsedData)` メソッドで処理

### 事前定義されたデータタイプ
`PredefinedCommonDataTypeNames` クラスには、よく使用されるデータタイプが定義されている：
- テキスト形式（JSON、XML、Base64など）
- ネットワーク関連（URL、IPアドレスなど）
- 画像形式（PNG、JPEG、SVGなど）
- その他の一般的な形式

## コマンドラインツール開発詳細

### サポートされるパラメータ型
- **基本型**: `bool`, `string`, `int`, `double`, `decimal`, `Guid`
- **日時型**: `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`
- **数値型**: `Int16`, `Int32`, `Int64`, `UInt16`, `UInt32`, `UInt64`, `Byte`, `SByte`, `Single`
- **ファイル型**: `FileInfo`, `DirectoryInfo`, `FileSystemInfo`
- **その他**: `Enum`, 上記型の`Array`
- **複合型**: `OneOf<T1, T2>` - 複数型を受け入れるパラメータ

### コマンドライン実行例
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

### ファイル操作
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

### エラーハンドリングのベストプラクティス
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

### CLI制限事項
以下のサービスはCLIツールでは使用できない：
- `IClipboard`: クリップボードアクセス
- `IThemeListener`: テーマ変更監視
- `IFontProvider`: フォント情報取得

### デバッグ設定
`launchSettings.json`でCLIツールのデバッグが可能：
```json
{
  "profiles": {
    "DevToys CLI": {
      "commandName": "Executable",
      "executablePath": "%DevToysCliDebugEntryPoint%",
      "commandLineArgs": "tool-name --input \"test data\"",
      "environmentVariables": {
        "EXTRAPLUGIN": "$(TargetDir)"
      }
    }
  }
}
