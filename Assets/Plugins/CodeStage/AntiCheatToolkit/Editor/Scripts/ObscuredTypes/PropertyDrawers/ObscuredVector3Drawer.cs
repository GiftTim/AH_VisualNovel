#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.ObscuredTypes.EditorCode;
using CodeStage.AntiCheat.Utils;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(ObscuredVector3))]
	internal class ObscuredVector3Drawer : WideObscuredTypeDrawer<SerializedObscuredVector3, Vector3>
	{
		private protected override void DrawProperty(Rect position, SerializedProperty sp, GUIContent label)
		{
			plain = EditorGUI.Vector3Field(position, label, plain);
		}

		private protected override void ApplyChanges()
		{
			var newKey = ObscuredVector3.GenerateKey();
			serialized.Key = newKey;
			serialized.Hidden = ObscuredVector3.Encrypt(plain, newKey);
			serialized.Hash = HashUtils.CalculateHash(plain);
			serialized.Version = ObscuredVector3.Version;
		}
	}
}