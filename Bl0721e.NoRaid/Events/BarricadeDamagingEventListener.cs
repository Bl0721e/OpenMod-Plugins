using System;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using OpenMod.API.Eventing;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Building.Events;
using SDG.Unturned;
using OpenMod.Unturned.Users;
using Steamworks;
using UnityEngine;

using Color = System.Drawing.Color;

namespace Bl0721e.NoRaid.Events
{
	public class BarricadeDamagingEventHandler : IEventListener<UnturnedBarricadeDamagingEvent>
	{
		private readonly IUserDataStore m_UserDataStore;
		private readonly IConfiguration m_Configuration;
		private readonly IStringLocalizer m_StringLocalizer;
		public BarricadeDamagingEventHandler(IUserDataStore userDataStore, IConfiguration configuration, IStringLocalizer stringLocalizer)
		{
			m_UserDataStore = userDataStore;
			m_Configuration = configuration;
			m_StringLocalizer = stringLocalizer;
		}
		[EventListener(Priority = EventListenerPriority.Lowest)]
		public async Task HandleEventAsync(object? sender, UnturnedBarricadeDamagingEvent @event)
		{
			string message = "";
			Color color = Color.FromName("White");
			List<EDamageOrigin> allowedOrigins = new List<EDamageOrigin> {
				EDamageOrigin.Carepackage_Timeout,
				EDamageOrigin.Charge_Self_Destruct,
				EDamageOrigin.Zombie_Fire_Breath,
				EDamageOrigin.Mega_Zombie_Boulder,
				EDamageOrigin.Trap_Wear_And_Tear,
				EDamageOrigin.Plant_Harvested,
				EDamageOrigin.Zombie_Swipe,
				EDamageOrigin.Radioactive_Zombie_Explosion,
				EDamageOrigin.Flamable_Zombie_Explosion,
				EDamageOrigin.Zombie_Electric_Shock,
				EDamageOrigin.Zombie_Stomp,
				EDamageOrigin.Horde_Beacon_Self_Destruct
			};
			if (@event.Instigator == null)
			{
				if (!allowedOrigins.Contains(@event.DamageOrigin))
				{
					@event.IsCancelled = true;
				}
			}
			else if (@event.DamageOrigin == EDamageOrigin.Vehicle_Bumper && @event.Instigator.CurrentVehicle!.Asset.VehicleType == "train")
			{
				return;
			}
			else
			{
				var position = @event.Buildable.Transform.Position;
				bool hasAccess = await @event.Buildable.Ownership.HasAccessAsync(@event.Instigator);
				Vector3 v3 = new UnityEngine.Vector3(position.X, position.Y, position.Z);
				bool ignoreNav = false;
				bool parentIsOthersVehicle = false;
				string fallbackLocale = m_Configuration.GetSection("locale:fallbackLocale").Get<string>()!;
				string locale = await m_UserDataStore.GetUserDataAsync<string>(@event.Instigator.SteamId.m_SteamID.ToString(), KnownActorTypes.Player, "localePreference") ?? fallbackLocale;
				if (@event.Buildable.BarricadeDrop.model != null && @event.Buildable.BarricadeDrop.model.parent != null)
				{
					ignoreNav = @event.Buildable.BarricadeDrop.model.parent.CompareTag("Vehicle");
					BarricadeRegion region;
					byte x;
					byte y;
					ushort plant;
					BarricadeManager.tryGetRegion(@event.Buildable.BarricadeDrop.model, out x, out y, out plant, out region);
					InteractableVehicle vehicle = BarricadeManager.getVehicleFromPlant(plant);
					if (vehicle.asset.engine.ToString() == "TRAIN" || !vehicle.isLocked || @event.Instigator.SteamId.m_SteamID == vehicle.lockedOwner.m_SteamID || (@event.Instigator.Player.quests.groupID != CSteamID.Nil && @event.Instigator.Player.quests.groupID.m_SteamID == vehicle.lockedGroup.m_SteamID))
					{
						parentIsOthersVehicle = true;
					}
				}
				if (!@event.Buildable.Ownership.HasOwner || hasAccess || (LevelNavigation.checkSafeFakeNav(v3) && !ignoreNav) || parentIsOthersVehicle)
				{
					var health = 0.0;
					if (@event.Buildable.State.Health > @event.DamageAmount)
					{
						health = @event.Buildable.State.Health - @event.DamageAmount;
					}
					message = m_StringLocalizer[$"{locale}:currentHealth", new { health = health }];
				}
				else
				{
					@event.IsCancelled = true;
					message = m_StringLocalizer[$"{locale}:damagingNotAllowed"];
					color = Color.FromName("Crimson");
				}
			}
			if (message == "")
			{
				return;
			}
			await @event.Instigator!.PrintMessageAsync(message, color);
		}
	}
}
