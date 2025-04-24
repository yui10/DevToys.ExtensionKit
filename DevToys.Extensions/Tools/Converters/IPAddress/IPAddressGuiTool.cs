using Microsoft.Extensions.Logging;
using netIPAddress = System.Net.IPAddress;
using DevToys.ExtensionKit.Models;

namespace DevToys.ExtensionKit.Tools.Converters.IPAddress;

[Export(typeof(IGuiTool))]
[Name("IPAddress")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uEE22',
    GroupName = PredefinedCommonToolGroupNames.Converters,
    ResourceManagerAssemblyIdentifier = nameof(DevToysExtensionKitResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.ExtensionKit.Tools.Converters.IPAddress.IPAddressParser",
    ShortDisplayTitleResourceName = nameof(IPAddressParser.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(IPAddressParser.LongDisplayTitle),
    DescriptionResourceName = nameof(IPAddressParser.Description),
    AccessibleNameResourceName = nameof(IPAddressParser.AccessibleName))]
internal sealed partial class IPAddressGuiTool : IGuiTool
{
    private enum GridColumn
    {
        Stretch
    }

    private enum GridRow
    {
        Settings,
        Inputs,
        Outputs,
        Subdivision
    }

    private enum OutputGridColumn
    {
        Left,
        Right
    }

    private enum OutputGridRow
    {
        NetworkInfo,
        BroadcastInfo,
        HostRange,
        UsableHosts
    }

    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIInfoBar _errorInfoBar = InfoBar("error-infobar");

    // Input fields
    private readonly IUISingleLineTextInput _ipAddressText = SingleLineTextInput("ip-address");
    private readonly IUINumberInput _subnetMaskNumber = NumberInput("subnet-mask");
    private readonly IUISelectDropDownList _subdivisionCountInput = SelectDropDownList("subdivision-count");

    // Output fields
    private readonly IUISingleLineTextInput _networkAddressText = SingleLineTextInput("network-address");
    private readonly IUISingleLineTextInput _broadcastAddressText = SingleLineTextInput("broadcast-address");
    private readonly IUISingleLineTextInput _firstUsableHostText = SingleLineTextInput("first-host");
    private readonly IUISingleLineTextInput _lastUsableHostText = SingleLineTextInput("last-host");
    private readonly IUISingleLineTextInput _wildcardMaskText = SingleLineTextInput("wildcard-mask");
    private readonly IUISingleLineTextInput _subnetMaskText = SingleLineTextInput("subnet-mask-text");
    private readonly IUISingleLineTextInput _usableHostsCountText = SingleLineTextInput("usable-hosts-count");
    private readonly IUILabel _networkSubdivisionLabel = Label("network-subdivision");
    private readonly IUILabel _subnetResultsLabel = Label("subnet-results");

    [ImportingConstructor]
    public IPAddressGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        // Set default values
        _ipAddressText.Text("192.168.1.1");
        _subnetMaskNumber.Value(24);

        // Initial display
        if (netIPAddress.TryParse(_ipAddressText.Text, out netIPAddress? ipAddress))
        {
            StartIPAddressConvert(ipAddress);
        }
    }

    public UIToolView View => new(
        isScrollable: true,
        Grid()
            .ColumnLargeSpacing()
            .RowLargeSpacing()
            .Columns((GridColumn.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))
            .Rows(
                (GridRow.Settings, new UIGridLength(1, UIGridUnitType.Auto)),
                (GridRow.Inputs, new UIGridLength(1, UIGridUnitType.Auto)),
                (GridRow.Outputs, new UIGridLength(1, UIGridUnitType.Auto)),
                (GridRow.Subdivision, new UIGridLength(1, UIGridUnitType.Fraction)))
            .Cells(
                Cell(
                    GridRow.Settings,
                    GridColumn.Stretch,
                    _errorInfoBar.Title(IPAddressParser.ErrorTitle).Error()
                ),
                Cell(
                    GridRow.Inputs,
                    GridColumn.Stretch,
                    Card(
                        Stack()
                            .Vertical()
                            .WithChildren(
                                Label().Text(IPAddressParser.InputTitle).Style(UILabelStyle.Subtitle),
                                Stack()
                                    .Horizontal()
                                    .LargeSpacing()
                                    .WithChildren(
                                        _ipAddressText.Title(IPAddressParser.IPAddressTitle)
                                            .OnTextChanged(OnIPAddressChanged),
                                        Label().Style(UILabelStyle.BodyStrong).Text("\n/"),
                                        _subnetMaskNumber
                                            .Title(IPAddressParser.PrefixLengthTitle)
                                            .Minimum(1)
                                            .Maximum(32)
                                            .Step(1)
                                            .OnValueChanged(OnSubnetMaskNumberChanged),
                                        _subdivisionCountInput
                                            .Title(IPAddressParser.SubdivisionCountTitle)
                                            .WithItems(
                                                Item(text: "1", value: 1),
                                                Item(text: "2", value: 2),
                                                Item(text: "4", value: 4),
                                                Item(text: "8", value: 8),
                                                Item(text: "16", value: 16),
                                                Item(text: "32", value: 32)
                                            )
                                            .OnItemSelected(OnItemSelected)
                                            .Select(0)
                                    )
                            )
                    )
                ),
                Cell(
                    GridRow.Outputs,
                    GridColumn.Stretch,
                    Card(
                        Stack()
                            .Vertical()
                            .WithChildren(
                                Label().Text(IPAddressParser.OutputTitle).Style(UILabelStyle.Subtitle),
                                NetworkInfoGrid()
                            )
                    )
                ),
                Cell(
                    GridRow.Subdivision,
                    GridColumn.Stretch,
                    Card(
                        Stack()
                            .Vertical()
                            .WithChildren(
                                Label().Text(IPAddressParser.SubdivisionTitle).Style(UILabelStyle.Subtitle),
                                _subnetResultsLabel.Style(UILabelStyle.Body)
                            )
                    )
                )
            )
    );

    private IUIElement NetworkInfoGrid() =>
        Grid()
            .Rows(
                (OutputGridRow.NetworkInfo, new UIGridLength(1, UIGridUnitType.Auto)),
                (OutputGridRow.BroadcastInfo, new UIGridLength(1, UIGridUnitType.Auto)),
                (OutputGridRow.HostRange, new UIGridLength(1, UIGridUnitType.Auto)),
                (OutputGridRow.UsableHosts, new UIGridLength(1, UIGridUnitType.Auto))
            )
            .Columns(
                (OutputGridColumn.Left, new UIGridLength(1, UIGridUnitType.Fraction)),
                (OutputGridColumn.Right, new UIGridLength(1, UIGridUnitType.Fraction))
            )
            .RowSmallSpacing()
            .ColumnMediumSpacing()
            .Cells(
                Cell(
                    OutputGridRow.NetworkInfo,
                    OutputGridColumn.Left,
                    _networkAddressText.Title(IPAddressParser.NetworkAddressTitle).ReadOnly()
                ),
                Cell(
                    OutputGridRow.NetworkInfo,
                    OutputGridColumn.Right,
                    _subnetMaskText.Title(IPAddressParser.SubnetMaskTitle).ReadOnly()
                ),
                Cell(
                    OutputGridRow.BroadcastInfo,
                    OutputGridColumn.Left,
                    _broadcastAddressText.Title(IPAddressParser.BroadcastAddressTitle).ReadOnly()
                ),
                Cell(
                    OutputGridRow.BroadcastInfo,
                    OutputGridColumn.Right,
                    _wildcardMaskText.Title(IPAddressParser.WildcardMaskTitle).ReadOnly()
                ),
                Cell(
                    OutputGridRow.HostRange,
                    OutputGridColumn.Left,
                    Stack()
                        .Horizontal()
                        .LargeSpacing()
                        .WithChildren(
                            _firstUsableHostText.Title(IPAddressParser.FirstHostTitle).ReadOnly(),
                            Label().Style(UILabelStyle.BodyStrong).Text("\n~"),
                            _lastUsableHostText.Title(IPAddressParser.LastHostTitle).ReadOnly()
                        )
                ),
                Cell(
                    OutputGridRow.UsableHosts,
                    OutputGridColumn.Left,
                    _usableHostsCountText.Title(IPAddressParser.UsableHostsTitle).ReadOnly()
                )
            );

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        // External data reception not implemented
    }

    private void OnIPAddressChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            _errorInfoBar.Description(IPAddressParser.ErrorEmptyInput);
            _errorInfoBar.Open();
            return;
        }

        if (!netIPAddress.TryParse(value, out netIPAddress? ipAddress))
        {
            _errorInfoBar.Description(IPAddressParser.ErrorInvalidIPFormat);
            _errorInfoBar.Open();
            return;
        }

        _errorInfoBar.Close();
        StartIPAddressConvert(ipAddress);
    }

    private void OnSubnetMaskNumberChanged(double value)
    {
        if (netIPAddress.TryParse(_ipAddressText.Text, out netIPAddress? _ipAddress))
        {
            StartIPAddressConvert(_ipAddress);
        }
    }

    private void OnItemSelected(IUIDropDownListItem? selectedItem)
    {
        if (selectedItem == null)
        {
            return;
        }

        if (netIPAddress.TryParse(_ipAddressText.Text, out netIPAddress? _ipAddress))
        {
            StartIPAddressConvert(_ipAddress);
        }
    }

    private void StartIPAddressConvert(netIPAddress ipAddress)
    {
        try
        {
            _logger.LogInformation("Starting IP address conversion for: {IPAddress}", ipAddress);
            int prefixLength = (int)_subnetMaskNumber.Value;

            // Create NetworkInfo object
            NetworkInfo networkInfo = new NetworkInfo(ipAddress, prefixLength);

            // Display calculation results in UI
            _networkAddressText.Text(networkInfo.NetworkAddress.ToString());
            _broadcastAddressText.Text(networkInfo.BroadcastAddress.ToString());
            _firstUsableHostText.Text(networkInfo.FirstUsableHost.ToString());
            _lastUsableHostText.Text(networkInfo.LastUsableHost.ToString());
            _wildcardMaskText.Text(networkInfo.WildcardMask.ToString());
            _subnetMaskText.Text(networkInfo.SubnetMask.ToString());

            // Usable hosts
            long usableHosts = networkInfo.GetUsableHostsCount();
            _usableHostsCountText.Text(string.Format(IPAddressParser.HostsCountFormat, usableHosts));

            // Network subdivision information
            GenerateNetworkSubdivisionInfo(networkInfo);

            _logger.LogInformation("Completed processing for IP address {IPAddress} with prefix length {PrefixLength}",
                ipAddress, prefixLength);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during IP address calculation for: {IPAddress}", ipAddress);
            _errorInfoBar.Description(string.Format(IPAddressParser.ErrorFormat, ex.Message));
            _errorInfoBar.Open();
        }
    }

    private void GenerateNetworkSubdivisionInfo(NetworkInfo networkInfo)
    {
        var subdivisionText = new System.Text.StringBuilder();

        // Current network and subnet mask
        subdivisionText.AppendLine(string.Format(IPAddressParser.CurrentNetworkFormat, networkInfo.ToCidrString()));

        // Add subnet information for further dividing this network
        var selectedItem = _subdivisionCountInput.SelectedItem;
        if (selectedItem == null)
        {
            return;
        }
        if (!int.TryParse(selectedItem.Value?.ToString(), out int subdivisionCount))
        {
            return;
        }
        if (networkInfo.PrefixLength + Math.Log2(subdivisionCount) <= 32)
        {
            subdivisionText.AppendLine("\n" + IPAddressParser.IfFurtherSubdividedText);

            var subnets = networkInfo.GetSubnet(subdivisionCount);
            foreach (var subnet in subnets)
            {
                subdivisionText.AppendLine(string.Format("{0}/{1} ({2})", subnet.NetworkAddress, subnet.PrefixLength, subnet.SubnetMask));
            }
        }

        _subnetResultsLabel.Text(subdivisionText.ToString());
    }
}

