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
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace Bl0721e.Behicle.Commands
{
	[Command("u")]
	[CommandParent(typeof(CommandBehicle))]
	[CommandSyntax("[u]")]
	public class CommandBehicleUpdate : Command
	{
		private readonly IVehicleDirectory m_VehicleDirectory;
		private readonly IUserDataStore m_UserDataStore;
		private readonly IEconomyProvider m_EconomyProvider;
		private readonly IConfiguration m_Configuration;
		public CommandBehicleUpdate(IVehicleDirectory vehicleDirectory,IUserDataStore userDataStore, IServiceProvider serviceProvider, IEconomyProvider economyProvider, IConfiguration configuration) : base(serviceProvider)
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
			IReadOnlyCollection<IVehicle> VehicleDirectory = await m_VehicleDirectory.GetVehiclesAsync();
			int updated_count = 0;
			var tasks = VehicleDirectory.Select(async vehicle => {
				InteractableVehicle interactableVehicle = VehicleManager.findVehicleByNetInstanceID(UInt32.Parse(vehicle.VehicleInstanceId));
				var wasNaturallySpawned = typeof(InteractableVehicle).GetField("_wasNaturallySpawned", BindingFlags.Instance | BindingFlags.NonPublic);
				if ((bool)wasNaturallySpawned.GetValue(interactableVehicle) && interactableVehicle.isLocked)
				{
					wasNaturallySpawned.SetValue(interactableVehicle, false);
					updated_count += 1;
				}
					}).ToList();
			await Task.WhenAll(tasks);
			var total_count = VehicleDirectory.Count();
			var natural_count = VehicleDirectory.Count(v => {
				InteractableVehicle Vehicle = VehicleManager.findVehicleByNetInstanceID(UInt32.Parse(v.VehicleInstanceId));
				var wasNaturallySpawned = (bool)typeof(InteractableVehicle).GetField("_wasNaturallySpawned", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Vehicle);
				return wasNaturallySpawned && !Vehicle.isLocked && !Vehicle.isExploded;
			});
			await Context.Actor.PrintMessageAsync($"{updated_count}, {natural_count}/{total_count}");
		}
	}
}
