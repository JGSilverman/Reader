using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.Domain
{
    public class AppUser : IdentityUser
    {
        public DateTime JoinedOn { get; set; } = DateTime.Now;
        public bool TermsAndConditionsAgreedTo { get; set; }
        public DateTime TermsAndConditionsAgreedToOn { get; set; }
        public bool EmailNotificationsEnabled { get; set; } = true;
        public DateTime PasswordLastChanged { get; set; }
    }
}
