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
        public static bool HasTeamPermission(ApplicationDbContext context, HttpContext httpContext, string permissionType)
        {
            try
            {
                var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return false; // Invalid or missing user ID
                }

                var teamMemberships = context.TeamMembers.Where(tm => tm.UserId == userId);

                return permissionType switch
                {
                    "CanManageLeads" => teamMemberships.Any(tm => tm.CanManageLeads),
                    "CanUploadDocs" => teamMemberships.Any(tm => tm.CanUploadDocs),
                    "CanVerifyDocs" => teamMemberships.Any(tm => tm.CanVerifyDocs),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                // Optional: log error here using Serilog/NLog/etc.
                // logger.LogError(ex, "Permission check failed");

                return false;
            }
        }
    }
}
