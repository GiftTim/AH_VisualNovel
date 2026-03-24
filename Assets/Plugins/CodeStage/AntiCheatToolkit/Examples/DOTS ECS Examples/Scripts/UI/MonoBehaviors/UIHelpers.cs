#if UNITY_DOTS_ENABLED
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

namespace CodeStage.AntiCheat.Examples
{
    /// <summary>
    /// Helper class for UI layout and interaction management
    /// Follows KISS principles with simple, focused methods
    /// </summary>
    public static class UIHelpers
    {
        private const float ButtonHeight = 28f;
        private const float SmallButtonHeight = 24f;
        private const float ColumnWidth = 290f;
        private const float LabelWidth = 115f;
        private const float TextFieldWidth = 60f;
        private const float ActionButtonWidth = 40f;

        /// <summary>
        /// Creates a UI action entity for processing by UIActionSystem
        /// </summary>
        public static void CreateUIAction(EntityManager em, UIActionType actionType, int intValue = 0, float floatValue = 0f, string stringValue = "")
        {
            if (em == null)
            {
                Debug.LogError("[ACTk] EntityManager is null, cannot create UI action");
                return;
            }

            if (actionType == UIActionType.None)
            {
                Debug.LogWarning("[ACTk] Attempted to create UI action with None type");
                return;
            }

            try
            {
                var actionEntity = em.CreateEntity();
                em.AddComponentData(actionEntity, new UIAction
                {
                    ActionType = actionType,
                    IntValue = intValue,
                    FloatValue = floatValue,
                    StringValue = stringValue
                });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ACTk] Failed to create UI action entity: {e.Message}");
            }
        }

        /// <summary>
        /// Draws a section header with consistent styling
        /// </summary>
        public static void DrawSectionHeader(string title)
        {
            GUILayout.Label(title, GUI.skin.box);
        }

        /// <summary>
        /// Draws a horizontal button group with consistent spacing
        /// </summary>
        public static void DrawButtonGroup(params (string text, System.Action action, bool enabled)[] buttons)
        {
            GUILayout.BeginHorizontal();
            foreach (var (text, action, enabled) in buttons)
            {
                GUI.enabled = enabled;
                if (GUILayout.Button(text, GUILayout.Height(ButtonHeight)))
                    action?.Invoke();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws a small horizontal button group for secondary actions
        /// </summary>
        public static void DrawSmallButtonGroup(params (string text, System.Action action)[] buttons)
        {
            GUILayout.BeginHorizontal();
            foreach (var (text, action) in buttons)
            {
                if (GUILayout.Button(text, GUILayout.Height(SmallButtonHeight)))
                    action?.Invoke();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws a storage input field with set/get buttons
        /// </summary>
        public static void DrawStorageField(string label, ref string value, System.Action setAction, System.Action getAction)
        {
            if (string.IsNullOrEmpty(label))
            {
                Debug.LogWarning("[ACTk] Storage field label is null or empty");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(LabelWidth));
            value = GUILayout.TextField(value ?? "", GUILayout.Width(TextFieldWidth));
            if (GUILayout.Button("Set", GUILayout.Width(ActionButtonWidth)))
            {
                try
                {
                    setAction?.Invoke();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ACTk] Set action failed for {label}: {e.Message}");
                }
            }
            if (GUILayout.Button("Get", GUILayout.Width(ActionButtonWidth)))
            {
                try
                {
                    getAction?.Invoke();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ACTk] Get action failed for {label}: {e.Message}");
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws detector status with color coding
        /// </summary>
        public static void DrawDetectorStatus(string name, bool isRunning, bool isCheatDetected)
        {
            string status = GetDetectorStatus(isRunning, isCheatDetected);
            Color originalColor = GUI.color;
            
            if (isCheatDetected)
                GUI.color = Color.red;
            else if (isRunning)
                GUI.color = Color.green;
            else
                GUI.color = Color.gray;
                
            GUILayout.Label($"{name}: {status}");
            GUI.color = originalColor;
        }

        /// <summary>
        /// Gets detector status text based on state
        /// </summary>
        public static string GetDetectorStatus(bool isRunning, bool isCheatDetected)
        {
            if (isCheatDetected)
                return "Cheating Detected";
            else if (isRunning)
                return "Running";
            else
                return "Idle";
        }

        /// <summary>
        /// Draws a two-column layout container
        /// </summary>
        public static void BeginTwoColumnLayout()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(ColumnWidth));
        }

        /// <summary>
        /// Ends the first column and starts the second
        /// </summary>
        public static void BeginSecondColumn()
        {
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(ColumnWidth));
        }

        /// <summary>
        /// Ends the two-column layout
        /// </summary>
        public static void EndTwoColumnLayout()
        {
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws a simple spacer
        /// </summary>
        public static void DrawSpacer(float height = 6f)
        {
            GUILayout.Space(height);
        }
    }
}
#endif
