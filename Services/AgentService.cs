using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.DTOs;
using TwoDPro3.Models;

namespace TwoDPro3.Services
{
    public class AgentService
    {
        private readonly AppDbContext _db;

        public AgentService(AppDbContext db)
        {
            _db = db;
        }

        // --------------------------------------------------
        // 1. Get already assigned agent (read-only)
        // --------------------------------------------------
        public async Task<AgentContactResponse?> GetAssignedAgentAsync(int userId)
        {
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new Exception("User not found.");

            if (user.AgentId == null)
                return null;

            var agent = await _db.Agents
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == user.AgentId);

            if (agent == null)
                throw new Exception("Assigned agent not found.");

            return new AgentContactResponse
            {
                AgentId = agent.Id,
                AgentName = agent.Name,
                TelegramUrl = agent.TelegramUrl
            };
        }

        // --------------------------------------------------
        // 2. ROUND ROBIN - Get next agent
        // --------------------------------------------------
        public async Task<Agent> GetNextRoundRobinAgentAsync()
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // 1. Get rotation state (single row)
                var rotation = await _db.AgentRotations
                    .FirstOrDefaultAsync(x => x.Id == 1);

                if (rotation == null)
                {
                    rotation = new AgentRotation
                    {
                        Id = 1,
                        LastAgentId = null
                    };

                    _db.AgentRotations.Add(rotation);
                    await _db.SaveChangesAsync();
                }

                // 2. Get active agents
                var agents = await _db.Agents
                    .Where(a => a.IsActive)
                    .OrderBy(a => a.Id)
                    .ToListAsync();

                if (!agents.Any())
                    throw new Exception("No active agents found.");

                // 3. Select next agent
                Agent nextAgent;

                if (rotation.LastAgentId == null)
                {
                    nextAgent = agents.First();
                }
                else
                {
                    var lastIndex = agents.FindIndex(a => a.Id == rotation.LastAgentId);

                    if (lastIndex == -1 || lastIndex == agents.Count - 1)
                        nextAgent = agents.First();
                    else
                        nextAgent = agents[lastIndex + 1];
                }

                // 4. Update rotation
                rotation.LastAgentId = nextAgent.Id;

                _db.AgentRotations.Update(rotation);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return nextAgent;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // --------------------------------------------------
        // 3. MAIN FLOW - Assign agent + create application
        // --------------------------------------------------
        public async Task<AgentContactResponse> AssignAgentAsync(int userId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // 1. Get user
                var user = await _db.Users
                    .FirstOrDefaultAsync(x => x.Id == userId);

                if (user == null)
                    throw new Exception("User not found.");

                // 2. If already assigned → return existing agent
                if (user.AgentId != null)
                {
                    var existingAgent = await _db.Agents
                        .FirstOrDefaultAsync(x => x.Id == user.AgentId);

                    return new AgentContactResponse
                    {
                        AgentId = existingAgent!.Id,
                        AgentName = existingAgent.Name,
                        TelegramUrl = existingAgent.TelegramUrl
                    };
                }

                // 3. Get next agent (ROUND ROBIN)
                var agent = await GetNextRoundRobinAgentAsync();

                // 4. Assign agent to user
                user.AgentId = agent.Id;

                _db.Users.Update(user);

                // 5. Create membership application
                var application = new MembershipApplication
                {
                    UserId = user.Id,
                    AgentId = agent.Id,
                    Status = "Pending",
                    AppliedAt = DateTime.UtcNow
                };

                _db.MembershipApplications.Add(application);

                // 6. Save all
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // 7. Return to MAUI
                return new AgentContactResponse
                {
                    AgentId = agent.Id,
                    AgentName = agent.Name,
                    TelegramUrl = agent.TelegramUrl
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}