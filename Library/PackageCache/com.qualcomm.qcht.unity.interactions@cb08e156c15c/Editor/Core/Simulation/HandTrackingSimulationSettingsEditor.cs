// /******************************************************************************
//  * File: HandTrackingSimulationSettingsEditor.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEditor;

namespace QCHT.Interactions.Core.Editor
{
    [CustomEditor(typeof(HandTrackingSimulationEditorSettings))]
    public class HandTrackingSimulationSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI() {
            // Start on load
            EditorGUI.BeginChangeCheck();
            var startOnLoad = serializedObject.FindProperty("enabled");
            EditorGUILayout.PropertyField(startOnLoad);
            if (EditorGUI.EndChangeCheck()) {
                if (startOnLoad.boolValue) {
                    if (EditorUtility.DisplayDialog("Qualcomm Hand Tracking", "The XR Device Simulator should be enabled in ProjectSettings/XR Interaction Toolkit in order to allow the simulation working. Please make sure to enable it before entering in playmode.", "Ok")) {
                    }
                }
            }
            
            // Data source
            EditorGUI.BeginChangeCheck();
            var dataSource = serializedObject.FindProperty("dataSource");
            EditorGUILayout.PropertyField(dataSource);
            if (EditorGUI.EndChangeCheck()) {
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}