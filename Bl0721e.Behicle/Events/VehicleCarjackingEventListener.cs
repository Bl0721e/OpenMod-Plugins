using System;
using System.Threading.Tasks;
using System.Drawing;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Vehicles.Events;
using System.Collections.Generic;
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
				if (!hasAccess && !isInClaim)
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
