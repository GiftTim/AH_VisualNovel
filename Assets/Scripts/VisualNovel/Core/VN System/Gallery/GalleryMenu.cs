using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/*
 * ============================================================
 * GalleryMenu (갤러리 UI 전체 관리)
 * ============================================================
 * 갤러리(CG 열람) 화면의 UI를 담당하는 클래스.
 *
 * [전체 구조]
 * - 갤러리 이미지는 여러 페이지로 나뉘어 표시된다.
 * - 각 페이지에는 최대 previewsPerPage(= galleryPreviewButtons.Length)개의
 *   이미지 버튼이 표시된다.
 * - 하단 내비게이션 바에는 최대 PAGE_BUTTON_LIMIT(3)개의 페이지 버튼이 표시되며,
 *   "슬라이딩 윈도우" 방식으로 이전/다음 버튼으로 이동한다.
 *
 * [잠금 시스템]
 * - GalleryConfig.unlockedImages에 이름이 있는 이미지만 컬러로 표시.
 * - 없는 이미지는 검은 박스(잠금 상태)로만 표시된다.
 *
 * [미리보기 패널]
 * - 이미지 버튼을 클릭하면 전체 화면 미리보기 패널이 열린다.
 * - previewPanel(CanvasGroup)의 Show/Hide로 제어한다.
 *
 * [포커스 관리]
 * - Update()에서 현재 선택된 UI 오브젝트가 없으면
 *   selectedPage의 페이지 버튼으로 포커스를 복구한다.
 *   (컨트롤러/키보드 입력 지원)
 * ============================================================
 */
