using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using OpenMod.Core.Users;
using OpenMod.API.Users;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Building.Events;
using SDG.Unturned;
using OpenMod.Unturned.Users;
using UnityEngine;

namespace Bl0721e.NoRaid.Events
{
    public class BuildableDeployedEventListener : IEventListener<UnturnedBuildableDeployedEvent>
	{
		private readonly IUserDataStore m_UserDataStore;
		private readonly IConfiguration m_Configuration;
		private readonly IStringLocalizer m_StringLocalizer;
		public BuildableDeployedEventListener(IUserDataStore userDataStore, IConfiguration configuration, IStringLocalizer stringLocalizer)
		{
			m_UserDataStore = userDataStore;
			m_Configuration = configuration;
			m_StringLocalizer = stringLocalizer;
		}
		public async Task HandleEventAsync(object? sender, UnturnedBuildableDeployedEvent @event)
		{
			string message = "";
			var position = @event.Buildable.Transform.Position;
			Vector3 v3 = new UnityEngine.Vector3(position.X, position.Y, position.Z);
			SteamPlayer Player = PlayerTool.getSteamPlayer(Convert.ToUInt64(@event.Buildable.Ownership.OwnerPlayerId));
			string fallbackLocale = m_Configuration.GetSection("locale:fallbackLocale").Get<string>()!;
			string locale = await m_UserDataStore.GetUserDataAsync<string>(@event.Buildable.Ownership.OwnerPlayerId!, KnownActorTypes.Player, "localePreference") ?? fallbackLocale;
			if (LevelNavigation.checkSafeFakeNav(v3) && Player != null)
			{
				message = "[提示]此建筑位于资源区内, 将不会受到任何保护";
				message = m_StringLocalizer[$"{locale}:nav"];
			}
			if (message == "" || Player == null)
			{
				return;
			}
			await UniTask.SwitchToMainThread();
			ChatManager.serverSendMessage(message, Color.red, toPlayer: Player);
		}
	}
}
