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
	[CustomPropertyDrawer(typeof(ObscuredShort))]
	internal class ObscuredShortDrawer : ObscuredTypeDrawer<SerializedObscuredShort, short>
	{
		private protected override void DrawProperty(Rect position, SerializedProperty sp, GUIContent label)
		{
#if UNITY_2022_1_OR_NEWER
			plain = (short)EditorGUI.DelayedIntField(position, label, plain);
#else
			plain = (short)EditorGUI.IntField(position, label, plain);
#endif
		}

		private protected override void ApplyChanges()
		{
			var newKey = ObscuredShort.GenerateKey();
			serialized.Key = newKey;
			serialized.Hidden = ObscuredShort.Encrypt(plain, newKey);
			serialized.Hash = HashUtils.CalculateHash(plain);
			serialized.Version = ObscuredShort.Version;
		}
	}
}