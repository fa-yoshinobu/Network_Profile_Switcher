using System;

namespace NetworkProfileSwitcher.Models
{
    public class NetworkPreset
    {
        public string Name { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public string Subnet { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public string DNS1 { get; set; } = string.Empty;
        public string DNS2 { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return "(無名のプリセット)";
            
            if (IP.ToLower() == "dhcp")
                return $"{Name} (DHCP)";
            return $"{Name} ({IP})";
        }
    }
} 