public class GalleryMenu : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────
    // PAGE_BUTTON_LIMIT: 내비게이션 바에 한 번에 보이는 페이지 버튼 최대 개수.
    //   예) 페이지가 10개라도 버튼은 최대 3개만 표시되며,
    //       ← → 버튼으로 윈도우를 이동한다.
    // maxPages   : 이미지 총 개수와 페이지당 버튼 수로 계산되는 총 페이지 수.
    // selectedPage: 현재 선택(표시)된 페이지 번호. 1부터 시작.
    // ─────────────────────────────────────────────────────────
    private const int PAGE_BUTTON_LIMIT = 3;
    private int maxPages = 0;
    private int selectedPage = 0;

    // ─────────────────────────────────────────────────────────
    // root     : 갤러리 메뉴 전체를 감싸는 CanvasGroup.
    //             Show/Hide 시 투명도 제어에 사용.
    // anim     : 갤러리 열기/닫기 애니메이션 재생용 Animator.
    //             "Enter" / "Exit" 트리거로 애니메이션 재생.
    // ─────────────────────────────────────────────────────────
    [SerializeField] private CanvasGroup root;
    [SerializeField] private Animator anim;

    // ─────────────────────────────────────────────────────────
    // galleryImages: Resources/Gallery 폴더에서 로드한 모든 Texture 배열.
    //   GalleryConfig.ImageIsUnlocked(이름)으로 잠금 여부를 확인한다.
    // ─────────────────────────────────────────────────────────
    [SerializeField] private Texture[] galleryImages;

    // ─────────────────────────────────────────────────────────
    // galleryPreviewButtons: 페이지에 표시되는 이미지 슬롯 버튼 배열.
    //   각 버튼의 targetGraphic(RawImage)에 이미지를 표시한다.
    //   배열 길이 = 한 페이지에 표시되는 이미지 수 (previewsPerPage).
    // panelSelectionPrefab: 내비게이션 바에 동적으로 생성할 페이지 버튼 프리팹.
    //   초기에는 비활성화 상태여야 하며, 생성 후 활성화된다.
    // ─────────────────────────────────────────────────────────
    [SerializeField] private Button[] galleryPreviewButtons;
    [SerializeField] private Button panelSelectionPrefab;

    // ─────────────────────────────────────────────────────────
    // 내비게이션 바 끝/이전/다음/끝 버튼.
    // LeftEndButton : 가장 첫 페이지 윈도우로 이동.
    // previousButton: 이전 페이지 윈도우로 이동 (PAGE_BUTTON_LIMIT 단위).
    // nextButton    : 다음 페이지 윈도우로 이동 (PAGE_BUTTON_LIMIT 단위).
    // RightEndButton: 가장 마지막 페이지 윈도우로 이동.
    // ─────────────────────────────────────────────────────────
    [SerializeField] private Button LeftEndButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button RightEndButton;

    // ─────────────────────────────────────────────────────────
    // previewPanel  : 이미지를 전체 화면으로 미리볼 때 표시되는 CanvasGroup.
    // previewPanelCG: previewPanel을 CanvasGroupController로 래핑한 것.
    //                 Show/Hide/Interactable 등의 메서드를 사용하기 위함.
    // previewButton : 미리보기 패널 내에서 이미지를 실제로 표시하는 버튼.
    //                 targetGraphic(RawImage)에 선택한 이미지를 표시한다.
    // ─────────────────────────────────────────────────────────
    [SerializeField] private CanvasGroup previewPanel;
    private CanvasGroupController previewPanelCG;
    [SerializeField] Button previewButton;

    // ─────────────────────────────────────────────────────────
    // pageButtons : 내비게이션 바에 동적으로 생성된 페이지 버튼 목록.
    //               panelSelectionPrefab을 Instantiate하여 추가된다.
    // windowStart : 슬라이딩 윈도우의 시작 페이지 번호.
    //               예) windowStart=1 → 1,2,3번 버튼 표시.
    //                   windowStart=4 → 4,5,6번 버튼 표시.
    // initialized : 최초 1회만 Initialize()를 호출하도록 막는 플래그.
    //               Open() 호출 시 초기화 여부를 확인하는 데 사용.
    // previewsPerPage: 한 페이지에 표시되는 이미지 수.
    //                  galleryPreviewButtons 배열 길이로 자동 계산.
    // ─────────────────────────────────────────────────────────
    private List<Button> pageButtons = new List<Button>();
    private int windowStart = 1;
    private bool initialized = false;
    private int previewsPerPage => galleryPreviewButtons.Length;

    /*
     * Start()
     * ────────────────────────────────────────────────────────
     * 씬 시작 시 한 번 호출. 기본 컴포넌트 설정과 이미지 목록 로드.
     *
     * 처리 순서:
     *  1) previewPanelCG 생성: previewPanel을 CanvasGroupController로 래핑.
     *  2) GalleryConfig.Load(): 저장된 잠금 해제 목록을 메모리에 로드.
     *  3) GetAllGalleryImages(): Resources에서 갤러리 이미지 목록 로드.
     *
     * 주의: Initialize()는 Open() 첫 호출 시에 실행된다.
     *       버튼/페이지 생성은 갤러리가 실제로 열릴 때까지 지연된다.
     * ────────────────────────────────────────────────────────
     */
    void Start()
    {
        // previewPanel을 CanvasGroupController로 래핑하여 편리하게 제어.
        previewPanelCG = new CanvasGroupController(this, previewPanel);

        // 저장 파일에서 잠금 해제된 이미지 목록 로드.
        GalleryConfig.Load();

        // Resources 폴더에서 갤러리 이미지 전체 목록 로드.
        GetAllGalleryImages();
    }

    /*
     * Open()
     * ────────────────────────────────────────────────────────
     * 갤러리 메뉴를 열고 열기 애니메이션을 재생한다.
     *
     * - 처음 열릴 때: Initialize()를 호출하여 버튼/페이지 구성.
     * - 이후 열릴 때: 이전에 선택된 페이지(selectedPage)의 버튼에
     *                포커스를 복원한다. (한 프레임 지연 필요)
     * ────────────────────────────────────────────────────────
     */
    public void Open()
    {
        if (!initialized)
        {
            // 최초 1회: 내비게이션 바 생성 및 첫 페이지 로드.
            Initialize();
        }
        else
        {
            // 두 번째 이후 열릴 때: 이전에 있던 페이지 버튼에 포커스 복원.
            // SetSelectedGameObject는 한 프레임 후에 적용해야 정상 작동하므로
            // 코루틴으로 1프레임 지연한다.
            StartCoroutine(SelectPageButtonNextFrame(selectedPage));
        }

        // "Enter" 트리거: 갤러리 등장 애니메이션 재생.
        anim.Play("Enter");
    }

    /*
     * Close()
     * ────────────────────────────────────────────────────────
     * 갤러리 메뉴를 닫고 닫기 애니메이션을 재생한다.
     * 실제 비활성화는 애니메이션 이벤트나 VNMenuManager에서 처리한다.
     * ────────────────────────────────────────────────────────
     */
    public void Close()
    {
        // "Exit" 트리거: 갤러리 사라짐 애니메이션 재생.
        anim.Play("Exit");
    }

    /*
     * GetAllGalleryImages()  [private]
     * ────────────────────────────────────────────────────────
     * Resources 폴더에서 갤러리에 사용할 Texture 에셋을 전부 로드한다.
     * FilePaths.resources_gallery 경로에 있는 모든 Texture가 대상이다.
     * 로드된 배열이 galleryImages에 저장되며, 이 수가 총 이미지 개수가 된다.
     * ────────────────────────────────────────────────────────
     */
    private void GetAllGalleryImages()
    {
        // Resources.LoadAll: 지정 경로의 모든 Texture 에셋을 배열로 반환.
        galleryImages = Resources.LoadAll<Texture>(FilePaths.resources_gallery);
    }

    /*
     * Initialize()  [private]
     * ────────────────────────────────────────────────────────
     * 갤러리가 처음 열릴 때 한 번만 실행되는 초기화 메서드.
     *
     * 처리 순서:
     *  1) initialized = true → 이후 Open()에서 다시 호출되지 않도록.
     *  2) ConstructNavBar(): 내비게이션 바 버튼 생성 및 배치.
     *  3) LoadPage(1): 1페이지 이미지 표시.
     *  4) SelectPageButtonNextFrame(1): 1번 페이지 버튼에 포커스 설정.
     * ────────────────────────────────────────────────────────
     */
    private void Initialize()
    {
        initialized = true;

        ConstructNavBar();

        LoadPage(1);
        StartCoroutine(SelectPageButtonNextFrame(1));
    }

    /*
     * ConstructNavBar()  [private]
     * ────────────────────────────────────────────────────────
     * 내비게이션 바에 페이지 버튼들을 동적으로 생성한다.
     *
     * 처리 순서:
     *  1) 총 페이지 수(maxPages) 계산: 총 이미지 / 페이지당 표시 수 (올림).
     *  2) 실제로 생성할 버튼 수 = min(PAGE_BUTTON_LIMIT, maxPages).
     *  3) panelSelectionPrefab을 Instantiate하여 부모에 버튼을 생성, 활성화.
     *  4) ArrangeNavBarOrder(): 이전/다음 버튼의 순서를 마지막으로 보정.
     *  5) UpdateNavBarWindow(): 생성된 버튼들에 페이지 번호와 이벤트 할당.
     *
     * [주의]
     * 버튼을 Instantiate하면 Hierarchy 순서가 변할 수 있다.
     * ArrangeNavBarOrder()에서 nextButton/RightEndButton을 마지막으로 이동.
     * ────────────────────────────────────────────────────────
     */
    private void ConstructNavBar()
    {
        int totalImages = galleryImages.Length;

        // 총 페이지 수: 이미지 총 개수를 페이지당 개수로 나눈 값을 올림.
        maxPages = Mathf.CeilToInt((float)totalImages / previewsPerPage);

        // 실제 생성할 버튼 수: PAGE_BUTTON_LIMIT와 maxPages 중 작은 값.
        int pageLimit = PAGE_BUTTON_LIMIT < maxPages ? PAGE_BUTTON_LIMIT : maxPages;

        for (int i = 1; i <= pageLimit; i++)
        {
            // 프리팹을 같은 부모(panelSelectionPrefab의 부모)에 복제하여 생성.
            GameObject buttonOB = Instantiate(panelSelectionPrefab.gameObject, panelSelectionPrefab.transform.parent);
            buttonOB.SetActive(true); // 프리팹은 비활성화 상태이므로 활성화.

            Button button = buttonOB.GetComponent<Button>();
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            buttonOB.name = i.ToString(); // 게임오브젝트 이름을 페이지 번호로 설정.
            pageButtons.Add(button);
        }

        // 버튼 생성으로 인해 Hierarchy 순서가 바뀔 수 있으므로 순서 보정.
        ArrangeNavBarOrder();

        // 생성된 버튼들에 페이지 번호, onClick 이벤트, 활성화 여부 설정.
        UpdateNavBarWindow();
    }

    /*
     * UpdateNavBarWindow()  [private]
     * ────────────────────────────────────────────────────────
     * 슬라이딩 윈도우의 현재 상태(windowStart)에 맞게
     * 내비게이션 바 버튼들의 표시 정보를 갱신한다.
     *
     * - 각 버튼에 windowStart 기준의 페이지 번호를 텍스트로 표시.
     * - 해당 페이지로 이동하는 onClick 리스너를 재등록.
     * - maxPages를 초과하는 버튼은 비활성화.
     * - 이전/다음 버튼의 interactable 여부를 경계 조건으로 설정.
     * ────────────────────────────────────────────────────────
     */
    private void UpdateNavBarWindow()
    {
        for (int i = 0; i < pageButtons.Count; i++)
        {
            int page = windowStart + i; // 이 버튼이 나타낼 페이지 번호.

            // 기존 이벤트 리스너를 제거 후 새로 등록 (클로저 캡처 문제 방지).
            pageButtons[i].onClick.RemoveAllListeners();
            pageButtons[i].onClick.AddListener(() => LoadPage(page));

            // 버튼 텍스트를 페이지 번호로 업데이트.
            pageButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = page.ToString();

            // maxPages를 넘어서는 버튼은 숨긴다.
            pageButtons[i].gameObject.SetActive(page <= maxPages);
        }

        // 이전/끝 버튼: windowStart가 1보다 크면 활성화.
        previousButton.interactable = (windowStart > 1);
        LeftEndButton.interactable  = (windowStart > 1);

        // 다음/끝 버튼: 윈도우가 마지막 페이지를 넘어서면 비활성화.
        nextButton.interactable     = (windowStart + pageButtons.Count <= maxPages);
        RightEndButton.interactable = (windowStart + pageButtons.Count <= maxPages);
    }

    /*
     * ArrangeNavBarOrder()  [private]
     * ────────────────────────────────────────────────────────
     * 내비게이션 바에서 nextButton과 RightEndButton을
     * 가장 마지막 자식으로 이동하여 순서를 정렬한다.
     *
     * [왜 필요한가?]
     * ConstructNavBar()에서 Instantiate로 버튼을 생성하면
     * 부모의 자식 순서에 새 오브젝트가 추가된다.
     * 그러면 nextButton과 RightEndButton이 새 버튼보다 앞에 위치할 수 있다.
     * 내비게이션 바 레이아웃은 [←끝][←이전][페이지1][페이지2][페이지3][다음→][끝→] 이므로
     * nextButton과 RightEndButton은 항상 마지막에 있어야 한다.
     * ────────────────────────────────────────────────────────
     */
    private void ArrangeNavBarOrder()
    {
        // nextButton을 부모의 마지막 자식으로 이동.
        nextButton.transform.SetSiblingIndex(nextButton.transform.parent.childCount - 1);
        // RightEndButton을 가장 마지막으로 이동.
        RightEndButton.transform.SetAsLastSibling();
    }

    /*
     * LoadPage(pageNumber)  [private]
     * ────────────────────────────────────────────────────────
     * 지정한 페이지 번호의 이미지들을 galleryPreviewButtons에 표시한다.
     *
     * [잠금 처리]
     * - GalleryConfig.ImageIsUnlocked(이름) == true:
     *     이미지를 컬러로 표시하고, 클릭 시 미리보기 패널을 열도록 설정.
     * - GalleryConfig.ImageIsUnlocked(이름) == false:
     *     이미지를 검은색으로 표시하고, 클릭 이벤트 없음 (잠금 상태).
     *
     * [이미지 수가 부족할 때]
     * - 해당 슬롯 버튼의 부모 오브젝트를 비활성화하여 빈 슬롯을 숨긴다.
     *
     * [매개변수]
     *  pageNumber: 표시할 페이지 번호 (1-based).
     * ────────────────────────────────────────────────────────
     */
    private void LoadPage(int pageNumber)
    {
        // 이 페이지의 첫 번째 이미지 인덱스 계산 (0-based).
        int startingIndex = (pageNumber - 1) * previewsPerPage;

        for (int i = 0; i < previewsPerPage; i++)
        {
            int index = i + startingIndex; // 실제 galleryImages 인덱스.
            Button button = galleryPreviewButtons[i];

            // 기존 클릭 이벤트 초기화.
            button.onClick.RemoveAllListeners();

            if (index >= galleryImages.Length)
            {
                // 이미지가 더 이상 없으면 슬롯을 숨긴다.
                button.transform.parent.gameObject.SetActive(false);
                continue;
            }
            else
            {
                button.transform.parent.gameObject.SetActive(true);

                // 버튼의 RawImage 컴포넌트에 이미지를 표시한다.
                RawImage renderer = button.targetGraphic as RawImage;
                Texture previewImage = galleryImages[index];

                if (GalleryConfig.ImageIsUnlocked(previewImage.name))
                {
                    // ── 잠금 해제된 이미지: 컬러로 표시 + 클릭 시 미리보기 ──
                    renderer.color   = Color.white;
                    renderer.texture = previewImage;
                    button.onClick.AddListener(() => ShowPreviewImage(previewImage));
                }
                else
                {
                    // ── 잠긴 이미지: 검은 박스로 표시, 클릭 불가 ──
                    renderer.color   = Color.black;
                    renderer.texture = null; // 이미지를 표시하지 않음.
                }
            }
        }

        // 현재 선택된 페이지 번호 업데이트.
        selectedPage = pageNumber;
    }

    /*
     * ShowPreviewImage(image)  [private]
     * ────────────────────────────────────────────────────────
     * 선택한 이미지를 전체 화면 미리보기 패널에 표시한다.
     * previewButton의 RawImage에 이미지를 설정하고,
     * previewPanelCG(CanvasGroupController)로 패널을 표시/활성화한다.
     *
     * [매개변수]
     *  image: 미리보기로 표시할 Texture.
     * ────────────────────────────────────────────────────────
     */
    private void ShowPreviewImage(Texture image)
    {
        // previewButton의 targetGraphic(RawImage)에 선택한 이미지 설정.
        RawImage renderer = previewButton.targetGraphic as RawImage;
        renderer.texture = image;

        // 미리보기 패널 표시 + 상호작용 활성화.
        previewPanelCG.Show();
        previewPanelCG.SetInteractableState(true);
    }

    /*
     * HidePreviewImage()  [public]
     * ────────────────────────────────────────────────────────
     * 전체 화면 미리보기 패널을 닫는다.
     * 미리보기 패널의 닫기 버튼에서 호출된다.
     *
     * 처리 순서:
     *  1) previewPanelCG.Hide(): 패널 투명하게 처리.
     *  2) SetInteractableState(false): 패널 버튼 클릭 불가 처리.
     *  3) SelectPageButton(selectedPage): 갤러리 페이지 버튼으로 포커스 복원.
     * ────────────────────────────────────────────────────────
     */
    public void HidePreviewImage()
    {
        previewPanelCG.Hide();
        previewPanelCG.SetInteractableState(false);

        // 미리보기가 닫히면 이전에 선택한 페이지 버튼으로 포커스 복원.
        SelectPageButton(selectedPage);
    }

    /*
     * SelectPageButton(pageNumber)  [private]
     * ────────────────────────────────────────────────────────
     * 지정한 페이지 번호에 해당하는 내비게이션 버튼에 UI 포커스를 설정한다.
     * EventSystem을 통해 키보드/컨트롤러 입력 포커스를 해당 버튼으로 이동.
     *
     * [매개변수]
     *  pageNumber: 포커스를 이동할 페이지 번호 (1-based).
     * ────────────────────────────────────────────────────────
     */
    private void SelectPageButton(int pageNumber)
    {
        // windowStart 기준으로 pageNumber가 pageButtons 배열에서
        // 몇 번째 인덱스인지 계산.
        int index = pageNumber - windowStart;

        // 유효한 범위 내에 있을 때만 포커스 설정.
        if (index >= 0 && index < pageButtons.Count)
            EventSystem.current.SetSelectedGameObject(pageButtons[index].gameObject);
    }

    /*
     * ToNextPage()  [public]
     * ────────────────────────────────────────────────────────
     * 내비게이션 윈도우를 PAGE_BUTTON_LIMIT 단위로 앞으로 이동한다.
     * 예) 1,2,3 표시 중 → 4,5,6 으로 이동.
     * ────────────────────────────────────────────────────────
     */
    public void ToNextPage()
    {
        windowStart += PAGE_BUTTON_LIMIT; // 윈도우 시작 위치를 앞으로 이동.
        UpdateNavBarWindow();             // 버튼 텍스트/이벤트 갱신.
        LoadPage(windowStart);            // 이동한 윈도우의 첫 페이지 표시.
        StartCoroutine(SelectPageButtonNextFrame(windowStart)); // 포커스 복원.
    }

    /*
     * ToPreviousPage()  [public]
     * ────────────────────────────────────────────────────────
     * 내비게이션 윈도우를 PAGE_BUTTON_LIMIT 단위로 뒤로 이동한다.
     * 예) 4,5,6 표시 중 → 1,2,3 으로 이동.
     * windowStart가 1 미만으로 내려가지 않도록 보정한다.
     * ────────────────────────────────────────────────────────
     */
    public void ToPreviousPage()
    {
        windowStart -= PAGE_BUTTON_LIMIT;
        if (windowStart < 1) windowStart = 1; // 1 미만으로 내려가지 않도록.
        UpdateNavBarWindow();
        LoadPage(windowStart);
        StartCoroutine(SelectPageButtonNextFrame(windowStart));
    }

    /*
     * ToLeftEnd()  [public]
     * ────────────────────────────────────────────────────────
     * 내비게이션 윈도우를 가장 처음(1페이지)으로 이동한다.
     * ────────────────────────────────────────────────────────
     */
    public void ToLeftEnd()
    {
        windowStart = 1;
        UpdateNavBarWindow();
        LoadPage(1);
        StartCoroutine(SelectPageButtonNextFrame(1));
    }

    /*
     * ToRightEnd()  [public]
     * ────────────────────────────────────────────────────────
     * 내비게이션 윈도우를 가장 마지막 페이지 그룹으로 이동한다.
     * 마지막 그룹의 시작 페이지를 계산하여 windowStart에 설정.
     *
     * [계산 방식]
     *  마지막 그룹 시작 = ((maxPages - 1) / PAGE_BUTTON_LIMIT) * PAGE_BUTTON_LIMIT + 1
     *  예) maxPages=10, LIMIT=3 → ((10-1)/3)*3+1 = 9*1+1 = 10? 아니, 3*3+1 = 10
     *  정수 나눗셈: (10-1)/3 = 3 → 3*3+1 = 10 → windowStart=10 (10번 페이지 그룹 시작)
     * ────────────────────────────────────────────────────────
     */
    public void ToRightEnd()
    {
        // 마지막 윈도우의 시작 페이지 계산.
        int lastWindowStart = ((maxPages - 1) / PAGE_BUTTON_LIMIT) * PAGE_BUTTON_LIMIT + 1;
        windowStart = lastWindowStart;
        UpdateNavBarWindow();
        LoadPage(maxPages); // 마지막 페이지 표시.
        StartCoroutine(SelectPageButtonNextFrame(maxPages));
    }

    /*
     * Update()
     * ────────────────────────────────────────────────────────
     * 매 프레임 실행. UI 포커스 자동 복구 로직.
     *
     * [목적]
     * 키보드/컨트롤러 입력 시, EventSystem의 현재 선택 오브젝트가
     * null이 되면 입력이 먹히지 않는다.
     * 이를 방지하기 위해 null이 되면 selectedPage의 버튼으로 자동 복구한다.
     *
     * [스킵 조건]
     * - initialized가 false면 버튼이 아직 없으므로 스킵.
     * - 미리보기 패널이 열려 있으면(isVisible) 갤러리 버튼이 포커스를 가지면 안 되므로 스킵.
     * ────────────────────────────────────────────────────────
     */
    void Update()
    {
        if (!initialized) return; // 초기화 전에는 실행하지 않음.
        if (previewPanelCG != null && previewPanelCG.isVisible) return; // 미리보기 중에는 스킵.

        // 현재 선택된 UI 오브젝트가 없으면 페이지 버튼으로 포커스 복구.
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == null)
        {
            SelectPageButton(selectedPage);
        }
    }

    /*
     * SelectPageButtonNextFrame(pageNumber)  [private 코루틴]
     * ────────────────────────────────────────────────────────
     * 1프레임 후에 지정 페이지 버튼에 포커스를 설정하는 코루틴.
     *
     * [왜 1프레임을 기다리나?]
     * EventSystem.SetSelectedGameObject()는 현재 프레임에서 UI 레이아웃이
     * 아직 업데이트되기 전에 호출하면 무시될 수 있다.
     * yield return null로 한 프레임을 건너뛰면 레이아웃이 확정된 후에
     * 포커스를 설정할 수 있어 안정적으로 작동한다.
     *
     * [매개변수]
     *  pageNumber: 포커스를 이동할 페이지 번호.
     * ────────────────────────────────────────────────────────
     */
    private IEnumerator SelectPageButtonNextFrame(int pageNumber)
    {
        yield return null; // 한 프레임 대기 (레이아웃 업데이트 완료 후 실행).
        SelectPageButton(pageNumber);
    }
}
