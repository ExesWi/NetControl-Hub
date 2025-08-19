using System;

namespace NetControl_Hub.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public string DisplayName { get; set; }
        public DateTime LastLogin { get; set; }
        public int LoginCount { get; set; }
    }

    public enum UserRole
    {
        Developer,
        Director,
        Employee
    }
}
