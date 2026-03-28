using UnityEditor;
using UnityEngine;
using VISUALNOVEL;

public class SaveAndLoadMenu : MenuPage
{
    public static SaveAndLoadMenu Instance {get; private set;}

    private const int MAX_FILES = 7;
    private string savePath = FilePaths.gameSaves;

    private int currentPage = 1;
    private bool loadedFilesForFirstTime = false;


    public enum MenuFunction { save, load }
    public MenuFunction menuFunction = MenuFunction.save;

    public SaveLoadSlot[] saveSlots;
    private int slotsPerPage => saveSlots.Length;

    public Texture emptyFileImage;

    private void Awake()
    {
        Instance = this;
    }

    public override void Open()
    {
        base.Open();

        if(!loadedFilesForFirstTime)
        {
            PopulateSaveSlotsForPage(currentPage);
        }

    }

    private void PopulateSaveSlotsForPage(int pageNumber)
    {
        int startingFile = ((currentPage -1) * slotsPerPage) + 1;

        for (int i = 0; i < slotsPerPage; i++)
        {
            SaveLoadSlot slot = saveSlots[i];
            slot.root.SetActive(false); // 초기 비활성화 후 조건에 따라 활성화

            if(i == 0)
            {
                slot.root.SetActive(true);
                slot.isAutoSlot = true;

                slot.fileNumber = 0;
                slot.filePath = $"{FilePaths.gameSaves}autosave{VNGameSave.FILE_TYPE}";

                slot.PopulateDetails(MenuFunction.load); // 항상 load 모드
                continue;
            }

            // 일반 슬롯
            int fileNum = startingFile + (i - 1);

            if(fileNum < MAX_FILES)
            {
                slot.root.SetActive(true);
                slot.isAutoSlot = false;

                string filePath = $"{FilePaths.gameSaves}{fileNum}{VNGameSave.FILE_TYPE}";

                slot.fileNumber = fileNum;
                slot.filePath = filePath;

                slot.PopulateDetails(menuFunction);
            }
            else
            {
                slot.root.SetActive(false);
            }
        }
    }
}
