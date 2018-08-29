using System.Linq;
using Harmony;
using Verse;
using RimWorld;

namespace BlueprintReport.SelectorChangeNotifiers
{
	[HarmonyPatch(typeof(Blueprint_Build))]
	[HarmonyPatch("MakeSolidThing")]
	class BlueprintTrackingTransferer
	{
		static void Postfix(Thing __result, Blueprint_Build __instance)
		{
			if (Find.Selector.NumSelected > 1 && Find.Selector.SelectedObjects.Contains(__instance))
				SelectResult(__result);
			else if (Find.CurrentMap.designationManager.SpawnedDesignationsOfDef(BlueprintReportUtility.tabulateDesignationDef).Any())
				DesignateResult(__result);
		}

		static void SelectResult(Thing result, bool notifyChange = true)
		{
			if (result != null)
				Find.Selector.SelectRaw(result);
		}

		static void DesignateResult(Thing result)
		{
			if (result != null)
				Find.CurrentMap.designationManager.AddDesignation(new Designation(result, BlueprintReportUtility.tabulateDesignationDef));
		}
	}
}
