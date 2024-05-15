using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Data;
using ShoppingCart.Extensions;
using ShoppingCart.ViewModels;

namespace ShoppingCart.Areas.Admin.Controllers
{
	[Area("admin")]
	public class HomeController : Controller
	{
		private readonly KHShop29Context _context;
		private readonly IMapper _mapper;
		public HomeController(KHShop29Context context, IMapper mapper) 
		{
			_context = context;
			_mapper = mapper;
		}
		public async Task<IActionResult> Index()
		{
			var list = await _context.HangHoas.ToListAsync();
			return View(list);
		}
		public async Task<IActionResult> CreateProduct(CreateHangHoaViewModel product, IFormFile Hinh)
		{
			//Nếu form nhập vào hợp lệ
			if (ModelState.IsValid)
			{
				try
				{
					var hanghoa = _mapper.Map<HangHoa>(product);
					hanghoa.GiamGia = 1.0;
					hanghoa.SoLanXem = 0;

					if (Hinh != null)
					{
						hanghoa.Hinh = UtilExtensions.UploadHinh(Hinh, "HangHoa"); //luu vao folder KhachHang trong wwwwroot
					}
					await _context.AddAsync(hanghoa);
					await _context.SaveChangesAsync();
					return RedirectToAction("Index", "Home"); // Dang ky thanh cong thi chuyen den trang chu Hang hoa
				}
				catch (Exception ex)
				{
					var mess = $"{ex.Message} Fail";
				}

			}

			return View();
		}
	}
}
