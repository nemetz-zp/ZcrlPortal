using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZcrlPortal.Models
{
    public enum DataGroupType
    {
        ArticleGroup, UserDepartment, TenderGroup
    }

    public class DataGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public DataGroupType RelatedGroup { get; set; }

        public virtual List<Publication> RelatedPublications { get; set; }
    }
}