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

        public static bool AreUsersInSameTeam(ApplicationDbContext context, int userId1, int userId2)
        {
            var user1TeamIds = context.TeamMembers
                .Where(tm => tm.UserId == userId1)
                .Select(tm => tm.TeamId)
                .ToList();

            var user2TeamIds = context.TeamMembers
                .Where(tm => tm.UserId == userId2)
                .Select(tm => tm.TeamId)
                .ToList();

            // ✅ Check if any team ID matches
            return user1TeamIds.Intersect(user2TeamIds).Any();
        }
public static List<User> GetOfficeMembersInSameTeam(ApplicationDbContext context, int callingUserId)
{
    // Step 1: Get all team IDs the calling user belongs to
    var teamIds = context.TeamMembers
        .Where(tm => tm.UserId == callingUserId)
        .Select(tm => tm.TeamId)
        .Distinct()
        .ToList();

    // Step 2: Get all office users who are in any of those teams
    var officeUsers = context.TeamMembers
        .Where(tm => teamIds.Contains(tm.TeamId))
        .Select(tm => tm.User)
        .Where(u => u.Role == "office")
        .Distinct()
        .ToList();

    return officeUsers;
}

    }
}
