#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

using System;
using System.IO;
using CodeStage.AntiCheat.Common;
using CodeStage.AntiCheat.Detectors;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.Processors
{
	public abstract class BaseLinkerProcessor : IUnityLinkerProcessor
	{
		public int callbackOrder { get; }
		private protected string path;
		private static string linkData;

		public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
		{
			try
			{
				linkData ??= ConstructLinkData();
				File.WriteAllText(path, linkData);
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Couldn't write link.xml!", e);
			}
			
			Debug.Log($"{ACTk.LogPrefix}Additional link.xml generated:\n{path}");
			return path;
		}

		private protected abstract string ConstructLinkData();
	}
}