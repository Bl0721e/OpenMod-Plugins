using System;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Users;
using OpenMod.API.Users;
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
		private readonly IUserDataStore m_UserDataStore;
		private readonly IConfiguration m_Configuration;
		private readonly IStringLocalizer m_StringLocalizer;
		private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;
		public VehicleDamagingEventListener(IUserDataStore userDataStore, IConfiguration configuration, IStringLocalizer stringLocalizer, IUnturnedUserDirectory unturnedUserDirectory)
		{
			m_UserDataStore = userDataStore;
			m_Configuration = configuration;
			m_StringLocalizer = stringLocalizer;
			m_UnturnedUserDirectory = unturnedUserDirectory;
		}
		public async Task HandleEventAsync(object? sender, UnturnedVehicleDamagingEvent @event)
		{
			string message = "";
			Color color = Color.FromName("White");
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
				string fallbackLocale = m_Configuration.GetSection("locale:fallbackLocale").Get<string>()!;
				string locale = await m_UserDataStore.GetUserDataAsync<string>(Player.SteamId.m_SteamID.ToString(), KnownActorTypes.Player, "localePreference") ?? fallbackLocale;
				bool hasAccess = await @event.Vehicle.Ownership.HasAccessAsync(Player);
				if (!(@event.Vehicle.Ownership.HasOwner && @event.Vehicle.Vehicle.isLocked) || hasAccess)
				{
					var health = 0;
					if (@event.Vehicle.Vehicle.health > @event.PendingTotalDamage)
					{
						health = @event.Vehicle.Vehicle.health - @event.PendingTotalDamage;
					}
					else if (@event.DamageOrigin == EDamageOrigin.Trap_Explosion && @event.Vehicle.Vehicle.health > 1)
					{
						@event.PendingTotalDamage = Convert.ToUInt16(@event.Vehicle.Vehicle.health - 1);
						health = 1;
					}
					message = m_StringLocalizer[$"{locale}:event:currentHealth", new { health = health }];

				}
				else
				{
					@event.IsCancelled = true;
					message = m_StringLocalizer[$"{locale}:event:damagingNotAllowed"];
					color = Color.FromName("Crimson");
				}
				if (message == "")
				{
					return;
				}
				await Player.PrintMessageAsync(message, color);
			}
		}
	}
}
