using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BlueprintReport
{
	class AllResourcesCounter : MapComponent
	{
		private Dictionary<ThingDef, int> cachedResourcesCounts = new Dictionary<ThingDef, int>();

		public AllResourcesCounter(Map map) : base(map)
		{
		}

		public override void MapComponentTick()
		{
			if (Find.TickManager.TicksGame % 204 == 0)
				UpdateResourceCounts();
		}

		public void UpdateResourceCounts()
		{
		}
	}
}
