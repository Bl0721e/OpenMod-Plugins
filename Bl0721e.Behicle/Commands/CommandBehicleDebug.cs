using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
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

namespace Bl0721e.Behicle.Commands
{
	[Command("debug")]
	[CommandParent(typeof(CommandBehicle))]
	[CommandSyntax("[debug]")]
	public class CommandBehicleDebug : Command
	{
		private readonly IVehicleDirectory m_VehicleDirectory;
		private readonly IUserDataStore m_UserDataStore;
		private readonly IEconomyProvider m_EconomyProvider;
		private readonly IConfiguration m_Configuration;
		public CommandBehicleDebug(IVehicleDirectory vehicleDirectory,IUserDataStore userDataStore, IServiceProvider serviceProvider, IEconomyProvider economyProvider, IConfiguration configuration) : base(serviceProvider)
		{
			m_VehicleDirectory = vehicleDirectory;
			m_UserDataStore = userDataStore;
			m_EconomyProvider = economyProvider;
			m_Configuration = configuration;
		}
		protected override async Task OnExecuteAsync()
		{
			if (Context.Parameters.Count !=0 || Context.Actor.Type == KnownActorTypes.Console)
			{
				throw new CommandWrongUsageException(Context);
			}
			string message = "";
			UnturnedUser unturnedUser = (UnturnedUser)Context.Actor;
			if (unturnedUser.Player.CurrentVehicle == null)
			{
				message = "无";
			}
			else
			{
				CSteamID dummyPlayer = default(CSteamID);
				Player player = PlayerTool.getPlayer(unturnedUser.SteamId);
				UnturnedVehicle unturnedVehicle = unturnedUser.Player.CurrentVehicle;
				var position = unturnedVehicle.Transform.Position;
				Vector3 v3 = new UnityEngine.Vector3(position.X, position.Y, position.Z);
				var decayTimer = typeof(InteractableVehicle).GetField("decayTimer", BindingFlags.Instance | BindingFlags.NonPublic);
				message = $"{ClaimManager.checkCanBuild(v3, unturnedUser.SteamId, player.quests.groupID, false)}, {ClaimManager.checkCanBuild(v3, dummyPlayer, dummyPlayer, false)}, {dummyPlayer.m_SteamID.ToString()}, {decayTimer.GetValue(unturnedVehicle.Vehicle)}";
			}
			await Context.Actor.PrintMessageAsync(message);
		}
	}
}
