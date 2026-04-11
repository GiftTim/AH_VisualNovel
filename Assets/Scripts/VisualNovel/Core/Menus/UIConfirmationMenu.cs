using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIConfirmationMenu : MonoBehaviour
{
    public static UIConfirmationMenu instance { get; private set; }

    [SerializeField] private Animator anim;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private LayoutGroup choiceLayoutGroup;
    [SerializeField] private GameObject buttonPrefab;

    private GameObject[] activeOptions = new GameObject[0];

    private void Awake()
    {
        instance = this;
    }

    public void Show(string title, params ConfirmationButton[] options)
    {
        if(options.Length == 0)
        {
            Debug.LogError("Confirmation menu must have at least 1 option provided for the user to select.");
            return;
        }

        this.title.text = title;
        
        CreateOptionButton(options);

        anim.Play("Enter");
    }

    public void Hide()
    {
        anim.Play("Exit");
    }

    private void CreateOptionButton(ConfirmationButton[] options)
    {
        foreach (GameObject g in activeOptions)
        {
            DestroyImmediate(g);
        }
        activeOptions = new GameObject[options.Length];

        for (int i = 0; i < options.Length; i++)
        {
            ConfirmationButton option = options[i];
            GameObject ob = Instantiate(buttonPrefab, choiceLayoutGroup.transform);
            ob.SetActive(true);

            Button button = ob.GetComponent<Button>();

            if(option.action != null)
            {
                button.onClick.AddListener(() => option.action.Invoke());
            }

            if(option.autoCloseOnQuit)
            {
                button.onClick.AddListener(() => Hide());
            }

            TextMeshProUGUI txt = ob.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = option.title;

            activeOptions[i] = ob;
        }
    }


    public struct ConfirmationButton
    {
        public string title;
        public Action action;
        public bool autoCloseOnQuit;

        public ConfirmationButton(string title, Action action, bool autoCloseOnClick = true)
        {
            this.title = title;
            this.action = action;
            this.autoCloseOnQuit = autoCloseOnClick;
        } 
    }
}
