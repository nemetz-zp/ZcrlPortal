using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZcrlPortal.Models;

namespace ZcrlPortal.ViewModels
{
    public class TenderItemGroup
    {
        public string GroupName { get; set; }
        public List<TenderItem> Items { get; set; }
    }
}