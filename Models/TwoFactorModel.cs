using System.ComponentModel.DataAnnotations;

namespace Identity_Samples.Models
{
    public class TwoFactorModel
    {
        [Required]
        public string Token { get; set; }
    }
}
