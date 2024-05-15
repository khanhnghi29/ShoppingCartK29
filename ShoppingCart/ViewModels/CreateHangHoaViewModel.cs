using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.ViewModels
{
	public class CreateHangHoaViewModel
	{
		[Key]
		[Required]
		public int MaHH {  get; set; }
		[Required]
		[MaxLength(50)]
        [Display(Name = "Ten Hang Hoa")]
        public string TenHH { get; set; }
		[Required]
		[Display(Name="Ma Loai")]
		public int MaLoai { get; set; }
		[Display(Name = "Don Gia")]
		public double? DonGia { get; set; }

		[MaxLength(50)]
		public string? Hinh { get; set; }

		[Display(Name ="Ngay San Xuat")]
		[DataType(DataType.Date)]
		public DateTime NgaySX { get; set; }

		[Required]
		[MaxLength(50)]
		[Display(Name ="Ma Nha Cung Cap")]
		public string MaNCC { get; set; }

	}
}
