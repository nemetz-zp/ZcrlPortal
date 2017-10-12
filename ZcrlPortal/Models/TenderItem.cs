using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZcrlPortal.Models
{
    public class TenderItem
    {
        public long Id { get; set; }

        public virtual DataGroup RelatedGroup { get; set; }
        public int? DataGroupId { get; set; }

        public virtual TenderYear Year { get; set; }
        public int TenderYearId { get; set; }

        public virtual UploadFile RelatedFile { get; set; }
        public long UploadFileId { get; set; }

        public DateTime PublicationDate { get; set; }
    }
}