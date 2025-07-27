using DevToys.ExtensionKit.Helpers.EncodersDecoders;
using Microsoft.Extensions.Logging;

namespace DevToys.ExtensionKit.Tools.EncodersDecoders.Unicode;

[Export(typeof(IGuiTool))]
[Name("UnicodeEncoder")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uED37',
    GroupName = PredefinedCommonToolGroupNames.EncodersDecoders,
    ResourceManagerAssemblyIdentifier = nameof(DevToysExtensionKitResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.ExtensionKit.Tools.EncodersDecoders.Unicode.UnicodeEncoderDecoder",
    ShortDisplayTitleResourceName = nameof(UnicodeEncoderDecoder.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(UnicodeEncoderDecoder.LongDisplayTitle),
    DescriptionResourceName = nameof(UnicodeEncoderDecoder.Description),
    AccessibleNameResourceName = nameof(UnicodeEncoderDecoder.AccessibleName))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Text)]
internal sealed partial class UnicodeEncoderDecoderGuiTool : IGuiTool
{
    /// <summary>
    /// Settings for whether to enable the conversion mode (encode/decode).<br />
    /// This determines whether the tool is in encoding mode (true) or decoding mode (false).
    /// </summary>
    private static readonly SettingDefinition<bool> conversionMode =
        new(name: $"{nameof(UnicodeEncoderDecoderGuiTool)}.{nameof(conversionMode)}", defaultValue: true);


    /// <summary>
    /// Settings for whether to enable the table display mode.<br />
    /// This determines whether the output is shown in a table format (true) or as plain text (false).
    /// </summary>
    private static readonly SettingDefinition<bool> IsTableDisplay =
        new(name: $"{nameof(UnicodeEncoderDecoderGuiTool)}.{nameof(IsTableDisplay)}", defaultValue: true);

    /// <summary>
    /// Settings for whether to enable all characters encoding.
    /// </summary>
    private static readonly SettingDefinition<bool> EnableAllCharactersEncoding =
        new(name: $"{nameof(UnicodeEncoderDecoderGuiTool)}.{nameof(EnableAllCharactersEncoding)}", defaultValue: false);

    private enum GridColumn
    {
        Stretch,
    }

    private enum GridRow
    {
        Settings,
        Input,
        Output
    }

    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;

    /// <summary>
    /// Toggle switch for encode/decode mode
    /// </summary>
    private readonly IUISwitch _conversionModeSwitch = Switch("base64-text-conversion-mode-switch");

    /// <summary>
    /// Input field for text to be converted
    /// </summary>
    private readonly IUIMultiLineTextInput _inputText = MultiLineTextInput("unicode-input-text");

    /// <summary>
    /// Text output display field.
    /// Always contains converted text, but hidden in table display mode
    /// </summary>
    private readonly IUIMultiLineTextInput _outputText = MultiLineTextInput("unicode-output-text");

    /// <summary>
    /// Grid display for detailed Unicode information,
    /// showing code points, byte sequences, and character details
    /// </summary>
    private readonly IUIDataGrid _outputDataGrid = DataGrid("unicode-output-data-grid");

    /// <summary>
    /// Error message display area
    /// </summary>
    private readonly IUIInfoBar _errorInfoBar = InfoBar("unicode-error");

    [ImportingConstructor]
    public UnicodeEncoderDecoderGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        _errorInfoBar.Close();

