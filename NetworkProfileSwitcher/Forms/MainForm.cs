using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NetworkProfileSwitcher.Models;
using System.Text.Json;
using System.IO;
using System.Net.NetworkInformation;
using System.Drawing;
using System.Diagnostics;
using System.Linq;

namespace NetworkProfileSwitcher.Forms
{
    public partial class MainForm : Form
    {
        private List<NetworkPreset> presets = new List<NetworkPreset>();
        private const string PresetsFilePath = "presets.json";
        private ListBox? adapterListBox;
        private ListBox? presetListBox;
        private Button? applyButton;
        private Button? addPresetButton;
        private Button? editPresetButton;
        private Button? deletePresetButton;
        private Button? duplicatePresetButton;
        private TextBox? currentIpLabel;
        private Button? refreshButton;
        private Button? openNetworkConnectionsButton;
        private MenuStrip? mainMenuStrip;
        private ToolStripMenuItem? helpMenuItem;
        private ToolStripMenuItem? versionMenuItem;
        private ToolStripMenuItem? licenseMenuItem;
        private GroupBox? adapterGroupBox;
        private GroupBox? presetGroupBox;
        private GroupBox? infoGroupBox;
        private GroupBox? actionGroupBox;
        private StatusStrip? statusStrip;
        private ToolStripStatusLabel? statusLabel;

