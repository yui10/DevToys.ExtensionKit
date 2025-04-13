using Microsoft.Extensions.Logging;
using netIPAddress = System.Net.IPAddress;
using DevToys.Extensions.Models;

namespace DevToys.Extensions.Tools.Converters.IPAddress;

[Export(typeof(IGuiTool))]
[Name("IPAddress")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uF33A',
    GroupName = PredefinedCommonToolGroupNames.Converters,
    ResourceManagerAssemblyIdentifier = nameof(DevToysExtensionsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Extensions.Tools.Converters.IPAddress.IPAddressParser",
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

    // Output fields
    private readonly IUISingleLineTextInput _networkAddressText = SingleLineTextInput("network-address");
    private readonly IUISingleLineTextInput _broadcastAddressText = SingleLineTextInput("broadcast-address");
    private readonly IUISingleLineTextInput _firstUsableHostText = SingleLineTextInput("first-host");
    private readonly IUISingleLineTextInput _lastUsableHostText = SingleLineTextInput("last-host");
    private readonly IUISingleLineTextInput _wildcardMaskText = SingleLineTextInput("wildcard-mask");
    private readonly IUISingleLineTextInput _subnetMaskText = SingleLineTextInput("subnet-mask-text");
    private readonly IUISingleLineTextInput _usableHostsCountText = SingleLineTextInput("usable-hosts-count");
    private readonly IUILabel _networkSubdivisionLabel = Label("network-subdivision");

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
                                            .OnValueChanged(OnSubnetMaskNumberChanged)
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
                                _networkSubdivisionLabel.Style(UILabelStyle.Body)
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

    private void StartIPAddressConvert(netIPAddress ipAddress)
    {
        try
        {
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
            _errorInfoBar.Description(string.Format(IPAddressParser.ErrorFormat, ex.Message));
            _errorInfoBar.Open();
            _logger.LogError(ex, "Error occurred during IP address calculation");
        }
    }
    
    private void GenerateNetworkSubdivisionInfo(NetworkInfo networkInfo)
    {
        var subdivisionText = new System.Text.StringBuilder();
        
        // Current network and subnet mask
        subdivisionText.AppendLine(string.Format(IPAddressParser.CurrentNetworkFormat, networkInfo.ToCidrString()));
        
        // Add subnet information for further dividing this network
        if (networkInfo.PrefixLength < 30) // Show subdivision info for prefix lengths less than /30
        {
            subdivisionText.AppendLine("\n" + IPAddressParser.IfFurtherSubdividedText);
            
            for (int i = 1; i <= Math.Min(3, 32 - networkInfo.PrefixLength); i++)
            {
                int newPrefixLength = networkInfo.PrefixLength + i;
                int subnetCount = (int)Math.Pow(2, i);
                long hostsPerSubnet = (long)Math.Pow(2, 32 - newPrefixLength) - 2;
                
                if (hostsPerSubnet < 0) hostsPerSubnet = 1; // Special cases for /31 and /32
                
                subdivisionText.AppendLine(string.Format(IPAddressParser.SubnetInfoFormat, 
                    newPrefixLength, subnetCount, hostsPerSubnet));
            }
        }
        
        _networkSubdivisionLabel.Text(subdivisionText.ToString());
    }
}

