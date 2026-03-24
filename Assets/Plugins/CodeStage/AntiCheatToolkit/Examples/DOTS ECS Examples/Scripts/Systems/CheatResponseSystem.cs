#if UNITY_DOTS_ENABLED
using UnityEngine;
using Unity.Entities;
using CodeStage.AntiCheat.Storage;

namespace CodeStage.AntiCheat.Examples
{
	// Run this on the main thread (no Burst) since it touches Unity logging/PlayerPrefs.
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public partial class CheatResponseSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			// Run on main thread; no Burst
			var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
			
			if (!SystemAPI.TryGetSingletonRW<CheatState>(out var cheatState))
			{
				var e = EntityManager.CreateEntity(typeof(CheatState));
				EntityManager.SetComponentData(e, new CheatState());
				cheatState = SystemAPI.GetSingletonRW<CheatState>();
			}
			
			var any = false;
			
			foreach (var (cd, e) in SystemAPI.Query<RefRO<CheatDetected>>().WithEntityAccess())
			{
				any = true;
				Debug.LogWarning($"[ACTk] Cheat detected (code {cd.ValueRO.Code}). Taking action.");
				
				// Update UI-visible state
				var s = cheatState.ValueRO;
				s.LastCode = cd.ValueRO.Code;
				s.TotalCount++;
				cheatState.ValueRW = s;
				
				ecb.DestroyEntity(e);
			}

			if (any)
				ecb.Playback(EntityManager);
			ecb.Dispose();
		}
	}
}
#endif