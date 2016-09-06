using System;
using System.Collections.Generic;

namespace HG.Coprorate.Firebrand.Models
{
    public class Book
    {
        public string HashCode { get; set; }

        public string ISBN { get; set; }

        public string Format { get; set; }

        public Title Title { get; set; }

        public Series Series { get; set; }

        public string SeriesNumeber { get; set; }

        public Publisher Publisher { get; set; }

        public List<Author> Authors { get; set; }

        public List<Author> Illustrators { get; set; }

        public string PageNumber { get; set; }

        public Category Category { get; set; }

        public List<string> BICCategories { get; set; }

        public string MinAge { get; set; }

        public string MaxAge { get; set; }

        public string Description { get; set; }

        public PublishStatus PublishStatus { get; set; }

        public Dimensions Dimensions { get; set; }

        public string Weight { get; set; }

        public string Price { get; set; }

        public string MediaFileLink { get; set; }

        public string ImprintName { get; set; }

        public DateTime PubDate { get; set; }
    }
}