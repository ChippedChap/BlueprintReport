﻿using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace BlueprintReport.BlueprintReportUtilities
{
	static class BlueprintReportUtility
	{
		public static readonly float ellipsesSize = Text.CalcSize("...").x;

		public static readonly MainButtonDef emptyInspectorDef = DefDatabase<MainButtonDef>.GetNamed("EmptyInspect");

		public static readonly DesignationDef tabulateDesignationDef = DefDatabase<DesignationDef>.GetNamed("TabulateConstructible");

		// Like ContractedBy() but for width only
		public static Rect WidthContractedBy(this Rect original, float xMargin)
		{
			return new Rect(original.x + xMargin, original.y, original.width - xMargin * 2, original.height);
		}

		// Takes two ThingCounts and returns a new one with the counts added
		public static ThingDefCount AddThingCounts(ThingDefCount tc1, ThingDefCount tc2)
		{
			if (tc1.ThingDef != tc2.ThingDef) Log.Warning("Added two ThingCounts of different ThingDefs. New ThingCount will have ThingDef of first ThingCount.");
			return new ThingDefCount(tc1.ThingDef, tc1.Count + tc2.Count);
		}

		// Get materials considering that IConstructible may be a Blueprint_Install
		public static List<ThingDefCountClass> GetMaterialsNeededSafely(this IConstructible constructible)
		{
			if (constructible is Blueprint_Install bInstall)
			{
				return new List<ThingDefCountClass>() { new ThingDefCountClass(bInstall.GetInnerIfMinified().def, 1) };
			}
			return constructible.MaterialsNeeded();
		}

		// Shitty algorithm to trim text for a certain pixel width using a binary-searchy method.
		public static string TrimTextToBeOfSize(string stringToTrim, float sizeToTrimTo)
		{
			// Deal with special cases
			if (Text.CalcSize(stringToTrim).x <= sizeToTrimTo)
				return stringToTrim;
			else if (Text.CalcSize(stringToTrim.Substring(0, 1)).x > sizeToTrimTo)
				return "";
			// Do the algorithm
			int lower = 0;
			int higher = stringToTrim.Length;
			while (higher - lower > 1)
			{
				int test = (higher + lower) / 2;
				float subStringWidth = Text.CalcSize(stringToTrim.Substring(0, test)).x;
				if (subStringWidth <= sizeToTrimTo)
				{
					lower = test;
				}
				else if (subStringWidth > sizeToTrimTo)
				{
					higher = test;
				}
			}
			return stringToTrim.Substring(0, lower);
		}
		
		// Select something by adding directly to the selector's selected object list and then notfying the drawer.
		public static void SelectRaw(this Selector selector, object objectToSelect, bool notifyChange = true)
		{
			selector.SelectedObjects.Add(objectToSelect);
			if (notifyChange)
				SelectionDrawer.Notify_Selected(objectToSelect);
		}

		// Get first IConstructible as Thing on an IntVec3
		public static Thing GetFirstConstructible(this IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i=0; i < list.Count; i++)
				if (list[i] is IConstructible thingAsConstructible)
					return thingAsConstructible as Thing;
			return null;
		}
		
		// Get the number of a certain ThingDef that exists on the map.
		public static int GetCountAll(this Map map, ThingDef def)
		{
			List<Thing> thingList = map.listerThings.ThingsOfDef(def);
			int count = 0;
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing innerIfMinified = thingList[i].GetInnerIfMinified();
				if (innerIfMinified.def.CountAsResource && !thingList[i].IsNotFresh() && !thingList[i].IsForbidden(Faction.OfPlayer))
					count += innerIfMinified.stackCount;
			}
			return count;
		}

		// Get difference between count and the number present on map
		public static int GetCountOnMapDifference(this Map map, ThingDefCount count)
		{
			if (count.ThingDef.IsBlueprint)
				return 0;
			return count.Count - map.GetCountAll(count.ThingDef);
		}

		// Same as above, but can pass amount present on map through out paramater.
		public static int GetCountOnMapDifference(this Map map, ThingDefCount count, out int numOnMap)
		{
			if (count.ThingDef.IsBlueprint)
			{
				numOnMap = count.Count;
				return 0;
			}
			numOnMap = map.GetCountAll(count.ThingDef);
			return count.Count - numOnMap;
		}

		// Get designator instance of type
		public static Designator GetDesigInstanceOfType(Type type)
		{
			List<DesignationCategoryDef> allDesigCats = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
			for (int i = 0; i < allDesigCats.Count; i++)
				for (int j = 0; j < allDesigCats[i].AllResolvedDesignators.Count; j++)
					if (allDesigCats[i].AllResolvedDesignators[j].GetType() == type)
						return allDesigCats[i].AllResolvedDesignators[j];
			return MakeDesignatorInstance(type);
		}

		private static Designator MakeDesignatorInstance(Type type)
		{
			Log.Message("Making new designator instance");
			try
			{
				return (Designator)Activator.CreateInstance(type);
			}
			catch (Exception ex)
			{
				Log.Error("Exception when BlueprintReportUtility tried to instantiate designator of type " + type.ToString() + ".\nException: \n" + ex.ToString());
				return null;
			}
		}
	}
}
