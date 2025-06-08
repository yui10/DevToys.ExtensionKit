using Microsoft.Extensions.Logging;
using netIPAddress = System.Net.IPAddress;
using DevToys.ExtensionKit.Models;
using System.Text;

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
    /// <summary>
    /// Settings for whether to enable the table display mode.
    /// テーブル表示モードを有効にするかどうかの設定。
    /// </summary>
    private static readonly SettingDefinition<bool> IsTableDisplay =
        new(name: $"{nameof(IPAddressGuiTool)}.{nameof(IsTableDisplay)}", defaultValue: true);

    /// <summary>
    /// Number of subdivisions for the network.
    /// ネットワークの分割数。
    /// </summary>
    private static readonly SettingDefinition<int> SubdivisionCount =
        new(name: $"{nameof(IPAddressGuiTool)}.{nameof(SubdivisionCount)}", defaultValue: 1);

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
    // 入力フィールド
    private readonly IUISingleLineTextInput _ipAddressText = SingleLineTextInput("ip-address");
    private readonly IUINumberInput _subnetMaskNumber = NumberInput("subnet-mask");

    // Output fields
    // 出力フィールド
    private readonly IUISingleLineTextInput _networkAddressText = SingleLineTextInput("network-address");
    private readonly IUISingleLineTextInput _broadcastAddressText = SingleLineTextInput("broadcast-address");
    private readonly IUISingleLineTextInput _firstUsableHostText = SingleLineTextInput("first-host");
    private readonly IUISingleLineTextInput _lastUsableHostText = SingleLineTextInput("last-host");
    private readonly IUISingleLineTextInput _wildcardMaskText = SingleLineTextInput("wildcard-mask");
    private readonly IUISingleLineTextInput _subnetMaskText = SingleLineTextInput("subnet-mask-text");
    private readonly IUISingleLineTextInput _usableHostsCountText = SingleLineTextInput("usable-hosts-count");
    // Network subdivision results
    // ネットワーク分割結果
    private readonly IUIMultiLineTextInput _subnetCSVText = MultiLineTextInput("subnet-csv-text");
    private readonly IUIDataGrid _subnetDataGrid = DataGrid("subnet-data-grid");

    [ImportingConstructor]
    public IPAddressGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        // Set default values
        // 初期値を設定
        _ipAddressText.Text("192.168.1.1");
        _subnetMaskNumber.Value(24);

        // Initial display
        // 初期表示
        if (netIPAddress.TryParse(_ipAddressText.Text, out netIPAddress? ipAddress))
        {
            StartIPAddressConvert(ipAddress);
        }

        // 初期表示モードを設定
        bool isTableEnabled = _settingsProvider.GetSetting(IsTableDisplay);
        OnSettingChanged(isTableEnabled);
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
                    Stack()
                        .Vertical()
                        .WithChildren(
                            _errorInfoBar.Title(IPAddressParser.ErrorTitle).Error().Close(),
                            Label().Text(IPAddressParser.Configuration),
                            Setting("subdivision-count")
                                .Icon("FluentSystemIcons", '\uF7A0')
                                .Title(IPAddressParser.SubdivisionCountTitle)
                                .Description(IPAddressParser.SubdivisionCountDescription)
                                .Handle(
                                    _settingsProvider,
                                    SubdivisionCount,
                                    OnSubnetDivisionChanged,
                                    Item(text: "1", value: 1),
                                    Item(text: "2", value: 2),
                                    Item(text: "4", value: 4),
                                    Item(text: "8", value: 8),
                                    Item(text: "16", value: 16),
                                    Item(text: "32", value: 32)
                                ),
                            Setting("is-table-enabled")
                                .Icon("FluentSystemIcons", '\uF75E')
                                .Title(IPAddressParser.TableDisplayTitle)
                                .Description(IPAddressParser.TableDisplayDescription)
                                .Handle(
                                    _settingsProvider,
                                    IsTableDisplay,
                                    OnSettingChanged
                                )
                        )
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
                                Label().Text(IPAddressParser.IfFurtherSubdividedText),
                                _subnetDataGrid,
                                _subnetCSVText
                                    .Title(IPAddressParser.SubnetCSVFormatTitle)
                                    .ReadOnly()
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

    /// <summary>
    /// Handles changes to the subnet division setting.
    /// サブネット分割設定変更時の処理
    /// </summary>
    private void OnSubnetDivisionChanged(int division)
    {
        if (netIPAddress.TryParse(_ipAddressText.Text, out netIPAddress? _ipAddress))
        {
            StartIPAddressConvert(_ipAddress);
        }
    }

    /// <summary>
    /// Handles changes to the table display mode setting.
    /// テーブル表示モードの設定変更時の処理
    /// </summary>
    private void OnSettingChanged(bool isEnabled)
    {
        _logger.LogInformation("Table display mode changed: {IsEnabled}", isEnabled);

        if (isEnabled)
        {
            _subnetDataGrid.Show();
            _subnetCSVText.Hide();
        }
        else
        {
            _subnetDataGrid.Hide();
            _subnetCSVText.Show();
        }
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
            _logger.LogInformation("Starting IP address conversion for: {IPAddress}", ipAddress);
            int prefixLength = (int)_subnetMaskNumber.Value;

            // Create NetworkInfo object
            // NetworkInfoオブジェクトを作成
            NetworkInfo networkInfo = new NetworkInfo(ipAddress, prefixLength);

            // Display calculation results in UI
            // 計算結果をUIに表示
            _networkAddressText.Text(networkInfo.NetworkAddress.ToString());
            _broadcastAddressText.Text(networkInfo.BroadcastAddress.ToString());
            _firstUsableHostText.Text(networkInfo.FirstUsableHost.ToString());
            _lastUsableHostText.Text(networkInfo.LastUsableHost.ToString());
            _wildcardMaskText.Text(networkInfo.WildcardMask.ToString());
            _subnetMaskText.Text(networkInfo.SubnetMask.ToString());

            // Usable hosts
            // 利用可能なホスト数
            long usableHosts = networkInfo.GetUsableHostsCount();
            _usableHostsCountText.Text(string.Format(IPAddressParser.HostsCountFormat, usableHosts));

            // Network subdivision information
            // ネットワーク分割情報
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
        var csvText = new StringBuilder();

        // CSV header
        // CSVヘッダー
        csvText.AppendLine("Network Address,Prefix Length,Subnet Mask,Broadcast Address,First Host,Last Host,Usable Hosts");

        // Add subnet information for further dividing this network
        // このネットワークをさらに分割するためのサブネット情報を追加
        int subdivisionCount = _settingsProvider.GetSetting(SubdivisionCount);

        // DataGridのデータを作成
        // Create a new DataGrid with all data
        var dataGridColumns = new string[]
        {
            IPAddressParser.NetworkAddressTitle,
            IPAddressParser.PrefixLengthTitle,
            IPAddressParser.SubnetMaskTitle,
            IPAddressParser.BroadcastAddressTitle,
            IPAddressParser.FirstHostTitle,
            IPAddressParser.LastHostTitle,
            IPAddressParser.UsableHostsTitle
        };

        // DataGridの行データを作成
        var rows = new List<IUIDataGridRow>();

        // テーブルの作成と表示
        _subnetDataGrid.WithColumns(dataGridColumns);

        if (networkInfo.PrefixLength + Math.Log2(subdivisionCount) <= 32)
        {
            var subnets = networkInfo.GetSubnet(subdivisionCount);

            foreach (var subnet in subnets)
            {
                string[] rowData =
                [
                    subnet.NetworkAddress.ToString(),
                    subnet.PrefixLength.ToString(),
                    subnet.SubnetMask.ToString(),
                    subnet.BroadcastAddress.ToString(),
                    subnet.FirstUsableHost.ToString(),
                    subnet.LastUsableHost.ToString(),
                    subnet.GetUsableHostsCount().ToString()
                ];
                // Add to CSV (for non-table mode)
                csvText.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", rowData));

                // Add to DataGrid (for table mode)
                rows.Add(Row(value: subnet, rowData));
            }
        }

        // テキスト表示を更新 (テーブル表示モードでない場合に使用)
        _subnetCSVText.Text(csvText.ToString());

        // 行データを設定(テーブル表示モードで使用)
        _subnetDataGrid.WithRows(rows.ToArray());
    }
}
