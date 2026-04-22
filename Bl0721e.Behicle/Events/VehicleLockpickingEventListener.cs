using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Users;
using OpenMod.API.Users;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Vehicles.Events;

namespace Bl0721e.Behicle.Events
{
	public class VehicleLockpickingEventListener : IEventListener<UnturnedVehicleLockpickingEvent>
	{
		private readonly IUserDataStore m_UserDataStore;
		private readonly IConfiguration m_Configuration;
		private readonly IStringLocalizer m_StringLocalizer;
		public VehicleLockpickingEventListener(IUserDataStore userDataStore, IConfiguration configuration, IStringLocalizer stringLocalizer)
		{
			m_UserDataStore = userDataStore;
			m_Configuration = configuration;
			m_StringLocalizer = stringLocalizer;
		}
		public async Task HandleEventAsync(object? sender, UnturnedVehicleLockpickingEvent @event)
		{
			bool hasAccess = await @event.Vehicle.Ownership.HasAccessAsync(@event.Instigator);
			string message = "";
			string fallbackLocale = m_Configuration.GetSection("locale:fallbackLocale").Get<string>()!;
			string locale = await m_UserDataStore.GetUserDataAsync<string>(@event.Instigator.SteamId.m_SteamID.ToString(), KnownActorTypes.Player, "localePreference") ?? fallbackLocale;
			if (!(@event.Vehicle.Ownership.HasOwner && @event.Vehicle.Vehicle.isLocked) || !hasAccess)
			{
				@event.IsCancelled = true;
				message = m_StringLocalizer[$"{locale}:event:itemUsageProhibited"];
			}
			if (message == "")
			{
				return;
			}
			await @event.Instigator.PrintMessageAsync(message, Color.FromName("Crimson"));
		}
	}
}
