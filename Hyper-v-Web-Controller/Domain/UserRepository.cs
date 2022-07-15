using Hyper_v_Web_Controller.Interfaces;
using Hyper_v_Web_Controller.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyper_v_Web_Controller.Domain
{
    public class UserRepository : IUserRepository
    {
        AppDBContext dbContext;

        public UserRepository(AppDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Create(User item)
        {
            dbContext.Users.Add(item);
        }

        public void Delete(int id)
        {
            User user = dbContext.Users.Find(id);
            dbContext.Remove(user);
        }

        public User? Get(int id)
        {
            return dbContext.Users.Include(e => e.Role).Where(e=>e.Id==id).FirstOrDefault();
        }
        public User? Get(string login)
        {
            return dbContext.Users.Include(e => e.Role).Where(e => e.Login == login).FirstOrDefault();
        }
        public IEnumerable<User> GetList()
        {
            return dbContext.Users.Include(e => e.Role);
        }

        public void Save()
        {
            dbContext.SaveChanges();
        }

        public void Update(User item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }
    }
}
