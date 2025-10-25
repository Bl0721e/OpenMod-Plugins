using System.Threading.Tasks;
using System.Drawing;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Vehicles.Events;

namespace Bl0721e.Behicle.Events
{
	public class VehicleLockpickingEventListener : IEventListener<UnturnedVehicleLockpickingEvent>
	{
		public async Task HandleEventAsync(object? sender, UnturnedVehicleLockpickingEvent @event)
		{
			bool hasAccess = await @event.Vehicle.Ownership.HasAccessAsync(@event.Instigator);
			string message = "";
			if (!(@event.Vehicle.Ownership.HasOwner && @event.Vehicle.Vehicle.isLocked) || !hasAccess)
			{
				@event.IsCancelled = true;
				message = "此物品已被禁止使用";
			}
			if (message == "")
			{
				return;
			}
			await @event.Instigator.PrintMessageAsync(message, Color.FromName("Crimson"));
		}
	}
}
