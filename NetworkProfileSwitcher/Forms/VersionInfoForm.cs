using System;
using System.Drawing;
using System.Windows.Forms;
using NetworkProfileSwitcher.Models;

namespace NetworkProfileSwitcher.Forms
{
    /// <summary>
    /// バージョン情報表示フォーム
    /// </summary>
    public partial class VersionInfoForm : Form
    {
        private ListView? libraryListView;
        private Label? appInfoLabel;
        private Button? closeButton;
        private Button? licenseButton;

        public VersionInfoForm()
        {
            InitializeComponent();
            LoadVersionInfo();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // アプリケーション情報ラベル
            appInfoLabel = new Label
            {
                Text = $"{LibraryManager.GetApplicationName()} v{LibraryManager.GetApplicationVersion()}",
                Location = new Point(12, 12),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };

            // ライブラリ一覧
            libraryListView = new ListView
            {
                Location = new Point(12, 50),
                Size = new Size(560, 300),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            // カラムヘッダーの設定
            libraryListView.Columns.Add("ライブラリ名", 200);
            libraryListView.Columns.Add("バージョン", 100);
            libraryListView.Columns.Add("ライセンス", 150);
            libraryListView.Columns.Add("説明", 200);

            // ライセンスボタン
            licenseButton = new Button
            {
                Text = "ライセンス情報を表示",
                Location = new Point(12, 360),
                Size = new Size(150, 30)
            };
            licenseButton.Click += LicenseButton_Click;

            // 閉じるボタン
            closeButton = new Button
            {
                Text = "閉じる",
                Location = new Point(450, 360),
                Size = new Size(100, 30),
                DialogResult = DialogResult.OK
            };

            // フォーム設定
            this.Text = "バージョン情報";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AcceptButton = closeButton;
            this.CancelButton = closeButton;

            this.Controls.AddRange(new Control[] {
                appInfoLabel,
                libraryListView,
                licenseButton,
                closeButton
            });

            this.ResumeLayout(false);
        }

        private void LoadVersionInfo()
        {
            if (libraryListView == null) return;

            libraryListView.Items.Clear();
            var libraries = LibraryManager.GetAllLibraries();

            foreach (var library in libraries)
            {
                var item = new ListViewItem(library.Name);
                item.SubItems.Add(library.Version);
                item.SubItems.Add(library.License);
                item.SubItems.Add(library.Description);
                item.Tag = library; // ライブラリ情報をタグに保存

                libraryListView.Items.Add(item);
            }
        }

        private void LicenseButton_Click(object? sender, EventArgs e)
        {
            var licenseForm = new LicenseInfoForm();
            licenseForm.ShowDialog(this);
        }
    }
}
