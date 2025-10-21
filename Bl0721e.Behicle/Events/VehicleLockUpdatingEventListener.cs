using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Vehicles;
using OpenMod.Extensions.Games.Abstractions.Vehicles;
using OpenMod.Unturned.Vehicles.Events;
using OpenMod.Unturned.Users;
using OpenMod.API.Users;
using SDG.Unturned;

namespace Bl0721e.Behicle.Events
{
	public class VehicleLockUpdatingEventListener : IEventListener<UnturnedVehicleLockUpdatingEvent>
	{
		private readonly IVehicleDirectory m_VehicleDirectory;
		private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;
		private readonly IUserDataStore m_UserDataStore;
		private readonly IConfiguration m_Configuration;
		public VehicleLockUpdatingEventListener(IVehicleDirectory vehicleDirectory, IUnturnedUserDirectory unturnedUserDirectory, IUserDataStore iUserDataStore, IConfiguration configuration)
		{
			m_VehicleDirectory = vehicleDirectory;
			m_UnturnedUserDirectory = unturnedUserDirectory;
			m_UserDataStore = iUserDataStore;
			m_Configuration = configuration;
		}
		public async Task HandleEventAsync(object? sender, UnturnedVehicleLockUpdatingEvent @event)
		{
			string message = "";
			Color color = Color.FromName("White");
			InteractableVehicle interactableVehicle = VehicleManager.findVehicleByNetInstanceID(UInt32.Parse(@event.Vehicle.VehicleInstanceId));
			var wasNaturallySpawned = typeof(InteractableVehicle).GetField("_wasNaturallySpawned", BindingFlags.Instance | BindingFlags.NonPublic);
			if (!@event.IsLocking)
			{
				if (@event.Vehicle.Asset.VehicleType == "train")
				{
					@event.IsCancelled = true;
					message = "你不能锁定这个载具";
					color = Color.FromName("Crimson");
				}
				else
				{
					IReadOnlyCollection<IVehicle> VehicleDirectory = await m_VehicleDirectory.GetVehiclesAsync();
					//var count = VehicleDirectory.Count(v => VehicleManager.findVehicleByNetInstanceID(UInt32.Parse(v.VehicleInstanceId)).lockedOwner == @event.Instigator.SteamId);
					var count = VehicleDirectory.Count(v => {
							InteractableVehicle Vehicle = VehicleManager.findVehicleByNetInstanceID(UInt32.Parse(v.VehicleInstanceId));
							return Vehicle.lockedOwner == @event.Instigator.SteamId && Vehicle.isLocked && !Vehicle.isExploded;
							});
					int limit = await m_UserDataStore.GetUserDataAsync<int>(@event.Instigator.SteamId.m_SteamID.ToString(), "Player", "behicle_limit");
					int max_limit = m_Configuration.GetSection("max_limit").Get<int>();
					int init_limit = m_Configuration.GetSection("init_limit").Get<int>();
					int price_init = m_Configuration.GetSection("price_init").Get<int>();
					int price_increment = m_Configuration.GetSection("price_increment").Get<int>();
					if (limit == 0 && init_limit != 0)
					{
						await m_UserDataStore.SetUserDataAsync<int>(@event.Instigator.SteamId.m_SteamID.ToString(), "Player", "behicle_limit", init_limit);
						limit = init_limit;
					}
					if (count < limit)
					{
						message = $"你已锁定{count+1}/{limit}个载具";
						wasNaturallySpawned.SetValue(interactableVehicle, false);
					}
					else
					{
						@event.IsCancelled = true;
						message = $"你锁定的载具数量已达到上限({count}/{limit})\n使用命令'/behicle p'以扩充上限";
						color = Color.FromName("Crimson");
					}
				}
			}
			else
			{
				wasNaturallySpawned.SetValue(interactableVehicle, true);
			}
			if (message == "")
			{
				return;
			}
//			var tasks = m_UnturnedUserDirectory.GetOnlineUsers().Select(item => item.PrintMessageAsync(message, color)).ToList();
//			await Task.WhenAll(tasks);
			await @event.Instigator.PrintMessageAsync(message, color);
		}
	}
}
