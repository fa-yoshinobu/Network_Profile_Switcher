using System;
using System.Windows.Forms;
using NetworkProfileSwitcher.Models;
using System.Drawing;
using System.Net.NetworkInformation;

namespace NetworkProfileSwitcher.Forms
{
    public partial class PresetEditorForm : Form
    {
        private NetworkPreset preset;
        private TextBox? nameTextBox;
        private TextBox? ipTextBox;
        private TextBox? subnetTextBox;
        private TextBox? gatewayTextBox;
        private TextBox? dns1TextBox;
        private TextBox? dns2TextBox;
        private TextBox? commentTextBox;
        private Button? okButton;
        private Button? cancelButton;
        private CheckBox? dhcpCheckBox;

        public PresetEditorForm(NetworkPreset? existingPreset = null)
        {
            InitializeComponent();
            preset = existingPreset ?? new NetworkPreset();
            LoadPresetData();
        }

        partial void InitializeComponent();

        private void LoadPresetData()
        {
            nameTextBox!.Text = preset.Name;
            commentTextBox!.Text = preset.Comment;

            // IP設定の読み込み
            if (preset.IP.ToLower() == "dhcp")
            {
                dhcpCheckBox!.Checked = true;
            }
            else
            {
                dhcpCheckBox!.Checked = false;
                ipTextBox!.Text = preset.IP;
                subnetTextBox!.Text = preset.Subnet;
                gatewayTextBox!.Text = preset.Gateway;
                dns1TextBox!.Text = preset.DNS1;
                dns2TextBox!.Text = preset.DNS2;
            }
        }

        private void DhcpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = !dhcpCheckBox!.Checked;
            ipTextBox!.Enabled = enabled;
            subnetTextBox!.Enabled = enabled;
            gatewayTextBox!.Enabled = enabled;
            dns1TextBox!.Enabled = enabled;
            dns2TextBox!.Enabled = enabled;

            if (dhcpCheckBox.Checked)
            {
                ipTextBox.Text = "";
                subnetTextBox.Text = "";
                gatewayTextBox.Text = "";
                dns1TextBox.Text = "";
                dns2TextBox.Text = "";
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox?.Text))
            {
                using (var errorForm = new ErrorDialogForm(
                    "エラー", 
                    "プリセット名を入力してください。",
                    "プリセット名は必須項目です。"))
                {
                    errorForm.ShowDialog();
                }
                nameTextBox!.Focus();
                return;
            }

            // プリセット名の前後の空白を削除
            preset.Name = nameTextBox!.Text.Trim();
            preset.Comment = commentTextBox?.Text?.Trim() ?? string.Empty;

            if (dhcpCheckBox?.Checked == true)
            {
                preset.IP = "dhcp";
                preset.Subnet = string.Empty;
                preset.Gateway = string.Empty;
                preset.DNS1 = string.Empty;
                preset.DNS2 = string.Empty;
            }
            else
            {
                // IPアドレスのバリデーション
                if (string.IsNullOrWhiteSpace(ipTextBox?.Text))
                {
                    using (var errorForm = new ErrorDialogForm(
                        "エラー", 
                        "IPアドレスを入力してください。",
                        "DHCPを使用しない場合は、IPアドレスを入力する必要があります。"))
                    {
                        errorForm.ShowDialog();
                    }
                    ipTextBox!.Focus();
                    return;
                }

                // IPアドレスの形式チェック
                if (!System.Net.IPAddress.TryParse(ipTextBox!.Text.Trim(), out _))
                {
                    using (var errorForm = new ErrorDialogForm(
                        "エラー", 
                        "IPアドレスの形式が正しくありません。",
                        $"入力された値: {ipTextBox.Text}\n" +
                        "正しい形式: 例) 192.168.1.1"))
                    {
                        errorForm.ShowDialog();
                    }
                    ipTextBox.Focus();
                    return;
                }

                // サブネットマスクの形式チェック
                if (!string.IsNullOrWhiteSpace(subnetTextBox?.Text) && 
                    !System.Net.IPAddress.TryParse(subnetTextBox.Text.Trim(), out _))
                {
                    using (var errorForm = new ErrorDialogForm(
                        "エラー", 
                        "サブネットマスクの形式が正しくありません。",
                        $"入力された値: {subnetTextBox.Text}\n" +
                        "正しい形式: 例) 255.255.255.0"))
                    {
                        errorForm.ShowDialog();
                    }
                    subnetTextBox.Focus();
                    return;
                }

                // デフォルトゲートウェイの形式チェック
                if (!string.IsNullOrWhiteSpace(gatewayTextBox?.Text) && 
                    !System.Net.IPAddress.TryParse(gatewayTextBox.Text.Trim(), out _))
                {
                    using (var errorForm = new ErrorDialogForm(
                        "エラー", 
                        "デフォルトゲートウェイの形式が正しくありません。",
                        $"入力された値: {gatewayTextBox.Text}\n" +
                        "正しい形式: 例) 192.168.1.1"))
                    {
                        errorForm.ShowDialog();
                    }
                    gatewayTextBox.Focus();
                    return;
                }

                // DNSサーバーの形式チェック
                if (!string.IsNullOrWhiteSpace(dns1TextBox?.Text) && 
                    !System.Net.IPAddress.TryParse(dns1TextBox.Text.Trim(), out _))
                {
                    using (var errorForm = new ErrorDialogForm(
                        "エラー", 
                        "DNSサーバー1の形式が正しくありません。",
                        $"入力された値: {dns1TextBox.Text}\n" +
                        "正しい形式: 例) 8.8.8.8"))
                    {
                        errorForm.ShowDialog();
                    }
                    dns1TextBox.Focus();
                    return;
                }

                if (!string.IsNullOrWhiteSpace(dns2TextBox?.Text) && 
                    !System.Net.IPAddress.TryParse(dns2TextBox.Text.Trim(), out _))
                {
                    using (var errorForm = new ErrorDialogForm(
                        "エラー", 
                        "DNSサーバー2の形式が正しくありません。",
                        $"入力された値: {dns2TextBox.Text}\n" +
                        "正しい形式: 例) 8.8.4.4"))
                    {
                        errorForm.ShowDialog();
                    }
                    dns2TextBox.Focus();
                    return;
                }

                preset.IP = ipTextBox.Text.Trim();
                preset.Subnet = string.IsNullOrWhiteSpace(subnetTextBox?.Text) ? "255.255.255.0" : subnetTextBox.Text.Trim();
                preset.Gateway = gatewayTextBox?.Text?.Trim() ?? string.Empty;
                preset.DNS1 = dns1TextBox?.Text?.Trim() ?? string.Empty;
                preset.DNS2 = dns2TextBox?.Text?.Trim() ?? string.Empty;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        public NetworkPreset GetPreset()
        {
            return preset;
        }
    }
} 