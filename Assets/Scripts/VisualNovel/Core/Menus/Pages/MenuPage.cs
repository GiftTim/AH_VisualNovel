using UnityEngine;

public class MenuPage : MonoBehaviour
{
    public enum PageType
    {
        SaveAndLoad,
        Config
    }
    public PageType pageType;   

    public const string OPEN = "Open";
    public const string CLOSE = "Close";
    public Animator anim;

    public virtual void Open()
    {
        anim.SetTrigger(OPEN);
    } 

    public virtual void Close(bool closeAllMenus = false)
    {
        anim.SetTrigger(CLOSE);

        if(closeAllMenus)
        {
            VNMenuManager.instance.CloseRoot();
        }
    }
}
