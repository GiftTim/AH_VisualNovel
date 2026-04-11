using System.Collections;
using UnityEngine;
using VISUALNOVEL;

public class MainMenu : MonoBehaviour
{
    public const string MAIN_MENU_SCENE = "Main Menu";
    public static MainMenu instance { get; private set; }
    
    public AudioClip menuMusic;
    public CanvasGroup mainPanel;
    private CanvasGroupController mainCG;

    private UIConfirmationMenu uiChoiceMenu => UIConfirmationMenu.instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        mainCG = new CanvasGroupController(this, mainPanel);

        AudioManager.instance.StopAllSoundEffects();
        AudioManager.instance.StopAllTracks();
        AudioManager.instance.PlayTrack(menuMusic, Channel: 0, startingVolume: 1f);
    }
    
    public void Click_StartNewGame()
    {
        uiChoiceMenu.Show("새로운 게임을 시작하겠습니까?", 
        new UIConfirmationMenu.ConfirmationButton("예", StartNewGame), 
        new UIConfirmationMenu.ConfirmationButton("아니오", null));
    }

    public void LoadGame(VNGameSave file)
    {
        VNGameSave.activeFile = file;
        StartCoroutine(StartingGame());
    }

    private void StartNewGame()
    {
        VNGameSave.activeFile = new VNGameSave();
        StartCoroutine(StartingGame());
    }

    private IEnumerator StartingGame()
    {
        mainCG.Hide(speed: 0.3f);
        AudioManager.instance.StopTrack(0);
        while (mainCG.isVisible)
        {
            yield return null;
        }

        VN_Configuration.activeConfig.Save();
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("VisualNovel");
    }

}
