#if UNITY_DOTS_ENABLED
using Unity.Entities;
using CodeStage.AntiCheat.ObscuredTypes;

namespace CodeStage.AntiCheat.Examples
{
	// Use these from non-Burst contexts
    public struct PlayerData : IComponentData
    {
        public ObscuredInt Score { get; set; }
        public ObscuredFloat Health { get; set; }
    }
}
#endif