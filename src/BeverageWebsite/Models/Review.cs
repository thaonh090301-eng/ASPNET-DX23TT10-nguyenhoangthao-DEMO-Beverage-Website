using System;

namespace BeverageWebsite.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public byte Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
