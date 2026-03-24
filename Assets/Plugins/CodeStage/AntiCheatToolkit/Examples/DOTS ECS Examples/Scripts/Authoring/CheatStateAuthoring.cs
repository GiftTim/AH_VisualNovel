#if UNITY_DOTS_ENABLED
namespace CodeStage.AntiCheat.Examples
{
	using Unity.Entities;
	using UnityEngine;

	public class CheatStateAuthoring : MonoBehaviour
	{
		private class Baker : Baker<CheatStateAuthoring>
		{
			public override void Bake(CheatStateAuthoring authoring)
			{
				var e = GetEntity(TransformUsageFlags.None);
				AddComponent(e, new CheatState { LastCode = CheatCode.None, TotalCount = 0 });
			}
		}
	}
}
#endif