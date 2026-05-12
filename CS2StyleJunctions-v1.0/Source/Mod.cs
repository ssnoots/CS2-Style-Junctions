using ICities;
using CitiesHarmony.API;
using ColossalFramework.UI;

namespace CS2StyleJunctions
{
    public class Mod : IUserMod
    {
        public string Name => "CS2-Style Junctions";
        public string Description => "Automatic CS2-inspired geometry polish for road junctions. Press Ctrl+Shift+J in-game to open the settings panel.";

        public void OnEnabled()
        {
            // Load saved slider values before patches install so the analyzer
            // sees the right values immediately.
            JunctionSettings.Load();

            HarmonyHelper.DoOnHarmonyReady(() => HarmonyManager.Install());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                HarmonyManager.Uninstall();
            }
            UI.UIManager.Teardown();
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("CS2-Style Junctions");

            group.AddCheckbox(
                "Enable automatic junction polish",
                NodeRegistry.ApplicationEnabled,
                (value) => NodeRegistry.ApplicationEnabled = value);

            group.AddCheckbox(
                "Also apply to nodes that existed before the save was loaded\n" +
                "(LEAVE OFF if you have hand-tuned interchanges with NCR / Move It)",
                NodeRegistry.AffectExistingNodes,
                (value) => NodeRegistry.AffectExistingNodes = value);

            group.AddSpace(8);

            // Tell users where the rest of the settings are.
            UIComponent infoLabel = ((UIPanel)((UIHelper)group).self).AddUIComponent<UILabel>();
            ((UILabel)infoLabel).text =
                "Per-road radius sliders are in the in-game panel.\n" +
                "Press Ctrl+Shift+J while in a city to open it.";
        }
    }
}
