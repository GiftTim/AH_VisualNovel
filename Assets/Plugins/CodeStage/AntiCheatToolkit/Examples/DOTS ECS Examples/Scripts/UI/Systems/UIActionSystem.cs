#if UNITY_DOTS_ENABLED
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using CodeStage.AntiCheat.Storage;
using CodeStage.AntiCheat.Genuine.Android;
using CodeStage.AntiCheat.Genuine.CodeHash;
using CodeStage.AntiCheat.Utils;
using System.Text;

namespace CodeStage.AntiCheat.Examples
{
    /// <summary>
    /// System that processes UI actions and updates game state accordingly
    /// Runs on main thread to access Unity APIs safely
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class UIActionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (action, entity) in SystemAPI.Query<RefRO<UIAction>>().WithEntityAccess())
            {
                ProcessUIAction(action.ValueRO, ecb);
                ecb.DestroyEntity(entity);
            }
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void ProcessUIAction(UIAction action, EntityCommandBuffer ecb)
        {
            switch (action.ActionType)
            {
                case UIActionType.ModifyScore:
                    ModifyScore(action.IntValue);
                    break;
                case UIActionType.SetScore:
                    SetScore(action.IntValue);
                    break;
                case UIActionType.ModifyHealth:
                    ModifyHealth(action.FloatValue);
                    break;
                case UIActionType.SetHealth:
                    SetHealth(action.FloatValue);
                    break;
                case UIActionType.SimulateCheat:
                    SimulateCheatEvent(ecb);
                    break;
                case UIActionType.ClearCheats:
                    ClearCheatState();
                    break;
                case UIActionType.SetObscuredPrefs:
                    SetObscuredPrefs(action.IntValue);
                    break;
                case UIActionType.GetObscuredPrefs:
                    GetObscuredPrefs();
                    break;
                case UIActionType.SetObscuredFile:
                    SetObscuredFile(action.IntValue);
                    break;
                case UIActionType.GetObscuredFile:
                    GetObscuredFile();
                    break;
                case UIActionType.SetObscuredFilePrefs:
                    SetObscuredFilePrefs(action.IntValue);
                    break;
                case UIActionType.GetObscuredFilePrefs:
                    GetObscuredFilePrefs();
                    break;
                case UIActionType.GenerateCodeHash:
                    GenerateCodeHash();
                    break;
#if UNITY_ANDROID
                case UIActionType.CheckInstallationSource:
                    CheckInstallationSource();
                    break;
                case UIActionType.BlockScreenRecording:
                    BlockScreenRecording();
                    break;
                case UIActionType.AllowScreenRecording:
                    AllowScreenRecording();
                    break;
#endif
            }
        }

        #region Player Actions

        private void ModifyScore(int delta)
        {
            if (SystemAPI.TryGetSingletonRW<PlayerData>(out var playerData))
            {
                int currentScore = playerData.ValueRO.Score; // Decrypt
                playerData.ValueRW.Score = currentScore + delta; // Encrypt
                Debug.Log($"[ACTk] Modified score by {delta}, new score: {currentScore + delta}");
            }
        }

        private void SetScore(int value)
        {
            if (SystemAPI.TryGetSingletonRW<PlayerData>(out var playerData))
            {
                playerData.ValueRW.Score = value; // Encrypt
                Debug.Log($"[ACTk] Set score to: {value}");
            }
        }

        private void ModifyHealth(float delta)
        {
            if (SystemAPI.TryGetSingletonRW<PlayerData>(out var playerData))
            {
                float currentHealth = playerData.ValueRO.Health; // Decrypt
                playerData.ValueRW.Health = currentHealth + delta; // Encrypt
                Debug.Log($"[ACTk] Modified health by {delta}, new health: {currentHealth + delta}");
            }
        }

        private void SetHealth(float value)
        {
            if (SystemAPI.TryGetSingletonRW<PlayerData>(out var playerData))
            {
                playerData.ValueRW.Health = value; // Encrypt
                Debug.Log($"[ACTk] Set health to: {value}");
            }
        }

        private void SimulateCheatEvent(EntityCommandBuffer ecb)
        {
            var cheatEntity = ecb.CreateEntity();
            ecb.AddComponent(cheatEntity, new CheatDetected { Code = CheatCode.SpeedHack });
            Debug.Log("[ACTk] Simulated cheat event posted.");
        }

        private void ClearCheatState()
        {
            if (SystemAPI.TryGetSingletonRW<CheatState>(out var cheatState))
            {
                cheatState.ValueRW.LastCode = 0;
                cheatState.ValueRW.TotalCount = 0;
                Debug.Log("[ACTk] Cleared cheat state.");
            }
        }

        #endregion

        #region Storage Actions

        private void SetObscuredPrefs(int value)
        {
            ObscuredPrefs.SetInt("DOTS_Example_Value", value);
            Debug.Log($"[ACTk] Set ObscuredPrefs value: {value}");
        }

        private void GetObscuredPrefs()
        {
            int value = ObscuredPrefs.GetInt("DOTS_Example_Value", 0);
            UpdateUIStorageValue(UIActionType.GetObscuredPrefs, value);
            Debug.Log($"[ACTk] Got ObscuredPrefs value: {value}");
        }

        private void SetObscuredFile(int value)
        {
            try
            {
                var settings = new ObscuredFileSettings(ObscuredFileLocation.Custom);
                var safeFile = new ObscuredFile(System.IO.Path.Combine(Application.temporaryCachePath, "dots_example.txt"), settings);
                var json = value.ToString();
                var result = safeFile.WriteAllBytes(Encoding.UTF8.GetBytes(json));
                
                if (result.Error.ErrorCode == ObscuredFileErrorCode.NoError)
                {
                    Debug.Log($"[ACTk] Set ObscuredFile value: {value}");
                }
                else
                {
                    Debug.LogError($"[ACTk] ObscuredFile write error: {result.Error.ErrorCode}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ACTk] ObscuredFile write exception: {e.Message}");
            }
        }

        private void GetObscuredFile()
        {
            try
            {
                var settings = new ObscuredFileSettings(ObscuredFileLocation.Custom);
                var safeFile = new ObscuredFile(System.IO.Path.Combine(Application.temporaryCachePath, "dots_example.txt"), settings);
                var result = safeFile.ReadAllBytes();
                
                if (result.Error.ErrorCode == ObscuredFileErrorCode.NoError && result.Data != null)
                {
                    var content = Encoding.UTF8.GetString(result.Data);
                    if (int.TryParse(content, out int value))
                    {
                        UpdateUIStorageValue(UIActionType.GetObscuredFile, value);
                        Debug.Log($"[ACTk] Got ObscuredFile value: {value}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[ACTk] ObscuredFile read error: {result.Error.ErrorCode}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ACTk] ObscuredFile read error: {e.Message}");
            }
        }

        private void SetObscuredFilePrefs(int value)
        {
            ObscuredFilePrefs.Set("DOTS_Example_FilePrefs", value);
            Debug.Log($"[ACTk] Set ObscuredFilePrefs value: {value}");
        }

        private void GetObscuredFilePrefs()
        {
            int value = ObscuredFilePrefs.Get("DOTS_Example_FilePrefs", 0);
            UpdateUIStorageValue(UIActionType.GetObscuredFilePrefs, value);
            Debug.Log($"[ACTk] Got ObscuredFilePrefs value: {value}");
        }

        private void UpdateUIStorageValue(UIActionType actionType, int value)
        {
            UIDataSystem.UpdateStorageValue(EntityManager, actionType, value);
        }

        #endregion

        #region Code Hash Actions

        private async void GenerateCodeHash()
        {
            if (!CodeHashGenerator.IsTargetPlatformCompatible())
            {
                Debug.LogWarning("[ACTk] Code hash generation not compatible with current platform");
                return;
            }

            if (SystemAPI.TryGetSingletonRW<UIData>(out var uiData))
            {
                uiData.ValueRW.IsGeneratingHash = true;
                uiData.ValueRW.CodeHash = "Generating...";
            }

            try
            {
                var result = await CodeHashGenerator.GenerateAsync();
                if (SystemAPI.TryGetSingletonRW<UIData>(out var uiDataAfter))
                {
                    if (result.Success)
                    {
                        uiDataAfter.ValueRW.CodeHash = result.SummaryHash;
                        Debug.Log($"[ACTk] Code hash generated: {result.SummaryHash}");
                    }
                    else
                    {
                        uiDataAfter.ValueRW.CodeHash = $"Error: {result.ErrorMessage}";
                        Debug.LogError($"[ACTk] Code hash generation failed: {result.ErrorMessage}");
                    }
                    uiDataAfter.ValueRW.IsGeneratingHash = false;
                }
            }
            catch (System.Exception e)
            {
                if (SystemAPI.TryGetSingletonRW<UIData>(out var uiDataError))
                {
                    uiDataError.ValueRW.CodeHash = $"Exception: {e.Message}";
                    uiDataError.ValueRW.IsGeneratingHash = false;
                }
                Debug.LogError($"[ACTk] Code hash generation exception: {e}");
            }
        }

        #endregion

#if UNITY_ANDROID
        #region Android Actions

        private void CheckInstallationSource()
        {
            try
            {
                var source = AppInstallationSourceValidator.GetAppInstallationSource();
                if (SystemAPI.TryGetSingletonRW<UIData>(out var uiData))
                {
                    if (source != null)
                    {
                        uiData.ValueRW.InstallationSource = $"{source.DetectedSource}";
                        Debug.Log($"[ACTk] App installation source: {source.DetectedSource}");
                    }
                    else
                    {
                        uiData.ValueRW.InstallationSource = "Unknown/Error";
                        Debug.LogWarning("[ACTk] Could not determine app installation source");
                    }
                }
            }
            catch (System.Exception e)
            {
                if (SystemAPI.TryGetSingletonRW<UIData>(out var uiData))
                {
                    uiData.ValueRW.InstallationSource = $"Error: {e.Message}";
                }
                Debug.LogError($"[ACTk] App installation source check failed: {e}");
            }
        }

        private void BlockScreenRecording()
        {
            try
            {
                AndroidScreenRecordingBlocker.PreventScreenRecording();
                if (SystemAPI.TryGetSingletonRW<UIData>(out var uiData))
                {
                    uiData.ValueRW.IsScreenRecordingBlocked = true;
                }
                Debug.Log("[ACTk] Screen recording blocked");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ACTk] Failed to block screen recording: {e}");
            }
        }

        private void AllowScreenRecording()
        {
            try
            {
                AndroidScreenRecordingBlocker.AllowScreenRecording();
                if (SystemAPI.TryGetSingletonRW<UIData>(out var uiData))
                {
                    uiData.ValueRW.IsScreenRecordingBlocked = false;
                }
                Debug.Log("[ACTk] Screen recording allowed");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ACTk] Failed to allow screen recording: {e}");
            }
        }

        #endregion
#endif
    }
}
#endif
