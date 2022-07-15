using System.ComponentModel.DataAnnotations;

namespace Hyper_v_Web_Controller.Models
{
    public class Role
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string RoleName { get; set; }
    }
}
