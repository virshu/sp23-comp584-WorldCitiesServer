using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCitiesApi.Dtos;
using WorldModel;

namespace WorldCitiesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly WorldCitiesContext _context;

        public CountriesController(WorldCitiesContext context)
        {
            _context = context;
        }

        // GET: api/Countries
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            List<Country> countries = await _context.Countries.ToListAsync();
            return countries;
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Country>> GetCountry(int id)
        {
            Country? country = await _context.Countries.FindAsync(id);
            return country is null ? NotFound() : country;
        }

        [HttpGet("Population/{id}")]
        public async Task<ActionResult<CountryPopulation>> GetCountryPopulation(int id)
        {
            CountryPopulation? countryPopulation = await _context.Countries.Where(c => c.Id == id)
                .Select(c => new CountryPopulation
                {
                    Id = c.Id,
                    Name = c.Name,
                    Population = c.Cities.Sum(t => t.Population)
                }).SingleOrDefaultAsync();
            return countryPopulation is null ? NotFound() : countryPopulation;
        }

        // PUT: api/Countries/5

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, Country country)
        {
            if (id != country.Id)
            {
                return BadRequest();
            }

            _context.Entry(country).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CountryExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(Country country)
        {
            _context.Countries.Add(country);
          await _context.SaveChangesAsync();

          return CreatedAtAction("GetCountryPopulation", new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            Country? country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CountryExists(int id) => _context.Countries.Any(e => e.Id == id);
    }
}
