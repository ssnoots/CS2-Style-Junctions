using ColossalFramework.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace CS2StyleJunctions
{
    // A serializable representation of a key combo. KeyCode + modifier flags.
    // XML-friendly because all fields are simple value types.
    public class Hotkey
    {
        public int KeyCode { get; set; } = (int)UnityEngine.KeyCode.J;
        public bool Ctrl { get; set; } = true;
        public bool Shift { get; set; } = true;
        public bool Alt { get; set; } = false;

        // Human-readable rendering for the UI button.
        [XmlIgnore]
        public string DisplayName
        {
            get
            {
                string s = "";
                if (Ctrl) s += "Ctrl+";
                if (Shift) s += "Shift+";
                if (Alt) s += "Alt+";
                s += ((KeyCode)KeyCode).ToString();
                return s;
            }
        }

        // True if this frame's input state matches the combo.
        public bool IsPressedThisFrame()
        {
            // Modifiers must MATCH exactly (both sides), so the user's combo
            // isn't accidentally triggered by overlapping keys. Then the
            // primary key must be pressed down THIS frame (edge-trigger, not
            // held-trigger).
            bool ctrlHeld = Input.GetKey(UnityEngine.KeyCode.LeftControl) || Input.GetKey(UnityEngine.KeyCode.RightControl);
            bool shiftHeld = Input.GetKey(UnityEngine.KeyCode.LeftShift) || Input.GetKey(UnityEngine.KeyCode.RightShift);
            bool altHeld = Input.GetKey(UnityEngine.KeyCode.LeftAlt) || Input.GetKey(UnityEngine.KeyCode.RightAlt);

            if (Ctrl != ctrlHeld) return false;
            if (Shift != shiftHeld) return false;
            if (Alt != altHeld) return false;

            return Input.GetKeyDown((UnityEngine.KeyCode)KeyCode);
        }
    }

    public class JunctionSettings
    {
        public float SmallRoadRadius { get; set; } = 4f;
        public float MediumRoadRadius { get; set; } = 8f;
        public float LargeRoadRadius { get; set; } = 14f;
        public float HighwayRadius { get; set; } = 20f;
        public float RampHighwaySideRadius { get; set; } = 40f;
        public float RampSideRadius { get; set; } = 35f;

        // New: the panel-toggle hotkey, defaults to Ctrl+Shift+J.
        public Hotkey ToggleHotkey { get; set; } = new Hotkey();

        public const float MinRadius = 1f;
        public const float MaxRadius = 30f;
        public const float MaxRampRadius = 60f;

        public static JunctionSettings Active { get; private set; } = new JunctionSettings();

        private const string ConfigFileName = "CS2StyleJunctionsSettings.xml";

        public static void Load()
        {
            try
            {
                string path = GetConfigPath();
                if (!System.IO.File.Exists(path))
                {
                    Debug.Log("[CS2SJ] No saved settings; using defaults.");
                    return;
                }

                using (var reader = new System.IO.StreamReader(path))
                {
                    var serializer = new XmlSerializer(typeof(JunctionSettings));
                    Active = (JunctionSettings)serializer.Deserialize(reader);
                }

                // Guard against settings files saved by older versions where
                // Hotkey didn't exist — Deserialize will leave it null.
                if (Active.ToggleHotkey == null)
                {
                    Active.ToggleHotkey = new Hotkey();
                }

                Debug.Log($"[CS2SJ] Loaded settings from {path} " +
                          $"(hotkey: {Active.ToggleHotkey.DisplayName})");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CS2SJ] Failed to load settings, using defaults: {e}");
                Active = new JunctionSettings();
            }
        }

        public static void Save()
        {
            try
            {
                string path = GetConfigPath();
                using (var writer = new System.IO.StreamWriter(path))
                {
                    var serializer = new XmlSerializer(typeof(JunctionSettings));
                    serializer.Serialize(writer, Active);
                }
                Debug.Log($"[CS2SJ] Saved settings to {path}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CS2SJ] Failed to save settings: {e}");
            }
        }

        private static string GetConfigPath()
        {
            return System.IO.Path.Combine(DataLocation.localApplicationData, ConfigFileName);
        }
    }
}
