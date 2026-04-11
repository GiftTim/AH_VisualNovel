using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using VISUALNOVEL;
using History;

public class SaveLoadSlot : MonoBehaviour
{
    public GameObject root;
    public Button saveButton;
    public Button loadButton;
    public RawImage previewImage;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI titleText;
    public Button deleteButton;
    public bool isAutoSlot = false;


    [HideInInspector] public int fileNumber = 0;
    [HideInInspector] public string filePath = "";

    private UIConfirmationMenu uichoiceMenu => UIConfirmationMenu.instance;

    public void PopulateDetails(SaveAndLoadMenu.MenuFunction function)
    {
        if(File.Exists(filePath))
        {
            VNGameSave file = VNGameSave.Load(filePath);
            PopulateDetailsFromFile(function, file);
        }
        else
        {
            PopulateDetailsFromFile(function, null);
        }
    }

    // SaveLoadSlot.cs의 PopulateDetailsFromFile 메서드 수정
    private void PopulateDetailsFromFile(SaveAndLoadMenu.MenuFunction function, VNGameSave file)
    {

        if (file == null)
        {
            titleText.text = "";
            dateText.text = "";
            previewImage.texture = SaveAndLoadMenu.Instance.emptyFileImage;

            if (isAutoSlot)
            {
                loadButton.gameObject.SetActive(true); // 26.03.28 AutoSlot은 처음부터 초기파일(시작파일)을 넣는 걸로 초기화 시킬 것이기 때문에 항상 Load 활성화
                saveButton.gameObject.SetActive(false); // 아예 등장시키질 않을 거라서 그냥 비활성화
                deleteButton.gameObject.SetActive(false); // 아예 등장시키질 않을 거라서 그냥 비활성화
            }
            else
            {
                loadButton.gameObject.SetActive(false); // 일반 슬롯 Load 버튼 강제 비활성화
                saveButton.gameObject.SetActive(function == SaveAndLoadMenu.MenuFunction.save); // 가장 기본 상태은 Save할 수 있는 상태
                var cEmpty = saveButton.image.color;
                saveButton.image.color = new Color(cEmpty.r, cEmpty.g, cEmpty.b, 1f); // 빈 슬롯이므로 Save 버튼 불투명 복원
                deleteButton.gameObject.SetActive(false); // Load 모드일 때만 삭제 가능하도록
            }
        }
        else
        {
            string folderLabel = GetFolderLabelFromSave(file);
            titleText.text = string.IsNullOrEmpty(folderLabel) ? string.Empty : $"{folderLabel}";
            dateText.text = $"{file.timeStamp}";

            loadButton.gameObject.SetActive(true);  // 파일 있으면 모드 관계없이 항상 Load 가능
            saveButton.gameObject.SetActive(function == SaveAndLoadMenu.MenuFunction.save);
            if (function == SaveAndLoadMenu.MenuFunction.save)
            {
                var c = saveButton.image.color;
                saveButton.image.color = new Color(c.r, c.g, c.b, 0f); // 저장된 슬롯: Save 버튼 투명화 → 뒤 LoadButton이 보이도록
            }
            deleteButton.gameObject.SetActive(true); // Load 모드일 때만 삭제 가능하도록
            
            byte[] data = File.ReadAllBytes(file.screenshotPath);
            Texture2D screenshotPreview = new Texture2D(2, 2);
            ImageConversion.LoadImage(screenshotPreview, data);
            previewImage.texture = screenshotPreview;            
        }

    }

    private string GetFolderLabelFromSave(VNGameSave file)
    {
        if (file.activeConversations == null || file.activeConversations.Length == 0)
            return string.Empty;

        try
        {
            var compressed = JsonUtility.FromJson<VISUALNOVEL.VN_ConversationDataCompressed>(file.activeConversations[0]);
            if (compressed == null || string.IsNullOrEmpty(compressed.fileName))
                return string.Empty;

            // "Dialogue Files/Chapter1/Coffee_Bean" → "Chapter1"
            string prefix = FilePaths.resources_dialogueFiles;
            string withoutPrefix = compressed.fileName.StartsWith(prefix)
                ? compressed.fileName.Substring(prefix.Length)
                : compressed.fileName;

            int slashIndex = withoutPrefix.IndexOf('/');
            return slashIndex > 0 ? withoutPrefix.Substring(0, slashIndex) : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public void Delete()
    {
        File.Delete(filePath);
        PopulateDetails(SaveAndLoadMenu.Instance.menuFunction);
    }

    public void Save()
    {
        if (HistoryManager.instance.isViewingHistory)
        {
            UIConfirmationMenu.instance.Show(
                "You cannot save while viewing history.", 
                new UIConfirmationMenu.ConfirmationButton("확인",null));
            return;
        }

        var activeSave = VNGameSave.activeFile;
        activeSave.slotNumber = fileNumber;

        activeSave.Save();

        PopulateDetailsFromFile(SaveAndLoadMenu.Instance.menuFunction, activeSave);
    }

    public void Load()
    {
        VNGameSave file = VNGameSave.Load(filePath, false);
        SaveAndLoadMenu.Instance.Close(closeAllMenus: true);

        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == MainMenu.MAIN_MENU_SCENE)
        {
            MainMenu.instance.LoadGame(file);
        }
        else
        {
            file.Activate();
        }


    }
}
