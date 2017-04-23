using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Blog.Models
{
    public class ApplicationUser : IdentityUser
    {
        private ICollection<Comment> comments;
        public ApplicationUser()
        {
            this.comments = new HashSet<Comment>();
        }

        [Reqired]
        public string FullName { get; set; }

        public virtual ICollection<Comment> Comments
        {
            get { return comments; }
            set { comments = value; }
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }
}