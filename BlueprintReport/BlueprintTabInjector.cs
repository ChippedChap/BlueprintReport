using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Harmony;

namespace BlueprintReport
{
	[HarmonyPatch(typeof(MainTabWindow_Inspect))]
	[HarmonyPatch("get_CurTabs")]
	class BlueprintTabInjector
	{
		static void Postfix(ref IEnumerable<InspectTabBase> __result)
		{
			if (Find.Selector.NumSelected > 1)
			{
				List<InspectTabBase> resultAsList = __result.ToList();
				InspectTabBase blueprintTabInstance = ITab_Blueprints.Instance;
				resultAsList.Add(blueprintTabInstance);
				__result = resultAsList;
			}
		}
	}
}
