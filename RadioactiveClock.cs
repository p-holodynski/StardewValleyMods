using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace MoreClocks.RadioactiveClock
{
	public class RadioactiveClockBuilding : Building
	{
		private static readonly BluePrint Blueprint = new("Radioactive Clock");

		public RadioactiveClockBuilding()
			: base(RadioactiveClockBuilding.Blueprint, Vector2.Zero) { }
	}
}