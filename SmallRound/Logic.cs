﻿using System;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Linq;
using MEC;
using System.Collections.Generic;

namespace SmallRound
{
	partial class EventHandler
	{
		private void LoadConfigs()
		{
			turnOffPlayers = instance.GetConfigInt("sr_turnoff_players");
		}

		private void CheckEscape(Player player, Smod2.API.Team team)
		{
			bool isDClass = team == Smod2.API.Team.CLASSD;
			Timing.RunCoroutine(GrantItemsDelay(player, player.GetInventory().Select(x => x.ItemType).ToList(), 0.2f));
			player.ChangeRole(isDClass ? Role.CHAOS_INSURGENCY : Role.NTF_SCIENTIST, true, false, true, true);
			int min = instance.Server.Round.Duration / 60;
			int sec = instance.Server.Round.Duration % 60;
			player.PersonalBroadcast(10, $"You have escaped as a {(isDClass ? "<color=#ff8e00>Class-D</color>" : "<color=#f6f677>Scientist</color>")} in {min} minutes and {sec} seconds.", false);
		}

		private IEnumerator<float> GrantItemsDelay(Player player, List<ItemType> items, float delay)
		{
			yield return Timing.WaitForSeconds(delay);

			Vector pos = player.GetPosition();

			foreach (ItemType item in items)
			{
				if (player.GetInventory().Count < 8)
				{
					player.GiveItem(item);
				}
				else
				{
					instance.Server.Map.SpawnItem(item, pos, Vector.Zero);
				}
			}
		}
	}
}
