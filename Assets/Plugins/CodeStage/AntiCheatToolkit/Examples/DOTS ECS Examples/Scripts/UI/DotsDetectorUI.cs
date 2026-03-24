using UnityEngine;

namespace CodeStage.AntiCheat.Examples
{
    [ExecuteInEditMode]
    public class DotsDetectorUI : MonoBehaviour
    {
        private bool isDotsEnabled;

        void Start()
        {
            isDotsEnabled = IsDotsEnabled();
        }

        void Update()
        {
            isDotsEnabled = IsDotsEnabled();
        }

        void OnGUI()
        {
            if (!isDotsEnabled)
            {
                DrawDotsDisabledMessage();
            }
        }
        
        private bool IsDotsEnabled()
        {
#if UNITY_DOTS_ENABLED
            return true;
#else
            return false;
#endif
        }

        private void DrawDotsDisabledMessage()
        {
            var messageStyle = new GUIStyle(GUI.skin.label);
            messageStyle.fontSize = 32;
            messageStyle.fontStyle = FontStyle.Bold;
            messageStyle.normal.textColor = Color.white;
            messageStyle.alignment = TextAnchor.MiddleCenter;

            var screenWidth = Screen.width;
            var screenHeight = Screen.height;

            GUI.Label(new Rect(0, 0,  screenWidth, screenHeight), "Unity ECS is not active", messageStyle);
        }

    }
}
