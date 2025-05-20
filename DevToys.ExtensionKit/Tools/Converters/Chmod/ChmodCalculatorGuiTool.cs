using Microsoft.Extensions.Logging;

namespace DevToys.ExtensionKit.Tools.Converters.Chmod;
[Export(typeof(IGuiTool))]
[Name("Chmod")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uF33A',
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
        Stretch
    }

    private enum GridRow
    {
        Settings,
        Inputs,
        Outputs
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

    private readonly IUISwitch OwnerReadSwitch = Switch("owner-read");
    private readonly IUISwitch GroupReadSwitch = Switch("group-read");
    private readonly IUISwitch OtherReadSwitch = Switch("other-read");

    private readonly IUISwitch OwnerWriteSwitch = Switch("owner-write");
    private readonly IUISwitch GroupWriteSwitch = Switch("group-write");
    private readonly IUISwitch OtherWriteSwitch = Switch("other-write");

    private readonly IUISwitch OwnerExecuteSwitch = Switch("owner-execute");
    private readonly IUISwitch GroupExecuteSwitch = Switch("group-execute");
    private readonly IUISwitch OtherExecuteSwitch = Switch("other-execute");

    private readonly IUISwitch[][] PermissionSwitches;

    private readonly IUISingleLineTextInput FileNameText = SingleLineTextInput("file-name");

    private readonly IUISingleLineTextInput ChmodOctalText = SingleLineTextInput("chmod-octal");
    private readonly IUISingleLineTextInput ChmodSymbolText = SingleLineTextInput("chmod-symbol");
    private readonly IUISingleLineTextInput ChmodOctalCommandText = SingleLineTextInput("chmod-octal-command");
    private readonly IUISingleLineTextInput ChmodSymbolCommandText = SingleLineTextInput("chmod-symbol-command");

    [ImportingConstructor]
    public ChmodCalculatorGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
        PermissionSwitches =
        [
            [OwnerReadSwitch, GroupReadSwitch, OtherReadSwitch],
            [OwnerWriteSwitch, GroupWriteSwitch, OtherWriteSwitch],
            [OwnerExecuteSwitch, GroupExecuteSwitch, OtherExecuteSwitch]
        ];
        UpdateChmodResult();
    }

    public UIToolView View => new(
        isScrollable: true,
        Grid()
            .ColumnLargeSpacing()
            .RowLargeSpacing()
            .Columns((GridColumn.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))
            .Rows(
                (GridRow.Settings, new UIGridLength(1, UIGridUnitType.Auto)),
                (GridRow.Inputs, new UIGridLength(1, UIGridUnitType.Fraction)),
                (GridRow.Outputs, new UIGridLength(1, UIGridUnitType.Fraction)))
            .Cells(
                Cell(
                    GridRow.Settings,
                    GridColumn.Stretch,
                    Stack()
                ),
                Cell(
                    GridRow.Inputs,
                    GridColumn.Stretch,
                    Stack()
                        .Vertical()
                        .WithChildren(
                            Card(InputsGrid()),                            FileNameText
                                .Title(ChmodCalculator.FileName)
                                .OnTextChanged(OnTextChanged)
                        )
                ),
                Cell(
                    GridRow.Outputs,
                    GridColumn.Stretch,
                    Stack()
                        .Vertical()
                        .WithChildren(
                            Grid()
                                .Rows(new UIGridLength(1, UIGridUnitType.Auto))
                                .Columns(3)
                                .Cells(
                                    Cell(0, 1, 1, 1, Label().Text(ChmodCalculator.Permission).Style(UILabelStyle.BodyLarge)),
                                    Cell(0, 2, 1, 1, ChmodOctalText.Text("000").ReadOnly().Title(ChmodCalculator.Octal)),
                                    Cell(0, 3, 1, 1, ChmodSymbolText.Text("---------").ReadOnly().Title(ChmodCalculator.Symbol))
                                ),
                            ChmodOctalCommandText
                                .ReadOnly()
                                .Title(ChmodCalculator.ChmodCommandOctal),
                            ChmodSymbolCommandText
                                .ReadOnly()
                                .Title(ChmodCalculator.ChmodCommandSymbol)
                        )
                )
            ));

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
                // Users Name
                Cell(
                    InputsGridRow.UsersName,
                    InputsGridColumn.Owner,
                    Label().Style(UILabelStyle.BodyStrong).Text(ChmodCalculator.Owner)
                    ),
                Cell(
                    InputsGridRow.UsersName,
                    InputsGridColumn.Group,
                    Label().Style(UILabelStyle.BodyStrong).Text(ChmodCalculator.Group)
                    ),
                Cell(
                    InputsGridRow.UsersName,
                    InputsGridColumn.Other,
                    Label().Style(UILabelStyle.BodyStrong).Text(ChmodCalculator.Other)
                    ),

                // Read
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

                // Write
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

                // Execute
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

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        throw new NotImplementedException();
    }

    private void OnTextChanged(string text)
    {
        UpdateChmodResult();
    }

    private void OnToggleChanged(bool isOn)
    {
        UpdateChmodResult();
    }

    private void UpdateChmodResult()
    {
        string file_name = FileNameText.Text;
        if (string.IsNullOrEmpty(file_name))
        {
            file_name = "file_name";
        }

        int ownerPermission = (OwnerReadSwitch.IsOn ? 4 : 0) | (OwnerWriteSwitch.IsOn ? 2 : 0) | (OwnerExecuteSwitch.IsOn ? 1 : 0);
        int groupPermission = (GroupReadSwitch.IsOn ? 4 : 0) | (GroupWriteSwitch.IsOn ? 2 : 0) | (GroupExecuteSwitch.IsOn ? 1 : 0);
        int otherPermission = (OtherReadSwitch.IsOn ? 4 : 0) | (OtherWriteSwitch.IsOn ? 2 : 0) | (OtherExecuteSwitch.IsOn ? 1 : 0);

        string chmodOctal = $"{Convert.ToString(ownerPermission, 8)}{Convert.ToString(groupPermission, 8)}{Convert.ToString(otherPermission, 8)}";
        string chmodOctalCommand = $"chmod {chmodOctal} {file_name}";


        string ownerPermissionSymbol = $"{(OwnerReadSwitch.IsOn ? "r" : "-")}{(OwnerWriteSwitch.IsOn ? "w" : "-")}{(OwnerExecuteSwitch.IsOn ? "x" : "-")}";
        string groupPermissionSymbol = $"{(GroupReadSwitch.IsOn ? "r" : "-")}{(GroupWriteSwitch.IsOn ? "w" : "-")}{(GroupExecuteSwitch.IsOn ? "x" : "-")}";
        string otherPermissionSymbol = $"{(OtherReadSwitch.IsOn ? "r" : "-")}{(OtherWriteSwitch.IsOn ? "w" : "-")}{(OtherExecuteSwitch.IsOn ? "x" : "-")}";

        string chmodSymbol = $"{ownerPermissionSymbol}{groupPermissionSymbol}{otherPermissionSymbol}";
        string chmodSymbolCommand = $"chmod {chmodSymbol} {file_name}";

        ChmodOctalText.Text(chmodOctal);
        ChmodSymbolText.Text(chmodSymbol);
        ChmodOctalCommandText.Text(chmodOctalCommand);
        ChmodSymbolCommandText.Text(chmodSymbolCommand);

        _logger.LogInformation("Chmod Result: {ChmodOctal}", chmodOctal);
    }
}

