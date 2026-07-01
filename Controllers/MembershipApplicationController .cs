using Microsoft.AspNetCore.Mvc;
using TwoDPro3.Services;
using TwoDPro3.DTOs;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembershipApplicationController : ControllerBase
    {
        private readonly AgentService _agentService;

        public MembershipApplicationController(AgentService agentService)
        {
            _agentService = agentService;
        }

        // --------------------------------------------------
        // APPLY FOR MEMBERSHIP (MAIN ENTRY FROM MAUI)
        // GET api/apply
        // --------------------------------------------------
        [HttpPost("apply")]
        public async Task<ActionResult<AgentContactResponse>> ApplyMembership([FromBody] ApplyMembershipRequest request)
        {
            try
            {
                if (request.UserId <= 0)
                    return BadRequest("Invalid user id.");

                var result = await _agentService.AssignAgentAsync(request.UserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    // --------------------------------------------------
    // Request DTO (only for this endpoint)
    // --------------------------------------------------
    public class ApplyMembershipRequest
    {
        public int UserId { get; set; }
    }
}