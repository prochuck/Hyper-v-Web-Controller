using Hyper_v_Web_Controller.Models;
namespace Hyper_v_Web_Controller.Interfaces
{
    public interface IUserRepository
    {
        IEnumerable<User> GetList();
        User? Get(int id);
        User? Get(string login);
        void Create(User item);
        void Update(User item);
        void Delete(int id);
        void Save();
    }
}
