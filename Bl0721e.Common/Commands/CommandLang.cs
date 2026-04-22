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
using OpenMod.Unturned.Users;
using OpenMod.API.Users;
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace Bl0721e.Common.Commands
{
	[Command("lang")]
	[CommandSyntax("[locale]")]
	[CommandActor(typeof(UnturnedUser))]
	public class CommandLang : Command
	{
		private readonly IVehicleDirectory m_VehicleDirectory;
		private readonly IUserDataStore m_UserDataStore;
		private readonly IEconomyProvider m_EconomyProvider;
		private readonly IConfiguration m_Configuration;
		private readonly IStringLocalizer m_StringLocalizer;
		public CommandLang(IVehicleDirectory vehicleDirectory,IUserDataStore userDataStore, IServiceProvider serviceProvider, IEconomyProvider economyProvider, IConfiguration configuration, IStringLocalizer stringLocalizer) : base(serviceProvider)
		{
			m_VehicleDirectory = vehicleDirectory;
			m_UserDataStore = userDataStore;
			m_EconomyProvider = economyProvider;
			m_Configuration = configuration;
			m_StringLocalizer = stringLocalizer;
		}
		protected override async Task OnExecuteAsync()
		{
			if (Context.Parameters.Count > 1)
			{
				throw new CommandWrongUsageException(Context);
			}
			string[] locales = m_Configuration.GetSection("locale:availableLocales").Get<string[]>()!;
			string fallbackLocale = m_Configuration.GetSection("locale:fallbackLocale").Get<string>()!;
			string locale = await m_UserDataStore.GetUserDataAsync<string>(Context.Actor.Id, KnownActorTypes.Player, "localePreference") ?? fallbackLocale;
			if (Context.Parameters.Count == 0)
			{
				await Context.Actor.PrintMessageAsync(m_StringLocalizer[$"{locale}:locale:displayLocaleList", new { locales = string.Join(", ", locales) }]);
			}
			else
			{
				string newLocale = await Context.Parameters.GetAsync<string>(0);
				if (!locales.Contains(newLocale))
				{
					throw new UserFriendlyException(m_StringLocalizer[$"{locale}:locale:localeNotAvailable"]);
				}
				await m_UserDataStore.SetUserDataAsync<string>(Context.Actor.Id, KnownActorTypes.Player, "localePreference", newLocale);
				await Context.Actor.PrintMessageAsync(m_StringLocalizer[$"{newLocale}:locale:localeUpdateSuccess", new { newLocale = newLocale }]);
			}
		}
	}
}
