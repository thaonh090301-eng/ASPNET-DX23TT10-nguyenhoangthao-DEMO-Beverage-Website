using System;
using System.ComponentModel.DataAnnotations;

namespace BeverageWebsite.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        [Range(
            0.01d,
            999999999.99d,
            ErrorMessage = "Giá sản phẩm phải lớn hơn 0 và không vượt quá giới hạn cho phép.")]
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public string BadgeType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
