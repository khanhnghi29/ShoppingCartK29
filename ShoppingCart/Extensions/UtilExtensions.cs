using System.Text;

namespace ShoppingCart.Extensions
{
	public class UtilExtensions
	{
		public static string UploadHinh(IFormFile Hinh, string folder)
		{
			try
			{
				var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", folder, Hinh.FileName);
				using(var myFile = new FileStream(imagePath, FileMode.CreateNew))
				{
					Hinh.CopyTo(myFile);
				}
				/*if(Hinh.FileName.Length <= 50)
					return Hinh.FileName;
				return string.Empty;*/

				return Hinh.FileName;
			}
			catch (Exception ex)
			{
				return string.Empty;
			}
		}

		public static string GenerateRandomKey(int length = 5)
		{
			var pattern = @"qwertyuiopasdfghjklzxcvbnmQWERTYUIIOPASDFGHJKLZXCCVBNM!";
			var sb = new StringBuilder();
			var ran = new Random();

			for(int i = 0; i < length; i++)
			{
				sb.Append(pattern[ran.Next(0, pattern.Length)]);
			}

			return sb.ToString();
		}
	}
}
