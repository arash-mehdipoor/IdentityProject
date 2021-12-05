using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityProject.Controllers
{
    [Authorize(Roles = "Admin,Auther")]
    //[Authorize(Roles = "Auther")]
    public class AuthorizeController : Controller
    {
        public string Index()
        {
            return "Index";
        }
    }
}
