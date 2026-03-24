#if UNITY_DOTS_ENABLED
using Unity.Entities;
using UnityEngine;

namespace CodeStage.AntiCheat.Examples
{
	public class PlayerAuthoring : MonoBehaviour
	{
		[Min(0)] public int startScore = 0;
		[Min(0)] public int startHealth = 100;

		private class Baker : Baker<PlayerAuthoring>
		{
			public override void Bake(PlayerAuthoring authoring)
			{
				var e = GetEntity(TransformUsageFlags.None);
				AddComponent(e, new PlayerData
				{
					Score  = authoring.startScore,
					Health = authoring.startHealth
				});
			}
		}
	}
}
#endif