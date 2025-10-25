using System;
using System.Threading.Tasks;
using System.Drawing;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Vehicles.Events;
using System.Collections.Generic;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Players;
using SDG.Unturned;
using Steamworks;

namespace Bl0721e.Behicle.Events
{
	public class VehicleCarjackingEventListener : IEventListener<UnturnedVehicleCarjackingEvent>
	{
		public async Task HandleEventAsync(object? sender, UnturnedVehicleCarjackingEvent @event)
		{
			string message = "";
			if (@event.Instigator != null && @event.Vehicle.Ownership.HasOwner && @event.Vehicle.Vehicle.isLocked)
			{
				bool hasAccess = await @event.Vehicle.Ownership.HasAccessAsync(@event.Instigator);
				if (!hasAccess)
				{
					@event.IsCancelled = true;
					message = "你不能对其他玩家的载具使用这个物品";
				}
				if (message == "")
				{
					return;
				}
				await @event.Instigator.PrintMessageAsync(message, Color.FromName("Crimson"));
			}
		}
	}
}
