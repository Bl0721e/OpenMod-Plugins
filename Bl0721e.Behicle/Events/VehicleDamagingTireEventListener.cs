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
	public class VehicleDamagingTireEventListener : IEventListener<UnturnedVehicleDamagingTireEvent>
	{
		private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;
		public VehicleDamagingTireEventListener(IUnturnedUserDirectory unturnedUserDirectory)
		{
			m_UnturnedUserDirectory = unturnedUserDirectory;
		}
		public Task HandleEventAsync(object? sender, UnturnedVehicleDamagingTireEvent @event)
		{
			if (@event.Instigator != null && @event.Vehicle.Ownership.HasOwner && @event.Vehicle.Vehicle.isLocked)
			{
				CSteamID @Instigator = @event.Instigator ?? throw new InvalidOperationException("Object was null");
				UnturnedPlayer Player = m_UnturnedUserDirectory.FindUser(@Instigator)!.Player;
				if (Player.SteamId.m_SteamID.ToString() != @event.Vehicle.Ownership.OwnerPlayerId)
				{
					@event.IsCancelled = true;
				}
			}
			return Task.CompletedTask;
		}
	}
}
