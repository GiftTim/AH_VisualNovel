#if UNITY_DOTS_ENABLED
using Unity.Entities;

namespace CodeStage.AntiCheat.Examples
{
	public struct CheatState : IComponentData
	{
		public CheatCode LastCode { get; set; }
		public int TotalCount { get; set; }
	}
}
#endif