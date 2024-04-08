using AutoMapper;
using ShoppingCart.Data;
using ShoppingCart.ViewModels;

namespace ShoppingCart.Extensions
{
	public class AutoMapperProfile : Profile 
	{
		public AutoMapperProfile() 
		{
				CreateMap<RegisterViewModel, KhachHang>();
			//.ForMember(kh => kh.HoTen, option => option.MapFrom(RegisterVM => RegisterVM.HoTen))
			// Dùng ForMember khi tên thuộc tính của 2 bên khác nhau, nếu giống nhau như trên rồi thì không cần
			//.ReverseMap(); Dùng để map 2 chiều 
		}
	}
}
