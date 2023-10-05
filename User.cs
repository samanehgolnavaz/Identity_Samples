using Microsoft.AspNetCore.Identity;

namespace Identity_Samples
{
    public class User : IdentityUser
    {
        //public string Id { get; set; }  
        //public string UserName { get; set; }
        //public string NormalizedUserName { get; set; }
        //public string PasswordHash { get; set; }
        public string Locale { get; set; } = "en-GB";
        public string OrgId { get; set; }
    }
    public class Organization
    {
        public string  Id { get; set; }
        public string Name { get; set; }
    }
}
