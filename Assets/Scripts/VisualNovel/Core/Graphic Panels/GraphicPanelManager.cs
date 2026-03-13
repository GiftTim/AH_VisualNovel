using UnityEngine;

public class GraphicPanelManager : MonoBehaviour
{
    public static GraphicPanelManager instance { get; private set; }

    public const float DEFAULT_TRANSITION_SPEED = 1f;

    [field:SerializeField] public GraphicPanel[] allPanels { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public GraphicPanel GetPanel(string name)
    {
        name = name.ToLower();

        foreach (var panel in allPanels)
        {
            if (panel.panelName.ToLower() == name)
            {
                return panel;
            }
        }

        Debug.LogError($"GraphicPanelManager: No panel found with name {name}");
        return null;
    }
}
