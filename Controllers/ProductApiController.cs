using BaiTapThucHanh.Data;
using BaiTapThucHanh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BaiTapThucHanh.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // Cho phép truy cập công khai để làm bài tập dễ dàng hơn
    public class ProductApi : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductApi(AppDbContext context)
        {
            _context = context;
        }

        // 1. LẤY DANH SÁCH SẢN PHẨM (GET: api/ProductApi)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }

        // 2. LẤY CHI TIẾT 1 SẢN PHẨM (GET: api/ProductApi/5)
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return product;
        }

        // 3. THÊM MỚI SẢN PHẨM (POST: api/ProductApi)
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // 4. CẬP NHẬT SẢN PHẨM (PUT: api/ProductApi/5)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id) return BadRequest();

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // 5. XÓA SẢN PHẨM (DELETE: api/ProductApi/5)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}