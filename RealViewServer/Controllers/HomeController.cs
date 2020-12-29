using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace RealViewServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public HomeController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var webRoot = _env.WebRootPath;
            var path = Path.Combine(webRoot, "index.html");
            return File(path, "text/html");
        }
    }
}
