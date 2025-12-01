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

        // 1. Tüm hocaları getiren metod
        // Adres: /api/TrainersApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Trainer>>> GetTrainers()
        {
            return await _context.Trainers.ToListAsync();
        }

        // 2. Filtreleme yapan metod (Arama kutusu bunu kullanacak)
        // Adres: /api/TrainersApi/Filter?specialization=Fitness
        [HttpGet("Filter")]
        public async Task<ActionResult<IEnumerable<Trainer>>> FilterTrainers(string specialization)
        {
            if (string.IsNullOrEmpty(specialization))
            {
                // Arama kutusu boşsa hepsini getir
                return await _context.Trainers.ToListAsync();
            }

            // Arama kelimesi varsa filtrele (LINQ Sorgusu)
            var trainers = await _context.Trainers
                                         .Where(t => t.Specialization.Contains(specialization))
                                         .ToListAsync();
            return trainers;
        }
    }
}