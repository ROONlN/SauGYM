using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SauGYM.Data;
using SauGYM.Models;

namespace SauGYM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{search?}")]
        public async Task<ActionResult<IEnumerable<Trainer>>> GetTrainers(string? search)
        {
            var query = _context.Trainers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Specialization.Contains(search) || t.FullName.Contains(search));
            }

            return await query.ToListAsync();
        }
    }
}