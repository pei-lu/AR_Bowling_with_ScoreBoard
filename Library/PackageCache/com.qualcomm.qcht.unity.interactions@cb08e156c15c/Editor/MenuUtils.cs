// /******************************************************************************
//  * File: MenuUtils.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core;
using QCHT.Interactions.Core;
using QCHT.Interactions.Proximal;
using UnityEditor;
using UnityEngine;

namespace QCHT.Interactions.Editor
{
    public static class MenuUtils
    {
        [MenuItem("GameObject/QCHT/Hand Tracking Manager", false, 0)]
        private static void CreateHandTrackingManager(MenuCommand menuCommand)
        {
            XRHandTrackingManager.GetOrCreate();
        }

        [MenuItem("GameObject/QCHT/GrabPoint", false, 11)]
        private static void CreateGrabPoint(MenuCommand menuCommand)
        {
            var go = new GameObject("GrabPoint");
            go.AddComponent<GrabPoint>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        
        #region QCHT3
#pragma warning disable CS0612
        [MenuItem("GameObject/QCHT/Legacy/QCHTManager", false, 20)]
        private static void CreateManager(MenuCommand menuCommand)
        {
            var go = new GameObject("QCHTManager");

            go.AddComponent<QCHTManager>();

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        
        [MenuItem("GameObject/QCHT/Legacy/Avatar", false, 21)]
        private static void LoadAvatar(MenuCommand menuCommand)
        {
            LoadPrefab("QCHTAvatar");
        }
#pragma warning restore CS0612
        #endregion

        private static void LoadPrefab(string prefabName)
        {
            var results = AssetDatabase.FindAssets(prefabName);

            if (results == null || results.Length <= 0)
            {
                Debug.LogError($"[LoadPrefab] Can't find the prefab {prefabName} in assets");
                return;
            }

            var prefabPath = AssetDatabase.GUIDToAssetPath(results[0]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var go = PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;

            Debug.Log($"[LoadPrefab] Instantiated a prefab {prefabName} from {prefabPath}!");
        }
    }
}