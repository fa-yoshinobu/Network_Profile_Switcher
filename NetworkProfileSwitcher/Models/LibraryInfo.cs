using System;
using System.Collections.Generic;

namespace NetworkProfileSwitcher.Models
{
    /// <summary>
    /// ライブラリ情報を管理するクラス
    /// </summary>
    public class LibraryInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public LibraryInfo(string name, string version, string license, string description = "", string url = "")
        {
            Name = name;
            Version = version;
            License = license;
            Description = description;
            Url = url;
        }
    }

    /// <summary>
    /// ライブラリ情報の管理クラス
    /// </summary>
    public static class LibraryManager
    {
        private static readonly List<LibraryInfo> _libraries = new()
        {
            new LibraryInfo(
                "System.Text.Json",
                "8.0.5",
                "MIT License",
                "JSONのシリアライゼーションとデシリアライゼーションを提供するライブラリ",
                "https://github.com/dotnet/runtime"
            ),
            new LibraryInfo(
                "System.Management",
                "8.0.0",
                "MIT License",
                "WMI（Windows Management Instrumentation）へのアクセスを提供するライブラリ",
                "https://github.com/dotnet/runtime"
            ),
            new LibraryInfo(
                "System.Text.Encoding.CodePages",
                "8.0.0",
                "MIT License",
                "追加の文字エンコーディングを提供するライブラリ",
                "https://github.com/dotnet/runtime"
            ),
            new LibraryInfo(
                ".NET 6.0",
                "6.0.0",
                "MIT License",
                "Microsoft .NET Framework",
                "https://github.com/dotnet/core"
            ),
            new LibraryInfo(
                "Windows Forms",
                "6.0.0",
                "MIT License",
                "Windowsデスクトップアプリケーション用のUIフレームワーク",
                "https://github.com/dotnet/winforms"
            )
        };

        /// <summary>
        /// すべてのライブラリ情報を取得
        /// </summary>
        public static List<LibraryInfo> GetAllLibraries()
        {
            return new List<LibraryInfo>(_libraries);
        }

        /// <summary>
        /// アプリケーションのバージョン情報を取得
        /// </summary>
        public static string GetApplicationVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version?.ToString() ?? "1.0.2.0";
        }

        /// <summary>
        /// アプリケーション名を取得
        /// </summary>
        public static string GetApplicationName()
        {
            return "Network Profile Switcher";
        }
    }
}
