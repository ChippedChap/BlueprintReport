using BlueprintReport.BlueprintReportUtilities;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace BlueprintReport
{
	class Designator_UntabulateConstructible : Designator_TabulateConstructible
	{
		public Designator_UntabulateConstructible()
		{
			defaultLabel = "DesignatorUntabulate".Translate();
			defaultDesc = "DesignatorUntabulateDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("untabulate", true);
			icon.mipMapBias = -1f;
			soundSucceeded = SoundDefOf.Designate_PlanRemove;
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			return Map.designationManager.DesignationOn(t, BlueprintReportUtility.tabulateDesignationDef) != null && base.CanDesignateThing(t).Accepted;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 loc)
		{
			AcceptanceReport result = base.CanDesignateCell(loc);
			if (!result.Accepted)
				return result;
			Thing constructibleAsThing = loc.GetFirstConstructible(Map);
			if (Map.designationManager.DesignationOn(constructibleAsThing, BlueprintReportUtility.tabulateDesignationDef) == null)
				return "MessageMustDesignateTabulatedConstructibles".Translate();
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (c.InBounds(Map))
				DesignateThing(c.GetFirstConstructible(Map));
		}

		public override void DesignateThing(Thing t)
		{
			Designation desigToRemove = Map.designationManager.DesignationOn(t, BlueprintReportUtility.tabulateDesignationDef);
			Map.designationManager.RemoveDesignation(desigToRemove);
		}

		protected override void FinalizeDesignationSucceeded()
		{
			ITab_Blueprints.Instance.constructibleTracker.UpdateTrackedConstructibles();
			if (!Map.designationManager.SpawnedDesignationsOfDef(BlueprintReportUtility.tabulateDesignationDef).Any())
				Find.MainTabsRoot.EscapeCurrentTab();
		}
	}
}
