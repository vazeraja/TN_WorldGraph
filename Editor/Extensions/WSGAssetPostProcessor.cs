using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.WorldGraph.Editor {

    public class WSGAssetPostProcessor : AssetPostprocessor {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths) {
            var windows = Resources.FindObjectsOfTypeAll<WorldGraphEditorWindow>();

            bool moved = movedAssets.Select(AssetDatabase.AssetPathToGUID).ToList().Any();
            if (moved) {
                foreach (var matGraphEditWindow in windows) {
                    foreach (string t in movedAssets) {
                        if (matGraphEditWindow.selectedGuid == AssetDatabase.AssetPathToGUID(t))
                            matGraphEditWindow.UpdateTitle();
                    }
                }
            }

            bool deleted = deletedAssets.Select(AssetDatabase.AssetPathToGUID).ToList().Any();
            if (deleted) {
                foreach (var matGraphEditWindow in windows) {
                    foreach (string t in deletedAssets) {
                        if (matGraphEditWindow.selectedGuid == AssetDatabase.AssetPathToGUID(t))
                            matGraphEditWindow.AssetWasDeleted();
                    }
                }
            }


            var changedGraphGuids = importedAssets.Select(AssetDatabase.AssetPathToGUID).ToList();
            foreach (var window in windows) {
                if (changedGraphGuids.Contains(window.selectedGuid)) {
                    window.CheckForChanges();
                }
            }
        }
    }

}