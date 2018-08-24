using System;
using Verse;
using RimWorld;
using Harmony;

namespace BlueprintReport.SelectorChangeNotifiers
{
	[HarmonyPatch(typeof(Selector))]
	[HarmonyPatch("ClearSelection")]
	class SelectorClearNotifier
	{
		static void Postfix()
		{
			SelectionChangeNotifierData.NotifyChange();
		}
	}
}
