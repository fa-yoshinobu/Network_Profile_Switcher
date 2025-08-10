using System;
using System.Windows.Forms;
using System.Security.Principal;
using System.Diagnostics;
using NetworkProfileSwitcher.Forms;
using System.IO;
using System.Text;

namespace NetworkProfileSwitcher
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // エンコーディングプロバイダーの登録（Shift_JISサポート用）
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // アプリケーションの実行パスを取得
                var baseDirectory = AppContext.BaseDirectory;
                var executablePath = Path.Combine(baseDirectory, "NetworkProfileSwitcher.exe");
                var workingDirectory = baseDirectory;

                // 管理者権限の確認
                if (!IsAdministrator())
                {
                    var result = MessageBox.Show(
                        "このアプリケーションは管理者権限が必要です。\n管理者として再起動しますか？",
                        "管理者権限が必要です",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        // 管理者として再起動
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = executablePath,
                            UseShellExecute = true,
                            Verb = "runas", // 管理者として実行
                            WindowStyle = ProcessWindowStyle.Normal,
                            WorkingDirectory = workingDirectory
                        };

                        try
                        {
                            var process = Process.Start(startInfo);
                            if (process != null)
                            {
                                process.WaitForExit(5000); // 5秒待機
                            }
                            return;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"管理者として再起動できませんでした。\n\nエラー: {ex.Message}\n\n" +
                                "以下の方法を試してください：\n" +
                                "1. アプリケーションを右クリックし、「管理者として実行」を選択\n" +
                                "2. セキュリティソフトの設定を確認\n" +
                                "3. ユーザーアカウント制御（UAC）の設定を確認",
                                "エラー",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                    return;
                }
                // 作業ディレクトリの設定
                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    Directory.SetCurrentDirectory(workingDirectory);
                }

                // アプリケーションの初期化
                ApplicationConfiguration.Initialize();
                
                // メインフォームの作成と実行
                using (var mainForm = new MainForm())
                {
                    Application.Run(mainForm);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"アプリケーションの起動に失敗しました。\n\nエラー: {ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static bool IsAdministrator()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"管理者権限の確認に失敗しました。\n\nエラー: {ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }
    }
} 