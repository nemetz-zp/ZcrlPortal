using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZcrlPortal.Models
{
    public enum LogRecordType
    {
        UserChanges, RegistrationsRequests, TendersAddEdit, NewsAddEdit, ArticlesAddEdit, BannerAddEdit
    }

    public class LogRecord
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public DateTime СreatedDate { get; set; }
        public LogRecordType RecordType { get; set; }
    }
}