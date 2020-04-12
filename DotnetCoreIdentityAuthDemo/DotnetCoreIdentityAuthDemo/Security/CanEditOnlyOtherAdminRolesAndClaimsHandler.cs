using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DotnetCoreIdentityAuthDemo.Security
{
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler : AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CanEditOnlyOtherAdminRolesAndClaimsHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageAdminRolesAndClaimsRequirement requirement)
        {
            //below logic to prevent edit of admin himself
            string loggedInAdminId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            //userId is passed in EditUser method as query string , to check logged in user is admin using user id below line of code 
            var routeData = httpContextAccessor.HttpContext.GetRouteData();
            var userIdBeingEdited = routeData?.Values["Id"]?.ToString();
            var adminIdBeingEdited = string.IsNullOrWhiteSpace(userIdBeingEdited) ? string.Empty : userIdBeingEdited;
            if (context.User.IsInRole("admin") && context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") && adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower())
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
