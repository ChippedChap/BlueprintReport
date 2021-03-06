﻿using Verse;
using Harmony;
using System.Reflection;

namespace BlueprintReport.BlueprintReportUtilities
{
	[StaticConstructorOnStartup]
	class BlueprintReportInitializer
	{
		static BlueprintReportInitializer()
		{
			var harmonyInstance = HarmonyInstance.Create("com.github.chippedchap.blueprintreport");
			harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
