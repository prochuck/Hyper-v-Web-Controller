using Hyper_v_Web_Controller.Models;
using Hyper_v_Web_Controller.Services;
using System.Management;

namespace Hyper_v_Web_Controller.Interfaces
{
    //todo прикрутить сюда авторизацию для пользователей.
    /// <summary>
    /// Переименовать. штука для управления hyper-v
    /// </summary>
    public interface IHyperVThing
    {       
  
        
        public void CreateVM(VMImage vMImage, VM vm,string userName);
        public bool DeleteVM(VM vm, string userName);
        public bool CreateSnapshot(VM vm);
        public bool RollbackMachine(VM VMToRollback,int snapShotId);
        public string? TurnOnVM(VM vm);
        public bool TurnOffVM(VM vm);
        public VMState GetVMState(VM vm);
        public string GetIpForVM(VM vM);
    }
}
