﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Data;
using ShoppingCart.ViewModels;
using X.PagedList;

namespace ShoppingCart.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly KHShop29Context _context;
        public HangHoaController(KHShop29Context context)
        {
            _context = context;
        }
        /// <summary>
        /// Hàm trả về 1 danh sách các hàng hóa theo loại nếu có ttham số loai truyền vào
        /// Còn không có thì GetAll
        /// </summary>
        /// <param name="loai"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int? loai, int? page) // int? co the co hoac khong
        {
            /*var hangHoas = _context.HangHoas.AsQueryable(); // GetttAll

            if (loai.HasValue)// Nếu loại duocj truyen vaof thi lay theo loai
            {
                hangHoas = hangHoas.Where(h => h.MaLoai == loai.Value);
            }
            // Kết quả trả về ttheo kiểu HangHoaViewModel == DTO trong Web API
            var result = await hangHoas.Select(h => new HangHoaViewModel
            {
                MaHh = h.MaHh,
                TenHH = h.TenHh,
                DonGia = h.DonGia ?? 0, // Có thể = 0
                Hinh = h.Hinh ?? "", // Có thẻ có hoặc rỗng
                MoTaNgan = h.MoTaDonVi ?? "",
                TenLoai = h.MaLoaiNavigation.TenLoai
            }).ToListAsync();
            return View(result);*/
            var hangHoas = _context.HangHoas.AsQueryable(); // Lấy tất cả

            if (loai.HasValue) // Nếu loại được truyền vào thì lấy theo loại
            {
                hangHoas = hangHoas.Where(h => h.MaLoai == loai.Value);
            }

            // Kết quả trả về theo kiểu HangHoaViewModel == DTO trong Web API
            var pageSize = 9; // Số sản phẩm trên mỗi trang
            var pageNumber = page ?? 1; // Trang hiện tại (nếu không có thì mặc định là 1)
            var result = await hangHoas.Select(h => new HangHoaViewModel
            {
                MaHh = h.MaHh,
                TenHH = h.TenHh,
                DonGia = h.DonGia ?? 0, // Có thể = 0
                Hinh = h.Hinh ?? "", // Có thể có hoặc rỗng
                MoTaNgan = h.MoTaDonVi ?? "",
                TenLoai = h.MaLoaiNavigation.TenLoai
            }).ToPagedListAsync(pageNumber, pageSize);
            ViewBag.loai = loai;
            return View(result);
        }

        /// <summary>
        /// Ham search nhan dau vao la 1 chuoi string, neu ko co thi getAll
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<IActionResult> Search(string? query, int? page)
        {
			var hangHoas = _context.HangHoas.AsQueryable();

			if (!string.IsNullOrEmpty(query))
			{
				hangHoas = hangHoas.Where(p => p.TenHh.Contains(query)); // Lọc theo tên sản phẩm chứa từ khóa
			}

			// Kết quả trả về theo kiểu HangHoaViewModel == DTO trong Web API
			var pageSize = 9; // Số sản phẩm trên mỗi trang
			var pageNumber = page ?? 1; // Trang hiện tại (nếu không có thì mặc định là 1)
			var result = await hangHoas.Select(p => new HangHoaViewModel
			{
				MaHh = p.MaHh,
				TenHH = p.TenHh,
				DonGia = p.DonGia ?? 0,
				Hinh = p.Hinh ?? "",
				MoTaNgan = p.MoTaDonVi ?? "",
				TenLoai = p.MaLoaiNavigation.TenLoai
			}).ToPagedListAsync(pageNumber, pageSize);

			return View(result);
		}


        /// <summary>
        /// Chi tiet cua 1 san pham theo ma 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Detail(int id)
        {
            var data = await _context.HangHoas
                            .Include(h => h.MaLoaiNavigation)
                            .SingleOrDefaultAsync(h => h.MaHh == id);
            if(data == null)
            {
                TempData["Message"] = $"Không tìm thấy sản phẩm có mã {id}";
                return Redirect("/404");
            }

            var result = new ChiTietHangHoaViewModel
            {
                MaHh = data.MaHh,
                TenHH = data.TenHh,
                DonGia = data.DonGia ?? 0,
                ChiTiet = data.MoTa ?? string.Empty,
                Hinh = data.Hinh ?? string.Empty,
                MoTaNgan = data.MoTaDonVi ?? string.Empty,
                TenLoai = data.MaLoaiNavigation.TenLoai,
                SoLuongTon = 10,
                DiemDanhGia = 5,
            };
            return View(result);
        }
    }
}