        public MainForm()
        {
            try
            {
                InitializeComponent();

                try
                {
                    LoadPresets();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"プリセットの読み込みに失敗しました。\n\nエラー: {ex.Message}",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                try
                {
                    InitializeNetworkAdapters();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"ネットワークアダプタの初期化に失敗しました。\n\nエラー: {ex.Message}",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"アプリケーションの初期化に失敗しました。\n\nエラー: {ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                throw;
            }
        }



        private void InitializeComponent()
        {
            this.SuspendLayout();

            // ステータスストリップ
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("準備完了");
            statusStrip.Items.Add(statusLabel);

            // アダプタグループボックス
            adapterGroupBox = new GroupBox
            {
                Text = "ネットワークアダプタ",
                Location = new Point(12, 30),
                Size = new Size(400, 280),
                Font = new Font("MS Gothic", 9, FontStyle.Bold)
            };

            // アダプタ一覧
            adapterListBox = new ListBox
            {
                Location = new Point(10, 20),
                Size = new Size(380, 200),
                SelectionMode = SelectionMode.One,
                HorizontalScrollbar = true,
                ScrollAlwaysVisible = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 24
            };
            adapterListBox.SelectedIndexChanged += AdapterListBox_SelectedIndexChanged;
            adapterListBox.DrawItem += AdapterListBox_DrawItem;

            // 更新ボタン
            refreshButton = new Button
            {
                Text = "更新",
                Location = new Point(10, 230),
                Size = new Size(80, 35)
            };
            refreshButton.Click += RefreshButton_Click;

            // ネットワーク接続画面を開くボタン
            openNetworkConnectionsButton = new Button
            {
                Text = "ネットワーク設定",
                Location = new Point(100, 230),
                Size = new Size(140, 35)
            };
            openNetworkConnectionsButton.Click += OpenNetworkConnectionsButton_Click;

            // プリセットグループボックス
            presetGroupBox = new GroupBox
            {
                Text = "ネットワークプリセット",
                Location = new Point(12, 320),
                Size = new Size(400, 280),
                Font = new Font("MS Gothic", 9, FontStyle.Bold)
            };

            // プリセット一覧
            presetListBox = new ListBox
            {
                Location = new Point(10, 20),
                Size = new Size(380, 200),
                SelectionMode = SelectionMode.One,
                HorizontalScrollbar = true,
                ScrollAlwaysVisible = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 24
            };
            presetListBox.SelectedIndexChanged += PresetListBox_SelectedIndexChanged;
            presetListBox.DrawItem += PresetListBox_DrawItem;

            // 情報グループボックス
            infoGroupBox = new GroupBox
            {
                Text = "アダプタ情報",
                Location = new Point(430, 30),
                Size = new Size(450, 280),
                Font = new Font("MS Gothic", 9, FontStyle.Bold)
            };

            // 現在のIP情報
            currentIpLabel = new TextBox
            {
                Location = new Point(10, 20),
                Size = new Size(430, 250),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("MS Gothic", 9),
                BackColor = Color.FromArgb(248, 248, 248),
                BorderStyle = BorderStyle.FixedSingle
            };

            // アクショングループボックス
            actionGroupBox = new GroupBox
            {
                Text = "操作",
                Location = new Point(430, 320),
                Size = new Size(450, 280),
                Font = new Font("MS Gothic", 9, FontStyle.Bold)
            };

            // 適用ボタン
            applyButton = new Button
            {
                Text = "適用",
                Location = new Point(10, 20),
                Size = new Size(120, 40),
                Enabled = false
            };
            applyButton.Click += ApplyButton_Click;

            // プリセット操作ボタン
            addPresetButton = new Button
            {
                Text = "追加",
                Location = new Point(10, 70),
                Size = new Size(100, 35)
            };
            addPresetButton.Click += AddPresetButton_Click;

            editPresetButton = new Button
            {
                Text = "編集",
                Location = new Point(120, 70),
                Size = new Size(100, 35),
                Enabled = false
            };
            editPresetButton.Click += EditPresetButton_Click;

            duplicatePresetButton = new Button
            {
                Text = "複製",
                Location = new Point(230, 70),
                Size = new Size(100, 35),
                Enabled = false
            };
            duplicatePresetButton.Click += DuplicatePresetButton_Click;

            deletePresetButton = new Button
            {
                Text = "削除",
                Location = new Point(340, 70),
                Size = new Size(100, 35),
                Enabled = false
            };
            deletePresetButton.Click += DeletePresetButton_Click;

            // メニューの作成
            mainMenuStrip = new MenuStrip();
            helpMenuItem = new ToolStripMenuItem("ヘルプ(&H)");
            
            versionMenuItem = new ToolStripMenuItem("バージョン情報(&V)");
            versionMenuItem.Click += VersionMenuItem_Click;
            
            licenseMenuItem = new ToolStripMenuItem("ライセンス情報(&L)");
            licenseMenuItem.Click += LicenseMenuItem_Click;
            
            helpMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                versionMenuItem,
                licenseMenuItem
            });
            
            mainMenuStrip.Items.Add(helpMenuItem);

            // フォーム設定
            this.ClientSize = new Size(900, 650);
            this.Text = "Network Profile Switcher";
            this.MainMenuStrip = mainMenuStrip;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(900, 650);
            
            // コントロールをグループボックスに追加
            adapterGroupBox.Controls.AddRange(new Control[] {
                adapterListBox,
                refreshButton,
                openNetworkConnectionsButton
            });

            presetGroupBox.Controls.AddRange(new Control[] {
                presetListBox
            });

            infoGroupBox.Controls.AddRange(new Control[] {
                currentIpLabel
            });

            actionGroupBox.Controls.AddRange(new Control[] {
                applyButton,
                addPresetButton,
                editPresetButton,
                duplicatePresetButton,
                deletePresetButton
            });

            this.Controls.AddRange(new Control[] {
                mainMenuStrip,
                statusStrip,
                adapterGroupBox,
                presetGroupBox,
                infoGroupBox,
                actionGroupBox
            });

            this.ResumeLayout(false);
        }

        private void AdapterListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateCurrentIpInfo();
            UpdateButtonStates();
        }

