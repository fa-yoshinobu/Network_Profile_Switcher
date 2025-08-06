using System;
using System.Windows.Forms;
using System.Security.Principal;
using System.Diagnostics;
using NetworkProfileSwitcher.Forms;
using System.IO;
using System.Reflection;
using System.Text;

namespace NetworkProfileSwitcher
{
    internal static class Program
    {
        private static readonly string LogFilePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Environment.CurrentDirectory,
            "debug.log");

        [STAThread]
        static void Main()
        {
            try
            {
                // エンコーディングプロバイダーの登録（Shift_JISサポート用）
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                
                // ログディレクトリの作成
                var logDirectory = Path.GetDirectoryName(LogFilePath);
                if (!string.IsNullOrEmpty(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // アプリケーションの実行パスを取得
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                var executablePath = Path.ChangeExtension(assemblyPath, ".exe");
                var workingDirectory = Path.GetDirectoryName(executablePath);

                LogMessage($"アセンブリパス: {assemblyPath}");
                LogMessage($"実行ファイルパス: {executablePath}");
                LogMessage($"作業ディレクトリ: {workingDirectory}");

                // 管理者権限の確認
                if (!IsAdministrator())
                {
                    LogMessage("管理者権限なしで起動");
                    var result = MessageBox.Show(
                        "このアプリケーションは管理者権限が必要です。\n管理者として再起動しますか？",
                        "管理者権限が必要です",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        LogMessage("管理者権限での再起動を試行");
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
                            LogMessage($"作業ディレクトリ: {startInfo.WorkingDirectory}");
                            LogMessage($"実行ファイル: {startInfo.FileName}");

                            var process = Process.Start(startInfo);
                            if (process != null)
                            {
                                LogMessage("管理者権限での再起動を開始");
                                process.WaitForExit(5000); // 5秒待機
                                if (!process.HasExited)
                                {
                                    LogMessage("新しいプロセスが正常に起動");
                                }
                                else
                                {
                                    LogMessage($"新しいプロセスが終了: 終了コード={process.ExitCode}");
                                }
                            }
                            else
                            {
                                LogMessage("新しいプロセスの起動に失敗");
                            }
                            return;
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"管理者権限での再起動に失敗: {ex.Message}");
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

                LogMessage("管理者権限で起動");
                // 作業ディレクトリの設定
                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    Directory.SetCurrentDirectory(workingDirectory);
                    LogMessage($"作業ディレクトリを設定: {workingDirectory}");
                }

                // アプリケーションの初期化
                ApplicationConfiguration.Initialize();
                
                LogMessage("メインフォームの作成を開始");
                // メインフォームの作成と実行
                using (var mainForm = new MainForm())
                {
                    LogMessage("メインフォームの実行を開始");
                    Application.Run(mainForm);
                    LogMessage("メインフォームの実行を終了");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"アプリケーションの起動に失敗: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show(
                    $"アプリケーションの起動に失敗しました。\n\nエラー: {ex.Message}\n\nスタックトレース:\n{ex.StackTrace}",
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
                    var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                    LogMessage($"管理者権限の確認: {isAdmin}");
                    return isAdmin;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"管理者権限の確認に失敗: {ex.Message}");
                MessageBox.Show(
                    $"管理者権限の確認に失敗しました。\n\nエラー: {ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private static void LogMessage(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] {message}{Environment.NewLine}";
                File.AppendAllText(LogFilePath, logEntry, Encoding.UTF8);
            }
            catch
            {
                // ログの書き込みに失敗しても無視
            }
        }
    }
} 