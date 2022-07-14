using System.ComponentModel.DataAnnotations;

namespace Hyper_v_Web_Controller.Models
{
    public class VM
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string VmName { get; set; }
        public string Ip { get; set; }
        public DateTime CreationTime { get; set; }
        public int UsageTime { get; set; }
        public int CreatorId { get; set; }
        public User Creator { get; set; }
    }
}