        private void PresetListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void ApplyButton_Click(object? sender, EventArgs e)
        {
            if (adapterListBox?.SelectedItem == null || presetListBox?.SelectedItem == null)
                return;

            var adapter = (NetworkInterface)adapterListBox.SelectedItem;
            var preset = (NetworkPreset)presetListBox.SelectedItem;

            // 無線LANの場合は確認ダイアログを表示
            if (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
            {
                var result = MessageBox.Show(
                    "無線LANの設定を変更しようとしています。\n" +
                    "無線LANの設定を変更すると、接続が切断される可能性があります。\n\n" +
                    "実行しますか？",
                    "確認",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            try
            {
                NetworkManager.ApplyPreset(adapter, preset);
                
                // アダプタリストを完全に更新
                InitializeNetworkAdapters();

                // 同じアダプタを再度選択
                var newAdapter = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(a => a.Id == adapter.Id);
                
                if (newAdapter != null)
                {
                    var index = adapterListBox.Items.OfType<NetworkInterface>()
                        .ToList()
                        .FindIndex(a => a.Id == newAdapter.Id);
                    
                    if (index >= 0)
                    {
                        adapterListBox.SelectedIndex = index;
                    }
                }

                // DHCPが既に有効な場合のメッセージを調整
                string message = "設定を適用しました。";
                if (preset.IP.ToLower() == "dhcp")
                {
                    message = "DHCP設定を適用しました。\n（既にDHCPが有効な場合は設定が確認されました）";
                }

                MessageBox.Show(message, "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateCurrentIpInfo();
            }
            catch (Exception ex)
            {
                var details = $"アダプタ: {adapter.Name}\n" +
                            $"プリセット: {preset.Name}\n" +
                            $"IP: {preset.IP}\n" +
                            $"サブネット: {preset.Subnet}\n" +
                            $"ゲートウェイ: {preset.Gateway}\n" +
                            $"DNS1: {preset.DNS1}\n" +
                            $"DNS2: {preset.DNS2}\n\n" +
                            $"エラー詳細:\n{ex.Message}\n\n" +
                            $"アダプタ詳細情報:\n{NetworkManager.GetAdapterDetailedInfo(adapter)}";

                using (var errorForm = new ErrorDialogForm(
                    "エラー", 
                    "設定の適用に失敗しました。",
                    details))
                {
                    errorForm.ShowDialog();
                }
            }
        }

        private bool ValidatePreset(NetworkPreset preset)
        {
            if (string.IsNullOrWhiteSpace(preset.Name))
            {
                using (var errorForm = new ErrorDialogForm("エラー", "プリセット名が空です。"))
                {
                    errorForm.ShowDialog();
                }
                return false;
            }

            if (preset.IP != "dhcp" && string.IsNullOrWhiteSpace(preset.IP))
            {
                using (var errorForm = new ErrorDialogForm("エラー", "IPアドレスが空です。"))
                {
                    errorForm.ShowDialog();
                }
                return false;
            }

            return true;
        }

        private void AddPresetButton_Click(object? sender, EventArgs e)
        {
            using (var form = new PresetEditorForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var preset = form.GetPreset();
                    
                    if (ValidatePreset(preset))
                    {
                        presets.Add(preset);
                        SavePresets();
                        UpdatePresetList();
                    }
                }
            }
        }

        private void EditPresetButton_Click(object? sender, EventArgs e)
        {
            if (presetListBox?.SelectedItem == null)
                return;

            var selectedPreset = (NetworkPreset)presetListBox.SelectedItem;
            using (var form = new PresetEditorForm(selectedPreset))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var preset = form.GetPreset();
                    
                    if (ValidatePreset(preset))
                    {
                        int index = presets.IndexOf(selectedPreset);
                        presets[index] = preset;
                        SavePresets();
                        UpdatePresetList();
                    }
                }
            }
        }

        private void DeletePresetButton_Click(object? sender, EventArgs e)
        {
            if (presetListBox?.SelectedItem == null)
                return;

            var result = MessageBox.Show(
                "選択したプリセットを削除しますか？",
                "確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                presets.Remove((NetworkPreset)presetListBox.SelectedItem);
                SavePresets();
                UpdatePresetList();
            }
        }

        private void UpdateCurrentIpInfo()
        {
            if (adapterListBox?.SelectedItem == null)
            {
                currentIpLabel!.Text = "アダプタが選択されていません";
                return;
            }

            var adapter = (NetworkInterface)adapterListBox.SelectedItem;
            var properties = adapter.GetIPProperties();

            var info = new System.Text.StringBuilder();

            // アダプタ情報
            info.AppendLine("■ アダプタ情報");
            info.AppendLine($"名前: {adapter.Name}");
            info.AppendLine($"説明: {adapter.Description}");
            info.AppendLine($"種類: {GetAdapterTypeName(adapter.NetworkInterfaceType)}");
            info.AppendLine($"状態: {GetOperationalStatusName(adapter.OperationalStatus)}");
            info.AppendLine($"速度: {GetSpeedString(adapter.Speed)}");
            info.AppendLine($"MACアドレス: {GetMacAddress(adapter)}");
            info.AppendLine();

            // IP設定
            info.AppendLine("■ IP設定");
            bool hasIpv4 = false;
            bool isDhcp = false;
            try
            {
                // DHCPの設定状態を確認
                var dhcpServerAddresses = properties.DhcpServerAddresses;
                isDhcp = dhcpServerAddresses.Count > 0;
            }
            catch
            {
                // エラーが発生した場合は無視
            }

            info.AppendLine($"DHCP: {(isDhcp ? "有効" : "無効")}");
            info.AppendLine();

            foreach (var ip in properties.UnicastAddresses)
            {
                if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    hasIpv4 = true;
                    info.AppendLine($"IPアドレス: {ip.Address}");
                    info.AppendLine($"サブネットマスク: {ip.IPv4Mask}");
                    info.AppendLine();
                }
            }
            if (!hasIpv4)
            {
                info.AppendLine("IPv4アドレスが設定されていません");
                info.AppendLine();
            }

            // ゲートウェイ
            info.AppendLine("■ ゲートウェイ");
            bool hasGateway = false;
            foreach (var gateway in properties.GatewayAddresses)
            {
                if (gateway.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    hasGateway = true;
                    info.AppendLine($"デフォルトゲートウェイ: {gateway.Address}");
                }
            }
            if (!hasGateway)
            {
                info.AppendLine("デフォルトゲートウェイが設定されていません");
            }
            info.AppendLine();

            // DNSサーバー
            info.AppendLine("■ DNSサーバー");
            bool hasDns = false;
            foreach (var dns in properties.DnsAddresses)
            {
                if (dns.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    hasDns = true;
                    info.AppendLine($"DNSサーバー: {dns}");
                }
            }
            if (!hasDns)
            {
                info.AppendLine("DNSサーバーが設定されていません");
            }

            currentIpLabel!.Text = info.ToString();
        }

