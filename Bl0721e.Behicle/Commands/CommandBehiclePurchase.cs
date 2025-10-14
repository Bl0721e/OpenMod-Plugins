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
	[Command("p")]
	[CommandParent(typeof(CommandBehicle))]
	[CommandSyntax("[p]")]
	public class CommandBehiclePurchase : Command
	{
		private readonly IVehicleDirectory m_VehicleDirectory;
		private readonly IUserDataStore m_UserDataStore;
		private readonly IEconomyProvider m_EconomyProvider;
		private readonly IConfiguration m_Configuration;
		public CommandBehiclePurchase(IVehicleDirectory vehicleDirectory,IUserDataStore userDataStore, IServiceProvider serviceProvider, IEconomyProvider economyProvider, IConfiguration configuration) : base(serviceProvider)
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
			var count = VehicleDirectory.Count(v => {
					InteractableVehicle Vehicle = VehicleManager.findVehicleByNetInstanceID(UInt32.Parse(v.VehicleInstanceId));
					return Vehicle.lockedOwner.m_SteamID.ToString() == Context.Actor.Id && Vehicle.isLocked && !Vehicle.isExploded;
					});
			int limit = await m_UserDataStore.GetUserDataAsync<int>(Context.Actor.Id, KnownActorTypes.Player, "behicle_limit");
			int max_limit = m_Configuration.GetSection("max_limit").Get<int>();
			int init_limit = m_Configuration.GetSection("init_limit").Get<int>();
			int price_init = m_Configuration.GetSection("price_init").Get<int>();
			int price_increment = m_Configuration.GetSection("price_increment").Get<int>();
			if (limit == 0 && init_limit != 0)
			{
				await m_UserDataStore.SetUserDataAsync<int>(Context.Actor.Id, "Player", "behicle_limit", init_limit);
				limit = init_limit;
			}
			int price = price_init + price_increment * (limit - init_limit);
			string message = "";
			Color color = Color.FromName("White");
			if (limit == max_limit)
			{
				message = message + $"你的载具上限已达最大值({limit}/{max_limit})";
			}
			else
			{
				try
				{
					await m_EconomyProvider.UpdateBalanceAsync(Context.Actor.Id, KnownActorTypes.Player, price * -1, "");
				}
				catch (NotEnoughBalanceException)
				{
					var balance = await m_EconomyProvider.GetBalanceAsync(Context.Actor.Id, KnownActorTypes.Player);
					message = $"你的余额不足({balance}/{price})";
					color = Color.FromName("Crimson");
				}
				if(message == "")
				{
					await m_UserDataStore.SetUserDataAsync<int>(Context.Actor.Id, KnownActorTypes.Player, "behicle_limit", limit+1);
					message = $"兑换完成, 你现在的载具锁定上限是{limit+1}";
				}
			}
			await Context.Actor.PrintMessageAsync(message, color);
		}
	}
}
