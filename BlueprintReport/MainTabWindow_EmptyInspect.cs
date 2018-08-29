using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
namespace BlueprintReport
{
	class MainTabWindow_EmptyInspect : MainTabWindow, IInspectPane
	{
		private List<Type> designatorTypesToDisplay = new List<Type>() { typeof(Designator_TabulateConstructible), typeof(Designator_UntabulateConstructible) };

		private List<Designator> resolvedDesignatorTypes = new List<Designator>();

		private Gizmo mouseoverGizmo;

		public List<Type> DesignatorTypesToDisplay
		{
			get { return designatorTypesToDisplay; }
			set
			{
				designatorTypesToDisplay = value;
				ResolveDesigsToDisplay();
			}
		}

		protected override float Margin { get { return 0; } }

		public override Vector2 RequestedTabSize { get { return InspectPaneUtility.PaneSizeFor(this); } }

		public float RecentHeight { get; set; }

		public Type OpenTabType { get; set; }

		public bool AnythingSelected
		{
			get { return true; }
		}

		public IEnumerable<InspectTabBase> CurTabs
		{
			get { return new InspectTabBase[1] { ITab_Blueprints.Instance }; }
		}

		public bool ShouldShowSelectNextInCellButton
		{
			get { return false; }
		}

		public bool ShouldShowPaneContents
		{
			get { return false; }
		}

		public float PaneTopY
		{
			get { return (float)UI.screenHeight - 165f - 35f; }
		}

		public MainTabWindow_EmptyInspect()
		{
			ResolveDesigsToDisplay();
		}


		public void CloseOpenTab()
		{
			OpenTabType = null;
		}

		public void DoInspectPaneButtons(Rect rect, ref float lineEndWidth)
		{
		}

		public void DoPaneContents(Rect rect)
		{
		}

		public void DrawInspectGizmos()
		{
			GizmoGridDrawer.DrawGizmoGrid(resolvedDesignatorTypes.Cast<Gizmo>(), InspectPaneUtility.PaneWidthFor(this) + 20f, out mouseoverGizmo);
		}

		public string GetLabel(Rect rect)
		{
			return "";
		}

		public void Reset()
		{
			OpenTabType = null;
		}

		public void SelectNextInCell()
		{
		}

		public override void PostOpen()
		{
			OpenTabType = typeof(ITab_Blueprints);
		}

		public override void PreClose()
		{
			Find.CurrentMap.designationManager.allDesignations.RemoveAll(d => d.def == BlueprintReportUtility.tabulateDesignationDef);
		}

		public override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			InspectPaneUtility.ExtraOnGUI(this);
		}

		public override void WindowUpdate()
		{
			base.WindowUpdate();
			InspectPaneUtility.UpdateTabs(this);
		}

		private void ResolveDesigsToDisplay()
		{
			resolvedDesignatorTypes.Clear();
			for (int i=0; i < designatorTypesToDisplay.Count; i++)
			{
				Designator potentialDesig = BlueprintReportUtility.GetDesigInstanceOfType(designatorTypesToDisplay[i]);
				if (potentialDesig != null)
					resolvedDesignatorTypes.Add(potentialDesig);
			}
		}
	}
}
