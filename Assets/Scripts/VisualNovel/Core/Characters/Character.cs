using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DIALOGUE;
using TMPro;


namespace CHARACTERS
{
    public abstract class Character
    {
        public  const bool   ENABLE_ON_START = false;
        private const float  UNHIGHLIGHTED_DARKEN_STRENGTH = 0.65f;
        public  const bool   DEFAULT_ORIENTATION_IS_FACING_LEFT = true;
        public  const string ANIMATION_REFRESH_TRIGGER = "Refresh";

        public DialogueSystem dialogueSystem => DialogueSystem.instance;
        protected CharacterManager characterManager => CharacterManager.instance;

        public string name = "";
        public string displayName = "";
        public RectTransform root = null;
        public CharacterConfigData config;
        public Animator animator;

        
        public Color color { get; protected set; } = Color.white;
        protected Color displayColor => highlighted ? highlightedColor : unhighlightedColor;
        protected Color highlightedColor   => color;
        protected Color unhighlightedColor 
            => new Color(color.r * UNHIGHLIGHTED_DARKEN_STRENGTH, 
                         color.g * UNHIGHLIGHTED_DARKEN_STRENGTH, 
                         color.b * UNHIGHLIGHTED_DARKEN_STRENGTH, 
                         color.a);
        public bool highlighted { get; protected set; } = true; 


        protected bool facingLeft = DEFAULT_ORIENTATION_IS_FACING_LEFT;


        public int priority { get; protected set; }


        //Coroutines
        protected Coroutine co_revealing, co_hiding, co_moving, co_changingColor, co_highlighting, co_flipping;

        public bool isRevealing      => co_revealing     != null;
        public bool isHiding         => co_hiding        != null;
        public bool isMoving         => co_moving        != null;
        public bool isChangingColor  => co_changingColor != null;
        public bool isHighlighting   => (highlighted && co_highlighting != null);
        public bool isUnHighlighting => (!highlighted && co_highlighting != null);
        public bool isFlipping       => co_flipping != null;


        public virtual bool isVisible { get; set; }
        
        public bool isFacingLeft => facingLeft;
        public bool isFacingRight => !facingLeft;
        

        public Character(string name, CharacterConfigData config, GameObject prefab)
        {
            this.name = name;
            displayName = name;
            this.config = config;

            if (prefab != null)
            {
                RectTransform parentPanel = null;
                switch(config.characterType)
                {
                    case CharacterType.Sprite:
                    case CharacterType.SpriteSheet:
                        parentPanel = characterManager.characterPanel;
                        break;
                    case CharacterType.Live2D:
                        parentPanel = characterManager.characterPanelLive2D;
                        break;
                    case CharacterType.Model3D:
                        parentPanel = characterManager.characterPanelModel3D;
                        break;
                }
                GameObject ob = Object.Instantiate(prefab, parentPanel);
                ob.name = characterManager.FormatCharacterPath(characterManager.characterPrefabNameFormat, name);
                ob.SetActive(true);
                root = ob.GetComponent<RectTransform>();
                animator = root.GetComponentInChildren<Animator>();
            }

        }

        public Coroutine Say(string dialogue) => Say(new List<string> { dialogue });
        public Coroutine Say(List<string> dialogue)
        {
            dialogueSystem.ShowSpeakerName(displayName);
            UpdateTextCustomizationOnScreen();
            return dialogueSystem.Say(dialogue);
        }

        public void SetNameFont(TMP_FontAsset font)     => config.nameFont = font;
        public void SetDialogueFont(TMP_FontAsset font) => config.dialogueFont = font;
        public void SetNameColor(Color color)           => config.nameColor = color;
        public void SetDialogueColor(Color color)       => config.dialogueColor = color;


        public void ResetConfigurationData() => config = CharacterManager.instance.GetCharacterConfig(name,getOriginal: true);
        public void UpdateTextCustomizationOnScreen() => dialogueSystem.ApplySpeakerDataToDialogueContainer(name);

        public virtual Coroutine Show(float speedMultiplier = 1f)
        {
            if (isRevealing)
            {
                characterManager.StopCoroutine(co_revealing);
            }

            if (isHiding)
            {
                characterManager.StopCoroutine(co_hiding);
            }

            co_revealing = characterManager.StartCoroutine(ShowingOrHiding(true, speedMultiplier));

            return co_revealing;
        }

        public virtual Coroutine Hide(float speedMultiplier = 1f)
        {
            if (isHiding)
            {
                characterManager.StopCoroutine(co_hiding);
            }

            if (isRevealing)
            {
                characterManager.StopCoroutine(co_revealing);
            }

            co_hiding = characterManager.StartCoroutine(ShowingOrHiding(false, speedMultiplier));

            return co_hiding;
        }

        public virtual IEnumerator ShowingOrHiding(bool show, float speedMultiplier = 1f)
        {
            Debug.Log("Show/Hide cannot be called from a base character type.");
            yield return null;
        }

        // 단간론파 할 때, 제거해야 할 부분
        public virtual void SetPosition(Vector2 position)
        {
            if (root == null)
                return;


            (Vector2 minAnchorTarget, Vector2 maxAnchorTarget) =
                ConvertUITargetPositionToRelativeCharacterAnchorTargets(position);

            root.anchorMin = minAnchorTarget;
            root.anchorMax = maxAnchorTarget;

        }

