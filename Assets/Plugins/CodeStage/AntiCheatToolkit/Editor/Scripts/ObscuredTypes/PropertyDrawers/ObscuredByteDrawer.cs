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
	[CustomPropertyDrawer(typeof(ObscuredByte))]
	internal class ObscuredByteDrawer : ObscuredTypeDrawer<SerializedObscuredByte, byte>
	{
		private protected override void DrawProperty(Rect position, SerializedProperty sp, GUIContent label)
		{
			plain = (byte)EditorGUI.IntField(position, label, plain);
		}

		private protected override void ApplyChanges()
		{
			var newKey = ObscuredByte.GenerateKey();
			serialized.Key = newKey;
			serialized.Hidden = ObscuredByte.Encrypt(plain, newKey);
			serialized.Hash = HashUtils.CalculateHash(plain);
			serialized.Version = ObscuredByte.Version;
		}
	}
}


