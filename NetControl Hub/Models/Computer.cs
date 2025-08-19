using System;

namespace NetControl_Hub.Models
{
    public class Computer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public DateTime LastSeen { get; set; }
        public bool IsOnline { get; set; }
        public bool RemoteAccessEnabled { get; set; }
        public string SharedFolders { get; set; } // JSON список разрешенных папок
        public DateTime AccessGrantedDate { get; set; }
        public string GrantedBy { get; set; }
    }
}
