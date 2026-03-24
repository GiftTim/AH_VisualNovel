#if UNITY_DOTS_ENABLED
using Unity.Entities;

namespace CodeStage.AntiCheat.Examples
{
	public enum CheatCode : byte
	{
		None = 0,
		SpeedHack = 1,
		TimeCheating = 2,
		ObscuredCheating = 3,
		WallHack = 4,
		Injection = 5,
	}
	
	public struct CheatDetected : IComponentData
	{
		public CheatCode Code { get; set; }
	}
}
#endif