using Hyper_v_Web_Controller.Models;
namespace Hyper_v_Web_Controller.Interfaces
{
    /// <summary>
    /// Переименовать. штука для управления hyper-v
    /// </summary>
    public interface IHyperVThing
    {
        public VM CreateVM();
    }
}
