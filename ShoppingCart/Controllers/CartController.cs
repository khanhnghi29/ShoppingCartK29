using MessagePack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Data;
using ShoppingCart.Extensions;
using ShoppingCart.ViewModels;
using System.Net.WebSockets;

namespace ShoppingCart.Controllers
{
    public class CartController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly KHShop29Context _context;
        public CartController(KHShop29Context context, PaypalClient paypalClient)
        {
            _paypalClient = paypalClient;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(Cart());
        }


        
        /// <summary>
        /// Luu  tru cac CArtItem vao trong Session thay cho DAtabase
        /// </summary>
        /// <returns></returns>
        public List<CartItemViewModel> Cart()
        {
            return HttpContext.Session.Get<List<CartItemViewModel>>(SettingExtensions.CARD_KEY) ?? new List<CartItemViewModel>();
        }
        /// <summary>
        /// Them 1 san pham vao gio hang, mac dinh so luong la 1, lay theo id , check tu Db 
        /// xem id san pham do con khong
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var gioHang = Cart();
            var item = gioHang.SingleOrDefault(h => h.MaHh == id);
            //Check xem trong giỏ hàng đã có mã sàn phẩm này hay chưa, nếu null thì get từ session ra
            if (item == null)
            {

                var hangHoa = await _context.HangHoas.SingleOrDefaultAsync(h => h.MaHh == id);
                //Check xem trong db có còn mặt hàng này không, ko có bắn ra 1 message
                if(hangHoa == null)
                {
                    TempData["Message"] = $"Không tìm thấy sản phẩm có mã {id}";
                    return Redirect("/404");
                }
                // Nếu trong Db còn mặt hàng này, Data transfer to Object, lay theo dang CartViewModel
                item = new CartItemViewModel
                {
                    MaHh = hangHoa.MaHh,
                    TenHH = hangHoa.TenHh,
                    DonGia = hangHoa.DonGia ?? 0,
                    Hinh = hangHoa.Hinh ?? string.Empty,
                    SoLuong = quantity
                };
                //Add vao List gio hang
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong = item.SoLuong + quantity;
            }
            // Luu gio hang vao Session
            HttpContext.Session.Set(SettingExtensions.CARD_KEY, gioHang);

            return RedirectToAction("Index");
        }
        /// <summary>
        /// Xoa item trong CArt theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> RemoveCart(int id)
        {
            var gioHang = Cart();
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if(item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(SettingExtensions.CARD_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (Cart().Count == 0)
            {
                return Redirect("/");
            }
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(Cart());
        }
		/// <summary>
		/// Ham Checkout Theo cach COD , voi 1 tham so nhan vao kieu CheckoutViewModel
		/// </summary>
		/// <param name="checkout"></param>
		/// <returns></returns>
		[HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout(CheckoutViewModel checkout)
        {
            if (ModelState.IsValid)
            {
                // Lấy ra CustomerId từ CookieAuthen dể đối chiếu với checkout và lưu hóa dơn vào databasse
                var customerId =  HttpContext.User.Claims.SingleOrDefault(c => c.Type == SettingExtensions.CLAIM_CUSTOMERID).Value;
                var khachHang = new KhachHang();
                if (checkout.GiongKhachHang)
                {
                    khachHang = await _context.KhachHangs.SingleOrDefaultAsync(kh => kh.MaKh == customerId);
                }
                var hoaDon = new HoaDon
                {
					MaKh = customerId,
					HoTen = checkout.HoTen ?? khachHang.HoTen,
					DiaChi = checkout.DiaChi ?? khachHang.DiaChi,
					DienThoai = checkout.DienThoai ?? khachHang.DienThoai,
					NgayDat = DateTime.Now,
					CachThanhToan = "COD", // Van chuyen den noiii moi nhan tien
					CachVanChuyen = "GRAB",
					MaTrangThai = 0,
					GhiChu = checkout.GhiChu
				};
                //Tao 1 phien giao dich
                await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.Database.CommitTransactionAsync();
                    await _context.AddAsync(hoaDon);
                    await _context.SaveChangesAsync();

                    var ChiTietHoaDons = new List<ChiTietHd>();

                    foreach(var item in Cart())
                    {
                        ChiTietHoaDons.Add(new ChiTietHd
                        {
							MaHd = hoaDon.MaHd,
							SoLuong = item.SoLuong,
							DonGia = item.DonGia,
							MaHh = item.MaHh,
							GiamGia = 0
						});
                    }

                    await _context.AddRangeAsync(ChiTietHoaDons);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.Set<List<CartItemViewModel>>(SettingExtensions.CARD_KEY, new List<CartItemViewModel>());

                    return View("Success");

                }
                catch 
                {
                    await _context.Database.RollbackTransactionAsync();
                }
            }

            return View(Cart());
        }


        /// <summary>
        /// Tao 1 API Tao don Hàng gửi về phía Paypal, 1 tham số đầu vào
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            // Gui cac thong tin nay len phia Paypal
            var tongTien = Cart().Sum(s => s.ThanhTien).ToString();
            var loaiTien = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                var response = await _paypalClient.CreateOrder(tongTien, loaiTien, maDonHangThamChieu);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }


        }

        [Authorize]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);
                //Reset gio hang
                HttpContext.Session.Set<List<CartItemViewModel>>(SettingExtensions.CARD_KEY, new List<CartItemViewModel>());
                return Ok(response);
            }
            catch (Exception ex)
            {
				var error = new { ex.GetBaseException().Message };
				return BadRequest(error);
			}
        }


		[Authorize]
		public async Task<IActionResult> PaymentSuccess()
		{
			return View("PaymentSuccess");
		}
	}
}
