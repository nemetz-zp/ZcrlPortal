using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZcrlPortal.Models
{
    public class AdvBanner
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string DestUrl { get; set; }

        public string ImgName { get; set; }

        public int ViewPriority { get; set; }
    }
}