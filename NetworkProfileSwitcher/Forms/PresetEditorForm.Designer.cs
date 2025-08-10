using System;
using System.Windows.Forms;
using System.Drawing;

namespace NetworkProfileSwitcher.Forms
{
    partial class PresetEditorForm
    {
        partial void InitializeComponent()
        {
            this.SuspendLayout();

            // プリセット名
            var nameLabel = new Label
            {
                Text = "プリセット名:",
                Location = new Point(12, 15),
                Size = new Size(100, 20),
                Font = new Font("MS Gothic", 9)
            };

            nameTextBox = new TextBox
            {
                Location = new Point(120, 12),
                Size = new Size(250, 20),
                Font = new Font("MS Gothic", 9),
                MaxLength = 50
            };

            // DHCPチェックボックス
            dhcpCheckBox = new CheckBox
            {
                Text = "DHCPを使用する",
                Location = new Point(12, 45),
                Size = new Size(150, 20),
                Font = new Font("MS Gothic", 9)
            };
            dhcpCheckBox.CheckedChanged += DhcpCheckBox_CheckedChanged;

            // IPアドレス
            var ipLabel = new Label
            {
                Text = "IPアドレス:",
                Location = new Point(12, 75),
                Size = new Size(100, 20),
                Font = new Font("MS Gothic", 9)
            };

            ipTextBox = new TextBox
            {
                Location = new Point(120, 72),
                Size = new Size(250, 20),
                Font = new Font("MS Gothic", 9)
            };

            // サブネットマスク
            var subnetLabel = new Label
            {
                Text = "サブネットマスク:",
                Location = new Point(12, 105),
                Size = new Size(100, 20),
                Font = new Font("MS Gothic", 9)
            };

            subnetTextBox = new TextBox
            {
                Location = new Point(120, 102),
                Size = new Size(250, 20),
                Font = new Font("MS Gothic", 9)
            };

            // デフォルトゲートウェイ
            var gatewayLabel = new Label
            {
                Text = "デフォルトゲートウェイ:",
                Location = new Point(12, 135),
                Size = new Size(100, 20),
                Font = new Font("MS Gothic", 9)
            };

            gatewayTextBox = new TextBox
            {
                Location = new Point(120, 132),
                Size = new Size(250, 20),
                Font = new Font("MS Gothic", 9)
            };

            // DNSサーバー1
            var dns1Label = new Label
            {
                Text = "DNSサーバー1:",
                Location = new Point(12, 165),
                Size = new Size(100, 20),
                Font = new Font("MS Gothic", 9)
            };

            dns1TextBox = new TextBox
            {
                Location = new Point(120, 162),
                Size = new Size(250, 20),
                Font = new Font("MS Gothic", 9)
            };

            // DNSサーバー2
            var dns2Label = new Label
            {
                Text = "DNSサーバー2:",
                Location = new Point(12, 195),
                Size = new Size(100, 20),
                Font = new Font("MS Gothic", 9)
            };

            dns2TextBox = new TextBox
            {
                Location = new Point(120, 192),
                Size = new Size(250, 20),
                Font = new Font("MS Gothic", 9)
            };

            // コメント
            var commentLabel = new Label
            {
                Text = "コメント:",
                Location = new Point(12, 225),
                Size = new Size(100, 20),
                Font = new Font("MS Gothic", 9)
            };

            commentTextBox = new TextBox
            {
                Location = new Point(120, 222),
                Size = new Size(250, 60),
                Multiline = true,
                Font = new Font("MS Gothic", 9),
                MaxLength = 200
            };

            // ボタン
            okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(200, 300),
                Size = new Size(80, 30),
                Font = new Font("MS Gothic", 9)
            };
            okButton.Click += OkButton_Click;

            cancelButton = new Button
            {
                Text = "キャンセル",
                DialogResult = DialogResult.Cancel,
                Location = new Point(290, 300),
                Size = new Size(80, 30),
                Font = new Font("MS Gothic", 9)
            };

            // フォーム設定
            this.ClientSize = new Size(400, 350);
            this.Text = "プリセットの編集";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            this.Controls.AddRange(new Control[] {
                nameLabel, nameTextBox,
                dhcpCheckBox,
                ipLabel, ipTextBox,
                subnetLabel, subnetTextBox,
                gatewayLabel, gatewayTextBox,
                dns1Label, dns1TextBox,
                dns2Label, dns2TextBox,
                commentLabel, commentTextBox,
                okButton, cancelButton
            });

            this.ResumeLayout(false);
        }
    }
} 