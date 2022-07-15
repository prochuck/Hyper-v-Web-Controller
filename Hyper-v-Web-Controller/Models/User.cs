using System.ComponentModel.DataAnnotations;

namespace Hyper_v_Web_Controller.Models
{
    public class User
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Login { get; set; }
        [Required]
        public string PasswordHash { get; set; }


        [Required]
        public int RoleId { get; set; }
        [Required]
        public Role Role { get; set; }
    }
}
