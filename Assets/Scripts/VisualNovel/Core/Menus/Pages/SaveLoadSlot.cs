using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using VISUALNOVEL;

public class SaveLoadSlot : MonoBehaviour
{
    public GameObject root;
    public Button saveButton;
    public Button loadButton;
    public RawImage previewImage;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Button deleteButton;
    public bool isAutoSlot = false;


    [HideInInspector] public int fileNumber = 0;
    [HideInInspector] public string filePath = "";

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
        // bool fileExists = File.Exists(filePath);
        // Debug.Log($"File Path: {filePath}, Exists: {fileExists}"); // 디버깅 로그 추가

        if (file == null)
        {
            titleText.text = "Empty File";
            dateText.text = "";
            descriptionText.text = "";
            previewImage.texture = SaveAndLoadMenu.Instance.emptyFileImage;

            if (isAutoSlot)
            {
                loadButton.gameObject.SetActive(true); // 26.03.28 AutoSlot은 처음부터 초기파일(시작파일)을 넣는 걸로 초기화 시킬 것이기 때문에 항상 Load 활성화
                saveButton.gameObject.SetActive(false);
                deleteButton.gameObject.SetActive(false);
            }
            else
            {
                loadButton.gameObject.SetActive(false); // 일반 슬롯 Load 버튼 강제 비활성화
                saveButton.gameObject.SetActive(true); // 가장 기본 상태은 Save할 수 있는 상태
                deleteButton.gameObject.SetActive(false);
            }
        }
        else
        {
            titleText.text = $"{fileNumber}.";
            dateText.text = $"{file.timeStamp}";
            
        }
    
    }

    public void Delete()
    {
        
    }

    public void Save()
    {
        var activeSave = VNGameSave.activeFile;
        activeSave.slotNumber = fileNumber;

        activeSave.Save();

        PopulateDetailsFromFile(SaveAndLoadMenu.Instance.menuFunction, activeSave);
    }

    public void Load()
    {
        VNGameSave file = VNGameSave.Load(filePath, true);

        SaveAndLoadMenu.Instance.Close(closeAllMenus: true);
    }
}
