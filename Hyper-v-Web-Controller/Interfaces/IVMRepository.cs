using Hyper_v_Web_Controller.Models;
namespace Hyper_v_Web_Controller.Interfaces
{
    public interface IVMRepository
    {
        IEnumerable<VM> GetList();
        IEnumerable<VM> GetList(int userId);
        VM Get(int id);
        void Create(VM item);
        void Update(VM item);
        void Delete(int id);
        void Save();
    }
}
