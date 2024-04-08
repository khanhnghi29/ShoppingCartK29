using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Data;
using ShoppingCart.ViewModels;

namespace ShoppingCart.ViewComponents
{
    public class MenuLoaiViewComponent : ViewComponent
    {
        // Sử dụng readonly như vậy thì không cần phải tạo mới 1 đối tượng kiểu HshopDb2023
        private readonly KHShop29Context _context;
        // Nhưng cần phải tạo 1 constructor của lớp hiện tại với thuộc tính truyền vào như trên
        public MenuLoaiViewComponent(KHShop29Context context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var data = await _context.Loais.Select(loai => new MenuLoaiViewModel
            {
                MaLoai = loai.MaLoai,
                TenLoai = loai.TenLoai,
                SoLuong = loai.HangHoas.Count
            }).OrderBy(loai => loai.TenLoai).ToListAsync();

            return View(data); // Mac dinh render ra Defaul.cshtml
            // Hoac co the ghi return View("Default" , data);
        }


    }
}
