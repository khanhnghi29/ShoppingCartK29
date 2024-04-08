using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Data;
using ShoppingCart.Extensions;
using ShoppingCart.ViewModels;
using System.Security.Claims;

namespace ShoppingCart.Controllers
{
	public class KhachHangController : Controller
	{
		private readonly KHShop29Context _context;
		private readonly IMapper _mapper;

		public KhachHangController(KHShop29Context context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}
		[HttpGet]
		public async Task<IActionResult> DangKy()
		{
			return View();
		}
		/// <summary>
		/// Ham dang ky tai khoan cho nguoi dung, co 2 tham so đầu vào la RegisterViewModel va Iformfile Hinh
		/// Resgiser dc map sang kiểu KhachHang ở Database, kiểu Hình ảnh được lưu vào folder owrr wwwroot và lưu ở dạng string 
		/// trong Database dạng (tenthumuc/folder/tenAnh);
		/// </summary>
		/// <param name="model"></param>
		/// <param name="Hinh"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> Dangky(RegisterViewModel UserSignUp, IFormFile Hinh)
		{
			//Nếu form nhập vào hợp lệ
			if(ModelState.IsValid)
			{
				try
				{
					var khachHang = _mapper.Map<KhachHang>(UserSignUp); //convert sang kieu khach hang = AutoMapper
					khachHang.RandomKey = UtilExtensions.GenerateRandomKey(); // tao mot khoa randomkey toi da 5 ky tu
					khachHang.MatKhau = UserSignUp.MatKhau.ToMd5Hash(khachHang.RandomKey); //Ma hoa mat khau 
					khachHang.HieuLuc = true;
					khachHang.VaiTro = 0;// Vai tro la nguoi mua Hang

					if (Hinh != null)
					{
						khachHang.Hinh = UtilExtensions.UploadHinh(Hinh, "KhachHang"); //luu vao folder KhachHang trong wwwwroot
					}
					await _context.AddAsync(khachHang);
					await _context.SaveChangesAsync();
					return RedirectToAction("Index", "HangHoa"); // Dang ky thanh cong thi chuyen den trang chu Hang hoa
				}
				catch (Exception ex)
				{
					var mess = $"{ex.Message} Fail";
				}
				
			}

			return View();
		}



		/// <summary>
		/// Get 1 cai view de hien thi form dang nhap
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public async Task<IActionResult> DangNhap(string? ReturnUrl)
		{
			ViewBag.ReturnUrl = ReturnUrl;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> DangNhap(LoginViewModel UserSignIn, string? ReturnUrl)
		{
			ViewBag.ReturnUrl = ReturnUrl;
			if (ModelState.IsValid)
			{
				var khachHang = await _context.KhachHangs.SingleOrDefaultAsync(kh => kh.MaKh == UserSignIn.UserName);
				if (khachHang == null)
				{
					ModelState.AddModelError("Error", "Tài khoản chưa được đăng ký");
				}
				else
				{
					if (!khachHang.HieuLuc)
					{
						ModelState.AddModelError("Error", "Tài khoản đã bị khóa");
					}
					else
					{
						if (khachHang.MatKhau != UserSignIn.Password.ToMd5Hash(khachHang.RandomKey))
						{
							ModelState.AddModelError("Error", "Sai thông tin đăng nhập");
						}
						else
						{
                            // Tạo danh sách claims (thông tin người dùng) cho việc xác thực
                            var claims = new List<Claim> {
								new Claim(ClaimTypes.Email, khachHang.Email),
								new Claim(ClaimTypes.Name, khachHang.HoTen),
								new Claim(SettingExtensions.CLAIM_CUSTOMERID, khachHang.MaKh),// Claim tùy chỉnh về mã khách hàng

								//claim - role động
								new Claim(ClaimTypes.Role, "Customer") //Claim về vai trò
							};
                            // Tạo identity với các claims
                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                            // Tạo principal (người dùng đã xác thực) với claims identity
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                            // Đăng nhập người dùng bằng cách tạo phiên xác thực dựa trên cookie
                            await HttpContext.SignInAsync(claimsPrincipal);

							if (Url.IsLocalUrl(ReturnUrl))
							{
								return Redirect(ReturnUrl);
							}

							else
							{
								return Redirect("/");
							}
						}
					}
				}
			}
			return View();


		}
		//Khi nao login moi dung dc 2 contrroller nay
		[Authorize]
        public async Task<IActionResult> Profile()
        {
            return View();
        }
		[Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
    }
}
