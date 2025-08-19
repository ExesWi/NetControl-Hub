using System;

namespace NetControl_Hub.Models
{
    public class ProgramInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string InstallPath { get; set; }
        public string DesktopShortcutPath { get; set; }
        public DateTime InstallDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string Version { get; set; }
        public bool IsLatestVersion { get; set; }
        public string InstalledBy { get; set; }
        public ProgramStatus Status { get; set; }
    }

    public enum ProgramStatus
    {
        Installed,
        Outdated,
        NotInstalled
    }
}
