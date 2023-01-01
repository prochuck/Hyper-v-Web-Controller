using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Management;
using Hyper_v_Web_Controller.Interfaces;
using Hyper_v_Web_Controller.Models;
using System.Security.Claims;
using Hyper_v_Web_Controller.Services;

namespace Hyper_v_Web_Controller.Controllers
{
	[Authorize]
	[Route("[controller]/[action]")]
	public class HyperVController : Controller
	{
		IVMRepository VMRepository;
		IVMImageRepository VMImageRepository;
		IHyperVThing hyperVThing;
		IConfiguration configuration;
		public HyperVController(IVMRepository vMRepository, IVMImageRepository VMImageRepository, IHyperVThing hyperVThing)
		{
			this.VMImageRepository = VMImageRepository;
			this.VMRepository = vMRepository;
			this.hyperVThing = hyperVThing;
		}
		[HttpPost]
		public IActionResult SwitchVMState(int Id)
		{
			if ((VMRepository.Get(Id).machineState) == VMState.Enabled)
			{
				VM vM = VMRepository.Get(Id);
				hyperVThing.TurnOffVM(vM);
			}
			else
			{
				VM vM = VMRepository.Get(Id);
				hyperVThing.TurnOnVM(vM);
			}
			return Redirect("/HyperV/GetVMs");
		}
		[HttpGet]
		public IActionResult GetVMs()
		{
			return View(VMRepository.GetList((int.Parse(HttpContext.User.Claims.Where(e => e.Type == "Id").First().Value))).ToList());
		}
		[HttpGet]
		public IActionResult GetVMImages()
		{
			return View(VMImageRepository.GetList().ToList());
		}

		[HttpGet]
		public IActionResult CreateVM()
		{
			return View();
		}
		[HttpPost]
		public IActionResult CreateVM(int imageId, string machineName)
		{
			try
			{
				if (!(machineName.Count() > 3 && machineName.All(c => char.IsLetterOrDigit(c))))
				{
					return BadRequest($"Имя должно быть больше 3х символов и разрешено использовать только цифры и буквы");
				}
				VMImage vMImage = VMImageRepository.Get(imageId);
				string userName = User.Claims.Where(e => e.Type == ClaimTypes.Name).First().Value;
				string vmName = $"{vMImage.Name}-{userName}-{machineName}";
				//доделать чтуки с созданием
				VM vM = new VM()
				{
					RealizedVMImageId = imageId,
					CreatorId = int.Parse(User.Claims.Where(e => e.Type == "Id").First().Value),
					VmName = vmName,
					machineState = VMState.Creating
				};
				VMRepository.Create(vM);
				VMRepository.Save();
				Task.Run(() =>
				{
					try
					{
						hyperVThing.CreateVM(vMImage, vM, userName);
						vM.machineState = VMState.Enabled;
						VMRepository.Update(vM);
						VMRepository.Save();
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
				});

				return Redirect("/HyperV/GetVMs");
			}
			catch (Exception ex)
			{
				return BadRequest($"что-то пошло не так:{ex.Message}");

			}
			return View();
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public IActionResult UpdateVMImagesList()
        {
			VMImageRepository.UpdateVMImagesList();
			return Redirect("/");
		}



	}
}
