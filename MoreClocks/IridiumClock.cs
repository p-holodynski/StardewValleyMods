using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace MoreClocks.IridiumClock
{
	public class IridiumClockBuilding : Building
	{
		private static readonly BluePrint Blueprint = new("Iridium Clock");

		public IridiumClockBuilding()
			: base(IridiumClockBuilding.Blueprint, Vector2.Zero) { }
	}
}