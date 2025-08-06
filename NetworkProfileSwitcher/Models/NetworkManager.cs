using System;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Linq;
using System.Reflection;

namespace NetworkProfileSwitcher.Models
{
    public static class NetworkManager
    {
        private static readonly string LogFilePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Environment.CurrentDirectory,
            "debug.log");

        private static void LogMessage(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [NetworkManager] {message}{Environment.NewLine}";
                File.AppendAllText(LogFilePath, logEntry, Encoding.UTF8);
            }
            catch
            {
                // ログの書き込みに失敗しても無視
            }
        }

        public static void ApplyPreset(NetworkInterface adapter, NetworkPreset preset)
        {
            LogMessage($"プリセット適用開始: アダプタ={adapter.Name}, プリセット={preset.Name}, IP={preset.IP}");
            
            // 管理者権限の確認
            if (!IsAdministrator())
            {
                var errorMsg = "ネットワーク設定の適用には管理者権限が必要です。\nアプリケーションを管理者として実行してください。";
                LogMessage($"エラー: {errorMsg}");
                throw new Exception(errorMsg);
            }

            // アダプタの状態確認と処理
            if (adapter.OperationalStatus != OperationalStatus.Up)
            {
                LogMessage($"アダプタ状態確認: {adapter.Name} = {adapter.OperationalStatus}");
                
                // アダプタがDown状態の場合、有効化を試行
                if (adapter.OperationalStatus == OperationalStatus.Down)
                {
                    LogMessage($"アダプタ有効化を試行: {adapter.Name}");
                    try
                    {
                        EnableAdapter(adapter);
                        // 有効化後、少し待機してから状態を再確認
                        System.Threading.Thread.Sleep(2000);
                        
                        // アダプタ情報を再取得
                        var refreshedAdapter = GetRefreshedAdapter(adapter.Name);
                        if (refreshedAdapter != null && refreshedAdapter.OperationalStatus == OperationalStatus.Up)
                        {
                            adapter = refreshedAdapter;
                            LogMessage($"アダプタ有効化成功: {adapter.Name}");
                        }
                        else
                        {
                            var errorMsg = $"アダプタ '{adapter.Name}' の有効化に失敗しました。\n現在の状態: {adapter.OperationalStatus}\n\n考えられる原因:\n- LANケーブルが接続されていない\n- ネットワークアダプタのドライバーに問題がある\n- 物理的な接続に問題がある\n\n対処方法:\n1. LANケーブルが正しく接続されているか確認\n2. ネットワークアダプタのドライバーを更新\n3. デバイスマネージャーでアダプタの状態を確認";
                            LogMessage($"エラー: {errorMsg}");
                            throw new Exception(errorMsg);
                        }
                    }
                    catch (Exception enableException)
                    {
                        var errorMsg = $"アダプタ '{adapter.Name}' の有効化に失敗しました。\n現在の状態: {adapter.OperationalStatus}\n\n有効化エラー: {enableException.Message}\n\n考えられる原因:\n- LANケーブルが接続されていない\n- ネットワークアダプタのドライバーに問題がある\n- 物理的な接続に問題がある\n\n対処方法:\n1. LANケーブルが正しく接続されているか確認\n2. ネットワークアダプタのドライバーを更新\n3. デバイスマネージャーでアダプタの状態を確認";
                        LogMessage($"エラー: {errorMsg}");
                        throw new Exception(errorMsg);
                    }
                }
                else
                {
                    var errorMsg = $"アダプタ '{adapter.Name}' が有効になっていません。\n現在の状態: {adapter.OperationalStatus}\nアダプタを有効にしてから再試行してください。";
                    LogMessage($"エラー: {errorMsg}");
                    throw new Exception(errorMsg);
                }
            }

            if (preset.IP.ToLower() == "dhcp")
            {
                LogMessage($"DHCP設定を適用: {adapter.Name}");
                // DHCP設定の適用（netshを試行、失敗した場合はWMIを使用）
                try
                {
                    ApplyDhcpWithNetsh(adapter);
                    LogMessage($"DHCP設定成功 (netsh): {adapter.Name}");
                }
                catch (Exception netshException)
                {
                    LogMessage($"netsh DHCP設定失敗: {adapter.Name}, エラー: {netshException.Message}");
                    // netshが失敗した場合、WMIを使用してDHCP設定を適用
                    try
                    {
                        ApplyDhcpWithWmi(adapter);
                        LogMessage($"DHCP設定成功 (WMI): {adapter.Name}");
                    }
                    catch (Exception wmiException)
                    {
                        // DHCPが既に有効な場合は成功として扱う
                        if (netshException.Message.Contains("DHCP is already enabled"))
                        {
                            LogMessage($"DHCP既に有効: {adapter.Name}");
                            return; // 成功として扱う
                        }
                        
                        var errorMsg = $"DHCP設定の適用に失敗しました。\nアダプタ: {adapter.Name}\nアダプタ状態: {adapter.OperationalStatus}\nアダプタ種類: {adapter.NetworkInterfaceType}\n\nnetshエラー:\n{netshException.Message}\n\nWMIエラー:\n{wmiException.Message}";
                        LogMessage($"エラー: {errorMsg}");
                        throw new Exception(errorMsg);
                    }
                }
            }
            else
            {
                LogMessage($"静的IP設定を適用: {adapter.Name}, IP={preset.IP}, Subnet={preset.Subnet}, Gateway={preset.Gateway}");
                // 静的IP設定の適用
                var result = RunNetshCommand($"interface ip set address \"{adapter.Name}\" static {preset.IP} {preset.Subnet} {preset.Gateway}");
                if (result != 0)
                {
                    var detailedError = GetDetailedNetshError($"interface ip set address \"{adapter.Name}\" static {preset.IP} {preset.Subnet} {preset.Gateway}");
                    var errorMsg = $"IPアドレスの設定に失敗しました。\nアダプタ: {adapter.Name}\nアダプタ状態: {adapter.OperationalStatus}\nアダプタ種類: {adapter.NetworkInterfaceType}\n\nエラー詳細:\n{detailedError}";
                    LogMessage($"エラー: {errorMsg}");
                    throw new Exception(errorMsg);
                }
                LogMessage($"IPアドレス設定成功: {adapter.Name}, IP={preset.IP}");

                // DNS設定
                if (!string.IsNullOrEmpty(preset.DNS1))
                {
                    LogMessage($"DNS1設定を適用: {adapter.Name}, DNS1={preset.DNS1}");
                    result = RunNetshCommand($"interface ip set dns \"{adapter.Name}\" static {preset.DNS1}");
                    if (result != 0)
                    {
                        var detailedError = GetDetailedNetshError($"interface ip set dns \"{adapter.Name}\" static {preset.DNS1}");
                        var errorMsg = $"DNSサーバー1の設定に失敗しました。\nアダプタ: {adapter.Name}\nアダプタ状態: {adapter.OperationalStatus}\nアダプタ種類: {adapter.NetworkInterfaceType}\n\nエラー詳細:\n{detailedError}";
                        LogMessage($"エラー: {errorMsg}");
                        throw new Exception(errorMsg);
                    }
                    LogMessage($"DNS1設定成功: {adapter.Name}, DNS1={preset.DNS1}");
                }

                if (!string.IsNullOrEmpty(preset.DNS2))
                {
                    LogMessage($"DNS2設定を適用: {adapter.Name}, DNS2={preset.DNS2}");
                    result = RunNetshCommand($"interface ip add dns \"{adapter.Name}\" {preset.DNS2} index=2");
                    if (result != 0)
                    {
                        var detailedError = GetDetailedNetshError($"interface ip add dns \"{adapter.Name}\" {preset.DNS2} index=2");
                        var errorMsg = $"DNSサーバー2の設定に失敗しました。\nアダプタ: {adapter.Name}\nアダプタ状態: {adapter.OperationalStatus}\nアダプタ種類: {adapter.NetworkInterfaceType}\n\nエラー詳細:\n{detailedError}";
                        LogMessage($"エラー: {errorMsg}");
                        throw new Exception(errorMsg);
                    }
                    LogMessage($"DNS2設定成功: {adapter.Name}, DNS2={preset.DNS2}");
                }
            }
            
            LogMessage($"プリセット適用完了: {adapter.Name}, プリセット={preset.Name}");
        }

        private static void EnableAdapter(NetworkInterface adapter)
        {
            var result = RunNetshCommand($"interface set interface \"{adapter.Name}\" admin=enable");
            if (result != 0)
            {
                var detailedError = GetDetailedNetshError($"interface set interface \"{adapter.Name}\" admin=enable");
                throw new Exception($"有効化エラー: {detailedError}");
            }
        }

        private static NetworkInterface? GetRefreshedAdapter(string adapterName)
        {
            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces();
                return adapters.FirstOrDefault(a => a.Name == adapterName);
            }
            catch
            {
                return null;
            }
        }

        public static string GetAdapterDetailedInfo(NetworkInterface adapter)
        {
            var info = new StringBuilder();
            info.AppendLine($"アダプタ名: {adapter.Name}");
            info.AppendLine($"説明: {adapter.Description}");
            info.AppendLine($"状態: {adapter.OperationalStatus}");
            info.AppendLine($"種類: {adapter.NetworkInterfaceType}");
            info.AppendLine($"速度: {GetSpeedString(adapter.Speed)}");
            info.AppendLine($"MACアドレス: {GetMacAddress(adapter)}");
            
            // 物理的な接続状態の確認
            if (adapter.OperationalStatus == OperationalStatus.Down)
            {
                info.AppendLine("\n⚠️ 物理的な接続の問題が考えられます:");
                info.AppendLine("- LANケーブルが接続されていない可能性");
                info.AppendLine("- ネットワークアダプタのドライバーに問題がある可能性");
                info.AppendLine("- 物理的な接続に問題がある可能性");
            }

            return info.ToString();
        }

        private static string GetSpeedString(long speed)
        {
            if (speed == -1) return "不明";
            if (speed >= 1000000000) return $"{speed / 1000000000} Gbps";
            if (speed >= 1000000) return $"{speed / 1000000} Mbps";
            if (speed >= 1000) return $"{speed / 1000} Kbps";
            return $"{speed} bps";
        }

        private static string GetMacAddress(NetworkInterface adapter)
        {
            try
            {
                return BitConverter.ToString(adapter.GetPhysicalAddress().GetAddressBytes()).Replace("-", ":");
            }
            catch
            {
                return "取得失敗";
            }
        }

        private static void ApplyDhcpWithNetsh(NetworkInterface adapter)
        {
            // DHCP設定の適用
            var result = RunNetshCommand($"interface ip set address \"{adapter.Name}\" dhcp");
            if (result != 0)
            {
                // 終了コードが0でない場合でも、DHCPが既に有効な場合は成功として扱う
                var detailedError = GetDetailedNetshError($"interface ip set address \"{adapter.Name}\" dhcp");
                if (detailedError.Contains("DHCP is already enabled"))
                {
                    // DHCPが既に有効な場合は成功として扱う
                    return;
                }
                throw new Exception($"DHCP設定の適用に失敗しました。\nアダプタ: {adapter.Name}\nアダプタ状態: {adapter.OperationalStatus}\nアダプタ種類: {adapter.NetworkInterfaceType}\n\nエラー詳細:\n{detailedError}");
            }

            // DNS設定もDHCPに設定
            result = RunNetshCommand($"interface ip set dns \"{adapter.Name}\" dhcp");
            if (result != 0)
            {
                var detailedError = GetDetailedNetshError($"interface ip set dns \"{adapter.Name}\" dhcp");
                if (detailedError.Contains("DHCP is already enabled"))
                {
                    // DHCPが既に有効な場合は成功として扱う
                    return;
                }
                throw new Exception($"DNSのDHCP設定の適用に失敗しました。\nアダプタ: {adapter.Name}\nアダプタ状態: {adapter.OperationalStatus}\nアダプタ種類: {adapter.NetworkInterfaceType}\n\nエラー詳細:\n{detailedError}");
            }
        }

        private static void ApplyDhcpWithWmi(NetworkInterface adapter)
        {
            try
            {
                // System.Managementアセンブリの存在確認
                var managementAssembly = typeof(System.Management.ManagementScope).Assembly;
                if (managementAssembly == null)
                {
                    throw new Exception("System.Managementアセンブリが見つかりません。.NET 6.0 Runtimeが正しくインストールされているか確認してください。");
                }

                // WMIを使用してDHCP設定を適用
                var scope = new ManagementScope("root\\cimv2");
                scope.Connect();

                // ネットワークアダプタ設定を検索
                var query = new SelectQuery($"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Description = '{adapter.Description}'");
                using (var searcher = new ManagementObjectSearcher(scope, query))
                {
                    var configurations = searcher.Get();
                    if (configurations.Count == 0)
                    {
                        throw new Exception($"WMIでアダプタ設定 '{adapter.Description}' が見つかりませんでした。");
                    }

                    foreach (ManagementObject configObj in configurations)
                    {
                        // DHCPを有効にする
                        var inParams = configObj.GetMethodParameters("EnableDHCP");
                        var outParams = configObj.InvokeMethod("EnableDHCP", inParams, null);

                        if (outParams != null && outParams["ReturnValue"] != null)
                        {
                            var returnValue = Convert.ToInt32(outParams["ReturnValue"]);
                            if (returnValue != 0)
                            {
                                throw new Exception($"WMIでDHCP設定の適用に失敗しました。戻り値: {returnValue}");
                            }
                        }
                    }
                }
            }
            catch (System.Management.ManagementException ex)
            {
                throw new Exception($"WMIを使用したDHCP設定の適用に失敗しました: {ex.Message}");
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                throw new Exception($"System.Managementアセンブリの読み込みに失敗しました。.NET 6.0 Runtimeが正しくインストールされているか確認してください。\n\nエラー詳細: {ex.Message}");
            }
            catch (System.IO.FileNotFoundException ex)
            {
                throw new Exception($"System.Managementアセンブリが見つかりません。.NET 6.0 Runtimeを再インストールしてください。\n\nエラー詳細: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"WMIを使用したDHCP設定の適用に失敗しました: {ex.Message}");
            }
        }

        private static bool IsAdministrator()
        {
            using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
            {
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
        }

        private static int RunNetshCommand(string command)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, // UTF-8で読み取り
                StandardErrorEncoding = Encoding.UTF8   // UTF-8で読み取り
            };

            try
            {
                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        throw new Exception("プロセスの起動に失敗しました。");
                    }

                    process.WaitForExit();

                    var error = DecodeOutput(process.StandardError.ReadToEnd());
                    var output = DecodeOutput(process.StandardOutput.ReadToEnd());

                    // DHCPが既に有効な場合は成功として扱う
                    if (process.ExitCode != 0)
                    {
                        if (output.Contains("DHCP is already enabled") || error.Contains("DHCP is already enabled"))
                        {
                            return 0; // 成功として扱う
                        }
                        
                        throw new Exception($"コマンドの実行に失敗しました: {command}\n終了コード: {process.ExitCode}\n\n標準エラー出力:\n{error}\n\n標準出力:\n{output}");
                    }

                    return process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"コマンドの実行に失敗しました: {command}\n\nエラー詳細: {ex.Message}");
            }
        }

        private static string DecodeOutput(string output)
        {
            try
            {
                // 文字化け対策: 複数のエンコーディングを試行
                if (string.IsNullOrEmpty(output))
                    return output;

                // 元のバイトデータを取得
                byte[] originalBytes;
                try
                {
                    originalBytes = Encoding.Default.GetBytes(output);
                }
                catch
                {
                    return output;
                }

                // 複数のエンコーディングを試行
                var encodings = new[]
                {
                    Encoding.GetEncoding("Shift_JIS"),
                    Encoding.UTF8,
                    Encoding.GetEncoding("CP932"),
                    Encoding.GetEncoding("GBK"),
                    Encoding.GetEncoding("Big5")
                };

                foreach (var encoding in encodings)
                {
                    try
                    {
                        var decoded = encoding.GetString(originalBytes);
                        // 文字化けしていないかチェック（制御文字以外の文字が含まれているか）
                        if (!string.IsNullOrEmpty(decoded) && decoded.Any(c => !char.IsControl(c) || c == '\n' || c == '\r' || c == '\t'))
                        {
                            return decoded;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                // すべてのエンコーディングが失敗した場合、元の文字列を返す
                return output;
            }
            catch
            {
                return output;
            }
        }

        private static string GetDetailedNetshError(string command)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = command,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8, // UTF-8で読み取り
                    StandardErrorEncoding = Encoding.UTF8   // UTF-8で読み取り
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        return "プロセスの起動に失敗しました。";
                    }

                    process.WaitForExit();

                    var error = DecodeOutput(process.StandardOutput.ReadToEnd());
                    var output = DecodeOutput(process.StandardError.ReadToEnd());

                    var result = $"終了コード: {process.ExitCode}\n";
                    if (!string.IsNullOrEmpty(error))
                    {
                        result += $"標準エラー出力:\n{error}\n";
                    }
                    if (!string.IsNullOrEmpty(output))
                    {
                        result += $"標準出力:\n{output}\n";
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                return $"エラー情報の取得に失敗: {ex.Message}";
            }
        }
    }
} 