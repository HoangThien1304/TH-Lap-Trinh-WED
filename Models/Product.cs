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

        [Range(0.01, 10000.00, ErrorMessage = "Giá sản phẩm phải nằm trong khoảng từ 0.01 đến 10000.")]
        public decimal Price { get; set; }

        public string Description { get; set; }

        public int CategoryId { get; set; }

        // === THÊM DÒNG NÀY ĐỂ LIÊN KẾT SANG MODEL CATEGORY ===
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // CÁC THUỘC TÍNH XỬ LÝ ẢNH GIỮ NGUYÊN
        public string? ImageUrl { get; set; }

        [NotMapped]
        [Display(Name = "Hình ảnh sản phẩm")]
        public IFormFile? ImageFile { get; set; }
    }
}