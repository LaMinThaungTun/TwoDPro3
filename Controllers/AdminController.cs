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
        public async Task<ActionResult<AdminContactResponse>> GetContact(string contact)
        {

            if (contact != "contact")
                return BadRequest();


            var admin = await _context.AdminContact

                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.is_active);

            if (admin == null)
                return NotFound("No active admin found.");

            return Ok(new AdminContactResponse
            {
                AdminName = admin.admin_name,
                TelegramUrl = admin.telegram_url,
                Phone = admin.phone
            });
        }
    }
}