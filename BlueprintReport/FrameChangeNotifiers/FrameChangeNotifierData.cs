using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Harmony;

namespace BlueprintReport.FrameChangeNotifiers
{
	class FrameChangeNotifierData
	{
		public static List<Action> methodsToNotify = new List<Action>();

		public static void RegisterMethod(Action method) => methodsToNotify.Add(method);

		public static void DeregisterMethod(Action method)
		{
			if (methodsToNotify.Contains(method))
				methodsToNotify.Remove(method);
		}

		public static void NotifyChange()
		{
			foreach (Action method in methodsToNotify)
				method();
		}
	}
}
