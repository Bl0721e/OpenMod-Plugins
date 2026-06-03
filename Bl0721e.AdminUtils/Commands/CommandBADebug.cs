using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Commands;
using OpenMod.Extensions.Economy.Abstractions;
using OpenMod.Extensions.Games.Abstractions.Vehicles;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.API.Users;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Vehicles;
using SDG.Unturned;
using UnityEngine;
using Steamworks;
using Command = OpenMod.Core.Commands.Command;

namespace Bl0721e.AdminUtils.Commands
{
	[Command("debug")]
	[CommandParent(typeof(CommandBA))]
	[CommandSyntax("[debug]")]
	public class CommandBADebug : Command
	{
		private readonly IVehicleDirectory m_VehicleDirectory;
		private readonly IUserDataStore m_UserDataStore;
		private readonly IEconomyProvider m_EconomyProvider;
		private readonly IConfiguration m_Configuration;
		public CommandBADebug(IVehicleDirectory vehicleDirectory,IUserDataStore userDataStore, IServiceProvider serviceProvider, IEconomyProvider economyProvider, IConfiguration configuration) : base(serviceProvider)
		{
			m_VehicleDirectory = vehicleDirectory;
			m_UserDataStore = userDataStore;
			m_EconomyProvider = economyProvider;
			m_Configuration = configuration;
		}
		protected override async Task OnExecuteAsync()
		{
			string message = "failed";
			var user = (UnturnedUser)Context.Actor;
			var look = user.Player.Player.look;

			await UniTask.SwitchToMainThread();
			if (!Physics.Raycast(new Ray(look.getEyesPosition(), look.aim.forward),
				out var hit, 8f, RayMasks.BARRICADE | RayMasks.VEHICLE))
			{
				return;
			}
			var interactable = hit.collider.GetComponent<Interactable>();
			if (interactable is InteractableStorage storage)
			{
				Transform root = interactable.transform;
				var barricadeDrop = BarricadeManager.FindBarricadeByRootTransform(root);
//				object obj = NetIdRegistry.Get(interactable.GetNetId());
//				if (obj != null && obj is BarricadeDrop barricadeDrop)
//				{
					message = $"{barricadeDrop.instanceID}, {barricadeDrop.GetServersideData().objActiveDate}, {Provider.time}";
//				}
			}
			else if (interactable is InteractableVehicle vehicle)
			{
				string id = vehicle.lockedOwner.m_SteamID.ToString();
				UserData userData = (await m_UserDataStore.GetUserDataAsync(id, KnownActorTypes.Player))!;
				DateTime dateTime = userData.LastSeen!.Value;
				long time = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
//				var decayTimer = typeof(InteractableVehicle).GetField("decayTimer", BindingFlags.Instance | BindingFlags.NonPublic);
//				var decayLastUpdateTime = typeof(InteractableVehicle).GetField("decayLastUpdateTime", BindingFlags.Instance | BindingFlags.NonPublic);
//				var decayLastUpdatePosition = typeof(InteractableVehicle).GetField("decayLastUpdatePosition", BindingFlags.Instance | BindingFlags.NonPublic);
//				message = $"{decayTimer.GetValue(vehicle)}, {decayLastUpdateTime.GetValue(vehicle)}, {decayLastUpdatePosition.GetValue(vehicle)}";
				message = $"{time}, {Provider.time}";
			}
			await PrintAsync(message);
		}
	}
}
