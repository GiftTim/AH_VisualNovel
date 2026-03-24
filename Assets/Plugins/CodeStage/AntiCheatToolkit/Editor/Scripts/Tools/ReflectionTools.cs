#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using System;
	using System.Reflection;
	using Common;
	using UnityEditor;
	using UnityEngine;
	using UnityEditor.Build;

	internal static class ReflectionTools
	{
		private static readonly Type ScriptingImplementationType = typeof(ScriptingImplementation);
		private delegate object GetScriptingImplementations(NamedBuildTarget target);
		private static readonly Type NamedBuildTargetType = typeof(NamedBuildTarget);
		private static readonly Type ModuleManagerType = ScriptingImplementationType.Assembly.GetType("UnityEditor.Modules.ModuleManager", false);
		private static readonly Type ScriptingImplementationsType = ScriptingImplementationType.Assembly.GetType("UnityEditor.Modules.IScriptingImplementations", false);

		private static GetScriptingImplementations getScriptingImplementationsDelegate;
		private static MethodInfo scriptingImplementationsTypeEnabledMethodInfo;

		public static bool IsScriptingImplementationSupported(ScriptingImplementation implementation, BuildTargetGroup target)
		{
			if (ModuleManagerType == null)
			{
				ACTk.PrintExceptionForSupport("Couldn't find UnityEditor.Modules.ModuleManager type!");
				return false;
			}

			if (ScriptingImplementationsType == null)
			{
				ACTk.PrintExceptionForSupport("Couldn't find UnityEditor.Modules.IScriptingImplementations type!");
				return false;
			}

			if (getScriptingImplementationsDelegate == null)
			{
				var mi = ModuleManagerType.GetMethod("GetScriptingImplementations", BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new []{NamedBuildTargetType}, null);
				if (mi == null)
				{
					ACTk.PrintExceptionForSupport("Couldn't find GetScriptingImplementations method!");
					return false;
				}
				getScriptingImplementationsDelegate = (GetScriptingImplementations)Delegate.CreateDelegate(typeof(GetScriptingImplementations), mi);
			}

			var namedTarget = NamedBuildTarget.FromBuildTargetGroup(target);
			var result = getScriptingImplementationsDelegate.Invoke(namedTarget);
			if (result == null) // happens for default platform support module
			{
				return PlayerSettings.GetDefaultScriptingBackend(namedTarget) == implementation;
			}

			if (scriptingImplementationsTypeEnabledMethodInfo == null)
			{
				scriptingImplementationsTypeEnabledMethodInfo = ScriptingImplementationsType.GetMethod("Enabled", BindingFlags.Public | BindingFlags.Instance);
				if (scriptingImplementationsTypeEnabledMethodInfo == null)
				{
					ACTk.PrintExceptionForSupport("Couldn't find IScriptingImplementations.Enabled() method!");
					return false;
				}
			}

			var enabledImplementations = (ScriptingImplementation[])scriptingImplementationsTypeEnabledMethodInfo.Invoke(result, null);
			return Array.IndexOf(enabledImplementations, implementation) != -1;
		}
	}
}