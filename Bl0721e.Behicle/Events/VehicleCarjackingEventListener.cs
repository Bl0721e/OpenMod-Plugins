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
		public Task HandleEventAsync(object? sender, UnturnedVehicleCarjackingEvent @event)
		{
			if (@event.Instigator != null && @event.Vehicle.Ownership.HasOwner && @event.Vehicle.Vehicle.isLocked)
			{
				if (@event.Instigator.SteamId.m_SteamID.ToString() != @event.Vehicle.Ownership.OwnerPlayerId)
				{
					@event.IsCancelled = true;
					@event.Instigator.PrintMessageAsync("你不能对其他玩家的载具使用这个物品", Color.FromName("Crimson"));
				}
			}
			return Task.CompletedTask;
		}
	}
}