        private string GetAdapterTypeName(NetworkInterfaceType type)
        {
            return type switch
            {
                NetworkInterfaceType.Ethernet => "有線LAN",
                NetworkInterfaceType.Wireless80211 => "無線LAN",
                NetworkInterfaceType.Loopback => "ループバック",
                NetworkInterfaceType.Ppp => "PPP",
                NetworkInterfaceType.Tunnel => "トンネル",
                _ => type.ToString()
            };
        }

        private string GetOperationalStatusName(OperationalStatus status)
        {
            return status switch
            {
                OperationalStatus.Up => "接続中",
                OperationalStatus.Down => "切断中",
                OperationalStatus.Testing => "テスト中",
                OperationalStatus.Unknown => "不明",
                OperationalStatus.Dormant => "休止中",
                OperationalStatus.NotPresent => "未接続",
                OperationalStatus.LowerLayerDown => "下位層が切断",
                _ => status.ToString()
            };
        }

        private string GetSpeedString(long speed)
        {
            if (speed < 1000)
                return $"{speed} bps";
            else if (speed < 1000000)
                return $"{speed / 1000.0:F1} Kbps";
            else if (speed < 1000000000)
                return $"{speed / 1000000.0:F1} Mbps";
            else
                return $"{speed / 1000000000.0:F1} Gbps";
        }

        private string GetMacAddress(NetworkInterface adapter)
        {
            var mac = adapter.GetPhysicalAddress();
            if (mac == null || mac.GetAddressBytes().Length == 0)
                return "不明";

            return BitConverter.ToString(mac.GetAddressBytes()).Replace("-", ":");
        }

