using Smod2.Attributes;

namespace SmallRound
{
	[PluginDetails(
	author = "Cyanox",
	name = "SmallRound",
	description = "Alters gameplay mechanics when there is a low player count on the server.",
	id = "cyan.smallround",
	version = "1.0.0",
	SmodMajor = 3,
	SmodMinor = 0,
	SmodRevision = 0
	)]
	public class Plugin : Smod2.Plugin
	{
		public override void OnDisable() { }

		public override void OnEnable() { }

		public override void Register()
		{
			AddEventHandlers(new EventHandler(this));

			AddConfig(new Smod2.Config.ConfigSetting("sr_turnoff_players", 8, true, "The amount of players that have to be in the server before this mode will turn off."));
		}
	}
}
