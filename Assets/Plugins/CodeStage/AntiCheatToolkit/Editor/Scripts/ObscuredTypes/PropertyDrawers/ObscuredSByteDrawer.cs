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
	[CustomPropertyDrawer(typeof(ObscuredSByte))]
	internal class ObscuredSByteDrawer : ObscuredTypeDrawer<SerializedObscuredSByte, sbyte>
	{
		private protected override void DrawProperty(Rect position, SerializedProperty sp, GUIContent label)
		{
			plain = (sbyte)EditorGUI.IntField(position, label, plain);
		}

		private protected override void ApplyChanges()
		{
			var newKey = ObscuredSByte.GenerateKey();
			serialized.Key = newKey;
			serialized.Hidden = ObscuredSByte.Encrypt(plain, newKey);
			serialized.Hash = HashUtils.CalculateHash(plain);
			serialized.Version = ObscuredSByte.Version;
		}
	}
}


