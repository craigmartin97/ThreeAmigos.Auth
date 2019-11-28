using System;

namespace ThAmCo.Auth.Models
{
    public class UserSummaryGetDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}
