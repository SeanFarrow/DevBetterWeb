﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevBetterWeb.Core;
using DevBetterWeb.Core.Events;
using DevBetterWeb.Core.Interfaces;
using DevBetterWeb.Web.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DevBetterWeb.Web.Pages.Admin
{
    [Authorize(Roles = AuthConstants.Roles.ADMINISTRATORS)]
    public class UserModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserRoleMembershipService _userRoleMembershipService;

        public UserModel(UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            IUserRoleMembershipService userRoleMembershipService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userRoleMembershipService = userRoleMembershipService;
        }

        public IdentityUser? IdentityUser { get; set; }

        public List<IdentityRole> Roles { get; set; } = new List<IdentityRole>();
        public List<SelectListItem> RolesNotAssignedToUser { get; set; } = new List<SelectListItem>();


        public async Task<IActionResult> OnGetAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                NotFound();
            }

            var currentUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (currentUser == null)
            {
                return BadRequest();
            }

            var roles = await _roleManager.Roles.ToListAsync();

            var unassignedRoles = new List<IdentityRole>();
            var assignedRoles = new List<IdentityRole>();
            foreach (var role in roles)
            {
                if (! (await _userManager.GetUsersInRoleAsync(role.Name)).Contains(currentUser))
                {
                    unassignedRoles.Add(role);
                }
                else
                {
                    assignedRoles.Add(role);
                }
            }

            IdentityUser = currentUser;
            RolesNotAssignedToUser = unassignedRoles.Select(x => new SelectListItem(x.Name, x.Id)).ToList();
            Roles = assignedRoles.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAddUserToRoleAsync(string userId, string roleId)
        {
            await _userRoleMembershipService.AddUserToRoleAsync(userId, roleId);

            return RedirectToPage("./User", new { userId = userId });
        }

        public async Task<IActionResult> OnPostRemoveUserFromRole(string userId, string roleId)
        {
            await _userRoleMembershipService.RemoveUserFromRoleAsync(userId, roleId);

            return RedirectToPage("./User", new { userId = userId});
        }
    }
}