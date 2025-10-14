using System.Threading.Tasks;
using System.Drawing;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Vehicles.Events;

namespace Bl0721e.Behicle.Events
{
	public class VehicleLockpickingEventListener : IEventListener<UnturnedVehicleLockpickingEvent>
	{
		public Task HandleEventAsync(object? sender, UnturnedVehicleLockpickingEvent @event)
		{
			@event.IsCancelled = true;
			@event.Instigator.PrintMessageAsync("此物品已被禁止使用", Color.FromName("Crimson"));
			return Task.CompletedTask;
		}
	}
}
