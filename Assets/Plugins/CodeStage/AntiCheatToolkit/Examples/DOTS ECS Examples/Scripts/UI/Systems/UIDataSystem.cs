#if UNITY_DOTS_ENABLED
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using CodeStage.AntiCheat.Detectors;
using CodeStage.AntiCheat.Time;
using CodeStage.AntiCheat.Genuine.Android;
using CodeStage.AntiCheat.Genuine.CodeHash;

namespace CodeStage.AntiCheat.Examples
{
    /// <summary>
    /// System responsible for updating UI data from various sources
    /// Runs on main thread to access Unity APIs safely
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class UIDataSystem : SystemBase
    {
        private bool speedHackProofTimeInitialized = false;

        protected override void OnCreate()
        {
            // Create UI singleton entity
            var uiEntity = EntityManager.CreateEntity(typeof(UIData), typeof(UISingleton));
            EntityManager.SetComponentData(uiEntity, new UIData());
            EntityManager.SetComponentData(uiEntity, new UISingleton { IsInitialized = true });
        }

        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonRW<UIData>(out var uiData))
                return;

            // Initialize SpeedHackProofTime when detector is ready
            if (!speedHackProofTimeInitialized && SpeedHackDetector.Instance != null && SpeedHackDetector.Instance.IsRunning)
            {
                SpeedHackProofTime.Init();
                speedHackProofTimeInitialized = true;
                Debug.Log("[ACTk] SpeedHackProofTime initialized after SpeedHackDetector started");
            }

            // Update player data display values
            UpdatePlayerData(ref uiData.ValueRW);
            
            // Update cheat state display
            UpdateCheatState(ref uiData.ValueRW);
            
            // Update detector states
            UpdateDetectorStates(ref uiData.ValueRW);
            
            // Update SpeedHackProofTime values
            UpdateSpeedHackProofTime(ref uiData.ValueRW);
            
#if UNITY_ANDROID
            // Update Android-specific values
            UpdateAndroidValues(ref uiData.ValueRW);
#endif
        }

        private void UpdatePlayerData(ref UIData uiData)
        {
            if (SystemAPI.TryGetSingleton<PlayerData>(out var playerData))
            {
                uiData.DisplayScore = playerData.Score;   // Decrypt
                uiData.DisplayHealth = playerData.Health; // Decrypt
            }
        }

        private void UpdateCheatState(ref UIData uiData)
        {
            if (SystemAPI.TryGetSingleton<CheatState>(out var cheatState))
            {
                uiData.CheatCount = cheatState.TotalCount;
                uiData.LastCheatCode = cheatState.LastCode;
            }
        }

        private void UpdateDetectorStates(ref UIData uiData)
        {
            uiData.SpeedHackDetected = SpeedHackDetector.Instance?.IsCheatDetected ?? false;
            uiData.WallHackDetected = WallHackDetector.Instance?.IsCheatDetected ?? false;
            uiData.TimeCheatingDetected = TimeCheatingDetector.Instance?.IsCheatDetected ?? false;
            uiData.ObscuredCheatingDetected = ObscuredCheatingDetector.Instance?.IsCheatDetected ?? false;
        }

        private void UpdateSpeedHackProofTime(ref UIData uiData)
        {
            if (speedHackProofTimeInitialized)
            {
                uiData.ProofTime = SpeedHackProofTime.time;
                uiData.ProofDeltaTime = SpeedHackProofTime.deltaTime;
                uiData.ProofUnscaledTime = SpeedHackProofTime.unscaledTime;
                uiData.ProofRealtimeSinceStartup = SpeedHackProofTime.realtimeSinceStartup;
            }
        }

        /// <summary>
        /// Updates storage values in UI data (called by UIActionSystem)
        /// </summary>
        public static void UpdateStorageValue(EntityManager em, UIActionType actionType, int value)
        {
            if (em.CreateEntityQuery(typeof(UIData)).TryGetSingletonRW<UIData>(out var uiData))
            {
                switch (actionType)
                {
                    case UIActionType.GetObscuredPrefs:
                        uiData.ValueRW.PrefsValue = value;
                        break;
                    case UIActionType.GetObscuredFile:
                        uiData.ValueRW.FileValue = value;
                        break;
                    case UIActionType.GetObscuredFilePrefs:
                        uiData.ValueRW.FilePrefsValue = value;
                        break;
                }
            }
        }

#if UNITY_ANDROID
        private void UpdateAndroidValues(ref UIData uiData)
        {
            // Update installation source if not already set
            if (uiData.InstallationSource.IsEmpty)
            {
                uiData.InstallationSource = "Not checked";
            }
        }
#endif
    }
}
#endif
