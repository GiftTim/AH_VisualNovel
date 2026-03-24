#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if ACTK_WALLHACK_LINK_XML

namespace CodeStage.AntiCheat.EditorCode.Processors
{
	using System;
	using System.IO;
	using Common;
	using Detectors;
	using UnityEditor.Build;
	using UnityEditor.Build.Reporting;
	using UnityEditor.UnityLinker;
	using UnityEngine;

	internal class WallHackDetectorLinkerProcessor : BaseLinkerProcessor
	{
		public WallHackDetectorLinkerProcessor()
		{
			path = Path.Combine(ACTkEditorConstants.ProjectTempFolder, "actk-wh-link.xml");
		}

		private protected override string ConstructLinkData()
		{
			return "<linker>\n" +
				   $"\t<assembly fullname=\"{nameof(UnityEngine)}\">\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(BoxCollider)}\" preserve=\"all\"/>\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(MeshCollider)}\" preserve=\"all\"/>\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(CapsuleCollider)}\" preserve=\"all\"/>\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(Camera)}\" preserve=\"all\"/>\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(Rigidbody)}\" preserve=\"all\"/>\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(MeshRenderer)}\" preserve=\"all\"/>\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(CharacterController)}\" preserve=\"all\"/>\n" +
				   "\t</assembly>\n" +
				   "</linker>";
		}
	}
}

#endif