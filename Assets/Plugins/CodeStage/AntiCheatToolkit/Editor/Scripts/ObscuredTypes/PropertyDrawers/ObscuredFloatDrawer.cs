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
	[CustomPropertyDrawer(typeof(ObscuredFloat))]
	internal class ObscuredFloatDrawer : ObscuredTypeDrawer<SerializedObscuredFloat, float>
	{
		private protected override void DrawProperty(Rect position, SerializedProperty sp, GUIContent label)
		{
#if UNITY_2022_1_OR_NEWER
			plain = EditorGUI.DelayedFloatField(position, label, plain);
#else
			plain = EditorGUI.FloatField(position, label, plain);
#endif
		}
		
		private protected override void ApplyChanges()
		{
			var newKey = ObscuredFloat.GenerateKey();
			serialized.Key = newKey;
			serialized.Hidden = ObscuredFloat.Encrypt(plain, newKey);
			serialized.Hash = HashUtils.CalculateHash(plain);
			serialized.Version = ObscuredFloat.Version;
		}
	}
}