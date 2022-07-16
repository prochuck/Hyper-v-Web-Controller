﻿using Hyper_v_Web_Controller.Models;
using System.Management;

namespace Hyper_v_Web_Controller.Interfaces
{
    //todo прикрутить сюда авторизацию для пользователей.
    /// <summary>
    /// Переименовать. штука для управления hyper-v
    /// </summary>
    public interface IHyperVThing
    {       
        public VMImage[] GetVMImages();
        public VM[] GetUserVMS(User user);
        public VM CreateVM(VMImage vMImage, string machineName,string userName);
        public bool DeleteVM(VM vm);
        public bool CreateSnapshot(VM vm);
        public bool RollbackMachine(VM VMToRollback,int snapShotId);
        public bool TurnOnVM(VM vm);
        public bool TurnOffVM(VM vm);

    }
}
