using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RealViewServer.Storage;

namespace RealViewServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;

        public ApiController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("{projectName}/photos")]
        public string Get(string projectName)
        {
            var project = _dbContext.Projects.Include(p => p.Photos).First(p => p.Name.Equals(projectName));
            var result = JsonConvert.SerializeObject(project.Photos, Formatting.Indented);
            return result;
        }

        [HttpGet]
        [Route("projects")]
        public string Get()
        {
            var projects = _dbContext.Projects.ToList();
            var result = JsonConvert.SerializeObject(projects, Formatting.Indented);
            return result;
        }
    }
}
