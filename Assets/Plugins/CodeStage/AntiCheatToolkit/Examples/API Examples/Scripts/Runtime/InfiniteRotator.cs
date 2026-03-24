#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Examples
{
	using UnityEngine;

	// dummy code, just to add some rotation to the cube from example scene
	[AddComponentMenu("")]
	internal class InfiniteRotator : MonoBehaviour
	{
		public Color cubeColor = Color.green;
		
		[Range(1f, 100f)]
		public float speed = 5f;
		
		private Renderer cubeRenderer;
		
		private void Start()
		{
			cubeRenderer = GetComponent<Renderer>();
			ApplyColorTexture();
		}

		private void ApplyColorTexture()
		{
			const int shaderTextureSize = 4;
			var shaderTexture = new Texture2D(shaderTextureSize, shaderTextureSize, TextureFormat.RGB24, false, false)
			{
				filterMode = FilterMode.Point
			};

			const int pixelsCount = shaderTextureSize * shaderTextureSize;
			var colors = new Color[pixelsCount];

			for (var i = 0; i < pixelsCount; i++)
			{
				colors[i] = cubeColor;
			}

			shaderTexture.SetPixels(colors, 0);
			shaderTexture.Apply();
			
			var material = cubeRenderer.material;
			material.mainTexture = shaderTexture;
			material.color = cubeColor;
			cubeRenderer.material = material;
		}

		private void Update()
		{
			transform.Rotate(0, speed * GetDeltaTime(), 0);
		}

		private protected virtual float GetDeltaTime()
		{
			return Time.deltaTime;
		}
	}
}