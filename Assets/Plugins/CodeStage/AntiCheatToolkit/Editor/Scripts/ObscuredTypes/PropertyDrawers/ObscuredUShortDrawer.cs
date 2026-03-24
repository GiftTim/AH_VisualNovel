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
	[CustomPropertyDrawer(typeof(ObscuredUShort))]
	internal class ObscuredUShortDrawer : ObscuredTypeDrawer<SerializedObscuredUShort, ushort>
	{
		private protected override void DrawProperty(Rect position, SerializedProperty sp, GUIContent label)
		{
			plain = (ushort)EditorGUI.IntField(position, label, plain);
		}

		private protected override void ApplyChanges()
		{
			var newKey = ObscuredUShort.GenerateKey();
			serialized.Key = newKey;
			serialized.Hidden = ObscuredUShort.Encrypt(plain, newKey);
			serialized.Hash = HashUtils.CalculateHash(plain);
			serialized.Version = ObscuredUShort.Version;
		}
	}
}


