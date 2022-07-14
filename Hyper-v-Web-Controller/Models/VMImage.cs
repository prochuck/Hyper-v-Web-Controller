using System.ComponentModel.DataAnnotations;


namespace Hyper_v_Web_Controller.Models
{
    /// <summary>
    /// Не совсем образ.
    /// </summary>
    public class VMImage
    {
        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
