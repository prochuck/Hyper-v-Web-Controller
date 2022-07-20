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
        IVMImageRepository VMRImageepository;
        IHyperVThing hyperVThing;
        public HyperVController(IVMRepository vMRepository, IVMImageRepository vMRImageepository, IHyperVThing hyperVThing)
        {
            this.VMRImageepository = vMRImageepository;
            this.VMRepository = vMRepository;
            this.hyperVThing = hyperVThing;
        }
        [HttpPost]
        public IActionResult SwitchVMState(int Id)
        {
            if (hyperVThing.GetVMState(VMRepository.Get(Id)) == VMState.Enabled)
            {
                VM vM = VMRepository.Get(Id);
                hyperVThing.TurnOffVM(vM);
                vM.machineState = VMState.Disabled;
                VMRepository.Update(vM);
                VMRepository.Save();
            }
            else
            {
                VM vM = VMRepository.Get(Id);
                vM.ip= hyperVThing.TurnOnVM(vM);
                vM.machineState = VMState.Enabled;
                VMRepository.Update(vM);
                VMRepository.Save();
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
            return View(VMRImageepository.GetList().ToList());
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
                //доделать чтуки с созданием
                VM vM = hyperVThing.CreateVM(VMRImageepository.Get(imageId), machineName, User.Claims.Where(e => e.Type == ClaimTypes.Name).First().Value);
               
                vM.machineState = VMState.Enabled;
                vM.CreatorId = 1;
                VMRepository.Create(vM);
                VMRepository.Save();
                return Ok($"ВМ {vM.VmName} создана");
            }
            catch (Exception ex)
            {
                return BadRequest($"что-то пошло не так:{ex.Message}");

            }
            return View();
        }


    }
}
