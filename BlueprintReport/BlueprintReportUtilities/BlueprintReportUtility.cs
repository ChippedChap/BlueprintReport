using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace BlueprintReport
{
	static class BlueprintReportUtility
	{
		public static readonly float ellipsesSize = Text.CalcSize("...").x;

		public static MainButtonDef emptyInspectorDef = DefDatabase<MainButtonDef>.GetNamed("EmptyInspect");

		public static DesignationDef tabulateDesignationDef = DefDatabase<DesignationDef>.GetNamed("TabulateConstructible");

		// Like ContractedBy() but for width only
		public static Rect WidthContractedBy(this Rect original, float xMargin)
		{
			return new Rect(original.x + xMargin, original.y, original.width - xMargin * 2, original.height);
		}

		// Takes two ThingCounts and returns a new one with the counts added
		public static ThingCount AddThingCounts(ThingCount tc1, ThingCount tc2)
		{
			if (tc1.ThingDef != tc2.ThingDef) Log.Warning("Added two ThingCounts of different ThingDefs. New ThingCount will have ThingDef of first ThingCount.");
			return new ThingCount(tc1.ThingDef, tc1.Count + tc2.Count);
		}

		// Get materials considering that IConstructible may be a Blueprint_Install
		public static List<ThingCountClass> GetMaterialsNeededSafely(this IConstructible constructible)
		{
			if (constructible is Blueprint_Install bInstall)
			{
				return new List<ThingCountClass>() { new ThingCount(bInstall.GetInnerIfMinified().def, 1) };
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

		// Convert int to string but add positive sign if integer is positive and not zero.
		public static string ToStringWithSign(this int integer)
		{
			if (integer > 0)
				return "+" + integer.ToString();
			return integer.ToString();
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
