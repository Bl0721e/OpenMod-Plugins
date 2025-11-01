using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
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
		private readonly IStringLocalizer m_StringLocalizer;
		public CommandBehiclePurchase(IVehicleDirectory vehicleDirectory,IUserDataStore userDataStore, IServiceProvider serviceProvider, IEconomyProvider economyProvider, IConfiguration configuration, IStringLocalizer stringLocalizer) : base(serviceProvider)
		{
			m_VehicleDirectory = vehicleDirectory;
			m_UserDataStore = userDataStore;
			m_EconomyProvider = economyProvider;
			m_Configuration = configuration;
			m_StringLocalizer = stringLocalizer;
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
			int maxLimit = m_Configuration.GetSection("maxLimit").Get<int>();
			int initLimit = m_Configuration.GetSection("initLimit").Get<int>();
			int initPrice = m_Configuration.GetSection("initPrice").Get<int>();
			int priceIncrement = m_Configuration.GetSection("priceIncrement").Get<int>();
			string fallbackLocale = m_Configuration.GetSection("locale:fallbackLocale").Get<string>()!;
			string locale = await m_UserDataStore.GetUserDataAsync<string>(Context.Actor.Id, KnownActorTypes.Player, "localePreference") ?? fallbackLocale;

			if (limit == 0 && initLimit != 0)
			{
				await m_UserDataStore.SetUserDataAsync<int>(Context.Actor.Id, "Player", "behicle_limit", initLimit);
				limit = initLimit;
			}
			int price = initPrice + priceIncrement * (limit - initLimit);
			string message = "";
			Color color = Color.FromName("White");
			if (limit == maxLimit)
			{
				message = m_StringLocalizer[$"{locale}:command:behicleExceededMaxLimit", new { limit = limit, maxLimit = maxLimit }];
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
					message = m_StringLocalizer[$"{locale}:command:behiclePurchaseFailed", new { balance = balance, price = price }];
					color = Color.FromName("Crimson");
				}
				if(message == "")
				{
					await m_UserDataStore.SetUserDataAsync<int>(Context.Actor.Id, KnownActorTypes.Player, "behicle_limit", limit+1);
					message = m_StringLocalizer[$"{locale}:command:behiclePurchaseSuccess", new { newLimit = limit + 1 }];
				}
			}
			await Context.Actor.PrintMessageAsync(message, color);
		}
	}
}
