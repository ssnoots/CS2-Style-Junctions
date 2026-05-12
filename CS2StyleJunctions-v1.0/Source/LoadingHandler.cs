using ICities;
using UnityEngine;

namespace CS2StyleJunctions
{
    public class LoadingHandler : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            bool isGameplayLoad =
                mode == LoadMode.NewGame ||
                mode == LoadMode.NewGameFromScenario ||
                mode == LoadMode.LoadGame ||
                mode == LoadMode.LoadScenario;

            if (!isGameplayLoad)
            {
                Debug.Log($"[CS2SJ] Skipping snapshot — non-gameplay load mode: {mode}");
                return;
            }

            NodeRegistry.SnapshotExistingNodes();
            NodeRegistry.ApplicationEnabled = true;
            Debug.Log("[CS2SJ] Patch is now active for new nodes.");

            // Spin up the in-game UI manager so the hotkey works.
            UI.UIManager.Initialize();
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            NodeRegistry.ApplicationEnabled = false;
            NodeRegistry.Clear();
            UI.UIManager.Teardown();
        }
    }
}
