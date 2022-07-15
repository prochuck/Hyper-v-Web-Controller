using Hyper_v_Web_Controller.Models;

namespace Hyper_v_Web_Controller.Interfaces
{
    public interface IVMImageRepository
    {
        IEnumerable<VMImage> GetList();
        VMImage Get(int id);
        void Create(VMImage item);
        void Update(VMImage item);
        void Delete(int id);
        void Save();
    }
}
