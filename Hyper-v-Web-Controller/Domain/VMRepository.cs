using Hyper_v_Web_Controller.Interfaces;
using Hyper_v_Web_Controller.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyper_v_Web_Controller.Domain
{
    public class VMRepository : IVMRepository
    {
        AppDBContext dbContext;


        public VMRepository(AppDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Create(VM item)
        {
            dbContext.VMs.Add(item);
        }

        public void Delete(int id)
        {
            VM vm = dbContext.VMs.Find(id);
            dbContext.Remove(vm);
        }

        public VM Get(int id)
        {
            return dbContext.VMs.Include(e => e.Creator).Include(e=>e.RealizedVMImage).Where(e => e.Id == id).First();
        }

        public IEnumerable<VM> GetList()
        {
            return dbContext.VMs.Include(e => e.Creator).Include(e => e.RealizedVMImage);
        }
        /// <summary>
        /// получает машины пользователя по его id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<VM> GetList(int userId)
        {
            return dbContext.VMs.Include(e => e.Creator).Include(e => e.RealizedVMImage).Where(e => e.CreatorId == userId);
        }

        public void Save()
        {
            dbContext.SaveChanges();
        }

        public void Update(VM item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }
    }
}

   

