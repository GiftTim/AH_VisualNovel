using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class GalleryMenu : MonoBehaviour
{
    private const int PAGE_BUTTON_LIMIT = 3;
    private int maxPages = 0;
    private int selectedPage = 0;

    [SerializeField] private CanvasGroup root;
    [SerializeField] private Animator anim;

    [SerializeField] private Texture[] galleryImages;

    [SerializeField] private Button[] galleryPreviewButtons;
    [SerializeField] private Button panelSelectionPrefab; 
    

    [SerializeField] private Button LeftEndButton;
    [SerializeField] private Button previousButton;

    [SerializeField] private Button nextButton;
    [SerializeField] private Button RightEndButton;

    [SerializeField] private CanvasGroup previewPanel;
    private CanvasGroupController previewPanelCG;

    [SerializeField] Button previewButton;

    private List<Button> pageButtons = new List<Button>();
    private int windowStart = 1;
    private bool initialized = false;
    private int previewsPerPage => galleryPreviewButtons.Length;
    
    void Start()
    {
        previewPanelCG = new CanvasGroupController(this, previewPanel);

        GalleryConfig.Load();

        GetAllGalleryImages();
    }

    public void Open()
    {
        if(!initialized)
        {
            Initialize();
        }
        else
        {
            StartCoroutine(SelectPageButtonNextFrame(selectedPage));
        }

        anim.Play("Enter");
    }

    public void Close()
    {
        anim.Play("Exit");
    }

    private void GetAllGalleryImages()
    {
        galleryImages = Resources.LoadAll<Texture>(FilePaths.resources_gallery);
    }

    private void Initialize()
    {
        initialized = true;

        ConstructNavBar();

        LoadPage(1);
        StartCoroutine(SelectPageButtonNextFrame(1));
    }

    private void ConstructNavBar()
    {
        int totalImages = galleryImages.Length;

        maxPages = Mathf.CeilToInt((float)totalImages / previewsPerPage);
        int pageLimit = PAGE_BUTTON_LIMIT < maxPages ? PAGE_BUTTON_LIMIT : maxPages;

        for(int i = 1; i <= pageLimit; i++)
        {
            GameObject buttonOB = Instantiate(panelSelectionPrefab.gameObject, panelSelectionPrefab.transform.parent);
            buttonOB.SetActive(true);

            Button button = buttonOB.GetComponent<Button>();
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            buttonOB.name = i.ToString();
            pageButtons.Add(button);
        }

        ArrangeNavBarOrder();
        UpdateNavBarWindow();
    }

    private void UpdateNavBarWindow()
    {
        for (int i = 0; i < pageButtons.Count; i++)
        {
            int page = windowStart + i;
            pageButtons[i].onClick.RemoveAllListeners();
            pageButtons[i].onClick.AddListener(() => LoadPage(page));
            pageButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = page.ToString();
            pageButtons[i].gameObject.SetActive(page <= maxPages);
        }
        previousButton.interactable   = (windowStart > 1);
        nextButton.interactable       = (windowStart + pageButtons.Count <= maxPages);
        LeftEndButton.interactable    = (windowStart > 1);
        RightEndButton.interactable   = (windowStart + pageButtons.Count <= maxPages);
    }

    private void ArrangeNavBarOrder()
    {
        nextButton.transform.SetSiblingIndex(nextButton.transform.parent.childCount - 1);
        RightEndButton.transform.SetAsLastSibling();
    }

    private void LoadPage(int pageNumber)
    {
        int startingIndex = (pageNumber -1)*previewsPerPage;

        for(int i = 0; i < previewsPerPage; i++)
        {
            int index = i + startingIndex;
            Button button = galleryPreviewButtons[i];

            button.onClick.RemoveAllListeners();

            if(index >= galleryImages.Length)
            {
                button.transform.parent.gameObject.SetActive(false);
                continue;
            }
            else
            {
                button.transform.parent.gameObject.SetActive(true);
                RawImage renderer = button.targetGraphic as RawImage;
                Texture previewImage = galleryImages[index];

                if(GalleryConfig.ImageIsUnlocked(previewImage.name))
                {
                    renderer.color = Color.white;
                    renderer.texture = previewImage;
                    button.onClick.AddListener(() => ShowPreviewImage(previewImage));
                }
                else
                {

                    renderer.color = Color.black;
                    renderer.texture = null;
                }
            }
        }

        selectedPage = pageNumber;
    }

    private void ShowPreviewImage(Texture image)
    {
        RawImage renderer = previewButton.targetGraphic as RawImage;
        renderer.texture = image;
        previewPanelCG.Show();
        previewPanelCG.SetInteractableState(true);
    }

    public void HidePreviewImage()
    {
        previewPanelCG.Hide();
        previewPanelCG.SetInteractableState(false);
        SelectPageButton(selectedPage);
    }

    private void SelectPageButton(int pageNumber)
    {
        int index = pageNumber - windowStart;
        if (index >= 0 && index < pageButtons.Count)
            EventSystem.current.SetSelectedGameObject(pageButtons[index].gameObject);
    }

    public void ToNextPage()
    {
        windowStart += PAGE_BUTTON_LIMIT;
        UpdateNavBarWindow();
        LoadPage(windowStart);
        StartCoroutine(SelectPageButtonNextFrame(windowStart));
    }

    public void ToPreviousPage()
    {
        windowStart -= PAGE_BUTTON_LIMIT;
        if (windowStart < 1) windowStart = 1;
        UpdateNavBarWindow();
        LoadPage(windowStart);
        StartCoroutine(SelectPageButtonNextFrame(windowStart));
    }

    public void ToLeftEnd()
    {
        windowStart = 1;
        UpdateNavBarWindow();
        LoadPage(1);
        StartCoroutine(SelectPageButtonNextFrame(1));
    }

    public void ToRightEnd()
    {
        int lastWindowStart = ((maxPages - 1) / PAGE_BUTTON_LIMIT) * PAGE_BUTTON_LIMIT + 1;
        windowStart = lastWindowStart;
        UpdateNavBarWindow();
        LoadPage(maxPages);
        StartCoroutine(SelectPageButtonNextFrame(maxPages));
    }

    void Update()
    {
        if (!initialized) return;
        if (previewPanelCG != null && previewPanelCG.isVisible) return;

        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == null)
        {
            SelectPageButton(selectedPage);
        }
    }

    private IEnumerator SelectPageButtonNextFrame(int pageNumber)
    {
        yield return null;
        SelectPageButton(pageNumber);
    }
}
