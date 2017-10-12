using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZcrlPortal.Models
{
    public enum StaticChapterType
    {
        History, PortalInformation
    }
    public class StaticChapter
    {
        public int Id { get; set; }

        public StaticChapterType ChapterType { get; set; }
        public string Content { get; set; }
    }
}