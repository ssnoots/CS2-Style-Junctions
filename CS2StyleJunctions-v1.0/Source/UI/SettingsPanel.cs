using ColossalFramework.UI;
using UnityEngine;

namespace CS2StyleJunctions.UI
{
    public class SettingsPanel : UIPanel
    {
        private const float PanelWidth = 320f;
        private const float PanelHeight = 510f;  // taller to fit hotkey row
        private const float Padding = 12f;
        private const float RowHeight = 44f;

        private UISprite _smallHighlight;
        private UISprite _mediumHighlight;
        private UISprite _largeHighlight;
        private UISprite _highwayHighlight;

        private UILabel _hintLabel;
        private UIButton _saveButton;
        private UIButton _retuneButton;
        private UIButton _hotkeyButton;
        private UILabel _toastLabel;

        private float _toastRemainingSeconds = 0f;
        private RoadClass _lastHighlighted = RoadClass.Unknown;

        // When true, the next keypress will be captured as the new hotkey
        // instead of being processed normally.
        public bool IsCapturingHotkey { get; private set; } = false;

        public override void Start()
        {
            base.Start();

            backgroundSprite = "MenuPanel2";
            color = new Color32(40, 40, 50, 220);

            width = PanelWidth;
            height = PanelHeight;
            relativePosition = new Vector3(
                GetUIView().fixedWidth - PanelWidth - 30f,
                100f);

            UIDragHandle dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.size = new Vector2(PanelWidth, 32f);

            UILabel title = AddUIComponent<UILabel>();
            title.text = "CS2-Style Junctions";
            title.textColor = Color.white;
            title.relativePosition = new Vector3(Padding, 8f);

            UIButton closeButton = AddUIComponent<UIButton>();
            closeButton.text = "X";
            closeButton.size = new Vector2(24f, 24f);
            closeButton.relativePosition = new Vector3(PanelWidth - 32f, 4f);
            closeButton.normalBgSprite = "ButtonMenu";
            closeButton.hoveredBgSprite = "ButtonMenuHovered";
            closeButton.pressedBgSprite = "ButtonMenuPressed";
            closeButton.eventClick += (c, p) => { isVisible = false; };

            float y = 40f;
            JunctionSettings s = JunctionSettings.Active;

            _smallHighlight = AddRowHighlight(y);
            AddSliderRow("Small road", y, s.SmallRoadRadius,
                JunctionSettings.MinRadius, JunctionSettings.MaxRadius,
                v => JunctionSettings.Active.SmallRoadRadius = v);
            y += RowHeight;

            _mediumHighlight = AddRowHighlight(y);
            AddSliderRow("Medium road", y, s.MediumRoadRadius,
                JunctionSettings.MinRadius, JunctionSettings.MaxRadius,
                v => JunctionSettings.Active.MediumRoadRadius = v);
            y += RowHeight;

            _largeHighlight = AddRowHighlight(y);
            AddSliderRow("Large road", y, s.LargeRoadRadius,
                JunctionSettings.MinRadius, JunctionSettings.MaxRadius,
                v => JunctionSettings.Active.LargeRoadRadius = v);
            y += RowHeight;

            _highwayHighlight = AddRowHighlight(y);
            AddSliderRow("Highway", y, s.HighwayRadius,
                JunctionSettings.MinRadius, JunctionSettings.MaxRadius,
                v => JunctionSettings.Active.HighwayRadius = v);
            y += RowHeight;

            AddSliderRow("Ramp (hwy side)", y, s.RampHighwaySideRadius,
                JunctionSettings.MinRadius, JunctionSettings.MaxRampRadius,
                v => JunctionSettings.Active.RampHighwaySideRadius = v);
            y += RowHeight;

            AddSliderRow("Ramp (off side)", y, s.RampSideRadius,
                JunctionSettings.MinRadius, JunctionSettings.MaxRampRadius,
                v => JunctionSettings.Active.RampSideRadius = v);
            y += RowHeight;

            _hintLabel = AddUIComponent<UILabel>();
            _hintLabel.text = "Highlighted: the selected road's class.\nRamp sliders apply at acute highway joins.";
            _hintLabel.textColor = new Color32(180, 180, 180, 255);
            _hintLabel.textScale = 0.7f;
            _hintLabel.relativePosition = new Vector3(Padding, y + 4f);

            // Hotkey rebinding row.
            UILabel hotkeyLabel = AddUIComponent<UILabel>();
            hotkeyLabel.text = "Toggle hotkey:";
            hotkeyLabel.textColor = Color.white;
            hotkeyLabel.textScale = 0.8f;
            hotkeyLabel.relativePosition = new Vector3(Padding, y + 42f);

            _hotkeyButton = AddUIComponent<UIButton>();
            _hotkeyButton.size = new Vector2(160f, 22f);
            _hotkeyButton.relativePosition = new Vector3(PanelWidth - 172f, y + 38f);
            _hotkeyButton.normalBgSprite = "ButtonMenu";
            _hotkeyButton.hoveredBgSprite = "ButtonMenuHovered";
            _hotkeyButton.pressedBgSprite = "ButtonMenuPressed";
            _hotkeyButton.textColor = Color.white;
            _hotkeyButton.textScale = 0.75f;
            _hotkeyButton.text = JunctionSettings.Active.ToggleHotkey.DisplayName;
            _hotkeyButton.eventClick += (c, p) => StartHotkeyCapture();

            // Save button.
            _saveButton = AddUIComponent<UIButton>();
            _saveButton.size = new Vector2(PanelWidth - 2 * Padding, 26f);
            _saveButton.relativePosition = new Vector3(Padding, y + 76f);
            _saveButton.normalBgSprite = "ButtonMenu";
            _saveButton.hoveredBgSprite = "ButtonMenuHovered";
            _saveButton.pressedBgSprite = "ButtonMenuPressed";
            _saveButton.textColor = Color.white;
            _saveButton.eventClick += (c, p) =>
            {
                JunctionSettings.Save();
                ShowToast("Settings saved");
            };

            // Retune button.
            _retuneButton = AddUIComponent<UIButton>();
            _retuneButton.size = new Vector2(PanelWidth - 2 * Padding, 26f);
            _retuneButton.relativePosition = new Vector3(Padding, y + 108f);
            _retuneButton.normalBgSprite = "ButtonMenu";
            _retuneButton.hoveredBgSprite = "ButtonMenuHovered";
            _retuneButton.pressedBgSprite = "ButtonMenuPressed";
            _retuneButton.disabledBgSprite = "ButtonMenuDisabled";
            _retuneButton.textColor = Color.white;
            _retuneButton.disabledTextColor = new Color32(140, 140, 140, 255);
            _retuneButton.eventClick += (c, p) =>
            {
                if (NodeRegistry.BakedDecisionCount == 0) return;
                int cleared = NodeRegistry.ClearBakedRadii();
                Debug.Log($"[CS2SJ] Re-tune: cleared {cleared} baked decisions.");
                ShowToast($"Re-tuning {cleared} junctions");
            };

            _toastLabel = AddUIComponent<UILabel>();
            _toastLabel.text = "";
            _toastLabel.textColor = new Color32(180, 255, 180, 0);
            _toastLabel.textScale = 0.8f;
            _toastLabel.relativePosition = new Vector3(Padding, y + 142f);

            UpdateButtonLabels();
        }

