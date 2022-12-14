using Sandbox;

namespace Breakfloor.Events;

public static class BFEVents
{
	public class BlockBreakAttribute : EventAttribute
	{
		// TODO: maybe use this for keeping stats of who broke the most blocks?
		// who did the most block dmg? who killed the most people by breaking their blocks?
		public BlockBreakAttribute() : base( "bf.block.break" ) { }
	}

	public const string LocalPlayerHurt = "bf.localplayer.hurt";
	public class LocalPlayerHurtEvent : EventAttribute
	{
		public LocalPlayerHurtEvent() : base( LocalPlayerHurt ) { }
	}

	/// <summary>
	/// When the round resets after time is up.
	/// </summary>
	public const string Reset = "bf.reset";
	/// <summary>
	/// When the round resets after time is up.
	/// </summary>
	public class ResetEvent : EventAttribute
	{
		public ResetEvent() : base( Reset ) { }
	}
}
