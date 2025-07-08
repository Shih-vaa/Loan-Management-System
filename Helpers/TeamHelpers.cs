// File: Helpers/TeamHelper.cs
using LoanManagementSystem.Data;
using LoanManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Helpers
{
    public static class TeamHelper
    {
        public static (List<Team>, List<TeamMember>) GetUserTeams(ApplicationDbContext context, int userId)
        {
            var teamMemberships = context.TeamMembers
                .Include(tm => tm.Team)
                .Include(tm => tm.User)
                .Where(tm => tm.UserId == userId)
                .ToList();

            var teamIds = teamMemberships.Select(tm => tm.TeamId).ToList();

            var teammates = context.TeamMembers
                .Include(tm => tm.User)
                .Include(tm => tm.Team)
                .Where(tm => teamIds.Contains(tm.TeamId))
                .ToList();

            var teams = teamMemberships.Select(tm => tm.Team).Distinct().ToList();
            return (teams, teammates);
        }
    }
}
