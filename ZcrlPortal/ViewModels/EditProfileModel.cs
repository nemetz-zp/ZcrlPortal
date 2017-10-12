using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZcrlPortal.ViewModels
{
    public class EditProfileModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public string Email { get; set; }
        public string Education { get; set; }

        //public string JobTitle
    }
}