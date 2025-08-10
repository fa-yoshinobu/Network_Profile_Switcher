using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using NetworkProfileSwitcher.Models;

namespace NetworkProfileSwitcher.Forms
{
    /// <summary>
    /// ライセンス情報表示フォーム
    /// </summary>
    public partial class LicenseInfoForm : Form
    {
        private ListView? libraryListView;
        private TextBox? licenseTextBox;
        private Button? closeButton;
        private Button? openUrlButton;
        private SplitContainer? splitContainer;

        public LicenseInfoForm()
        {
            InitializeComponent();
            LoadLicenseInfo();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // スプリットコンテナ
            splitContainer = new SplitContainer
            {
                Location = new Point(12, 12),
                Size = new Size(760, 400),
                Orientation = Orientation.Vertical,
                SplitterDistance = 300
            };

            // ライブラリ一覧（左側）
            libraryListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Dock = DockStyle.Fill
            };

            libraryListView.Columns.Add("ライブラリ名", 150);
            libraryListView.Columns.Add("バージョン", 80);
            libraryListView.Columns.Add("ライセンス", 70);

            libraryListView.SelectedIndexChanged += LibraryListView_SelectedIndexChanged;

            // ライセンス詳細（右側）
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            licenseTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9),
                Dock = DockStyle.Fill
            };

            // ボタンパネル
            var buttonPanel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Bottom
            };

            openUrlButton = new Button
            {
                Text = "URLを開く",
                Location = new Point(10, 8),
                Size = new Size(100, 25),
                Enabled = false
            };
            openUrlButton.Click += OpenUrlButton_Click;

            closeButton = new Button
            {
                Text = "閉じる",
                Location = new Point(120, 8),
                Size = new Size(100, 25),
                DialogResult = DialogResult.OK
            };

            buttonPanel.Controls.AddRange(new Control[] { openUrlButton, closeButton });

            rightPanel.Controls.Add(licenseTextBox);
            rightPanel.Controls.Add(buttonPanel);

            splitContainer.Panel1.Controls.Add(libraryListView);
            splitContainer.Panel2.Controls.Add(rightPanel);

            // フォーム設定
            this.Text = "ライセンス情報";
            this.Size = new Size(800, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AcceptButton = closeButton;
            this.CancelButton = closeButton;

            this.Controls.Add(splitContainer);

            this.ResumeLayout(false);
        }

        private void LoadLicenseInfo()
        {
            if (libraryListView == null) return;

            libraryListView.Items.Clear();
            var libraries = LibraryManager.GetAllLibraries();

            foreach (var library in libraries)
            {
                var item = new ListViewItem(library.Name);
                item.SubItems.Add(library.Version);
                item.SubItems.Add(library.License);
                item.Tag = library;

                libraryListView.Items.Add(item);
            }

            // 最初のアイテムを選択
            if (libraryListView.Items.Count > 0)
            {
                libraryListView.Items[0].Selected = true;
            }
        }

        private void LibraryListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (libraryListView?.SelectedItems.Count == 0) return;

            var selectedItem = libraryListView.SelectedItems[0];
            var library = selectedItem.Tag as LibraryInfo;

            if (library is not null)
            {
                DisplayLicenseInfo(library);
            }
        }

        private void DisplayLicenseInfo(LibraryInfo library)
        {
            if (licenseTextBox == null || openUrlButton == null) return;

            var licenseText = GetLicenseText(library);
            licenseTextBox.Text = licenseText;

            // URLボタンの有効化
            openUrlButton.Enabled = !string.IsNullOrEmpty(library.Url);
            openUrlButton.Tag = library.Url;
        }

        private string GetLicenseText(LibraryInfo library)
        {
            return library.Name switch
            {
                "System.Text.Json" => GetMITLicenseText("System.Text.Json", "Microsoft Corporation"),
                "System.Management" => GetMITLicenseText("System.Management", "Microsoft Corporation"),
                "System.Text.Encoding.CodePages" => GetMITLicenseText("System.Text.Encoding.CodePages", "Microsoft Corporation"),
                ".NET 6.0" => GetMITLicenseText(".NET 6.0", "Microsoft Corporation"),
                "Windows Forms" => GetMITLicenseText("Windows Forms", "Microsoft Corporation"),
                _ => $"ライセンス情報が見つかりません: {library.Name}"
            };
        }

        private string GetMITLicenseText(string libraryName, string copyright)
        {
            return $@"{libraryName} - MIT License

Copyright (c) {DateTime.Now.Year} {copyright}

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.";
        }

        private void OpenUrlButton_Click(object? sender, EventArgs e)
        {
            if (openUrlButton?.Tag is string url && !string.IsNullOrEmpty(url))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"URLを開けませんでした: {ex.Message}",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}
