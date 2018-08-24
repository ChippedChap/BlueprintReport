﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace BlueprintReport
{
	class ITab_Blueprints : ITab
	{
		private readonly float listDistanceFromTop = 50f;
		private readonly float listElementRectHeight = 27.5f;
		private readonly float listElementsMargin = 3f;
		private readonly float buttonWidth = 210f;
		private readonly float buttonNum = 2;

		private readonly Vector2 WinSize = new Vector2(250f, 400f);
		private Vector2 scrollPosition = new Vector2(0f, 0f);
		private float largestNumberWidth;
		private TotalsSortModes currentSortMode = TotalsSortModes.Absolute;
		private bool sortDescending = true;
		public IConstructibleTotalsTracker constructibleTracker;

		public static ITab_Blueprints Instance
		{
			get { return (ITab_Blueprints)InspectTabManager.GetSharedInstance(typeof(ITab_Blueprints)); }
		}

		public static MainTabWindow_EmptyInspect EmptyInspectPane
		{
			get { return (MainTabWindow_EmptyInspect)BlueprintReportUtility.emptyInspectorDef.TabWindow; }
		}

		protected override bool StillValid
		{
			get
			{
				return base.StillValid || (Find.MainTabsRoot.OpenTab == BlueprintReportUtility.emptyInspectorDef && ((MainTabWindow_EmptyInspect)Find.MainTabsRoot.OpenTab.TabWindow).CurTabs.Contains(this));
			}
		}

		public override bool IsVisible
		{
			get { return constructibleTracker.NumConstructiblesTracked > 0; }
		}

		public ITab_Blueprints()
		{
			constructibleTracker = new IConstructibleTotalsTracker();
			constructibleTracker.UpdateTrackedConstructibles();
			size = WinSize;
			labelKey = "BlueprintsTabLabel";
		}

		protected override void CloseTab()
		{
			base.CloseTab();
			EmptyInspectPane.CloseOpenTab();
		}

		protected override void FillTab()
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect baseTabRect = new Rect(Vector3.zero, WinSize).ContractedBy(10f);
			DoModeButton(baseTabRect);
			DoOrderButton(baseTabRect);
			// Rects for scrolling setup
			Rect listHolderRect = new Rect(baseTabRect.x, baseTabRect.y+listDistanceFromTop, baseTabRect.width, baseTabRect.height-listDistanceFromTop);
			Rect listRect = new Rect(0f, 0f, listHolderRect.width - 20f, GetListRectHeight(listHolderRect));
			// Prevent tooltips of list element from showing when mouse is not in list holder.
			bool rowCanDrawTips = Mouse.IsOver(listHolderRect);
			// Draw resource list
			List<ThingCount> thingCountList = constructibleTracker.GetRequirementsTotals(currentSortMode, sortDescending);
			UpdateLargestNumberWidth(thingCountList);
			Widgets.BeginScrollView(listHolderRect, ref scrollPosition, listRect, true);
			for (int i=0; i<constructibleTracker.NumOfUniqueThingDefsInTotals; i++)
			{
				DrawRequirementRow(thingCountList[i], listRect, i, rowCanDrawTips);
			}
			Widgets.EndScrollView();
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DoModeButton(Rect baseRect)
		{
			Rect buttonRect = new Rect(baseRect.x, baseRect.y, buttonWidth, listDistanceFromTop/buttonNum - 2.5f);
			string modeButtonString = "Error - currentSortMode does not match cases";
			switch (currentSortMode)
			{
				case TotalsSortModes.Absolute:
					modeButtonString = "TotalsSortModes_Absolute".Translate();
					TooltipHandler.TipRegion(buttonRect, new TipSignal("TotalsSortModes_AbsoluteTip".Translate()));
					break;
				case TotalsSortModes.Difference:
					modeButtonString = "TotalsSortModes_Difference".Translate();
					TooltipHandler.TipRegion(buttonRect, new TipSignal("TotalsSortModes_DifferenceTip".Translate()));
					break;
			}
			if (Widgets.ButtonText(buttonRect, modeButtonString, true, true, true))
				currentSortMode = (TotalsSortModes.Absolute == currentSortMode) ? TotalsSortModes.Difference : TotalsSortModes.Absolute;
		}

		private void DoOrderButton(Rect baseRect)
		{
			Rect buttonRect = new Rect(baseRect.x, baseRect.y + listDistanceFromTop/buttonNum - 2.5f, buttonWidth, listDistanceFromTop/buttonNum - 2.5f);
			string buttonLabel = (sortDescending) ? "SortButtonDescending".Translate() : "SortButtonAscending".Translate();
			if (Widgets.ButtonText(buttonRect, buttonLabel, true, true, true))
				sortDescending = !sortDescending;
		}

		private float GetListRectHeight(Rect listHolder)
		{
			float possibleHeight = constructibleTracker.NumOfUniqueThingDefsInTotals * listElementRectHeight;
			return (possibleHeight > listHolder.height) ? possibleHeight : listHolder.height + 0.1f;
		}

		private void UpdateLargestNumberWidth(List<ThingCount> tcList)
		{
			if (tcList.Count == 0) return;
			int firstPotentialIndex = 0;
			int secondPotentialIndex = tcList.Count - 1;
			largestNumberWidth = Mathf.Max(Text.CalcSize(tcList[firstPotentialIndex].Count.ToStringWithSign()).x, Text.CalcSize(tcList[secondPotentialIndex].Count.ToStringWithSign()).x);
		}

		private void DrawRequirementRow(ThingCount thingCount, Rect listHolder, int listPos, bool canDrawToolTips)
		{
			float yPosition = listPos * listElementRectHeight;
			Rect highlightRect = new Rect(0f, yPosition, listHolder.width, listElementRectHeight);
			// Do tool tips and background.
			if (listPos % 2 == 0) Widgets.DrawAltRect(highlightRect);
			if (canDrawToolTips) TooltipHandler.TipRegion(highlightRect, new TipSignal(GetReqRowTooltip(thingCount)));
			// Draw thingdef icon
			Rect mainElementsRect = highlightRect.WidthContractedBy(listElementsMargin);
			Rect iconRect = new Rect(mainElementsRect.x, yPosition, listElementRectHeight, listElementRectHeight);
			Widgets.ThingIcon(iconRect, thingCount.ThingDef);
			// Do text labels
			Rect labelsRect = new Rect(listElementRectHeight, yPosition, listHolder.width - listElementRectHeight, listElementRectHeight).WidthContractedBy(2 * listElementsMargin);
			DoTextLabel(labelsRect, thingCount.ThingDef.LabelCap);
			DoNumberLabel(labelsRect, thingCount);
		}

		private float GetFillFraction(ThingCount tc)
		{
			float needed = tc.Count;
			float available = Find.VisibleMap.resourceCounter.GetCount(tc.ThingDef);
			return Mathf.Clamp(available / needed, 0, 1);
		}

		private string GetReqRowTooltip(ThingCount rowTC)
		{
			// First line
			string tipString = "ReqRowTipLine1".Translate(new object[] {
				rowTC.Count,
				rowTC.ThingDef.label
			});
			// Second line
			int difference = rowTC.Count - Find.VisibleMap.resourceCounter.GetCount(rowTC.ThingDef);
			if (difference < 0)
			{
				tipString += "ReqRowTipLine2Excess".Translate(new object[] {
					-difference,
					rowTC.ThingDef.label
				});
			}
			else if (difference > 0)
			{
				tipString += "ReqRowTipLine2Lack".Translate(new object[] {
					difference,
					rowTC.ThingDef.label
				});
			}
			return tipString;
		}

		private void DoTextLabel(Rect rowRect, string thingDefName)
		{
			float maxWidth = rowRect.width - largestNumberWidth - listElementsMargin - BlueprintReportUtility.ellipsesSize;
			string labelText = (Text.CalcSize(thingDefName).x > maxWidth) ? BlueprintReportUtility.TrimTextToBeOfSize(thingDefName, maxWidth)+"..." : thingDefName;
			Widgets.Label(rowRect, labelText);
		}

		private void DoNumberLabel(Rect rowRect, ThingCount tc)
		{
			Rect textRect = new Rect(rowRect.x + rowRect.width - largestNumberWidth, rowRect.y, largestNumberWidth, rowRect.height);
			switch (currentSortMode)
			{
				case TotalsSortModes.Absolute:
					Widgets.Label(textRect, tc.Count.ToString());
					break;
				case TotalsSortModes.Difference:
					int difference = tc.Count - Find.VisibleMap.resourceCounter.GetCount(tc.ThingDef);
					string strToDisplay = difference.ToStringWithSign();
					Widgets.Label(textRect, strToDisplay);
					break;
			}
		}
	}
}