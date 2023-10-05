using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Identity_Samples
{
    public class UserClaimPrincipalFactory
        : UserClaimsPrincipalFactory<User>
    {
        public UserClaimPrincipalFactory(UserManager<User> userManager,
            IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
        {
        }
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity=await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("locale", user.Locale));
            return identity;
        }
    }
}



