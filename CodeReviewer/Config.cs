using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Security;
using CodeReviewer.Models;
using WebMatrix.WebData;

namespace CodeReviewer
{
    internal sealed class Configuration : DbMigrationsConfiguration<CodeReviewer.Models.CodeReviewerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(CodeReviewer.Models.CodeReviewerContext context)
        {
            // TODO: Lookup CodeReviewerContext
            WebSecurity.InitializeDatabaseConnection("CodeReviewerContext", "WebUser", "Id", "Email", true);
            var roles = (SimpleRoleProvider)Roles.Provider;
            var membership = (SimpleMembershipProvider)Membership.Provider;

            if (!roles.RoleExists("Admin"))
            {
                roles.CreateRole("Admin");
            }
            if (membership.GetUser("admin", false) == null)
            {
                membership.CreateUserAndAccount("admin", "sa");
            }
            if (!roles.GetRolesForUser("admin").Contains("Admin"))
            {
                roles.AddUsersToRoles(new[] { "sallen" }, new[] { "admin" });
            }
        }
    }
}
