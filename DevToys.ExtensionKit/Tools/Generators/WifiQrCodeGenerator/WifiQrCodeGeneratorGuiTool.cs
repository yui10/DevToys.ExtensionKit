using DevToys.Api;
using DevToys.ExtensionKit.Helpers.Generators;
using DevToys.ExtensionKit.Models.Generators;
using Microsoft.Extensions.Logging;

namespace DevToys.ExtensionKit.Tools.Generators.WifiQrCodeGenerator;

[Export(typeof(IGuiTool))]
[Name("WifiQrCodeGenerator")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uF8AC',
    GroupName = PredefinedCommonToolGroupNames.Generators,
    ResourceManagerAssemblyIdentifier = nameof(DevToysExtensionKitResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.ExtensionKit.Tools.Generators.WifiQrCodeGenerator.WifiQrCodeGenerator",
    ShortDisplayTitleResourceName = nameof(WifiQrCodeGenerator.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(WifiQrCodeGenerator.LongDisplayTitle),
    DescriptionResourceName = nameof(WifiQrCodeGenerator.Description),
    AccessibleNameResourceName = nameof(WifiQrCodeGenerator.AccessibleName),
    SearchKeywordsResourceName = nameof(WifiQrCodeGenerator.SearchKeywords))]
internal sealed partial class WifiQrCodeGeneratorGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Setting definition for the WiFi security type.
    /// </summary>
    private static readonly SettingDefinition<WifiSecurityType> SecurityType =
       new(name: $"{nameof(WifiQrCodeGeneratorGuiTool)}.{nameof(SecurityType)}", defaultValue: WifiSecurityType.WPA);

    /// <summary>
    /// Setting definition for the WiFi Hidden.
    /// </summary>
    private static readonly SettingDefinition<bool> IsHidden =
        new(name: $"{nameof(WifiQrCodeGeneratorGuiTool)}.{nameof(IsHidden)}", defaultValue: false);

    private enum GridRow
    {
        Settings,
        Configuration,
        QrCode
    }

    private enum GridColumn
    {
        Stretch
    }

    // Core services and utilities
    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;

    // UI Controls
    private readonly IUISingleLineTextInput _ssidInput = SingleLineTextInput("wifi-ssid-input");
    private readonly IUIPasswordInput _passwordInput = PasswordInput("wifi-password-input");
    private readonly IUIImageViewer _qrCodeViewer = ImageViewer("wifi-qrcode-preview");
    private readonly IUIInfoBar _errorInfoBar = InfoBar("wifi-error-info");

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public WifiQrCodeGeneratorGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        // Set example values
        _ssidInput.Text("MyWiFi");
        _passwordInput.Text("mypassword123");

        // Generate initial QR code
        GenerateQrCode();
    }

    /// <summary>
    /// Gets the current background work task for testing purposes.
    /// </summary>
    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: true,
            Grid()
                .RowLargeSpacing()
                .Rows(
                    (GridRow.Settings, new UIGridLength(1, UIGridUnitType.Auto)),
                    (GridRow.Configuration, new UIGridLength(1, UIGridUnitType.Auto)),
                    (GridRow.QrCode, new UIGridLength(1, UIGridUnitType.Auto)))
                .Columns(
                    (GridColumn.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))
                .Cells(
                    // Settings section for security type and hidden network options
                    Cell(
                    GridRow.Settings,
                    GridColumn.Stretch,
                    Stack()
                        .Vertical()
                        .WithChildren(
                            _errorInfoBar.Title(WifiQrCodeGenerator.ErrorTitle).Error().Close(),
                            Label().Text(WifiQrCodeGenerator.Configuration),
                            Setting("wifi-security-type-setting")
                                .Icon("FluentSystemIcons", '\uF384')
                                .Title(WifiQrCodeGenerator.SecurityTypeSettingTitle)
                                .Description(WifiQrCodeGenerator.SecurityTypeSettingDescription)
                                .Handle(
                                    _settingsProvider,
                                    SecurityType,
                                    OnSecurityTypeChanged,
                                    Item(text: "None", value: WifiSecurityType.None),
                                    Item(text: "WEP", value: WifiSecurityType.WEP),
                                    Item(text: "WPA/WPA2-PSK", value: WifiSecurityType.WPA)
                                    ),
                            Setting("wifi-is-hidden-setting")
                                .Icon("FluentSystemIcons", '\uF384')
                                .Title(WifiQrCodeGenerator.IsHiddenSettingTitle)
                                .Description(WifiQrCodeGenerator.IsHiddenSettingDescription)
                                .Handle(
                                    _settingsProvider,
                                    IsHidden,
                                    OnIsHiddenToggled
                                    )
                        )
                    ),
                    // Configuration section for SSID and password input
                    Cell(
                        GridRow.Configuration,
                        GridColumn.Stretch,
                        Card(
                            Stack()
                                .Vertical()
                                .WithChildren(
                                    Label().Text(WifiQrCodeGenerator.InputTitle).Style(UILabelStyle.Subtitle),

                                    _ssidInput
                                        .Title(WifiQrCodeGenerator.NetworkNameTitle)
                                        .OnTextChanged(OnInputChanged),

                                    _passwordInput
                                        .Title(WifiQrCodeGenerator.PasswordTitle)
                                        .OnTextChanged(OnInputChanged)
                                )
                        )
                    ),
                    // QR code preview section
                    Cell(
                        GridRow.QrCode,
                        GridColumn.Stretch,
                        Card(
                            Stack()
                                .Vertical()
                                .WithChildren(
                                    Label().Text(WifiQrCodeGenerator.QrCodePreviewTitle).Style(UILabelStyle.Subtitle),
                                    _qrCodeViewer
                                        .ManuallyHandleSaveAs("svg", OnSaveAsSvgAsync))
                            )
                        )
                ));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        // WiFi QR codes don't typically receive external data
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    /// <summary>
    /// Handles input field changes and triggers QR code regeneration.
    /// </summary>
    /// <param name="_">The changed text value (not used).</param>
    private void OnInputChanged(string _)
    {
        GenerateQrCode();
    }

    /// <summary>
    /// Handles security type setting changes and updates the UI accordingly.
    /// </summary>
    /// <param name="securityType">The new security type value.</param>
    private void OnSecurityTypeChanged(WifiSecurityType securityType)
    {
        _settingsProvider.SetSetting(SecurityType, securityType);
        
        // Disable password field for open networks
        if (securityType == WifiSecurityType.None)
        {
            _passwordInput.Disable();
        }
        else
        {
            _passwordInput.Enable();
        }
        
        GenerateQrCode();
    }

    /// <summary>
    /// Handles hidden network setting toggle and regenerates QR code.
    /// </summary>
    /// <param name="_">The toggle state (not used, retrieved from settings).</param>
    private void OnIsHiddenToggled(bool _)
    {
        GenerateQrCode();
    }

    /// <summary>
    /// Validates input and initiates QR code generation with current settings.
    /// </summary>
    private void GenerateQrCode()
    {
        string ssid = _ssidInput.Text;
        string password = _passwordInput.Text;
        bool isHidden = _settingsProvider.GetSetting(IsHidden);
        WifiSecurityType securityType = _settingsProvider.GetSetting(SecurityType);

        // Validate configuration before generation
        var (isValid, errorMessage) = WifiQrCodeHelper.ValidateWifiConfiguration(ssid, password, securityType);

        if (!isValid)
        {
            _errorInfoBar.Description(errorMessage).Open();
            _qrCodeViewer.Clear();
            return;
        }

        _errorInfoBar.Close();

        // Generate QR code asynchronously
        StartGenerateQrCode(ssid, password, securityType, isHidden);
    }

    /// <summary>
    /// Starts the asynchronous QR code generation process with proper cancellation handling.
    /// </summary>
    /// <param name="ssid">WiFi network name (SSID).</param>
    /// <param name="password">WiFi password.</param>
    /// <param name="securityType">Security type (None, WEP, WPA).</param>
    /// <param name="isHidden">Whether the network is hidden.</param>
    private void StartGenerateQrCode(string ssid, string password, WifiSecurityType securityType, bool isHidden)
    {
        // Cancel any existing generation task
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = GenerateQrCodeAsync(ssid, password, securityType, isHidden, _cancellationTokenSource.Token);
    }

    /// <summary>
    /// Generates the WiFi QR code image asynchronously on a background thread.
    /// </summary>
    /// <param name="ssid">WiFi network name (SSID).</param>
    /// <param name="password">WiFi password.</param>
    /// <param name="securityType">Security type (None, WEP, WPA).</param>
    /// <param name="isHidden">Whether the network is hidden.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task GenerateQrCodeAsync(string ssid, string password, WifiSecurityType securityType, bool isHidden, CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        try
        {
            using (await _semaphore.WaitAsync(cancellationToken))
            {
                // Don't generate QR code for empty SSID
                if (string.IsNullOrWhiteSpace(ssid))
                {
                    _qrCodeViewer.Clear();
                    return;
                }

                // Generate QR code image using helper
                var image = WifiQrCodeHelper.GenerateWifiQrCode(ssid, password, securityType, isHidden, 512);

                // Display the generated image
                _qrCodeViewer.WithImage(image, disposeAutomatically: true);

                _logger.LogInformation("WiFi QR code generated successfully for SSID: {Ssid}", ssid);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate WiFi QR code");
            _errorInfoBar.Description(WifiQrCodeGenerator.GenerationFailedError).Open();
            _qrCodeViewer.Clear();
        }
    }

    /// <summary>
    /// Handles saving the QR code as an SVG file.
    /// </summary>
    /// <param name="fileStream">The file stream to write the SVG content to.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    private async ValueTask OnSaveAsSvgAsync(FileStream fileStream)
    {
        try
        {
            // Get current configuration
            string ssid = _ssidInput.Text;
            string password = _passwordInput.Text;
            bool isHidden = _settingsProvider.GetSetting(IsHidden);
            WifiSecurityType securityType = _settingsProvider.GetSetting(SecurityType);

            // Generate SVG content
            string svg = WifiQrCodeHelper.GenerateWifiQrCodeSvg(ssid, password, securityType, isHidden, 512);

            // Write SVG to file
            using var fileWriter = new StreamWriter(fileStream);
            await fileWriter.WriteAsync(svg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save WiFi QR code as SVG");
        }
    }
}
