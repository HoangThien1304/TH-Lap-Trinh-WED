using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaiTapThucHanh.Models
{
    [Table("ProductImages")]
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        // Lưu tên file ảnh phụ tương tự như thuộc tính ImageUrl bên Product.cs
        [Required(ErrorMessage = "Đường dẫn hình ảnh không được để trống.")]
        [StringLength(255)]
        public string ImageUrl { get; set; }

        // Khóa ngoại lưu Id của sản phẩm sở hữu hình ảnh này
        [Required]
        public int ProductId { get; set; }

        // Mối quan hệ liên kết ngược về đối tượng Product
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}