#if UNITY_DOTS_ENABLED
using Unity.Entities;
using CodeStage.AntiCheat.ObscuredTypes;
using Unity.Collections;

namespace CodeStage.AntiCheat.Examples
{
    /// <summary>
    /// UI-specific data that needs to be displayed and managed
    /// </summary>
    public struct UIData : IComponentData
    {
        // Player display values (decrypted for UI)
        public int DisplayScore;
        public float DisplayHealth;
        
        // Cheat state display
        public int CheatCount;
        public CheatCode LastCheatCode;
        
        // Storage values
        public int PrefsValue;
        public int FileValue;
        public int FilePrefsValue;
        
        // Code hash
        public FixedString128Bytes CodeHash;
        public bool IsGeneratingHash;
        
        // Detector states
        public bool SpeedHackDetected;
        public bool WallHackDetected;
        public bool TimeCheatingDetected;
        public bool ObscuredCheatingDetected;
#if !ENABLE_IL2CPP
        public bool InjectionDetected;
#endif
        
        // SpeedHackProofTime values
        public float ProofTime;
        public float ProofDeltaTime;
        public float ProofUnscaledTime;
        public float ProofRealtimeSinceStartup;
        
#if UNITY_ANDROID
        // Android-specific values
        public FixedString128Bytes InstallationSource;
        public bool IsScreenRecordingBlocked;
#endif
    }
    
    /// <summary>
    /// Singleton component for UI state management
    /// </summary>
    public struct UISingleton : IComponentData
    {
        public bool IsInitialized;
    }
}
#endif
