using Hyper_v_Web_Controller.Interfaces;
using Hyper_v_Web_Controller.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyper_v_Web_Controller.Domain
{
    public class VMImageRepository : IVMImageRepository
    {
        AppDBContext dbContext;
        IConfiguration configuration;
        public VMImageRepository(IConfiguration configuration,AppDBContext dbContext)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
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

        public void UpdateVMImagesList()
        {
            string VmImagesFolder = configuration["VMImagesFolder"];

            string[] vMImagesNames = Directory.GetDirectories(VmImagesFolder).Select(e => Path.GetFileName(e)).ToArray();

            dbContext.VMImages.AddRange(vMImagesNames.Except(dbContext.VMImages.Select(e => e.Name).ToArray())
                .Select(e => new VMImage() { Name = e, Path = VmImagesFolder + "\\" + e }));
            dbContext.VMImages.RemoveRange(dbContext.VMImages.Where(e => !vMImagesNames.Contains(e.Name)));
            dbContext.SaveChanges();
        }
    }
}

    