using Hyper_v_Web_Controller.Domain;
using Hyper_v_Web_Controller.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using Hyper_v_Web_Controller.Models;

namespace Hyper_v_Web_Controller.Controllers
{
	[Authorize(Roles = "Admin")]
	[Route("[controller]/[action]")]
	public class UserController : Controller
	{
		IUserRepository UserRepository;
		IHashService hashService;
		public UserController(IHashService hashService, IUserRepository userRepository)
		{	
			this.hashService = hashService;
			this.UserRepository = userRepository;			
		}
		[HttpGet]
		public IActionResult GetUsers()
		{
			return View(UserRepository.GetList().ToList());
		}
		[HttpGet]
		public IActionResult EditUser(int Id)
		{
			User user = UserRepository.Get(Id);
			if (user is null)
			{
				return BadRequest("Пользователь не найден!!!");
			}
			return View(user);
		}
		[HttpPost]
		public IActionResult EditUser(int Id,string Login,string Password)
		{
			User user = UserRepository.Get(Id);
			if (!(user.Login == Login))
			{
				if (!(UserRepository.Get(Login) is null))
				{
					return BadRequest("Логин занят");
				}
				if (!(Login.Count() > 3 && Login.Count() < 30 && Login.All(c => char.IsLetterOrDigit(c))))
				{
					return BadRequest("Логин должен содержать только буквы и цифры и быть длиной от 3 до 30");
				}
				user.Login = Login;
			}
			
			if (!(Password.Count()==0))
			{
				if (!(Password.Count() >= 3 && Password.Count() < 30 && Password.All(c => char.IsLetterOrDigit(c))))
				{
					return BadRequest("Пароль должен содержать только буквы и цифры и быть длиной от 3 до 30");
				}
				user.PasswordHash = hashService.GetHash(Password);
			}
			UserRepository.Update(user);
			UserRepository.Save();
			return Redirect("/User/GetUsers");
		}
		[HttpPost]
		public IActionResult DeleteUser(int Id)
		{
			UserRepository.Delete(Id);
			UserRepository.Save();
			return Redirect("/User/GetUsers");
		}
	}
}
