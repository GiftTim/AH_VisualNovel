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
	[CustomPropertyDrawer(typeof(ObscuredQuaternion))]
	internal class ObscuredQuaternionDrawer : WideObscuredTypeDrawer<SerializedObscuredQuaternion, Quaternion>
	{
		private protected override void DrawProperty(Rect position, SerializedProperty sp, GUIContent label)
		{
			plain = Vector4ToQuaternion(EditorGUI.Vector4Field(position, label, QuaternionToVector4(plain)));
		}

		private protected override void ApplyChanges()
		{
			var newKey = ObscuredQuaternion.GenerateKey();
			serialized.Key = newKey;
			serialized.Hidden = ObscuredQuaternion.Encrypt(plain, newKey);
			serialized.Hash = HashUtils.CalculateHash(plain);
			serialized.Version = ObscuredQuaternion.Version;
		}

		private static Vector4 QuaternionToVector4(Quaternion value)
		{
			return new Vector4(value.x, value.y, value.z, value.w);
		}
		
		private static Quaternion Vector4ToQuaternion(Vector4 value)
		{
			return new Quaternion(value.x, value.y, value.z, value.w);
		}
	}
}