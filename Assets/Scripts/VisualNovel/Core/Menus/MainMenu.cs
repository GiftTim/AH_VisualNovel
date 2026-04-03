using UnityEngine;
using VISUALNOVEL;

public class MainMenu : MonoBehaviour
{
    public AudioClip menuMusic;

    void Start()
    {
        AudioManager.instance.PlayTrack(menuMusic, startingVolume: 1f);
    }
    
    public void StartNewGame()
    {
        VNGameSave.activeFile = new VNGameSave();
        UnityEngine.SceneManagement.SceneManager.LoadScene("VisualNovel");
    }

}
