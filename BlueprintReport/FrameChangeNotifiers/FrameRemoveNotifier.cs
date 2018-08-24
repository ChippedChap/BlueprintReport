using System;
using System.Collections.Generic;
using Harmony;
using Verse;
using RimWorld;

namespace BlueprintReport.FrameChangeNotifiers
{
	[HarmonyPatch(typeof(ThingOwner<Thing>))]
	[HarmonyPatch("NotifyRemoved")]
	class FrameRemoveNotifier
	{
		static void Postfix(ThingOwner<Thing> __instance)
		{
			if (__instance.Owner is Frame)
				FrameChangeNotifierData.NotifyChange();
		}
	}
}
