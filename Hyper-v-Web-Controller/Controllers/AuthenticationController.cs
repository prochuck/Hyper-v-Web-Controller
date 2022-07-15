using Hyper_v_Web_Controller.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Hyper_v_Web_Controller.Domain;
using Hyper_v_Web_Controller.Interfaces;

/*
Сделать 2 метода для получения данных.
Получить мои виртуалки - получение данных по логину(проверять логин на инъекции)
Получить все виртуалки - получение всех виртуалок(только админ)
*/

namespace Hyper_v_Web_Controller.Controllers
{
    [Route("[controller]/[action]")]
    public class AuthenticationController : Controller
    {
        IUserRepository userRepository;

        public AuthenticationController(IUserRepository repository)
        {
            userRepository = repository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string login, string password, string? ReturnUrl)
        {
            User user = userRepository.Get(login);
            
            if (!(user is null))
            {
                if (user.PasswordHash != password.GetHashCode().ToString())
                    return View("не верный пароль");
                var claims = new List<Claim> { 
                    new Claim(ClaimTypes.Name, user.Login),
                    new Claim(ClaimTypes.Role,user.Role.RoleName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(claimsIdentity));
                return Redirect(ReturnUrl is null ? "/HyperV/ShowVMs" : ReturnUrl);
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterUser()
        {
            return View();
        }

        //переделать хэширование
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterUser(string login, string password,int roleId)
        {
            if (!(userRepository.Get(login) is null))
            {
                return BadRequest("Логин занят");
            }
            userRepository.Create(new User() { Login = login, PasswordHash = password.GetHashCode().ToString(), RoleId = roleId });
            userRepository.Save();
            return View();
        }
    }
}
