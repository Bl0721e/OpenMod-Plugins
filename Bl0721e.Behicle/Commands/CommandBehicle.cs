using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
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
	[Command("behicle")]
	[CommandSyntax("[p]")]
	public class CommandBehicle : Command
	{
		private readonly IVehicleDirectory m_VehicleDirectory;
		private readonly IUserDataStore m_UserDataStore;
		private readonly IEconomyProvider m_EconomyProvider;
		private readonly IConfiguration m_Configuration;
		private readonly IStringLocalizer m_StringLocalizer;
		public CommandBehicle(IVehicleDirectory vehicleDirectory,IUserDataStore userDataStore, IServiceProvider serviceProvider, IEconomyProvider economyProvider, IConfiguration configuration, IStringLocalizer stringLocalizer) : base(serviceProvider)
		{
			m_VehicleDirectory = vehicleDirectory;
			m_UserDataStore = userDataStore;
			m_EconomyProvider = economyProvider;
			m_Configuration = configuration;
			m_StringLocalizer = stringLocalizer;
		}
		protected override async Task OnExecuteAsync()
		{
			if (Context.Parameters.Count != 0 || Context.Actor.Type == KnownActorTypes.Console)
			{
				throw new CommandWrongUsageException(Context);
			}
			IReadOnlyCollection<IVehicle> VehicleDirectory = await m_VehicleDirectory.GetVehiclesAsync();
			var ownedCount = VehicleDirectory.Count(v => {
					InteractableVehicle Vehicle = VehicleManager.findVehicleByNetInstanceID(UInt32.Parse(v.VehicleInstanceId));
					return Vehicle.lockedOwner.m_SteamID.ToString() == Context.Actor.Id && Vehicle.isLocked && !Vehicle.isExploded;
					});
			var naturalCount = VehicleDirectory.Count(v => {
					InteractableVehicle Vehicle = VehicleManager.findVehicleByNetInstanceID(UInt32.Parse(v.VehicleInstanceId));
					var wasNaturallySpawned = (bool)typeof(InteractableVehicle).GetField("_wasNaturallySpawned", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Vehicle);
					return wasNaturallySpawned && !Vehicle.isLocked && !Vehicle.isExploded;
					});
			int limit = await m_UserDataStore.GetUserDataAsync<int>(Context.Actor.Id, "Player", "behicle_limit");
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
			string message = m_StringLocalizer[$"{locale}:command:behicle", new {naturalCount = naturalCount, totalCount = VehicleDirectory.Count(), ownedCount = ownedCount, limit = limit }]+"\n";
			if (limit < maxLimit)
			{
				int price = initPrice + priceIncrement * (limit - initLimit);
				message = message + m_StringLocalizer[$"{locale}:command:behiclePurchaseAvailable", new {count = maxLimit - limit, price = price, currency = m_EconomyProvider.CurrencyName }];
			}
			else
			{
				message = message + m_StringLocalizer[$"{locale}:command:behicleExceededMaxLimit", new {limit = limit, maxLimit = maxLimit}];
			}
			await Context.Actor.PrintMessageAsync(message, Color.FromName("White"));
		}
	}
}
