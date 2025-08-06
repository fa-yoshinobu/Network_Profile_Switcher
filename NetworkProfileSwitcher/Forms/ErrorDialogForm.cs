using System;
using System.Windows.Forms;
using System.Drawing;

namespace NetworkProfileSwitcher.Forms
{
    public partial class ErrorDialogForm : Form
    {
        private TextBox? messageTextBox;
        private Button? okButton;
        private Button? detailsButton;
        private TextBox? detailsTextBox;
        private bool isDetailsVisible = false;

        public ErrorDialogForm(string title, string message, string? details = null)
        {
            InitializeComponent();
            this.Text = title;
            this.messageTextBox!.Text = message;
            
            if (!string.IsNullOrEmpty(details))
            {
                this.detailsTextBox!.Text = details;
                this.detailsButton!.Visible = true;
            }
            else
            {
                this.detailsButton!.Visible = false;
            }
        }

        private void InitializeComponent()
        {
            this.messageTextBox = new TextBox();
            this.okButton = new Button();
            this.detailsButton = new Button();
            this.detailsTextBox = new TextBox();

            // メッセージテキストボックス
            this.messageTextBox.Multiline = true;
            this.messageTextBox.ReadOnly = true;
            this.messageTextBox.Location = new Point(12, 12);
            this.messageTextBox.Size = new Size(360, 100);
            this.messageTextBox.BorderStyle = BorderStyle.FixedSingle;
            this.messageTextBox.BackColor = Color.White;
            this.messageTextBox.ScrollBars = ScrollBars.Vertical;

            // 詳細ボタン
            this.detailsButton.Text = "詳細";
            this.detailsButton.Location = new Point(216, 124);
            this.detailsButton.Size = new Size(75, 30);
            this.detailsButton.Click += DetailsButton_Click;

            // OKボタン
            this.okButton.Text = "OK";
            this.okButton.DialogResult = DialogResult.OK;
            this.okButton.Location = new Point(297, 124);
            this.okButton.Size = new Size(75, 30);
            this.okButton.Click += (s, e) => this.Close();

            // 詳細テキストボックス
            this.detailsTextBox.Multiline = true;
            this.detailsTextBox.ReadOnly = true;
            this.detailsTextBox.Location = new Point(12, 166);
            this.detailsTextBox.Size = new Size(360, 100);
            this.detailsTextBox.BorderStyle = BorderStyle.FixedSingle;
            this.detailsTextBox.BackColor = Color.White;
            this.detailsTextBox.ScrollBars = ScrollBars.Vertical;
            this.detailsTextBox.Visible = false;

            // フォーム設定
            this.ClientSize = new Size(384, 166);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Controls.AddRange(new Control[] { 
                this.messageTextBox, 
                this.detailsButton, 
                this.okButton, 
                this.detailsTextBox 
            });
        }

        private void DetailsButton_Click(object? sender, EventArgs e)
        {
            isDetailsVisible = !isDetailsVisible;
            this.detailsTextBox!.Visible = isDetailsVisible;
            this.ClientSize = new Size(384, isDetailsVisible ? 278 : 166);
            this.detailsButton!.Text = isDetailsVisible ? "詳細を隠す" : "詳細";
        }
    }
} 