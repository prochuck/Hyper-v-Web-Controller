using Hyper_v_Web_Controller.Interfaces;
using Hyper_v_Web_Controller.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyper_v_Web_Controller.Domain
{
    public class VMRepository : IVMRepository
    {
        AppDBContext dbContext;
        IHyperVThing hyperVThing;

        public VMRepository(AppDBContext dbContext, IHyperVThing hyperVThing)
        {
            this.dbContext = dbContext;
            this.hyperVThing = hyperVThing;
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
            VM vM = dbContext.VMs.Include(e => e.Creator).Include(e => e.RealizedVMImage).Where(e => e.Id == id).First();
            vM.machineState = hyperVThing.GetVMState(vM);
            if (vM.machineState == VMState.Enabled)
            {
                vM.machineState = hyperVThing.GetVMState(vM);
                string ip = null;
                if (vM.machineState == VMState.Enabled)
                {
                    ip = hyperVThing.GetIpForVM(vM, 1);
                    if (ip is null)
                    {
                        vM.machineState = VMState.Starting;
                    }
                    else
                    {
                        vM.ip = ip;
                    }
                }
            }
            return vM;
        }

        public IEnumerable<VM> GetList()
        {
            IEnumerable<VM> vMs = dbContext.VMs.Include(e => e.Creator).Include(e => e.RealizedVMImage);
            foreach(VM vM in vMs)
            {
                vM.machineState = hyperVThing.GetVMState(vM);
                string ip = null;
                if (vM.machineState == VMState.Enabled)
                {
                    ip = hyperVThing.GetIpForVM(vM,1);
                    if (ip is null)
                    {
                        vM.machineState = VMState.Starting;
                    }
                    else
                    {
                        vM.ip=ip;
                    }
                }
            }
            

            return vMs;
        }
        /// <summary>
        /// получает машины пользователя по его id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<VM> GetList(int userId)
        {
            IEnumerable<VM> vMs = dbContext.VMs.Include(e => e.Creator).Include(e => e.RealizedVMImage).Where(e => e.CreatorId == userId);
            foreach (VM vM in vMs)
            {
                vM.machineState = hyperVThing.GetVMState(vM);
                string ip = null;
                if (vM.machineState == VMState.Enabled)
                {
                    ip = hyperVThing.GetIpForVM(vM, 1);
                    if (ip is null)
                    {
                        vM.machineState = VMState.Starting;
                    }
                    else
                    {
                        vM.ip = ip;
                    }
                }
            }
            return vMs;
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



