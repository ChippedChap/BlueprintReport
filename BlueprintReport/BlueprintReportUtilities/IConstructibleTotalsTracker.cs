using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BlueprintReport
{
	enum TotalsSortModes
	{
		Absolute,
		Difference,
		Unsorted
	}

	class IConstructibleTotalsTracker
	{
		private HashSet<IConstructible> trackedConstructibles = new HashSet<IConstructible>();
		private List<ThingDefCount> cachedRequirementsTotals;
		private bool cachedTotalsAreUpdated = false;
		private TotalsSortModes cachedTotalsSortMode = TotalsSortModes.Unsorted;
		public bool sortDescending = true;

		public int NumOfUniqueThingDefsInTotals
		{
			get
			{
				if (cachedRequirementsTotals == null)
					GetRequirementsTotals();
				return cachedRequirementsTotals.Count;
			}
		}

		public int NumConstructiblesTracked => trackedConstructibles.Count;

		public IConstructibleTotalsTracker()
		{
			SelectorChangeNotifiers.SelectionChangeNotifierData.RegisterMethod(UpdateTrackedConstructibles);
			FrameChangeNotifiers.FrameChangeNotifierData.RegisterMethod(UpdateTrackedConstructibles);
		}

		public bool BlueprintBuildIsTracked(Blueprint_Build blueprint) => trackedConstructibles.Contains(blueprint);

		public List<ThingDefCount> GetRequirementsTotals(TotalsSortModes modeToSortCountsBy = TotalsSortModes.Absolute, bool ascending = false)
		{
			if (!cachedTotalsAreUpdated)
			{
				List<ThingDefCount> protoRequirementsTotals = new List<ThingDefCount>();
				foreach(IConstructible tracked in trackedConstructibles)
				{
					foreach(ThingDefCount requirements in tracked.GetMaterialsNeededSafely())
					{
						int thingCountWithSameThingDefIndex = protoRequirementsTotals.FindIndex(x => x.ThingDef == requirements.ThingDef);
						if (thingCountWithSameThingDefIndex == -1)
						{
							protoRequirementsTotals.Add(requirements);
						}
						else
						{
							ThingDefCount tcWithSameThingDef = protoRequirementsTotals[thingCountWithSameThingDefIndex];
							protoRequirementsTotals[thingCountWithSameThingDefIndex] = BlueprintReportUtility.AddThingCounts(tcWithSameThingDef, requirements);
						}
					}
				}
				cachedRequirementsTotals = protoRequirementsTotals;
				cachedTotalsAreUpdated = true;
			}
			SortTotalsIfNecessary(modeToSortCountsBy, ascending);
			return cachedRequirementsTotals;
		}

		public void ForceCacheRegeneration()
		{
			cachedTotalsAreUpdated = false;
			cachedTotalsSortMode = TotalsSortModes.Unsorted;
		}

		public void UpdateTrackedConstructibles()
		{
			if (Find.Selector.NumSelected > 0)
				UpdateTrackedFromSelObjs();
			else
				UpdateTrackedFromDesigThings();
			ForceCacheRegeneration();
		}

		private void SortTotalsIfNecessary(TotalsSortModes sortMode, bool descending)
		{
			if (sortMode == cachedTotalsSortMode && sortDescending == descending) return;
			switch (sortMode)
			{
				case TotalsSortModes.Absolute:
					cachedRequirementsTotals.Sort((x, y) => x.Count.CompareTo(y.Count));
					break;
				case TotalsSortModes.Difference:
					cachedRequirementsTotals.Sort((x, y) => (Find.CurrentMap.GetCountOnMapDifference(x)).CompareTo(
						Find.CurrentMap.GetCountOnMapDifference(y)));
					break;
			}
			if (descending) cachedRequirementsTotals.Reverse();
			cachedTotalsSortMode = sortMode;
			sortDescending = descending;
		}

		private void UpdateTrackedFromSelObjs()
		{
			// Add new selected but untracked constructibles.
			for (int i=0; i < Find.Selector.SelectedObjects.Count; i++)
			{
				object selectedObj = Find.Selector.SelectedObjects[i];
				if (selectedObj is IConstructible && !trackedConstructibles.Contains((IConstructible)selectedObj))
					trackedConstructibles.Add((IConstructible)selectedObj);
			}
			// Remove unselected or deleted tracked constructibles.
			List<IConstructible> trackedListCopy = trackedConstructibles.ToList();
			for (int i=0; i < trackedListCopy.Count; i++)
			{
				IConstructible trackedObj = trackedListCopy[i];
				if (!Find.Selector.SelectedObjects.Contains(trackedObj) || ((Thing)trackedObj).Destroyed)
					trackedConstructibles.Remove(trackedObj);
			}
		}

		private void UpdateTrackedFromDesigThings()
		{
			// Add new designated constructibles.
			foreach(Designation designation in Find.CurrentMap.designationManager.SpawnedDesignationsOfDef(BlueprintReportUtility.tabulateDesignationDef))
			{
				LocalTargetInfo desTgt = designation.target;
				if (desTgt.HasThing && desTgt.Thing is IConstructible && !trackedConstructibles.Contains(desTgt.Thing as IConstructible))
					trackedConstructibles.Add(desTgt.Thing as IConstructible);
			}
			// Remove undesignated constructibles.
			foreach (IConstructible constructible in new HashSet<IConstructible>(trackedConstructibles))
				if (Find.CurrentMap.designationManager.DesignationOn((Thing)constructible, BlueprintReportUtility.tabulateDesignationDef) == null)
					trackedConstructibles.Remove(constructible);
		}
	}
}
