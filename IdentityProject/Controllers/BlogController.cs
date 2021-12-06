using System.Linq;
using IdentityProject.Data;
using IdentityProject.Models.Entities;
using IdentityProject.Models.ViewModels.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityProject.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class BlogController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IAuthorizationService _authorizationService;
        public BlogController(DatabaseContext context,
            UserManager<User> userManager
            , IAuthorizationService authorizationService)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }
        public IActionResult Index()
        {
            var blogs = _context.Blogs
                .Include(p => p.User)
                .Select(
                p => new BlogViewModel
                {
                    Id = p.Id,
                    Body = p.Body,
                    Title = p.Title,
                    UserName = p.User.UserName
                });
            return View(blogs);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(BlogViewModel blog)
        {
            var user = _userManager.GetUserAsync(User).Result;
            Blog newBlog = new Blog()
            {
                Body = blog.Body,
                Title = blog.Title,
                User = user,
            };
            _context.Add(newBlog);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(long Id)
        {
            var blog = _context.Blogs
                .Include(p => p.User)
                .Where(p => p.Id == Id)
                .Select(p => new BlogViewModel
                {
                    Body = p.Body,
                    Id = p.Id,
                    Title = p.Title,
                    UserId = p.UserId,
                    UserName = p.User.UserName
                }).FirstOrDefault();

           var result = _authorizationService.AuthorizeAsync(User, blog, "IsBlogUser").Result;
            if (result.Succeeded)
            {
                return View(blog);
            }
            else
            {
                return new ChallengeResult();
            }
        }

        [HttpPost]
        public IActionResult Edit(BlogViewModel blog)
        {
            ///
            return View();
        }
    }
}
