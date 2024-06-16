using Microsoft.AspNetCore.Mvc;
using SidusAPI.ServerModels;
namespace SidusAPI.Controllers
{
    public class LoginController
    {
        [HttpGet]
        [Route("GetGameMatch")]
        public GameMatch? GetGameMatch(Guid gameGuid)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.GameMatches?.FirstOrDefault(x => x.GameGuid == gameGuid);
            }
        }
    }
}
