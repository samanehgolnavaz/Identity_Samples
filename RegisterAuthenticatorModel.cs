using System.ComponentModel.DataAnnotations;

namespace Identity_Samples
{
    public class RegisterAuthenticatorModel
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public string AuthenticatorKey { get; set; }
    }
}
