using BaiTapThucHanh.Models;
using System.Collections.Generic;
using BaiTapThucHanh.Models; // Thay thế bằng namespace thực tế của bạn

public interface IProductRepository
{
    IEnumerable<Product> GetAll();
    Product GetById(int id);
    void Add(Product product);
    void Update(Product product);
    void Delete(int id);
}