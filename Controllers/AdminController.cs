using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.DTOs;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly CalendarContext _context;

        public AdminController(CalendarContext context)
        {
            _context = context;
        }

        [HttpGet("contact")]
        public async Task<ActionResult<AdminContactResponse>> GetContact()
        {
            var admin = await _context.AdminContacts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IsActive);

            if (admin == null)
                return NotFound("No active admin found.");

            return Ok(new AdminContactResponse
            {
                AdminName = admin.AdminName,
                TelegramUrl = admin.TelegramUrl,
                Phone = admin.Phone
            });
        }
    }
}