        OnEncodeViewModeChanged(_settingsProvider.GetSetting(IsTableDisplay));
    }

    // UI Tool View Definition
    public UIToolView View => new(
        isScrollable: true,
        Grid()
            .ColumnLargeSpacing()
            .RowLargeSpacing()
            .Columns(
                (GridColumn.Stretch, new UIGridLength(1, UIGridUnitType.Fraction))
            )
            .Rows(
                (GridRow.Settings, new UIGridLength(1, UIGridUnitType.Auto)),
                (GridRow.Input, new UIGridLength(1, UIGridUnitType.Auto)),
                (GridRow.Output, new UIGridLength(1, UIGridUnitType.Auto)))
            .Cells(
                Cell(
                    GridRow.Settings,
                    GridColumn.Stretch,
                    Stack()
                        .Vertical()
                        .WithChildren(
                            _errorInfoBar.Error().Close(),
                            Label().Text(UnicodeEncoderDecoder.Settings),
                            Setting("encode-mode")
                                .Icon("FluentSystemIcons", '\uF18D')
                                .Title(UnicodeEncoderDecoder.EncodeModeTitle)
                                .Description(UnicodeEncoderDecoder.EncodeModeDescription)
                                .InteractiveElement(
                                    _conversionModeSwitch
                                        .OnText(UnicodeEncoderDecoder.EncodeText)
                                        .OffText(UnicodeEncoderDecoder.DecodeText)
                                        .OnToggle(OnConversionModeChanged)
                                ),
                            Setting("encode-all-mode")
                                .Icon("FluentSystemIcons", '\uED68')
                                .Title(UnicodeEncoderDecoder.EncodeAllCharactersTitle)
                                .Description(UnicodeEncoderDecoder.EncodeAllCharactersDescription)
                                .Handle(
                                    _settingsProvider,
                                    EnableAllCharactersEncoding,
                                    OnEncodeAllModeChanged
                                ),
                            Setting("table-display-mode")
                                .Icon("FluentSystemIcons", '\uF75E')
                                .Title(UnicodeEncoderDecoder.TableDisplayModeTitle)
                                .Description(UnicodeEncoderDecoder.TableDisplayModeDescription)
                                .Handle(
                                    _settingsProvider,
                                    IsTableDisplay,
                                    OnEncodeViewModeChanged
                                )
                        )
                ),
                Cell(
                    GridRow.Input,
                    GridColumn.Stretch,
                    Stack()
                        .Vertical()
                        .WithChildren(
                            Label().Text(UnicodeEncoderDecoder.Input).Style(UILabelStyle.Subtitle),
                            _inputText
                                .Title(UnicodeEncoderDecoder.InputTitle)
                                .OnTextChanged(OnInputTextChanged)
                        )
                ),
                Cell(
                    GridRow.Output,
                    GridColumn.Stretch,
                    Stack()
                        .Vertical()
                        .WithChildren(
                            Label().Text(UnicodeEncoderDecoder.Output).Style(UILabelStyle.Subtitle),
                            _outputText
                                .Title(UnicodeEncoderDecoder.Result)
                                .ReadOnly(),
                            _outputDataGrid
                                .Title(UnicodeEncoderDecoder.UnicodeInformation)
                                .Extendable()
                                .WithColumns(
                                    UnicodeEncoderDecoder.Value,
                                    UnicodeEncoderDecoder.CodePoint,
                                    UnicodeEncoderDecoder.UTF8,
                                    UnicodeEncoderDecoder.UTF16BE,
                                    UnicodeEncoderDecoder.Category
                                )
                        )

                )
            )
    );

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Text && parsedData is string text)
        {
            _conversionModeSwitch.On();
            _inputText.Text(text);
        }
    }

    /// <summary>
    /// Handles encode/decode mode changes.
    /// Moves current output to input and performs reverse conversion
    /// </summary>
    /// <param name="isOn">true: encode mode, false: decode mode</param>
    private void OnConversionModeChanged(bool isOn)
    {
        _settingsProvider.SetSetting(conversionMode, isOn);
        ChangeTableVisible(_settingsProvider.GetSetting(IsTableDisplay));

        // Move output to input and convert
        _inputText.Text(_outputText.Text);
        OnInputTextChanged(_inputText.Text);
    }

    /// <summary>
    /// Handles changes to all-characters encoding mode.
    /// Controls whether ASCII characters are also escaped
    /// </summary>
    /// <param name="isOn">true: escape all characters, false: escape non-ASCII only</param>
    private void OnEncodeAllModeChanged(bool isOn)
    {
        if (!string.IsNullOrEmpty(_inputText.Text))
        {
            OnInputTextChanged(_inputText.Text);
        }
    }

    /// <summary>
    /// Handles changes to table display mode.
    /// Toggles between text and table view, reanalyzing Unicode data if needed
    /// </summary>
    /// <param name="isOn">true: table view, false: text view</param>
    private void OnEncodeViewModeChanged(bool isOn)
    {
        ChangeTableVisible(isOn);

        if (!string.IsNullOrEmpty(_inputText.Text))
        {
            OnInputTextChanged(_inputText.Text);
        }
    }

    /// <summary>
    /// Handles input text changes.
    /// Performs Unicode conversion according to current mode and updates display
    /// </summary>
    /// <param name="text">Input text to process</param>
    private void OnInputTextChanged(string text)
    {
        try
        {
            _errorInfoBar.Close();

            if (string.IsNullOrEmpty(text))
            {
                _outputText.Text(string.Empty);
                _outputDataGrid.WithRows([]);
                return;
            }

            // Get current settings
            bool _conversionMode = _settingsProvider.GetSetting(conversionMode);
            bool encodeAll = _settingsProvider.GetSetting(EnableAllCharactersEncoding);
            bool isTableDisplay = _settingsProvider.GetSetting(IsTableDisplay);

            string result;
            if (_conversionMode)
            {
                // Encode mode: Text to Unicode
                result = encodeAll ? UnicodeHelper.EncodeToUnicodeAll(text) : UnicodeHelper.EncodeToUnicode(text);

                // Add Unicode details to grid if table display is enabled
                if (isTableDisplay)
                {
                    List<IUIDataGridRow> rows = [];
                    var unicodeAnalysis = UnicodeHelper.AnalyzeUnicode(text);
                    foreach (var info in unicodeAnalysis)
                    {
                        // Convert byte arrays to hex strings
                        string utf8Hex = string.Join(" ", info.Utf8Bytes.Select(b => b.ToString("X2")));
                        string utf16Hex = string.Join(" ", info.Utf16BEBytes.Select(b => b.ToString("X2")));

                        rows.Add(
                            Row(
                                null,
                                Cell(info.DisplayText),
                                Cell(info.CodePointHex),
                                Cell(utf8Hex),
                                Cell(utf16Hex),
                                Cell(info.Category.ToString())
                            )
                        );
                    }

                    _outputDataGrid.WithRows([.. rows]);
                }
            }
            else
            {
                // Decode mode: Unicode to Text
                result = UnicodeHelper.DecodeFromUnicode(text);
                _outputDataGrid.WithRows([]); // Clear grid in decode mode
            }

            // Set the result to the output text
            _outputText.Text(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unicode conversion error");
            _errorInfoBar.Open().Description(string.Format(UnicodeEncoderDecoder.ConversionError, ex.Message));
            _outputText.Text(string.Empty);
        }
    }

    /// <summary>
    /// Toggles the visibility of table display.<br />
    /// In encode mode with table display enabled, hides text output and shows the data grid.
    /// Otherwise, shows text output and hides the data grid.
    /// </summary>
    /// <param name="isTableShow">Whether to enable table display</param>
    private void ChangeTableVisible(bool isTableShow)
    {
        if (isTableShow && _settingsProvider.GetSetting(conversionMode))
        {
            // Table display mode and encode mode
            _outputText.Hide();
            _outputDataGrid.Show();
        }
        else
        {
            // Text display mode or decode mode
            _outputText.Show();
            _outputDataGrid.Hide();
        }
    }
}
