using System;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.IO;
using System.Management;

namespace NetworkProfileSwitcher.Models
{
    public static class NetworkManager
    {
        public static void ApplyPreset(NetworkInterface adapter, NetworkPreset preset)
        {
            // 管理者権限の確認
            if (!IsAdministrator())
            {
                throw new Exception("ネットワーク設定の適用には管理者権限が必要です。\nアプリケーションを管理者として実行してください。");
            }

            // アダプタの状態確認
            if (adapter.OperationalStatus != OperationalStatus.Up)
            {
                throw new Exception($"アダプタ '{adapter.Name}' が有効になっていません。\n現在の状態: {adapter.OperationalStatus}\nアダプタを有効にしてから再試行してください。");
            }

            if (preset.IP.ToLower() == "dhcp")
            {
                // DHCP設定の適用（netshを試行、失敗した場合はWMIを使用）
                try
                {
                    ApplyDhcpWithNetsh(adapter);
                }
                catch (Exception netshException)
                {
                    // netshが失敗した場合、WMIを使用してDHCP設定を適用
                    try
                    {
                        ApplyDhcpWithWmi(adapter);
                    }
                    catch (Exception wmiException)
                    {
                        throw new Exception($"DHCP設定の適用に失敗しました。\nアダプタ: {adapter.Name}\nアダプタ状態: {adapter.OperationalStatus}\nアダプタ種類: {adapter.NetworkInterfaceType}\n\nnetshエラー:\n{netshException.Message}\n\nWMIエラー:\n{wmiException.Message}");
                    }
                }
            }
            else
            {
                // 静的IP設定の適用
                var result = RunNetshCommand($"interface ip set address \"{adapter.Name}\" static {preset.IP} {preset.Subnet} {preset.Gateway}");
                if (result != 0)
                {
                    var detailedError = GetDetailedNetshError($"interface ip set address \"{adapter.Name}\" static {preset.IP} {preset.Subnet} {preset.Gateway}");
                    throw new Exception($"IPアドレスの設定に失敗しました。\nアダプタ: {adapter.Name}\nアダプタ状態: {adapter.OperationalStatus}\nアダプタ種類: {adapter.NetworkInterfaceType}\n\nエラー詳細:\n{detailedError}");
                }

                // DNS設定
                if (!string.IsNullOrEmpty(preset.DNS1))
                {
                    result = RunNetshCommand($"interface ip set dns \"{adapter.Name}\" static {preset.DNS1}");
                    if (result != 0)
                    {
                        var detailedError = GetDetailedNetshError($"interface ip set dns \"{adapter.Name}\" static {preset.DNS1}");
                        throw new Exception($"DNSサーバー1の設定に失敗しました。\nアダプタ: {adapter.Name}\nアダプタ状態: {adapter.OperationalStatus}\nアダプタ種類: {adapter.NetworkInterfaceType}\n\nエラー詳細:\n{detailedError}");
                    }

                    if (!string.IsNullOrEmpty(preset.DNS2))
                    {
                        result = RunNetshCommand($"interface ip add dns \"{adapter.Name}\" {preset.DNS2} index=2");
                        if (result != 0)
                        {
                            var detailedError = GetDetailedNetshError($"interface ip add dns \"{adapter.Name}\" {preset.DNS2} index=2");
                            throw new Exception($"DNSサーバー2の設定に失敗しました。\nアダプタ: {adapter.Name}\nアダプタ状態: {adapter.OperationalStatus}\nアダプタ種類: {adapter.NetworkInterfaceType}\n\nエラー詳細:\n{detailedError}");
                        }
                    }
                }
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
                RedirectStandardError = true
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

                    if (process.ExitCode != 0)
                    {
                        var error = process.StandardError.ReadToEnd();
                        var output = process.StandardOutput.ReadToEnd();
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
                    RedirectStandardError = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        return "プロセスの起動に失敗しました。";
                    }

                    process.WaitForExit();

                    var error = process.StandardError.ReadToEnd();
                    var output = process.StandardOutput.ReadToEnd();

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