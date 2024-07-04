using Microsoft.AspNetCore.Mvc;
using SidusAPI.Data;
using SidusAPI.ServerModels;
namespace SidusAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        [Route("[action]")]
        public ActionResult<Account> CreateAccount([FromBody]Account account, int clientVersion)
        {
            try
            {
                var clientVersionText = CheckClientVersion(clientVersion);
                if (!String.IsNullOrEmpty(clientVersionText)) { return BadRequest(clientVersionText); }
                using (var context = new ApplicationDbContext())
                {

                    var oldAccount = context.Accounts.FirstOrDefault(a => a.AccountId == account.AccountId);
                    if (oldAccount == null)
                    {
                        account.PlayerGuid = Guid.NewGuid();
                        context.Accounts.Add(account);
                        context.SaveChanges();
                        return account;
                    }
                    else
                    {
                        return oldAccount;
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex}");
            }
        }
        
        [HttpGet]
        [Route("[action]")]
        public ActionResult<Account> GetAccount(string accountId, int clientVersion)
        {
            try
            {
                var clientVersionText = CheckClientVersion(clientVersion);
                if (!String.IsNullOrEmpty(clientVersionText)) { return BadRequest(clientVersionText); }
                using (var context = new ApplicationDbContext())
                {
                    return context.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex}");
            }
        }

        private string CheckClientVersion(int clientVersion)
        {
            using (var context = new ApplicationDbContext())
            {
                var serverVersion = context.Settings.FirstOrDefault().ClientVersion;
                return serverVersion > clientVersion ? $"New Client version {serverVersion} available. Current Client version {clientVersion}" : "";
            }
        }
    }
}
