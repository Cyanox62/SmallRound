using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Linq;
using UnityEngine;
using MEC;
using System;
using Smod2.EventSystem.Events;

namespace SmallRound
{
	partial class EventHandler : 
		IEventHandlerRoundStart, IEventHandlerWaitingForPlayers, IEventHandlerSpawn,
		IEventHandlerLCZDecontaminate, IEventHandlerCheckRoundEnd, IEventHandlerElevatorUse
	{
		private readonly Plugin instance;

		private bool isEnabled = false;
		private bool isDecon = false;

		private Vector mtfSpawnPoint;
		private Vector chaosSpawnPoint;
		private Vector scp173SpawnPoint;

		// Configs
		private int turnOffPlayers;
		private float tutorialSpawnDelay;

		public EventHandler(Plugin plugin)
		{
			instance = plugin;
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			LoadConfigs();
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			isDecon = false;
			isEnabled = ev.Server.GetPlayers().Count < turnOffPlayers;

			if (isEnabled)
			{
				mtfSpawnPoint = instance.Server.Map.GetElevators().FirstOrDefault(x => x.ElevatorType == ElevatorType.LiftB).GetPositions()[0];
				chaosSpawnPoint = instance.Server.Map.GetElevators().FirstOrDefault(x => x.ElevatorType == ElevatorType.LiftA).GetPositions()[0];
				scp173SpawnPoint = instance.Server.Map.GetRandomSpawnPoint(Role.SCP_173);

				foreach (Smod2.API.Item item in ev.Server.Map.GetItems(ItemType.ZONE_MANAGER_KEYCARD, true))
				{
					Vector pos = item.GetPosition();
					item.Remove();
					ev.Server.Map.SpawnItem(ItemType.SCIENTIST_KEYCARD, pos, Vector.Zero);
				}

				foreach (Smod2.API.Item item in ev.Server.Map.GetItems(ItemType.MAJOR_SCIENTIST_KEYCARD, true))
				{
					Vector pos = item.GetPosition();
					item.Remove();
					ev.Server.Map.SpawnItem(ItemType.SCIENTIST_KEYCARD, pos, Vector.Zero);
				}

				foreach (Player player in ev.Server.GetPlayers())
				{
					player.PersonalBroadcast
					(
						10,
						"The server is in small round mode due to the low player count." +
						" Press [`] or [~] to learn how it works.",
						false
					);

					player.SendConsoleMessage
					(
						"Small Round Mode\n" +
						$"This server enters small round mode when " +
						$"the server has less than {turnOffPlayers} players. " +
						$"This mode takes place entirely in Light Containment Zone, and " +
						$"all SCPs will spawn in SCP-173's chamber. As a D-Class " +
						$"or Scientist, you can escape by activating one of the two " +
						$"Light Containment Zone elevators. If you escape, you will " +
						$"change into your respective role as usual and will spawn in " +
						$"the same place you escaped at. Decontamination will force end " +
						$"the round."
					);

					Vector pos = GetProperSpawnPos(player);
					if (player.TeamRole.Team == Smod2.API.Team.TUTORIAL)
					{
						Timing.RunCoroutine(SpawnDelay(player, pos, tutorialSpawnDelay));
					}
					else
					{
						player.Teleport(pos);
					}
				}
			}
		}

		public void OnDecontaminate()
		{
			if (isEnabled)
			{
				isDecon = true;
			}
		}

		public void OnSpawn(PlayerSpawnEvent ev)
		{
			if (isEnabled)
			{
				Vector pos = GetProperSpawnPos(ev.Player);
				if (ev.Player.TeamRole.Team == Smod2.API.Team.TUTORIAL)
				{
					Timing.RunCoroutine(SpawnDelay(ev.Player, pos, tutorialSpawnDelay));
				}
				else
				{
					ev.SpawnPos = pos;
				}
			}
		}

		public void OnCheckRoundEnd(CheckRoundEndEvent ev)
		{
			if (isDecon && isEnabled)
			{
				ev.Status = ROUND_END_STATUS.NO_VICTORY;
			}
		}

		public void OnElevatorUse(PlayerElevatorUseEvent ev)
		{
			if ((ev.Elevator.ElevatorType == ElevatorType.LiftA ||
				ev.Elevator.ElevatorType == ElevatorType.LiftB) && isEnabled)
			{
				ev.AllowUse = false;

				if (ev.Player.TeamRole.Team == Smod2.API.Team.CLASSD)
				{
					CheckEscape(ev.Player, Smod2.API.Team.CLASSD);
				}
				else if (ev.Player.TeamRole.Team == Smod2.API.Team.SCIENTIST)
				{
					CheckEscape(ev.Player, Smod2.API.Team.SCIENTIST);
				}
			}
		}
	}
}
