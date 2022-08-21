using Microsoft.AspNetCore.Mvc;

namespace Hyper_v_Web_Controller.Controllers
{
	[Route("")]
	public class IndexController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}
