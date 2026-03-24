#if UNITY_DOTS_ENABLED
using Unity.Entities;

namespace CodeStage.AntiCheat.Examples
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public partial class PlayerBootstrapSystem : SystemBase
	{
		protected override void OnCreate()
		{
			if (SystemAPI.QueryBuilder().WithAll<PlayerData>().Build().IsEmpty)
			{
				var e = EntityManager.CreateEntity(typeof(PlayerData));
				EntityManager.SetComponentData(e, new PlayerData
				{
					Score  = 0,
					Health = 100f
				});
			}
		}
		protected override void OnUpdate() { }
	}
}
#endif