using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonBehaviours : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject selectImageObject;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectImageObject != null)
            selectImageObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (selectImageObject != null)
            selectImageObject.SetActive(false);
    }
}
