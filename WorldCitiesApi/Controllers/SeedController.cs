using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CsvHelper.Configuration;
using WorldCitiesApi.Data;
using WorldModel;
using CsvReader = CsvHelper.CsvReader;

namespace WorldCitiesApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase {
        private readonly UserManager<WorldCitiesUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly WorldCitiesContext _context;
        private readonly string _pathName;

        public SeedController(UserManager<WorldCitiesUser> userManager, RoleManager<IdentityRole> roleManager, 
            IConfiguration configuration, WorldCitiesContext context, IHostEnvironment environment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
            _pathName = Path.Combine(environment.ContentRootPath, "Data/worldcities.csv");
        }

        // POST: api/Seed
        [HttpPost("Countries")]
        public async Task<IActionResult> ImportCountries() {
            // create a lookup dictionary containing all the countries already existing 
            // into the Database (it will be empty on first run).
            Dictionary<string, Country> countriesByName = _context.Countries
                .AsNoTracking().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            CsvConfiguration config = new(CultureInfo.InvariantCulture) {
                HasHeaderRecord = true,
                HeaderValidated = null
            };

            using StreamReader reader = new(_pathName);
            using CsvReader csv = new(reader, config);

            IEnumerable<WorldCitiesCsv>? records = csv.GetRecords<WorldCitiesCsv>();
            foreach (WorldCitiesCsv record in records) {
                if (countriesByName.ContainsKey(record.country)) {
                    continue;
                }

                Country country = new() {
                    Name = record.country,
                    Iso2 = record.iso2,
                    Iso3 = record.iso3
                };
                await _context.Countries.AddAsync(country);
                countriesByName.Add(record.country, country);
            }

            await _context.SaveChangesAsync();

            return new JsonResult(countriesByName.Count);
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
