using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using DIALOGUE;


public class ConfigMenu : MenuPage
{
    public static ConfigMenu instance { get; private set; }

    [SerializeField] private GameObject[] panels;
    private GameObject activePanel;
    
    public UI_ITEMS ui;

    private VN_Configuration config => VN_Configuration.activeConfig;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        for(int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == 0);
        }

        activePanel = panels[0];

        SetAvailableResolutions();

        LoadConfig();
    }

    private void LoadConfig()
    {
        if(File.Exists(VN_Configuration.filePath))
        {
            VN_Configuration.activeConfig = FileManager.Load<VN_Configuration>(VN_Configuration.filePath, encrypt: VN_Configuration.ENCRYPT);
        }
        else
        {
            VN_Configuration.activeConfig = new VN_Configuration();
        }

        VN_Configuration.activeConfig.Load();
    }

    private void OnApplicationQuit()
    {
        VN_Configuration.activeConfig.Save();
        VN_Configuration.activeConfig = null;
    }

    public void OpenPanel(string panelName)
    {
        GameObject panel = panels.First(p => p.name == panelName.ToLower());

        if(panel == null)
        {
            Debug.LogWarning($"Did not find panel called '{panelName}' in config menu.");
            return;
        }

        panel.SetActive(true);

        if(activePanel != null && activePanel != panel)
        {
            activePanel.SetActive(false);
        }

        panel.SetActive(true);
        activePanel = panel;
    }



    private void SetAvailableResolutions()
    {
        List<string> options = new List<string>()
        {
            "전체창 (1920 x 1080)",
            "창모드 (1920 x 1080)",
            "창모드 (1600 x 900)",
            "창모드 (1280 x 720)",
            "창모드 (1024 x 768)",
            "창모드 (800 x 600)",
            "창모드 (640 x 480)"
        };

        ui.resolutions.ClearOptions();
        ui.resolutions.AddOptions(options);
    }

    [System.Serializable]
    public class UI_ITEMS
    {
        private static Color button_selectedColor = new Color(1, 0.35f, 0, 1);
        private static Color button_unselectedColor = new Color(1f, 1f, 1f, 1);
        private static Color text_selectedColor = new Color(1, 1f, 0, 1);
        private static Color text_unselectedColor = new Color(0.25f, 0.25f, 0.25f, 1);
        public static Color musicOnColor = new Color(1, 0.118f, 0.118f, 1);
        public static Color musicOffColor = new Color(0.5f, 0.5f, 0.5f, 1);

        [Header("Setting")]
        public TMP_Dropdown resolutions;

        public Slider architectSpeed;
        public Slider autoReaderSpeed;

        public Slider musicVolume;
        public Image musicFill;
        public Slider sfxVolume;
        public Image sfxFill;
        public Slider voicesVolume;
        public Image voicesFill;

        public Image musicMute;
        public Image sfxMute;
        public Image voicesMute;

        public Sprite mutedSymbol;
        public Sprite unmutedSymbol;
    }

    //UI CALLABLE FUNCTIONS
    public void SetDisplayResolution()
    {
        int index = ui.resolutions.value;

        if (index == 0)
        {
            Screen.SetResolution(1920, 1080, FullScreenMode.ExclusiveFullScreen);
        }
        else
        {
            string rawText = ui.resolutions.captionText.text;
            string resolution = ExtractResolution(rawText);
            string[] values = resolution.Split('x');

            if(int.TryParse(values[0].Trim(), out int width) && int.TryParse(values[1].Trim(), out int height))
            {
                Screen.SetResolution(width, height, FullScreenMode.Windowed);
            }
            else
            {
                Debug.LogWarning($"Parsing error for screen resolution! ['{resolution}'] could not be parsed into WIDTH X HEIGHT from string ");
            }
        }

        config.display_resolution = ui.resolutions.captionText.text;
    }
    public static string ExtractResolution(string optionText)
    {
        int start = optionText.IndexOf('(');
        int end = optionText.IndexOf(')');
        if (start >= 0 && end > start)
            return optionText.Substring(start + 1, end - start - 1).Trim();
        return optionText;
    }

    public void SetTextArchitectSpeed()
    {
        config.dialogueTextSpeed = ui.architectSpeed.value;
        
        if(DialogueSystem.instance != null)
        {
            DialogueSystem.instance.conversationManager.architect.speed = config.dialogueTextSpeed;
        }
        
    }
    public void SetAutoReaderSpeed()
    {
        config.dialogueAutoReadSpeed = ui.autoReaderSpeed.value;

        if(DialogueSystem.instance == null)
        {
            return;
        }

        AutoReader autoReader = DialogueSystem.instance.autoReader;

        if(autoReader != null)
        {
            autoReader.speed = config.dialogueAutoReadSpeed;
        }
    }


    public void SetMusicVolume()
    {
        config.musicVolume = ui.musicVolume.value;
        AudioManager.instance.SetMusicVolume(config.musicVolume, config.muteMusic);

        ui.musicFill.color = config.muteMusic ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;

    }
    public void SetSFXVolume()
    {
        config.sfxVolume = ui.sfxVolume.value;
        AudioManager.instance.SetSFXVolume(config.sfxVolume, config.muteSFX);

        ui.sfxFill.color = config.muteSFX ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
    }
    public void SetVoiceVolume()
    {
        config.voicesVolume = ui.voicesVolume.value;
        AudioManager.instance.SetVoicesVolume(config.voicesVolume, config.muteVoices);

        ui.voicesFill.color = config.muteVoices ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
    }

    public void SetMusicMute()
    {
        config.muteMusic = !config.muteMusic;
        ui.musicVolume.fillRect.GetComponent<Image>().color = config.muteMusic ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
        ui.musicMute.sprite = config.muteMusic ? ui.mutedSymbol : ui.unmutedSymbol;
    
        AudioManager.instance.SetMusicVolume(config.musicVolume, config.muteMusic);
    }
    public void SetSFXMute()
    {
        config.muteSFX = !config.muteSFX;
        ui.sfxVolume.fillRect.GetComponent<Image>().color = config.muteSFX ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
        ui.sfxMute.sprite = config.muteSFX ? ui.mutedSymbol : ui.unmutedSymbol;

        AudioManager.instance.SetSFXVolume(config.sfxVolume, config.muteSFX);
    }
    public void SetVoiceMute()
    {
        config.muteVoices = !config.muteVoices;
        ui.voicesVolume.fillRect.GetComponent<Image>().color = config.muteVoices ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
        ui.voicesMute.sprite = config.muteVoices ? ui.mutedSymbol : ui.unmutedSymbol; 
    
        AudioManager.instance.SetVoicesVolume(config.voicesVolume, config.muteVoices);
    }
}

