using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace BlueprintReport
{
	class Designator_TabulateConstructible : Designator
	{
		public Designator_TabulateConstructible()
		{
			defaultLabel = "DesignatorTabulate".Translate();
			defaultDesc = "DesignatorTabulateDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("tabulate", true);
			soundSucceeded = SoundDefOf.Designate_PlanAdd;
			useMouseIcon = true;
		}

		public override int DraggableDimensions
		{
			get { return 2; }
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			return t is Frame || t is Blueprint;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 loc)
		{
			if (!loc.InBounds(Map) && loc.Fogged(Map))
				return false;
			Thing constructible = loc.GetFirstConstructible(Map);
			if (constructible == null)
				return "MessageMustDesignateConstructibles".Translate();
			AcceptanceReport result = CanDesignateThing(constructible);
			return result.Accepted;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (c.InBounds(Map))
				DesignateThing(c.GetFirstConstructible(Map));
		}

		public override void DesignateThing(Thing t)
		{
			Designation dDesWith = new Designation(t, BlueprintReportUtility.tabulateDesignationDef);
			if (Map.designationManager.DesignationOn(t) == null)
				Map.designationManager.AddDesignation(dDesWith);
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}

		protected override void FinalizeDesignationSucceeded()
		{
			// Avoid updating twice
			if (Find.Selector.NumSelected > 0)
				DesignateSelected();
			else
				ITab_Blueprints.Instance.constructibleTracker.UpdateTrackedConstructibles();
			Find.MainTabsRoot.SetCurrentTab(BlueprintReportUtility.emptyInspectorDef);
		}

		private void DesignateSelected()
		{
			for (int i = 0; i < Find.Selector.NumSelected; i++)
				if (Find.Selector.SelectedObjects[i] is IConstructible)
					DesignateThing(Find.Selector.SelectedObjects[i] as Thing);
			Find.Selector.ClearSelection();
		}
	}
}
