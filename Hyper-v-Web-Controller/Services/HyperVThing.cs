﻿using Hyper_v_Web_Controller.Interfaces;
using Hyper_v_Web_Controller.Models;
using System.Management;
using System.Globalization;

namespace Hyper_v_Web_Controller.Services
{
	public class HyperVThing : IHyperVThing
	{
		string path=null;
		public HyperVThing(IConfiguration configuration)
        {
			path = configuration["VMFolder"];
		}
		static ManagementScope scope = new ManagementScope(@"\root\virtualization\v2", null); //Путь к API Hyper-V
		public VMImage[] GetVMImages() //Возвращаяет доступные образы для клонирования
		{
			return null;
		}
		public VM[] GetUserVMS(User user) //Возвращаяет ВМ пользователя
		{
			return null;
		}
		public VM CreateVM(VMImage vMImage, string machineName,string userName) //Возвращаяет созданную ВМ
		{
			//VMImage userVMimage = null;
			VM userVM = null;


			//получение vsms - при наличии vsms удалить
			ManagementObject virtualSystemService = null;
			string vmQueryWql2 = string.Format(CultureInfo.InvariantCulture,
				   "SELECT * FROM {0}", "Msvm_VirtualSystemManagementService");
			SelectQuery vmQuery2 = new SelectQuery(vmQueryWql2);
			using (ManagementObjectSearcher vmSearcher2 = new ManagementObjectSearcher(scope, vmQuery2))
			using (ManagementObjectCollection vmCollection2 = vmSearcher2.Get())
			{ virtualSystemService = vmCollection2.Cast<ManagementObject>().First(); }
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
			userVM = new VM() {VmName = machineName, RealizedVMImageId = vMImage.Id};
			if (!System.IO.Directory.Exists(path + @"\" + userName + @"\" + machineName))
				System.IO.Directory.CreateDirectory(path + @"\" + userName + @"\" + machineName);
			else return null;
			if (System.IO.Directory.Exists(vMImage.Path + @"\Virtual Hard Disks"))
			{
				string[] files = System.IO.Directory.GetFiles(vMImage.Path + @"\Virtual Hard Disks", "*", SearchOption.AllDirectories);
				// Copy the files and overwrite destination files if they already exist.					
				foreach (string s in files)
				 System.IO.File.Copy(s, System.IO.Path.Combine(path + @"\" + userName + @"\" + machineName, System.IO.Path.GetFileName(s)), true);				
			}
			else return null;


			//Изменить путь диска
			ManagementObject[] disks = planVM.GetRelated("Msvm_VirtualSystemSettingData").Cast<ManagementObject>().First()
				 .GetRelated("Msvm_ResourceAllocationSettingData").Cast<ManagementObject>()
				 .Where(e => e.Properties.Cast<PropertyData>().Where(e2 => e2.Value as string == "Параметры виртуального жесткого диска (Майкрософт).").Count() != 0)
				 .Select(e => e.GetRelated("Msvm_StorageAllocationSettingData").Cast<ManagementObject>().First()).ToArray();
			ManagementObject @object = disks[0]; //Вроде бы работает только для одного VHD!!!!
			@object.SetPropertyValue("HostResource", new string[] { Directory.GetFiles(path + @"\" + userName + @"\" + machineName).Where(e => Path.GetExtension(e).ToLower() == ".vhdx").First() });
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
				VMname.SetPropertyValue("ElementName", machineName);
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
			return userVM;
		}
		public bool DeleteVM(VM vm, string userName) //Возвращаяет сообщение об успехе проведения удаления ВМ
		{
			//получение vsms
			ManagementObject virtualSystemService = null;
			string vmQueryWql2 = string.Format(CultureInfo.InvariantCulture,
				   "SELECT * FROM {0}", "Msvm_VirtualSystemManagementService");
			SelectQuery vmQuery2 = new SelectQuery(vmQueryWql2);
			using (ManagementObjectSearcher vmSearcher2 = new ManagementObjectSearcher(scope, vmQuery2))
			using (ManagementObjectCollection vmCollection2 = vmSearcher2.Get())
			{ virtualSystemService = vmCollection2.Cast<ManagementObject>().First(); }
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
		public bool TurnOnVM(VM vm) //Возвращаяет сообщение об успехе включения ВМ
		{
			return false;
		}
		public bool TurnOffVM(VM vm) //Возвращаяет сообщение об успехе выключения ВМ
		{
			return false;
		}

		//=================Вспомогательные классы и методы==========================

		static class JobState
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
		/*public static class ReturnCode
		{
			public const UInt32 Completed = 0;
			public const UInt32 Started = 4096;
			public const UInt32 Failed = 32768;
			public const UInt32 AccessDenied = 32769;
			public const UInt32 NotSupported = 32770;
			public const UInt32 Unknown = 32771;
			public const UInt32 Timeout = 32772;
			public const UInt32 InvalidParameter = 32773;
			public const UInt32 SystemInUse = 32774;
			public const UInt32 InvalidState = 32775;
			public const UInt32 IncorrectDataType = 32776;
			public const UInt32 SystemNotAvailable = 32777;
			public const UInt32 OutofMemory = 32778;
		}*/
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
		private static ManagementObject  GetVmObject(string name,  string className,  ManagementScope scope)
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
				if (JobCompleted(outParams, scope))
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
		static bool JobCompleted(ManagementBaseObject outParams, ManagementScope scope)
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
				System.Threading.Thread.Sleep(1000);
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