        private void UpdateButtonStates()
        {
            bool hasSelectedAdapter = adapterListBox?.SelectedItem != null;
            bool hasSelectedPreset = presetListBox?.SelectedItem != null;

            applyButton!.Enabled = hasSelectedAdapter && hasSelectedPreset;
            editPresetButton!.Enabled = hasSelectedPreset;
            duplicatePresetButton!.Enabled = hasSelectedPreset;
            deletePresetButton!.Enabled = hasSelectedPreset;

            // ステータスバーの更新
            UpdateStatusBar();
        }

        private void UpdateStatusBar()
        {
            if (statusLabel == null) return;

            var adapterCount = adapterListBox?.Items.Count ?? 0;
            var presetCount = presetListBox?.Items.Count ?? 0;
            var selectedAdapter = adapterListBox?.SelectedItem != null ? "選択中" : "未選択";
            var selectedPreset = presetListBox?.SelectedItem != null ? "選択中" : "未選択";

            statusLabel.Text = $"アダプタ: {adapterCount}個 ({selectedAdapter}) | プリセット: {presetCount}個 ({selectedPreset})";
        }

        private void UpdatePresetList()
        {
            presetListBox!.Items.Clear();
            presetListBox.HorizontalExtent = 0;  // 横スクロールをリセット
            foreach (var preset in presets)
            {
                presetListBox.Items.Add(preset);
            }
            UpdateStatusBar();
        }

        private void LoadPresets()
        {
            if (File.Exists(PresetsFilePath))
            {
                string json = File.ReadAllText(PresetsFilePath);
                presets = JsonSerializer.Deserialize<List<NetworkPreset>>(json) ?? new List<NetworkPreset>();
            }
            UpdatePresetList();
        }

