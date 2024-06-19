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
        public Guid CreateAccount([FromBody]Account account)
        {
            using (var context = new ApplicationDbContext())
            {
                try{
                    var oldAccount = context.Accounts.FirstOrDefault(a => a.AccountId == account.AccountId);
                    if (oldAccount == null)
                    {
                        account.PlayerGuid = Guid.NewGuid();
                        context.Accounts.Add(account);
                        context.SaveChanges();
                        return account.PlayerGuid;
                    }
                    else
                    {
                        return oldAccount.PlayerGuid;
                    }
                }
                catch(Exception ex){
                    return Guid.Empty;
                }
            }
        }
        
        [HttpGet]
        [Route("[action]")]
        public Account GetAccount(string accountId)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.Accounts.FirstOrDefault(a => a.AccountId == accountId);
            }
        }
    }
}
