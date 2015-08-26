using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.ModelBinding;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using BrockAllen.MembershipReboot;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using WebHost.IdSvr;
using WebHost.Models;
using WebHost.MR;

namespace WebHost.Controllers
{
    public class LocalRegistrationController : Controller
    {
        private readonly CustomGroupRepository _repository;

        static UserAccountService<CustomUser> GetUserAccountService(CustomDatabase db)
        {
            //CustomUserAccountService
            var repo = new CustomUserRepository(db);
            var svc = new CustomUserAccountService(CustomConfig.Config, repo);
            return svc;
        }


        public LocalRegistrationController()
        {
         //   _repository = new CustomGroupRepository();
        }

        [Route("core/localregistration")]
        [HttpGet]
        public ActionResult Index(string signin)
        {
            return View();
        }

        [Route("core/localregistration")]
        [HttpPost]
        public ActionResult Index(string signin, LocalRegistrationModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new CustomDatabase("MembershipReboot"))
                {
                    var svc = GetUserAccountService(db);
                    var account = svc.GetByUsername(model.Username);
                    if (account == null)
                    {
                        Console.WriteLine("Creating new account");
                        account = svc.CreateAccount(model.Username,model.Password,model.Email);

                        account.FirstName = model.First;
                        account.LastName = model.Last;
                        account.Age = 21;
                        svc.Update(account);
                    }
                    else
                    {
                        Console.WriteLine("Updating existing account");
                        account.Age++;
                        svc.Update(account);
                    }
                }
                var user = new LocalRegistrationUserService.CustomUser
                {
                    Username = model.Username, 
                    Password = model.Password, 
                    Subject = Guid.NewGuid().ToString(),
                    Claims = new List<Claim>()
                };
                LocalRegistrationUserService.Users.Add(user);

                

                user.Claims.Add(new Claim(Constants.ClaimTypes.GivenName, model.First));
                user.Claims.Add(new Claim(Constants.ClaimTypes.FamilyName, model.Last));

                return Redirect("~/core/" + Constants.RoutePaths.Login + "?signin=" + signin);
            }

            return View();
        }
    }
}
