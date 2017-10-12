using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZcrlPortal.Models
{
    public enum UploadFileType
    {
        AdminPrivateUpload, TenderUpload, PublicationUpload
    }

    public class UploadFile
    {
        public long Id { get; set; }

        public string FileName { get; set; }
        public string DisplayName { get; set; }

        public UploadFileType FileType { get; set; }

        public virtual UserProfile Author { get; set; }
        public int? UserProfileId { get; set; }

        public long DownloadCount { get; set; }
    }
}