using DevToys.ExtensionKit.Helpers.Converters;
using DevToys.ExtensionKit.Models.Converters;
using Microsoft.Extensions.Logging;

namespace DevToys.ExtensionKit.Tools.Converters.Chmod;

/// <summary>
/// UNIX chmod (permission) calculator tool
/// UNIX chmod（パーミッション）計算ツール
/// </summary>
[Export(typeof(IGuiTool))]
[Name("Chmod")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uF5B4',
    GroupName = PredefinedCommonToolGroupNames.Converters,
    ResourceManagerAssemblyIdentifier = nameof(DevToysExtensionKitResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.ExtensionKit.Tools.Converters.Chmod.ChmodCalculator",
    ShortDisplayTitleResourceName = nameof(ChmodCalculator.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(ChmodCalculator.LongDisplayTitle),
    DescriptionResourceName = nameof(ChmodCalculator.Description),
    AccessibleNameResourceName = nameof(ChmodCalculator.AccessibleName))]
internal sealed partial class ChmodCalculatorGuiTool : IGuiTool
{
    private enum GridColumn
    {
        Left,
        Right
    }

    private enum GridRow
    {
        Stretch
    }

    private enum InputsGridColumn
    {
        Title,
        Owner,
        Group,
        Other
    }

    private enum InputsGridRow
    {
        UsersName,
        Read,
        Write,
        Execute
    }

    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;

    // Permission switches for each user type
    // 各ユーザータイプのパーミッションスイッチ
    private readonly IUISwitch OwnerReadSwitch = Switch("owner-read");
    private readonly IUISwitch GroupReadSwitch = Switch("group-read");
    private readonly IUISwitch OtherReadSwitch = Switch("other-read");

    private readonly IUISwitch OwnerWriteSwitch = Switch("owner-write");
    private readonly IUISwitch GroupWriteSwitch = Switch("group-write");
    private readonly IUISwitch OtherWriteSwitch = Switch("other-write");

    private readonly IUISwitch OwnerExecuteSwitch = Switch("owner-execute");
    private readonly IUISwitch GroupExecuteSwitch = Switch("group-execute");
    private readonly IUISwitch OtherExecuteSwitch = Switch("other-execute");

    // 2D array of switches [permission type][user type]
    // スイッチの2次元配列 [パーミッションタイプ][ユーザータイプ]
    private readonly IUISwitch[][] PermissionSwitches;

    // Input fields
    // 入力フィールド
    private readonly IUISingleLineTextInput FileNameText = SingleLineTextInput("file-name");
    private readonly IUISingleLineTextInput ChmodOctalText = SingleLineTextInput("chmod-octal");
    private readonly IUISingleLineTextInput ChmodSymbolText = SingleLineTextInput("chmod-symbol");

    // Output fields (commands)
    // 出力フィールド（コマンド）
    private readonly IUISingleLineTextInput ChmodOctalCommandText = SingleLineTextInput("chmod-octal-command");
    private readonly IUISingleLineTextInput ChmodSymbolCommandText = SingleLineTextInput("chmod-symbol-command");

    /// <summary>
    /// Constructor
    /// コンストラクター
    /// </summary>
    [ImportingConstructor]
    public ChmodCalculatorGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        // Initialize the 2D array of switches
        // スイッチの2次元配列を初期化
        PermissionSwitches =
        [
            [OwnerReadSwitch, OwnerWriteSwitch, OwnerExecuteSwitch],
            [GroupReadSwitch, GroupWriteSwitch, GroupExecuteSwitch],
            [OtherReadSwitch, OtherWriteSwitch, OtherExecuteSwitch]
        ];

        FileNameText.Text(ChmodCalculator.DefaultFileName);
        ChmodOctalText.Text("000");
        ChmodSymbolText.Text("---------");

        UpdateChmodResult();
    }

    public UIToolView View => new(
        isScrollable: true,
        Stack()
            .Vertical()
            .LargeSpacing()
            .WithChildren(
                Label()
                    .Style(UILabelStyle.Subtitle)
                    .Text(ChmodCalculator.Input),
                Card(InputsGrid()),
                Grid()
                    .Rows(
                        (GridRow.Stretch, new UIGridLength(1, UIGridUnitType.Auto))
                    )
                    .Columns(
                        (GridColumn.Left, new UIGridLength(1, UIGridUnitType.Auto)),
                        (GridColumn.Right, new UIGridLength(1, UIGridUnitType.Auto))
                    )
                    .Cells(
                        Cell(
                            GridRow.Stretch,
                            GridColumn.Left,
                            ChmodOctalText
                                .Title(ChmodCalculator.Octal)
                                .OnTextChanged(OnOctalInputChanged)
                        ),
                        Cell(
                            GridRow.Stretch,
                            GridColumn.Right,
                            ChmodSymbolText
                                .Title(ChmodCalculator.Symbol)
                                .OnTextChanged(OnPermissionSymbolInputChanged)
                        )
                    ),
                FileNameText
                    .Title(ChmodCalculator.FileName)
                    .OnTextChanged(OnTextChanged),
                Label()
                    .Style(UILabelStyle.Subtitle)
                    .Text(ChmodCalculator.Output),
                ChmodOctalCommandText
                    .ReadOnly()
                    .Title(ChmodCalculator.ChmodCommandOctal),
                ChmodSymbolCommandText
                    .ReadOnly()
                    .Title(ChmodCalculator.ChmodCommandSymbol)
            )         
        );

    /// <summary>
    /// Create grid layout for permission settings UI
    /// </summary>
    private IUIElement InputsGrid() =>
        Grid()
            .Rows(
                (InputsGridRow.UsersName, new UIGridLength(1, UIGridUnitType.Fraction)),
                (InputsGridRow.Read, new UIGridLength(1, UIGridUnitType.Fraction)),
                (InputsGridRow.Write, new UIGridLength(1, UIGridUnitType.Fraction)),
                (InputsGridRow.Execute, new UIGridLength(1, UIGridUnitType.Fraction))
            )
            .Columns(
                (InputsGridColumn.Title, new UIGridLength(1, UIGridUnitType.Auto)),
                (InputsGridColumn.Owner, new UIGridLength(1, UIGridUnitType.Auto)),
                (InputsGridColumn.Group, new UIGridLength(1, UIGridUnitType.Auto)),
                (InputsGridColumn.Other, new UIGridLength(1, UIGridUnitType.Auto))
            )
            .RowSmallSpacing()
            .ColumnSmallSpacing()
            .Cells(
                // User type headers
                Cell(
                    InputsGridRow.UsersName,
                    InputsGridColumn.Owner,
                    Label().Style(UILabelStyle.BodyStrong).Text(ChmodCalculator.Owner)
                    ), Cell(
                    InputsGridRow.UsersName,
                    InputsGridColumn.Group,
                    Label().Style(UILabelStyle.BodyStrong).Text(ChmodCalculator.Group)
                    ),
                Cell(
                    InputsGridRow.UsersName,
                    InputsGridColumn.Other,
                    Label().Style(UILabelStyle.BodyStrong).Text(ChmodCalculator.Other)
                    ),

                // Read permissions
                Cell(
                    InputsGridRow.Read,
                    InputsGridColumn.Title,
                    Label().Style(UILabelStyle.BodyStrong).Text(ChmodCalculator.Read)
                    ),
                Cell(
                    InputsGridRow.Read,
                    InputsGridColumn.Owner,
                    OwnerReadSwitch.OnToggle(OnToggleChanged)
                    ),
                Cell(
                    InputsGridRow.Read,
                    InputsGridColumn.Group,
                    GroupReadSwitch.OnToggle(OnToggleChanged)
                    ),
                Cell(
                    InputsGridRow.Read,
                    InputsGridColumn.Other,
                    OtherReadSwitch.OnToggle(OnToggleChanged)
                    ),

                // Write permissions
                Cell(
                    InputsGridRow.Write,
                    InputsGridColumn.Title,
                    Label().Style(UILabelStyle.BodyStrong).Text(ChmodCalculator.Write)
                    ),
                Cell(
                    InputsGridRow.Write,
                    InputsGridColumn.Owner,
                    OwnerWriteSwitch.OnToggle(OnToggleChanged)
                    ),
                Cell(
                    InputsGridRow.Write,
                    InputsGridColumn.Group,
                    GroupWriteSwitch.OnToggle(OnToggleChanged)
                    ),
                Cell(
                    InputsGridRow.Write,
                    InputsGridColumn.Other,
                    OtherWriteSwitch.OnToggle(OnToggleChanged)
                    ),

                // Execute permissions
                Cell(
                    InputsGridRow.Execute,
                    InputsGridColumn.Title,
                    Label().Style(UILabelStyle.BodyStrong).Text(ChmodCalculator.Execute)
                    ),
                Cell(
                    InputsGridRow.Execute,
                    InputsGridColumn.Owner,
                    OwnerExecuteSwitch.OnToggle(OnToggleChanged)
                    ),
                Cell(
                    InputsGridRow.Execute,
                    InputsGridColumn.Group,
                    GroupExecuteSwitch.OnToggle(OnToggleChanged)
                    ),
                Cell(
                    InputsGridRow.Execute,
                    InputsGridColumn.Other,
                    OtherExecuteSwitch.OnToggle(OnToggleChanged)
                    )
            );

    /// <summary>
    /// Handle data received from Smart Detection feature
    /// Smart Detection 機能から受信したデータを処理
    /// </summary>
    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        // In the future, this could handle permission strings received through Smart Detection
        // 将来的には、Smart Detection を通じて受信したパーミッション文字列を処理できるようにする
        _logger.LogInformation("OnDataReceived called with dataTypeName: {DataTypeName}", dataTypeName);
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle file name text change
    /// ファイル名テキストの変更を処理
    /// </summary>
    private void OnTextChanged(string text)
    {
        UpdateChmodResult();
    }

    /// <summary>
    /// Handle permission switch toggle
    /// パーミッションスイッチのトグル操作を処理
    /// </summary>
    private void OnToggleChanged(bool isOn)
    {
        UpdateChmodResult();
    }

    /// <summary>
    /// Handle octal input changes
    /// 8進数入力の変更を処理
    /// </summary>
    private void OnOctalInputChanged(string text)
    {
        if (int.TryParse(text, out int octalValue) && octalValue >= 0 && octalValue <= 777)
        {
            var (owner, group, other) = ChmodHelper.ParseOctalToPermissions(octalValue);
            SetSwitches(PermissionSwitches[0], owner);
            SetSwitches(PermissionSwitches[1], group);
            SetSwitches(PermissionSwitches[2], other);
            UpdateChmodResult();
        }
    }

    /// <summary>
    /// Handle symbolic notation input changes
    /// シンボリック表記入力の変更を処理
    /// </summary>
    private void OnPermissionSymbolInputChanged(string text)
    {
        if (ChmodHelper.IsValidPermissionSymbol(text))
        {
            var (owner, group, other) = ChmodHelper.ParseSymbolToPermissions(text);
            SetSwitches(PermissionSwitches[0], owner);
            SetSwitches(PermissionSwitches[1], group);
            SetSwitches(PermissionSwitches[2], other);
            UpdateChmodResult();
        }
    }


    /// <summary>
    /// Set permission switches based on permission value
    /// パーミッション値に基づいてスイッチを設定
    /// </summary>
    private static void SetSwitches(IUISwitch[] switches, Permission permission)
    {
        if (permission.HasFlag(Permission.Read))
        {
            switches[0].On();
        }
        else
        {
            switches[0].Off();
        }

        if (permission.HasFlag(Permission.Write))
        {
            switches[1].On();
        }
        else
        {
            switches[1].Off();
        }

        if (permission.HasFlag(Permission.Execute))
        {
            switches[2].On();
        }
        else
        {
            switches[2].Off();
        }
    }

    /// <summary>
    /// Update chmod results based on current UI state
    /// 現在のUI状態に基づいてchmod結果を更新
    /// </summary>
    private void UpdateChmodResult()
    {
        string file_name = FileNameText.Text;
        if (string.IsNullOrEmpty(file_name))
        {
            file_name = ChmodCalculator.DefaultFileName;
        }

        // Build permissions from switch states
        // スイッチの状態からパーミッションを構築
        Permission owner = BuildPermission(PermissionSwitches[0]);
        Permission group = BuildPermission(PermissionSwitches[1]);
        Permission other = BuildPermission(PermissionSwitches[2]);

        var (chmodOctal, chmodSymbol) = ChmodHelper.Calculate(owner, group, other);
        string chmodOctalCommand = $"chmod {chmodOctal} {file_name}";
        string chmodSymbolCommand = $"chmod {chmodSymbol} {file_name}";

        ChmodOctalText.Text(chmodOctal);
        ChmodSymbolText.Text(chmodSymbol);
        ChmodOctalCommandText.Text(chmodOctalCommand);
        ChmodSymbolCommandText.Text(chmodSymbolCommand);

        _logger.LogInformation("Chmod Result: {ChmodOctal} {ChmodSymbol}", chmodOctal, chmodSymbol);
    }

    /// <summary>
    /// Build permission from switch states
    /// スイッチの状態からパーミッションを構築
    /// </summary>
    private static Permission BuildPermission(IUISwitch[] switches)
    {
        return (switches[0].IsOn ? Permission.Read : Permission.None) |
               (switches[1].IsOn ? Permission.Write : Permission.None) |
               (switches[2].IsOn ? Permission.Execute : Permission.None);
    }
}

