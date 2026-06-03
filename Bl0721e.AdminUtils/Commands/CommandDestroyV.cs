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
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Vehicles;
using SDG.Unturned;
using UnityEngine;
using Steamworks;
using Command = OpenMod.Core.Commands.Command;

namespace Bl0721e.AdminUtils.Commands
{
	[Command("destrov")]
	[CommandSyntax("")]
	public class CommandDestroyV : Command
	{
		private readonly IVehicleDirectory m_VehicleDirectory;
		private readonly IUserDataStore m_UserDataStore;
		private readonly IEconomyProvider m_EconomyProvider;
		private readonly IConfiguration m_Configuration;
		public CommandDestroyV(IVehicleDirectory vehicleDirectory,IUserDataStore userDataStore, IServiceProvider serviceProvider, IEconomyProvider economyProvider, IConfiguration configuration) : base(serviceProvider)
		{
			m_VehicleDirectory = vehicleDirectory;
			m_UserDataStore = userDataStore;
			m_EconomyProvider = economyProvider;
			m_Configuration = configuration;
		}
		protected override async Task OnExecuteAsync()
		{
			UnturnedUser user = (UnturnedUser)Context.Actor;
			var vehicle = user.Player.CurrentVehicle;
			string message = "failed";
			if (vehicle != null);
			{
				await vehicle!.DestroyAsync();
				message = "destroyed";
			}
			await PrintAsync(message);
		}
	}
}
