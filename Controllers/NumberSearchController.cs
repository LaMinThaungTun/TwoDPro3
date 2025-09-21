using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwoDPro3.Models;
using TwoDPro3.Services;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NumberSearchController : ControllerBase
    {
        private readonly FourWeeksResend _fourWeeksResend;

        public NumberSearchController(FourWeeksResend fourWeeksResend)
        {
            _fourWeeksResend = fourWeeksResend;
        }

        /// <summary>
        /// Search for a number and return the 4-week combination (week-2, week-1, week, week+1).
        /// Example: api/NumberSearch?number=24&day=Monday&time=AM
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Calendar>>> SearchNumber(
            [FromQuery] int number,
            [FromQuery] string day,
            [FromQuery] string time)
        {
            if (string.IsNullOrEmpty(day) || string.IsNullOrEmpty(time))
                return BadRequest("Day and Time must be provided.");

            var results = await _fourWeeksResend.GetFourWeeksDataAsync(number, day, time);

            if (results == null || results.Count == 0)
                return NotFound("No matching records found.");

            return Ok(results);
        }
    }
}