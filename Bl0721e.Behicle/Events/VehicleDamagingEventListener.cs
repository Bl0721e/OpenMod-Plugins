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
	public class VehicleDamagingEventListener : IEventListener<UnturnedVehicleDamagingEvent>
	{
		private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;
		public VehicleDamagingEventListener(IUnturnedUserDirectory unturnedUserDirectory)
		{
			m_UnturnedUserDirectory = unturnedUserDirectory;
		}
		public Task HandleEventAsync(object? sender, UnturnedVehicleDamagingEvent @event)
		{
			if (@event.Vehicle.Ownership.HasOwner && @event.Vehicle.Vehicle.isLocked)
			{
				if (@event.Instigator == null)
				{
					List<EDamageOrigin> allowedOrigins = new List<EDamageOrigin>
					{
						EDamageOrigin.Mega_Zombie_Boulder,
						EDamageOrigin.Zombie_Swipe,
						EDamageOrigin.Radioactive_Zombie_Explosion,
						EDamageOrigin.Zombie_Electric_Shock,
						EDamageOrigin.Zombie_Stomp,
						EDamageOrigin.Zombie_Fire_Breath,
						EDamageOrigin.Vehicle_Collision_Self_Damage,
						EDamageOrigin.Lightning,
						EDamageOrigin.VehicleDecay
					};
					if(!allowedOrigins.Contains(@event.DamageOrigin))
					{
						@event.IsCancelled = true;
					}
				}
				else
				{
					CSteamID @Instigator = @event.Instigator ?? throw new InvalidOperationException("Object was null");
					UnturnedPlayer Player = m_UnturnedUserDirectory.FindUser(@Instigator)!.Player;
					if (Player.SteamId.m_SteamID.ToString() != @event.Vehicle.Ownership.OwnerPlayerId || @event.DamageOrigin == EDamageOrigin.Trap_Explosion)
					{
						@event.IsCancelled = true;
					}
				}
			}
			return Task.CompletedTask;
		}
	}
}
