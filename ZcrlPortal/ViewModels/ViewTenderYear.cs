using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZcrlPortal.ViewModels
{
    public class ViewTenderYear
    {
        private string name;

        public string Name
        {
            get
            {
                return name + " рік";
            }
            set
            {
                name = value;
            }
        }

        public int Value { get; set; }
    }
}