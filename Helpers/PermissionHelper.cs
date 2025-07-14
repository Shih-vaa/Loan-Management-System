using LoanManagementSystem.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace LoanManagementSystem.Helpers
{
    public static class PermissionHelper
    {
        /// <summary>
        /// Checks if the current logged-in user has the given permission based on their team membership.
        /// </summary>
        /// <param name="context">EF Core database context</param>
        /// <param name="httpContext">Current HTTP request context</param>
        /// <param name="permissionType">One of: CanManageLeads, CanUploadDocs, CanVerifyDocs</param>
        /// <returns>True if permission is granted, otherwise false</returns>
        public static bool HasTeamPermission(ApplicationDbContext context, HttpContext httpContext, string permission)
        {
            int userId = int.Parse(httpContext.User.Claims.First(c => c.Type == "UserId").Value);

            var teamMemberships = context.TeamMembers.Where(tm => tm.UserId == userId);
            var member = context.TeamMembers.FirstOrDefault(tm => tm.UserId == userId);

            return permission switch
            {
                "CanVerifyDocs" => teamMemberships.Any(tm => tm.CanVerifyDocs),
                "CanUploadDocs" => teamMemberships.Any(tm => tm.CanUploadDocs),
                "CanManageLeads" => teamMemberships.Any(tm => tm.CanManageLeads),
                _ => false
            };
        }


    }
}