        private UISprite AddRowHighlight(float y)
        {
            UISprite sprite = AddUIComponent<UISprite>();
            sprite.spriteName = "EmptySprite";
            sprite.size = new Vector2(PanelWidth - 8f, RowHeight - 4f);
            sprite.relativePosition = new Vector3(4f, y - 2f);
            sprite.color = new Color32(80, 140, 220, 0);
            sprite.SendToBack();

            // CRITICAL: make the highlight non-interactive so it doesn't eat
            // clicks meant for the slider behind it. Without this, the user
            // can't drag the slider on the highlighted row.
            sprite.isInteractive = false;

            return sprite;
        }

        public void SetActiveRoadClass(RoadClass cls)
        {
            if (cls == _lastHighlighted) return;
            _lastHighlighted = cls;

            byte hidden = 0;
            byte visible = 80;

            _smallHighlight.color = new Color32(80, 140, 220, hidden);
            _mediumHighlight.color = new Color32(80, 140, 220, hidden);
            _largeHighlight.color = new Color32(80, 140, 220, hidden);
            _highwayHighlight.color = new Color32(80, 140, 220, hidden);

            switch (cls)
            {
                case RoadClass.Small:
                    _smallHighlight.color = new Color32(80, 140, 220, visible);
                    break;
                case RoadClass.Medium:
                    _mediumHighlight.color = new Color32(80, 140, 220, visible);
                    break;
                case RoadClass.Large:
                    _largeHighlight.color = new Color32(80, 140, 220, visible);
                    break;
                case RoadClass.Highway:
                    _highwayHighlight.color = new Color32(80, 140, 220, visible);
                    break;
            }
        }

