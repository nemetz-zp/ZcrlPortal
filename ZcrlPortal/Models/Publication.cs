using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZcrlPortal.Models
{
    public enum PublicationType
    {
        News, Article
    }

    public class Publication
    {
        public Publication()
        {
            Themes = new List<DataGroup>();
        }

        [NotMapped]
        private const int CONTENT_LENGTH = 150;

        public long Id { get; set; }

        [MaxLength(128)]
        public string Title { get; set; }

        public string TitleImage { get; set; }

        public DateTime PublicationDate { get; set; }

        public string Content { get; set; }

        public string Summary { get; set; }

        public PublicationType InformationType { get; set; }

        public virtual UserProfile Author { get; set; }
        public int? UserProfileId { get; set; }

        public string GetPortalDate()
        {
            var culture = new System.Globalization.CultureInfo("uk-UA");
            string dateDoW = culture.DateTimeFormat.GetDayName(PublicationDate.DayOfWeek);
            string dateDoWNorm = dateDoW.ToUpper().First() + dateDoW.Substring(1, dateDoW.Length - 1);
            return dateDoWNorm + ", " + PublicationDate.ToString("dd.MM.yyyy");
        }

        public virtual List<DataGroup> Themes { get; set; }
    }
}