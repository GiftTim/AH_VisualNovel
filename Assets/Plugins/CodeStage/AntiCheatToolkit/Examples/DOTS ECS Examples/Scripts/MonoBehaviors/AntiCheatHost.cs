#if UNITY_DOTS_ENABLED
using CodeStage.AntiCheat.Detectors;
using Unity.Entities;
using UnityEngine;

namespace CodeStage.AntiCheat.Examples
{
	public class AntiCheatHost : MonoBehaviour
	{
		[Header("Enable what you need")]
		public bool detectSpeedHack = true;
		public bool detectObscuredCheating = true;
		public bool useHoneyPot = true;
		public bool detectTimeCheating = true;
		public bool detectWallHack = true;
#if !ENABLE_IL2CPP
		public bool injectionDetector = true;
#endif

		private EntityManager em;

		private void Awake()
		{
			var world = World.DefaultGameObjectInjectionWorld;
			em = world.EntityManager;
		}

		private void Start()
		{
			if (detectSpeedHack)
				SpeedHackDetector.StartDetection(OnSpeedHackDetected);

			if (detectObscuredCheating)
				ObscuredCheatingDetector.StartDetection(OnObscuredCheatingDetected).honeyPot = useHoneyPot;

			if (detectTimeCheating)
				TimeCheatingDetector.StartDetection(OnTimeCheatingDetected);

			if (detectWallHack)
				WallHackDetector.StartDetection(OnWallHackDetected);

#if !ENABLE_IL2CPP
			if (injectionDetector)
				InjectionDetector.StartDetection(OnInjectionDetected);
#endif
		}

		private void OnDestroy()
		{
			if (detectSpeedHack) SpeedHackDetector.StopDetection();
			if (detectObscuredCheating) ObscuredCheatingDetector.StopDetection();
			if (detectTimeCheating) TimeCheatingDetector.StopDetection();
			if (detectWallHack) WallHackDetector.StopDetection();
#if !ENABLE_IL2CPP
			if (injectionDetector) InjectionDetector.StopDetection();
#endif
		}

		private void OnSpeedHackDetected()
		{
			ProcessCheatDetection(CheatCode.SpeedHack);
		}
		
		private void OnObscuredCheatingDetected()
		{
			ProcessCheatDetection(CheatCode.ObscuredCheating);
		}

		private void OnTimeCheatingDetected(TimeCheatingDetector.CheckResult result, TimeCheatingDetector.ErrorKind error)
		{
			ProcessCheatDetection(CheatCode.TimeCheating);
		}

		private void OnWallHackDetected()
		{
			ProcessCheatDetection(CheatCode.WallHack);
		}

#if !ENABLE_IL2CPP
		private void OnInjectionDetected(string reason)
		{
			ProcessCheatDetection(CheatCode.Injection);
		}
#endif

		private void ProcessCheatDetection(CheatCode code)
		{
			// Push a one-shot signal into ECS
			var e = em.CreateEntity();
			em.AddComponentData(e, new CheatDetected { Code = code });
		}
	}
}
#endif