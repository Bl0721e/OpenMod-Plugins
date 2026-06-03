using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.Extensions.Games.Abstractions.Vehicles;
using OpenMod.Core.Users;
using OpenMod.API.Users;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;

namespace Bl0721e.AdminUtils.Commands
{
	[Command("ba")]
	[CommandSyntax("<action>")]
	public class CommandBA : Command
	{
		private readonly IVehicleDirectory m_VehicleDirectory;
		private readonly IUserDataStore m_UserDataStore;
		private readonly IConfiguration m_Configuration;
		public CommandBA(IVehicleDirectory vehicleDirectory,IUserDataStore userDataStore, IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider)
		{
			m_VehicleDirectory = vehicleDirectory;
			m_UserDataStore = userDataStore;
			m_Configuration = configuration;
		}
		protected override async Task OnExecuteAsync()
		{
			throw new CommandWrongUsageException(Context);
		}
	}
}
