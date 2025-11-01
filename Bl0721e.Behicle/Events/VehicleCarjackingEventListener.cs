using System;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using OpenMod.Core.Users;
using OpenMod.API.Users;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Vehicles.Events;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Vehicles;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

using Color = System.Drawing.Color;

namespace Bl0721e.Behicle.Events
{
	public class VehicleCarjackingEventListener : IEventListener<UnturnedVehicleCarjackingEvent>
	{
		private readonly IUserDataStore m_UserDataStore;
		private readonly IConfiguration m_Configuration;
		private readonly IStringLocalizer m_StringLocalizer;
		public VehicleCarjackingEventListener(IUserDataStore userDataStore, IConfiguration configuration, IStringLocalizer stringLocalizer)
		{
			m_UserDataStore = userDataStore;
			m_Configuration = configuration;
			m_StringLocalizer = stringLocalizer;
		}
		public async Task HandleEventAsync(object? sender, UnturnedVehicleCarjackingEvent @event)
		{
			string message = "";
			if (@event.Instigator != null && @event.Vehicle.Ownership.HasOwner && @event.Vehicle.Vehicle.isLocked && !@event.Vehicle.Vehicle.isDrowned)
			{
				bool hasAccess = await @event.Vehicle.Ownership.HasAccessAsync(@event.Instigator);
				CSteamID dummyPlayer = default(CSteamID);
				Player player = PlayerTool.getPlayer(@event.Instigator.SteamId);
				var position = @event.Vehicle.Transform.Position;
				Vector3 v3 = new UnityEngine.Vector3(position.X, position.Y, position.Z);
				bool isInClaim = ClaimManager.checkCanBuild(v3, @event.Instigator.SteamId, player.quests.groupID, false) && !ClaimManager.checkCanBuild(v3, dummyPlayer, dummyPlayer, false);
				string fallbackLocale = m_Configuration.GetSection("locale:fallbackLocale").Get<string>()!;
				string locale = await m_UserDataStore.GetUserDataAsync<string>(@event.Instigator.SteamId.m_SteamID.ToString(), KnownActorTypes.Player, "localePreference") ?? fallbackLocale;
				if (!hasAccess && !isInClaim)
				{
					@event.IsCancelled = true;
					message = m_StringLocalizer[$"{locale}:event:itemUsageProhibited"];
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