        // 단간론파 할 때, 제거해야 할 부분
        public virtual Coroutine MoveToPosition(Vector2 position, float speed = 2f, bool smooth = false)
        {
            if (root == null)
                return null;

            if (isMoving)
                characterManager.StopCoroutine(co_moving);

            co_moving = characterManager.StartCoroutine(MovingToPosition(position, speed, smooth));

            return co_moving;
        }

        // 단간론파 할 때, 제거해야 할 부분
        private IEnumerator MovingToPosition(Vector2 position, float speed, bool smooth)
        {
            (Vector2 minAnchorTarget, Vector2 maxAnchorTarget) =
                ConvertUITargetPositionToRelativeCharacterAnchorTargets(position);
            Vector2 padding = root.anchorMax - root.anchorMin;

            while (root.anchorMin != minAnchorTarget || root.anchorMax != maxAnchorTarget)
            {
                root.anchorMin = smooth ?
                    Vector2.Lerp(root.anchorMin, minAnchorTarget, speed * Time.deltaTime)
                    : Vector2.MoveTowards(root.anchorMin, minAnchorTarget, speed * Time.deltaTime * 0.35f);

                root.anchorMax = root.anchorMin + padding;

                if(smooth && Vector2.Distance(root.anchorMin, minAnchorTarget)<= 0.001f)
                {
                    root.anchorMin = minAnchorTarget;
                    root.anchorMax = maxAnchorTarget;
                    break;
                }

                yield return null;
            }

            Debug.Log("Done moving");
            co_moving = null;

        }

        // 단간론파 할 때, 제거해야 할 부분
        protected (Vector2, Vector2) ConvertUITargetPositionToRelativeCharacterAnchorTargets(Vector2 position)
        {
            Vector2 padding = root.anchorMax - root.anchorMin;
                                                            
            float minX = 1f - padding.x;
            float minY = 1f - padding.y;

            Vector2 minAnchorTarget = new Vector2(minX * position.x, minY * position.y);
            Vector2 maxAnchorTarget = minAnchorTarget + padding;

            return (minAnchorTarget, maxAnchorTarget);
        }

        public virtual void SetColor(Color color)
        {
            this.color = color;
        }

        public Coroutine TransitionColor(Color color, float speed =1f)
        {
            this.color = color;

            if(isChangingColor)
                characterManager.StopCoroutine(co_changingColor);

            co_changingColor = characterManager.StartCoroutine(ChangingColor(displayColor, speed));

            return co_changingColor;
        }

        public virtual IEnumerator ChangingColor(Color color, float speed)
        {
            Debug.Log("Color changing is not applicable on this character type!");
            yield return null;
        }

        // 단간론파 할 때, 제거해야 할 부분
        public Coroutine Highlight(float speed = 1f, bool immediate = false)
        {
            if (isHighlighting || isUnHighlighting) 
            {
                characterManager.StopCoroutine(co_highlighting);
            }

            highlighted = true;
            co_highlighting = characterManager.StartCoroutine(Highlighting(speed, immediate));

            return co_highlighting;
        }

        // 단간론파 할 때, 제거해야 할 부분
        public Coroutine UnHighlight(float speed = 1f, bool immediate = false)
        {
            if (isHighlighting || isUnHighlighting)
            {
                characterManager.StopCoroutine(co_highlighting);
            }

            highlighted = false;
            co_highlighting = characterManager.StartCoroutine(Highlighting(speed, immediate));

            return co_highlighting;
        }

        // 단간론파 할 때, 제거해야 할 부분
        public virtual IEnumerator Highlighting(float speed, bool immediate = false)
        {
            Debug.Log("Highlighting is not available on this character type!");
            yield return null;
        }

        public Coroutine Flip(float speed = 1, bool immediate = false)
        {
            if (isFacingLeft)
                return FaceRight(speed, immediate);
            else
                return FaceLeft(speed, immediate);
        }

        public Coroutine FaceLeft(float speed = 1, bool immediate = false)
        {
            if (isFlipping)
                characterManager.StopCoroutine(co_flipping);

            facingLeft = true;
            co_flipping = characterManager.StartCoroutine(FaceDirection(facingLeft, speed, immediate));

            return co_flipping;
        }
        
        public Coroutine FaceRight(float speed = 1, bool immediate = false)
        {
            if (isFlipping)
                characterManager.StopCoroutine(co_flipping);

            facingLeft = false;
            co_flipping = characterManager.StartCoroutine(FaceDirection(facingLeft, speed, immediate));

            return co_flipping;
        }

        public virtual IEnumerator FaceDirection(bool FaceLeft, float speedMultiplier, bool immediate)
        {
            Debug.Log("Cannot flip a character of this type!");
            yield return null;
        }

        public void SetPriority(int priority, bool autoSortCharactersOnUI = true)
        {
            this.priority = priority;

            if (autoSortCharactersOnUI)
                characterManager.SortCharacters();
        }

        public void Animate(string animation)
        {
            animator.SetTrigger(animation);
        }

        public void Animate(string animation, bool state)
        {
            animator.SetBool(animation, state);
            animator.SetTrigger(ANIMATION_REFRESH_TRIGGER);
        }

        public virtual void OnReceiveCastingExpression(int layer, string expression)
        {
            Debug.Log("Cannot cast expression on a character of this type!");

            return;
        }

        public enum CharacterType
        {
            Text,
            Sprite,
            SpriteSheet,
            Live2D,
            Model3D
        }
    }
}