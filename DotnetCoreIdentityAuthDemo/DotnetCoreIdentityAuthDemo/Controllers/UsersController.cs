using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetCoreIdentityAuthDemo.Models.ExtendUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DotnetCoreIdentityAuthDemo.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<CustomUser> userManager;

        public UsersController(UserManager<CustomUser> userManager)
        {
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users;
            return View(users);
        }
    }
}