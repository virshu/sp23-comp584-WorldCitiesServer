using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorldModel;

namespace WorldCitiesApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase {
        private readonly UserManager<WorldCitiesUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly WorldCitiesContext _context;

        public SeedController(UserManager<WorldCitiesUser> userManager, RoleManager<IdentityRole> roleManager, 
            IConfiguration configuration, WorldCitiesContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("Users")]
        public async Task<IActionResult> ImportUsers() {
            const string roleUser = "RegisteredUser";
            const string roleAdmin = "Administrator";

            if (await _roleManager.FindByNameAsync(roleUser) is null) {
                await _roleManager.CreateAsync(new IdentityRole(roleUser));
            }
            if (await _roleManager.FindByNameAsync(roleAdmin) is null) {
                await _roleManager.CreateAsync(new IdentityRole(roleAdmin));
            }

            List<WorldCitiesUser> addedUserList = new();
            (string name, string email) = ("admin", "admin@email.com");

            if (await _userManager.FindByNameAsync(name) is null) {
                WorldCitiesUser userAdmin = new() {
                    UserName = name,
                    Email = email,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                await _userManager.CreateAsync(userAdmin, _configuration["DefaultPasswords:Administrator"]
                    ?? throw new InvalidOperationException());
                await _userManager.AddToRolesAsync(userAdmin, new[] { roleUser, roleAdmin });
                userAdmin.EmailConfirmed = true;
                userAdmin.LockoutEnabled = false;
                addedUserList.Add(userAdmin);
            }

            (string name, string email) registered = ("user", "user@email.com");

            if (await _userManager.FindByNameAsync(registered.name) is null) {
                WorldCitiesUser user = new() {
                    UserName = registered.name,
                    Email = registered.email,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                await _userManager.CreateAsync(user, _configuration["DefaultPasswords:RegisteredUser"]
                    ?? throw new InvalidOperationException());
                await _userManager.AddToRoleAsync(user, roleUser);
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
                addedUserList.Add(user);
            }

            if (addedUserList.Count > 0) {
                await _context.SaveChangesAsync();
            }

            return new JsonResult(new {
                addedUserList.Count,
                Users = addedUserList
            });

        }
    }
}
