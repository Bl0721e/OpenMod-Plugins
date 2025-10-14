using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
	[Command("behicle")]
	[CommandSyntax("[p]")]
	public class CommandBehicle : Command
	{
		private readonly IVehicleDirectory m_VehicleDirectory;
		private readonly IUserDataStore m_UserDataStore;
		private readonly IEconomyProvider m_EconomyProvider;
		private readonly IConfiguration m_Configuration;
		public CommandBehicle(IVehicleDirectory vehicleDirectory,IUserDataStore userDataStore, IServiceProvider serviceProvider, IEconomyProvider economyProvider, IConfiguration configuration) : base(serviceProvider)
		{
			m_VehicleDirectory = vehicleDirectory;
			m_UserDataStore = userDataStore;
			m_EconomyProvider = economyProvider;
			m_Configuration = configuration;
		}
		protected override async Task OnExecuteAsync()
		{
			if (Context.Parameters.Count != 0 || Context.Actor.Type == KnownActorTypes.Console)
			{
				throw new CommandWrongUsageException(Context);
			}
			IReadOnlyCollection<IVehicle> VehicleDirectory = await m_VehicleDirectory.GetVehiclesAsync();
			var count = VehicleDirectory.Count(v => {
					InteractableVehicle Vehicle = VehicleManager.findVehicleByNetInstanceID(UInt32.Parse(v.VehicleInstanceId));
					return Vehicle.lockedOwner.m_SteamID.ToString() == Context.Actor.Id && Vehicle.isLocked && !Vehicle.isExploded;
					});
			int limit = await m_UserDataStore.GetUserDataAsync<int>(Context.Actor.Id, "Player", "behicle_limit");
			int max_limit = m_Configuration.GetSection("max_limit").Get<int>();
			int init_limit = m_Configuration.GetSection("init_limit").Get<int>();
			int price_init = m_Configuration.GetSection("price_init").Get<int>();
			int price_increment = m_Configuration.GetSection("price_increment").Get<int>();
			if (limit == 0 && init_limit != 0)
			{
				await m_UserDataStore.SetUserDataAsync<int>(Context.Actor.Id, "Player", "behicle_limit", init_limit);
				limit = init_limit;
			}
			string message = $"你当前拥有{count}/{limit}个载具";
			if (limit < max_limit)
			{
				int price = price_init + price_increment * (limit - init_limit);
				message = message + $"\n使用命令'/behicle p'解锁更多载具上限\n还可以解锁{max_limit - limit}个, 下一次需要花费{price}{m_EconomyProvider.CurrencyName}";
			}
			else
			{
				message = message + $"\n你的可锁定载具上限已达最大值({limit}/{max_limit})";
			}
			await Context.Actor.PrintMessageAsync(message, Color.FromName("White"));
		}
	}
}
