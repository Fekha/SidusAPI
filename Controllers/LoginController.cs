using Microsoft.AspNetCore.Mvc;
using SidusAPI.Data;
using SidusAPI.ServerModels;
namespace SidusAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        [HttpGet]
        [Route("[action]")]
        public void Test()
        {
            using (var context = new ApplicationDbContext())
            {
                
            }
        }
    }
}
