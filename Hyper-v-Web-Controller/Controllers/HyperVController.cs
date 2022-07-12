using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Management;

namespace Hyper_v_Web_Controller.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class HyperVController : Controller
    {
        [HttpGet]
        public string ShowVMs()
        {
            string result = "";
            ManagementScope scope = new ManagementScope(@"\root\virtualization\v2", null);
            string vmQueryWql = string.Format(CultureInfo.InvariantCulture,
                        "SELECT * FROM {0}", "Msvm_ComputerSystem");

            SelectQuery vmQuery = new SelectQuery(vmQueryWql);

            using (ManagementObjectSearcher vmSearcher = new ManagementObjectSearcher(scope, vmQuery))
            using (ManagementObjectCollection vmCollection = vmSearcher.Get())
            {
                if (vmCollection.Count == 0)
                {
                    throw new ManagementException(string.Format(CultureInfo.CurrentCulture,
                        "No {0} could be found with name \"{1}\"",
                        "Msvm_ComputerSystem",
                        "korolko_mint"));
                }

                ManagementObject vm = null;
                foreach (ManagementObject managementObject in vmCollection)
                {
                    result += managementObject.GetPropertyValue("ElementName");
                }
            }
            return result;
        }
        
    }
}
