using UnityEngine;

[System.Serializable]
public class VN_Configuration 
{
    public static VN_Configuration activeConfig;

    public static string filePath => $"{FilePaths.gameDatas}/vnconfig.cfg";

    public const bool ENCRYPT = false;


    //Display Settings
    public string display_resolution = "1920 x 1080";

    //Text Settings
    public float dialogueTextSpeed = 1f;
    public float dialogueAutoReadSpeed = 1f;

    //Audio Settings
    public float musicVolume = 1f;
    public bool muteMusic = false;
    public float sfxVolume = 1f;
    public bool muteSFX = false;
    public float voicesVolume = 1f;
    public bool muteVoices = false;

    //Other Setttings
    public float historyLogScale = 1f;

    public void Load()
    {
        var ui = ConfigMenu.instance.ui;

        // Set the screen resolution
        int res_index = 0;
        for(int i = 0; i < ui.resolutions.options.Count; i++)
        {
            string optionText = ui.resolutions.options[i].text;
            string optionResolution = ConfigMenu.ExtractResolution(optionText);

            if(optionResolution == display_resolution)
            {
                res_index = i;
                break;
            }
        }

        ui.resolutions.value = res_index;
        ConfigMenu.instance.SetDisplayResolution();

        // Set text speed
        ui.architectSpeed.value = dialogueTextSpeed;
        ui.autoReaderSpeed.value = dialogueAutoReadSpeed;

        // Set audio mixer volumes
        ui.musicVolume.value = musicVolume;
        ui.sfxVolume.value = sfxVolume;
        ui.voicesVolume.value = voicesVolume;

        ui.musicMute.sprite = muteMusic ? ui.mutedSymbol : ui.unmutedSymbol;
        ui.sfxMute.sprite = muteSFX ? ui.mutedSymbol : ui.unmutedSymbol;
        ui.voicesMute.sprite = muteVoices ? ui.mutedSymbol : ui.unmutedSymbol;
    }

    public void Save()
    {
        FileManager.Save(filePath, JsonUtility.ToJson(this), encrypt: ENCRYPT);
    }
}
