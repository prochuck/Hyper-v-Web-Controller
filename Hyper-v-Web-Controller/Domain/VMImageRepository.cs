using Hyper_v_Web_Controller.Interfaces;
using Hyper_v_Web_Controller.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyper_v_Web_Controller.Domain
{
    public class VMImageRepository : IVMImageRepository
    {
        AppDBContext dbContext;

        public VMImageRepository(AppDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Create(VMImage item)
        {
            dbContext.VMImages.Add(item);
        }

        public void Delete(int id)
        {
            VMImage vm = dbContext.VMImages.Find(id);
            dbContext.VMImages.Remove(vm);
        }

        public VMImage Get(int id)
        {
            return dbContext.VMImages.Where(e => e.Id == id).First();
        }

        public IEnumerable<VMImage> GetList()
        {
            return dbContext.VMImages;
        }

        public void Save()
        {
            dbContext.SaveChanges();
        }

        public void Update(VMImage item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }
    }
}

    