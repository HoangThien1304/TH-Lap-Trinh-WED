using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace BaiTapThucHanh.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm.")]
        [Range(0, 100000000, ErrorMessage = "Giá sản phẩm phải nằm trong khoảng từ 0 đến 100,000,000 VNĐ.")]
        public decimal Price { get; set; }

        public string Description { get; set; }

        public int CategoryId { get; set; }

        // Thuộc tính liên kết thực thể (Navigation Property)
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public string? ImageUrl { get; set; }

        [NotMapped]
        [Display(Name = "Hình ảnh sản phẩm")]
        public IFormFile? ImageFile { get; set; }

        // Khai báo danh sách ảnh phụ của sản phẩm (Quan hệ 1 - Nhiều)
        public virtual ICollection<ProductImage>? ProductImages { get; set; }
    }
}