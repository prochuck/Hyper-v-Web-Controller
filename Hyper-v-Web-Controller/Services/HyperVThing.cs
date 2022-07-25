using Hyper_v_Web_Controller.Interfaces;
using Hyper_v_Web_Controller.Models;
using System.Management;
using System.Globalization;
using System.Diagnostics;

namespace Hyper_v_Web_Controller.Services
{
    
    public class HyperVThing : IHyperVThing
    {
        string path = null;
        public HyperVThing(IConfiguration configuration)
        {
            path = configuration["VMFolder"];
        }

        static ManagementScope scope = new ManagementScope(@"\root\virtualization\v2", null); //Путь к API Hyper-V

        public void CreateVM(VMImage vMImage, VM vM, string userName) //Возвращаяет созданную ВМ
        {
            //VMImage userVMimage = null;



            ManagementObject virtualSystemService = GetVSMS();
            ManagementBaseObject inParams = null;
            ManagementBaseObject outParams = null;


            //Импорт штук из виртуалки
            inParams = virtualSystemService.GetMethodParameters("ImportSystemDefinition");
            string FilePath = Directory.GetFiles(vMImage.Path + @"\Virtual Machines").Where(e => Path.GetExtension(e).ToLower() == ".vmcx").First();
            inParams["SystemDefinitionFile"] = FilePath;
            inParams["SnapshotFolder"] = vMImage.Path + @"\Snapshots";
            inParams["GenerateNewSystemIdentifier"] = true;
            outParams = virtualSystemService.InvokeMethod("ImportSystemDefinition", inParams, null);
            WaitForJob(outParams, virtualSystemService, scope);
            ManagementObject planVM = new ManagementObject((string)outParams["ImportedSystem"]);


            //Копирование директории ВМ с новым именем
            //userVMimage = new VMImage() { Name = machineName, Path = vMImage.Path +@"\" + vMImage.Name + @"\" + machineName };

            if (!System.IO.Directory.Exists(path + @"\" + userName + @"\" + vM.VmName))
                System.IO.Directory.CreateDirectory(path + @"\" + userName + @"\" + vM.VmName);
            else throw (new Exception("Путь занят"));
            if (System.IO.Directory.Exists(vMImage.Path + @"\Virtual Hard Disks"))
            {
                string[] files = System.IO.Directory.GetFiles(vMImage.Path + @"\Virtual Hard Disks", "*", SearchOption.AllDirectories);
                // Copy the files and overwrite destination files if they already exist.					
                foreach (string s in files)
                    System.IO.File.Copy(s, System.IO.Path.Combine(path + @"\" + userName + @"\" + vM.VmName, System.IO.Path.GetFileName(s)), true);
            }
            else throw (new Exception("Что-то с файлами пошло не так"));


            //Изменить путь диска
            ManagementObject[] disks = planVM.GetRelated("Msvm_VirtualSystemSettingData").Cast<ManagementObject>().First()
                 .GetRelated("Msvm_ResourceAllocationSettingData").Cast<ManagementObject>()
                 .Where(e => e.Properties.Cast<PropertyData>().Where(e2 => e2.Value as string == "Параметры виртуального жесткого диска (Майкрософт).").Count() != 0)
                 .Select(e => e.GetRelated("Msvm_StorageAllocationSettingData").Cast<ManagementObject>().First()).ToArray();
            ManagementObject @object = disks[0]; //Вроде бы работает только для одного VHD!!!!
            @object.SetPropertyValue("HostResource", new string[] { Directory.GetFiles(path + @"\" + userName + @"\" + vM.VmName).Where(e => Path.GetExtension(e).ToLower() == ".vhdx").First() });
            /*string[] DiscToStr = new string[disks.Count()];
			 for (int i = 0; i < disks.Count(); i++)
			 {
				 disks[i].SetPropertyValue("HostResource", new string[] { vMImage.Path + @"\" + machineName + @"\disk" + i}); //Подразумевается, что диски будут названы по шаблону: {disk0, disk1...}
				 DiscToStr[i] = disks[i].GetText(TextFormat.CimDtd20);
			 }*/


            //применить изменения пути диска 
            inParams = virtualSystemService.GetMethodParameters("ModifyResourceSettings");
            inParams["ResourceSettings"] = new string[] { @object.GetText(TextFormat.CimDtd20) };
            ManagementBaseObject outt = virtualSystemService.InvokeMethod("ModifyResourceSettings", inParams, null);
            WaitForJob(outParams, planVM, scope);


            //Измение названия ВМ
            foreach (ManagementObject VMname in planVM.GetRelated("Msvm_VirtualSystemSettingData").Cast<ManagementObject>())
            {
                VMname.SetPropertyValue("ElementName", vM.VmName);
                inParams = virtualSystemService.GetMethodParameters("ModifySystemSettings");
                inParams["SystemSettings"] = VMname.GetText(TextFormat.CimDtd20);
                ManagementBaseObject outtt = virtualSystemService.InvokeMethod("ModifySystemSettings", inParams, null);
                WaitForJob(outtt, virtualSystemService, scope);
            }


            //реализация системы
            inParams = virtualSystemService.GetMethodParameters("RealizePlannedSystem");
            inParams["PlannedSystem"] = planVM;
            outParams = virtualSystemService.InvokeMethod("RealizePlannedSystem", inParams, null);
            WaitForJob(outParams, virtualSystemService, scope);
        }
        public bool DeleteVM(VM vm, string userName) //Возвращаяет сообщение об успехе проведения удаления ВМ
        {
            ManagementObject virtualSystemService = GetVSMS();
            ManagementBaseObject inParams = null;
            ManagementBaseObject outParams = null;

            //Указазание пути до удаляемой ВМ
            ManagementObject pvm = GetVirtualMachine(vm.VmName, scope);
            inParams = virtualSystemService.GetMethodParameters("DestroySystem");
            inParams["AffectedSystem"] = pvm.Path;

            //Удаление ВМ
            outParams = virtualSystemService.InvokeMethod("DestroySystem", inParams, null);
            if (WaitForJob(outParams, virtualSystemService, scope) == null) return false;

            //Удаление каталога с VHDX
            DirectoryInfo dirInfo = new DirectoryInfo(path + @"/" + userName + @"/" + vm.VmName);
            if (dirInfo.Exists) dirInfo.Delete(true);

            return true;
        }
        public bool CreateSnapshot(VM vm) //Возвращаяет сообщение об успехе создания снимка ВМ
        {
            return false;
        }
        public bool RollbackMachine(VM VMToRollback, int snapShotId) //Возвращаяет сообщение об успехе отката ВМ
        {
            return false;
        }
        public string? TurnOnVM(VM vm) //Возвращаяет сообщение об успехе включения ВМ
        {
            ManagementObject virtualSystem = GetVS(vm.VmName);
            ManagementBaseObject inParams = virtualSystem.GetMethodParameters("RequestStateChange");
            inParams["RequestedState"] = VMState.Enabled;
            virtualSystem.InvokeMethod("RequestStateChange", inParams, null);            
            return GetIpForVM(virtualSystem, scope,30).Result;
        }
        public bool TurnOffVM(VM vm) //Возвращаяет сообщение об успехе выключения ВМ
        {
            ManagementObject virtualSystem = GetVS(vm.VmName);
            ManagementBaseObject inParams = virtualSystem.GetMethodParameters("RequestStateChange");
            inParams["RequestedState"] = VMState.Disabled;
            virtualSystem.InvokeMethod("RequestStateChange", inParams, null);
            return true;
        }
        public VMState GetVMState(VM vm)
        {
            ManagementObject virtualSystem = GetVS(vm.VmName);
            return (VMState)Convert.ToInt32(virtualSystem["EnabledState"]);
        }
        //=================Вспомогательные классы и методы==========================
                
        public static class JobState
        {
            public const UInt16 New = 2;
            public const UInt16 Starting = 3;
            public const UInt16 Running = 4;
            public const UInt16 Suspended = 5;
            public const UInt16 ShuttingDown = 6;
            public const UInt16 Completed = 7;
            public const UInt16 Terminated = 8;
            public const UInt16 Killed = 9;
            public const UInt16 Exception = 10;
            public const UInt16 Service = 11;
        }
        public ManagementObject GetVSMS()//получение VSMS
        {
            ManagementObject virtualSystemService = null;
            string vmQueryWql2 = string.Format(CultureInfo.InvariantCulture,
                   "SELECT * FROM {0}", "Msvm_VirtualSystemManagementService");
            SelectQuery vmQuery2 = new SelectQuery(vmQueryWql2);
            using (ManagementObjectSearcher vmSearcher2 = new ManagementObjectSearcher(scope, vmQuery2))
            using (ManagementObjectCollection vmCollection2 = vmSearcher2.Get())
            { virtualSystemService = vmCollection2.Cast<ManagementObject>().First(); }
            return virtualSystemService;
        }
        public ManagementObject GetVS (string Vm)//получение vs
        {
            ManagementObject virtualSystem = null;
            string vmQueryWql2 = string.Format(CultureInfo.InvariantCulture,
                   "SELECT * FROM {0} where ElementName=\"{1}\"", "Msvm_ComputerSystem", Vm);
            SelectQuery vmQuery2 = new SelectQuery(vmQueryWql2);
            using (ManagementObjectSearcher vmSearcher2 = new ManagementObjectSearcher(scope, vmQuery2))
            using (ManagementObjectCollection vmCollection2 = vmSearcher2.Get())
            { virtualSystem = vmCollection2.Cast<ManagementObject>().First(); }
            return virtualSystem;
        }
        public static ManagementObject GetServiceObject(ManagementScope scope, string serviceName)
        {

            scope.Connect();
            ManagementPath wmiPath = new ManagementPath(serviceName);
            ManagementClass serviceClass = new ManagementClass(scope, wmiPath, null);
            ManagementObjectCollection services = serviceClass.GetInstances();

            ManagementObject serviceObject = null;

            foreach (ManagementObject service in services)
            {
                serviceObject = service;
            }
            return serviceObject;
        }
        public static ManagementObject GetVirtualMachine(string name, ManagementScope scope)
        {
            return GetVmObject(name, "Msvm_ComputerSystem", scope);
        }
        private static ManagementObject GetVmObject(string name, string className, ManagementScope scope)
        {
            string vmQueryWql = string.Format(CultureInfo.InvariantCulture,
                "SELECT * FROM {0} WHERE ElementName=\"{1}\"", className, name);

            SelectQuery vmQuery = new SelectQuery(vmQueryWql);

            using (ManagementObjectSearcher vmSearcher = new ManagementObjectSearcher(scope, vmQuery))
            using (ManagementObjectCollection vmCollection = vmSearcher.Get())
            {
                if (vmCollection.Count == 0)
                {
                    throw new ManagementException(string.Format(CultureInfo.CurrentCulture,
                        "No {0} could be found with name \"{1}\"",
                        className,
                        name));
                }

                //
                // If multiple virtual machines exist with the requested name, return the first 
                // one.
                //
                ManagementObject vm = GetFirstObjectFromCollection(vmCollection);

                return vm;
            }
        }
        public static ManagementObject GetFirstObjectFromCollection(ManagementObjectCollection collection)
        {
            if (collection.Count == 0)
            {
                throw new ArgumentException("The collection contains no objects", "collection");
            }

            foreach (ManagementObject managementObject in collection)
            {
                return managementObject;
            }

            return null;
        }
        static ManagementBaseObject WaitForJob(ManagementBaseObject outParams, ManagementObject managementObject, ManagementScope scope)
        {
            if ((UInt32)outParams["ReturnValue"] == 4096)
            {
                if (JobCompleted(outParams, scope).Result)
                {
                    Console.WriteLine("VM '{0}' were exported successfully.", managementObject["ElementName"]);
                    return outParams;
                }
                else
                {
                    return null;
                    Console.WriteLine("Failed to export VM");
                }
            }
            else if ((UInt32)outParams["ReturnValue"] == 0)
            {

                Console.WriteLine("VM '{0}' were exported successfully.", managementObject["ElementName"]);
                return outParams;
            }
            else
            {

                Console.WriteLine("Export virtual system failed with error:{0}", outParams["ReturnValue"]);
                return null;
            }
            return null;
        }
        static async Task<bool> JobCompleted(ManagementBaseObject outParams, ManagementScope scope)
        {
            bool jobCompleted = true;

            //Retrieve msvc_StorageJob path. This is a full wmi path
            string JobPath = (string)outParams["Job"];
            ManagementObject Job = new ManagementObject(scope, new ManagementPath(JobPath), null);
            //Try to get storage job information
            Job.Get();
            while ((UInt16)Job["JobState"] == JobState.Starting
                || (UInt16)Job["JobState"] == JobState.Running)
            {
                Console.WriteLine("In progress... {0}% completed.", Job["PercentComplete"]);

                await Task.Delay(1000);
                Job.Get();
            }

            //Figure out if job failed
            UInt16 jobState = (UInt16)Job["JobState"];
            if (jobState != JobState.Completed)
            {
                UInt16 jobErrorCode = (UInt16)Job["ErrorCode"];
                Console.WriteLine("Error Code:{0}", jobErrorCode);
                Console.WriteLine("ErrorDescription: {0}", (string)Job["ErrorDescription"]);
                jobCompleted = false;
            }
            return jobCompleted;
        }
        public string GetIpForVM(VM vM)
        {
            return GetIpForVM( this.GetVS(vM.VmName), scope, 3).Result;
        }
        static async Task<string> GetIpForVM(ManagementObject vm, ManagementScope scope,int timeOutTime)
        {
            string ip = null;

            TimeSpan timeOut = TimeSpan.FromSeconds(timeOutTime);


            string vmQueryWql = string.Format(CultureInfo.InvariantCulture,
                   "SELECT * FROM {0} where InstanceID like \"Microsoft:GuestNetwork\\\\{1}\\\\%\"", "Msvm_GuestNetworkAdapterConfiguration", vm["Name"]);
            SelectQuery vmQuery = new SelectQuery(vmQueryWql);
            string managmentPath;
            using (ManagementObjectSearcher vmSearcher = new ManagementObjectSearcher(scope, vmQuery))
            using (ManagementObjectCollection vmCollection = vmSearcher.Get())
            {
                managmentPath = vmCollection.Cast<ManagementObject>().First().Path.Path;
            }
            ManagementObject networkAdaper = new ManagementObject(managmentPath);
            int count =0;
            do
            {
                if (count != 0)
                    await Task.Delay(1000);
                networkAdaper.Get();
                if (((string[])networkAdaper.GetPropertyValue("IPAddresses")).Count()==2)
                {
                    ip = ((string[])networkAdaper.GetPropertyValue("IPAddresses"))[0];
                }
                count++;
            } while (ip is null && TimeSpan.FromSeconds(count)<timeOut);


            return ip;

        }


        /*static string GetConfigOnlyVirtualSystemExportSettingDataInstance(ManagementScope scope)
		{
			ManagementPath settingPath = new ManagementPath("Msvm_VirtualSystemExportSettingData");

			ManagementClass exportSettingDataClass = new ManagementClass(scope, settingPath, null);
			ManagementObject exportSettingData = exportSettingDataClass.CreateInstance();

			// Do not copy VHDs and AVHDs but copy the Snapshot configuration and Saved State information (Runtime information) if present
			exportSettingData["CopySnapshotConfiguration"] = 0;
			exportSettingData["CopyVmRuntimeInformation"] = true;
			exportSettingData["CopyVmStorage"] = false;
			exportSettingData["CreateVmExportSubdirectory"] = true;

			string settingData = exportSettingData.GetText(TextFormat.CimDtd20);

			exportSettingData.Dispose();
			exportSettingDataClass.Dispose();

			return settingData;
		}
		*/
    }
}