        private void SavePresets()
        {
            string json = JsonSerializer.Serialize(presets, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(PresetsFilePath, json);
        }

        private void InitializeNetworkAdapters()
        {
            // 現在選択されているアダプタのIDを保存
            string? selectedAdapterId = null;
            if (adapterListBox?.SelectedItem != null)
            {
                selectedAdapterId = ((NetworkInterface)adapterListBox.SelectedItem).Id;
            }

            // アダプタリストをクリア
            adapterListBox!.Items.Clear();
            adapterListBox.HorizontalExtent = 0;  // 横スクロールをリセット

            // ネットワークアダプタを再取得
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in adapters)
            {
                if ((adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                    adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) &&
                    !adapter.Description.ToLower().Contains("bluetooth"))
                {
                    adapterListBox.Items.Add(adapter);
                }
            }

            // アダプタの表示形式を設定
            adapterListBox.DisplayMember = "Name";

            // 以前選択されていたアダプタを再度選択
            if (!string.IsNullOrEmpty(selectedAdapterId))
            {
                var index = adapterListBox.Items.OfType<NetworkInterface>()
                    .ToList()
                    .FindIndex(a => a.Id == selectedAdapterId);
                
                if (index >= 0)
                {
                    adapterListBox.SelectedIndex = index;
                }
                else if (adapterListBox.Items.Count > 0)
                {
                    adapterListBox.SelectedIndex = 0;
                }
            }
            else if (adapterListBox.Items.Count > 0)
            {
                adapterListBox.SelectedIndex = 0;
            }

            UpdateStatusBar();
        }

        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            InitializeNetworkAdapters();
            UpdateCurrentIpInfo();
        }

        private void DuplicatePresetButton_Click(object? sender, EventArgs e)
        {
            if (presetListBox?.SelectedItem == null)
                return;

            var selectedPreset = (NetworkPreset)presetListBox.SelectedItem;
            var newPreset = new NetworkPreset
            {
                Name = $"{selectedPreset.Name} (コピー)",
                IP = selectedPreset.IP,
                Subnet = selectedPreset.Subnet,
                Gateway = selectedPreset.Gateway,
                DNS1 = selectedPreset.DNS1,
                DNS2 = selectedPreset.DNS2,
                Comment = selectedPreset.Comment
            };

            presets.Add(newPreset);
            SavePresets();
            UpdatePresetList();
            presetListBox!.SelectedItem = newPreset;
        }

        private void OpenNetworkConnectionsButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // コントロールパネルのネットワーク接続を開く
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "control.exe",
                    Arguments = "ncpa.cpl",
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"ネットワーク接続を開けませんでした。\n\nエラー: {ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void VersionMenuItem_Click(object? sender, EventArgs e)
        {
            var versionForm = new VersionInfoForm();
            versionForm.ShowDialog(this);
        }

        private void LicenseMenuItem_Click(object? sender, EventArgs e)
        {
            var licenseForm = new LicenseInfoForm();
            licenseForm.ShowDialog(this);
        }

        private void AdapterListBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || adapterListBox?.Items[e.Index] is not NetworkInterface adapter)
                return;

            e.DrawBackground();

            // 選択状態に応じて色を設定
            Color textColor;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                textColor = SystemColors.HighlightText;
            }
            else
            {
                // 無線LANアダプタの場合は赤色
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    textColor = Color.Red;
                }
                else
                {
                    try
                    {
                        var properties = adapter.GetIPProperties();
                        var dhcpServerAddresses = properties.DhcpServerAddresses;
                        
                        // DHCPが有効な場合は緑色、無効な場合は青色
                        textColor = dhcpServerAddresses.Count > 0 ? Color.Green : Color.Blue;
                    }
                    catch
                    {
                        // エラーが発生した場合は通常の色
                        textColor = SystemColors.WindowText;
                    }
                }
            }

            // フォントスタイルを設定
            FontStyle fontStyle = FontStyle.Regular;
            try
            {
                var properties = adapter.GetIPProperties();
                var unicastAddresses = properties.UnicastAddresses
                    .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .ToList();

                // IPv4アドレスが設定されていて、DHCPでない場合は太字
                if (unicastAddresses.Count > 0)
                {
                    var dhcpServerAddresses = properties.DhcpServerAddresses;
                    if (dhcpServerAddresses.Count == 0)
                    {
                        fontStyle = FontStyle.Bold;
                    }
                }
            }
            catch
            {
                // エラーが発生した場合は通常のフォントスタイルを使用
            }

            using (var font = new Font(e.Font!, fontStyle))
            using (var brush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(
                    adapter.Name,
                    font,
                    brush,
                    e.Bounds);

                // テキストの幅を計算し、必要に応じてHorizontalExtentを更新
                if (sender is ListBox listBox)
                {
                    var textWidth = (int)e.Graphics.MeasureString(adapter.Name, font).Width + 20;
                    if (textWidth > listBox.HorizontalExtent)
                    {
                        listBox.HorizontalExtent = textWidth;
                    }
                }
            }

            e.DrawFocusRectangle();
        }

        private void PresetListBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || presetListBox?.Items[e.Index] is not NetworkPreset preset)
                return;

            e.DrawBackground();

            // 選択状態に応じて色を設定
            Color textColor;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                textColor = SystemColors.HighlightText;
            }
            else
            {
                // DHCPプリセットの場合は緑色、それ以外は通常の色
                textColor = preset.IP.ToLower() == "dhcp"
                    ? Color.Green
                    : SystemColors.WindowText;
            }

            // 表示テキストを生成
            string displayText = $"{preset.Name} ({preset.IP})";

            using (var brush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(
                    displayText,
                    e.Font!,
                    brush,
                    e.Bounds);

                // テキストの幅を計算し、必要に応じてHorizontalExtentを更新
                if (sender is ListBox listBox)
                {
                    var textWidth = (int)e.Graphics.MeasureString(displayText, e.Font!).Width + 20;
                    if (textWidth > listBox.HorizontalExtent)
                    {
                        listBox.HorizontalExtent = textWidth;
                    }
                }
            }

            e.DrawFocusRectangle();
        }
    }
} 