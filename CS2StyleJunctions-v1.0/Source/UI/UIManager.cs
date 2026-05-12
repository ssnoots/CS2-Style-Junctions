using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace CS2StyleJunctions.UI
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        private static GameObject _hostObject;
        private SettingsPanel _panel;

        private NetInfo _lastSeenPrefab;

        public static void Initialize()
        {
            if (_instance != null) return;
            _hostObject = new GameObject("CS2SJ_UIManager");
            DontDestroyOnLoad(_hostObject);
            _instance = _hostObject.AddComponent<UIManager>();
            Debug.Log("[CS2SJ] UIManager initialized.");
        }

        public static void Teardown()
        {
            if (_instance != null && _instance._panel != null)
            {
                Destroy(_instance._panel.gameObject);
            }
            if (_hostObject != null)
            {
                Destroy(_hostObject);
                _hostObject = null;
            }
            _instance = null;
        }

        public void Update()
        {
            // If we're capturing a new hotkey, intercept keypresses here
            // BEFORE checking the toggle.
            if (_panel != null && _panel.IsCapturingHotkey)
            {
                HandleHotkeyCapture();
                return;  // skip toggle this frame
            }

            // Normal hotkey: read the user's configured combo from settings.
            if (JunctionSettings.Active.ToggleHotkey != null &&
                JunctionSettings.Active.ToggleHotkey.IsPressedThisFrame())
            {
                Toggle();
            }

            if (_panel != null && _panel.isVisible)
            {
                UpdateActiveRoadClass();
            }
        }

        // Polls every KeyCode value looking for one being pressed THIS frame.
        // When found, hands it to the panel for capture.
        private void HandleHotkeyCapture()
        {
            // Escape cancels capture without rebinding.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _panel.CancelHotkeyCapture();
                return;
            }

            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    _panel.CaptureHotkey(key);
                    return;
                }
            }
        }

        private void UpdateActiveRoadClass()
        {
            NetTool tool = ToolsModifierControl.GetTool<NetTool>();
            NetInfo prefab = (tool != null && tool.enabled) ? tool.m_prefab : null;

            if (prefab == _lastSeenPrefab) return;
            _lastSeenPrefab = prefab;

            RoadClass cls = JunctionAnalyzer.ClassifyRoad(prefab);
            _panel.SetActiveRoadClass(cls);
        }

        private void Toggle()
        {
            if (_panel == null)
            {
                CreatePanel();
                _panel.isVisible = true;
                _lastSeenPrefab = null;
                UpdateActiveRoadClass();
            }
            else
            {
                // If hiding while a capture is in progress, cancel it.
                if (_panel.isVisible && _panel.IsCapturingHotkey)
                {
                    _panel.CancelHotkeyCapture();
                }
                _panel.isVisible = !_panel.isVisible;
            }
        }

        private void CreatePanel()
        {
            UIView view = UIView.GetAView();
            if (view == null)
            {
                Debug.LogError("[CS2SJ] No UIView available; can't create panel.");
                return;
            }

            GameObject panelObject = new GameObject("CS2SJ_SettingsPanel");
            panelObject.transform.SetParent(view.transform, false);
            _panel = panelObject.AddComponent<SettingsPanel>();
            _panel.isVisible = false;
        }
    }
}
