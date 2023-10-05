using System.ComponentModel.DataAnnotations;

namespace Identity_Samples.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
