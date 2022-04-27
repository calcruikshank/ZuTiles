using UnityEngine;

namespace Gameboard.Tools
{
    public class ScreenTouchButton : ScreenTouchInput
    {
        public delegate void ButtonEvent();
            public ButtonEvent buttonPressed;

        public GameObject buttonTarget;
        public string buttonMethod;

        public Sprite idleSprite;
        public Sprite downSprite;
        public Sprite disabledSprite;

        /// <summary>
        /// Buttons should only activate when a Began touch event occurs. This prevents the 'Surprise Arm' scenario, where
        /// a button springs into existence beneath someone's arm and then is suddenly pressed because of the arm touching the screen.
        /// </summary>
        protected override bool BeginOnFirstTouchOnly { get { return true; } }

        private SpriteRenderer _buttonSprite;
        public SpriteRenderer buttonSprite
        {
            get
            {
                if (_buttonSprite == null)
                {
                    _buttonSprite = GetComponent<SpriteRenderer>();
                }

                return _buttonSprite;
            }
        }

        void Update()
        {
            UpdateButtonSprite();
        }

        public void PressButton()
        {
            SetState_Down();
        }

        protected override void SetState_Down()
        {
            if ((Time.time - lastInputTime) < 0.35f)
            {
                return;
            }

            lastInputTime = Time.time;

            if (buttonTarget != null && !string.IsNullOrEmpty(buttonMethod))
            {
                buttonTarget.SendMessage(buttonMethod);
            }

            buttonPressed?.Invoke();
        }

        private void UpdateButtonSprite()
        {
            if (buttonSprite == null)
            {
                return;
            }

            Sprite targetSprite = null;
            switch (activeTouchState)
            {
                case TouchStates.NONE: break;
                case TouchStates.Idle: targetSprite = idleSprite; break;
                case TouchStates.Down: targetSprite = downSprite; break;
                case TouchStates.Up: targetSprite = idleSprite; break;
                case TouchStates.Disabled: targetSprite = disabledSprite; break;
            }

            if (buttonSprite.sprite != targetSprite)
            {
                buttonSprite.sprite = targetSprite;
            }
        }
    }
}