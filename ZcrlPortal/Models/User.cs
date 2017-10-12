using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZcrlPortal.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public virtual Role UserRole { get; set; }
        public int RoleId { get; set; }
    }
}