        private void StartHotkeyCapture()
        {
            IsCapturingHotkey = true;
            _hotkeyButton.text = "Press a key...";
        }

        // Called by UIManager when a key is detected during capture mode.
        // The captured key becomes the new hotkey. Modifiers are read
        // from the current input state at the moment of capture.
        public void CaptureHotkey(KeyCode key)
        {
            // Don't capture modifier-only presses — would create unbindable keys.
            if (key == KeyCode.LeftControl || key == KeyCode.RightControl ||
                key == KeyCode.LeftShift || key == KeyCode.RightShift ||
                key == KeyCode.LeftAlt || key == KeyCode.RightAlt) return;

            Hotkey hk = new Hotkey
            {
                KeyCode = (int)key,
                Ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl),
                Shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift),
                Alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)
            };
            JunctionSettings.Active.ToggleHotkey = hk;

            IsCapturingHotkey = false;
            _hotkeyButton.text = hk.DisplayName;
            ShowToast($"Hotkey set to {hk.DisplayName}");
        }

        // Cancel capture if the user clicks away or the panel hides.
        public void CancelHotkeyCapture()
        {
            if (!IsCapturingHotkey) return;
            IsCapturingHotkey = false;
            _hotkeyButton.text = JunctionSettings.Active.ToggleHotkey.DisplayName;
        }

        public override void Update()
        {
            base.Update();
            if (!isVisible) return;

            UpdateButtonLabels();

            if (_toastRemainingSeconds > 0f)
            {
                _toastRemainingSeconds -= Time.unscaledDeltaTime;
                if (_toastRemainingSeconds <= 0f)
                {
                    _toastLabel.text = "";
                    _toastLabel.textColor = new Color32(180, 255, 180, 0);
                }
                else
                {
                    float alpha = Mathf.Clamp01(_toastRemainingSeconds);
                    _toastLabel.textColor = new Color32(180, 255, 180, (byte)(alpha * 255f));
                }
            }
        }

        private void UpdateButtonLabels()
        {
            int count = NodeRegistry.BakedDecisionCount;
            if (count == 0)
            {
                _retuneButton.text = "Re-tune existing junctions";
                _retuneButton.isEnabled = false;
            }
            else
            {
                _retuneButton.text = $"Re-tune {count} existing junction" + (count == 1 ? "" : "s");
                _retuneButton.isEnabled = true;
            }
            _saveButton.text = "Save settings";
        }

        private void ShowToast(string message)
        {
            _toastLabel.text = message;
            _toastRemainingSeconds = 2f;
            _toastLabel.textColor = new Color32(180, 255, 180, 255);
        }

        private UISlider AddSliderRow(
            string labelText, float y, float currentValue,
            float minVal, float maxVal,
            System.Action<float> onChanged)
        {
            UILabel label = AddUIComponent<UILabel>();
            label.text = labelText;
            label.textColor = Color.white;
            label.textScale = 0.8f;
            label.relativePosition = new Vector3(Padding, y);

            UILabel valueLabel = AddUIComponent<UILabel>();
            valueLabel.text = $"{currentValue:F1} m";
            valueLabel.textColor = Color.white;
            valueLabel.textScale = 0.8f;
            valueLabel.relativePosition = new Vector3(PanelWidth - 60f, y);

            UISlider slider = AddUIComponent<UISlider>();
            slider.size = new Vector2(PanelWidth - 2 * Padding, 14f);
            slider.relativePosition = new Vector3(Padding, y + 18f);
            slider.minValue = minVal;
            slider.maxValue = maxVal;
            slider.stepSize = 0.5f;
            slider.value = currentValue;

            UISprite thumb = slider.AddUIComponent<UISprite>();
            thumb.spriteName = "SliderBudget";
            thumb.size = new Vector2(12f, 16f);
            slider.thumbObject = thumb;

            UISprite bg = slider.AddUIComponent<UISprite>();
            bg.spriteName = "BudgetSlider";
            bg.size = slider.size;
            bg.relativePosition = Vector3.zero;
            slider.SendToBack();

            slider.eventValueChanged += (component, value) =>
            {
                valueLabel.text = $"{value:F1} m";
                onChanged(value);
            };

            return slider;
        }
    }
}
