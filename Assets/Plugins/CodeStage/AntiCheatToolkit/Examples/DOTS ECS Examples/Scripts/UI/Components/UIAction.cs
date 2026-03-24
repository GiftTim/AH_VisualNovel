#if UNITY_DOTS_ENABLED
using Unity.Collections;
using Unity.Entities;

namespace CodeStage.AntiCheat.Examples
{
    /// <summary>
    /// Commands for UI actions that need to be processed by systems
    /// </summary>
    public struct UIAction : IComponentData
    {
        public UIActionType ActionType;
        public int IntValue;
        public float FloatValue;
        public FixedString128Bytes StringValue;
    }

    public enum UIActionType : byte
    {
        None = 0,
        
        // Player actions
        ModifyScore,
        SetScore,
        ModifyHealth,
        SetHealth,
        SimulateCheat,
        ClearCheats,
        
        // Storage actions
        SetObscuredPrefs,
        GetObscuredPrefs,
        SetObscuredFile,
        GetObscuredFile,
        SetObscuredFilePrefs,
        GetObscuredFilePrefs,
        
        // Code hash actions
        GenerateCodeHash,
        
        // Android actions
        CheckInstallationSource,
        BlockScreenRecording,
        AllowScreenRecording
    }
}
#endif
