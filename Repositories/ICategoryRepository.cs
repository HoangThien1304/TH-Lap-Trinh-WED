using BaiTapThucHanh.Models;
using System.Collections.Generic;


public interface ICategoryRepository
{
    IEnumerable<Category> GetAllCategories();
}