// /******************************************************************************
//  * File: HandTrackingSimulationEditorSettings.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Editor;

namespace QCHT.Interactions.Core.Editor
{
    [ScriptableSettingsPath(Path)]
    public class HandTrackingSimulationEditorSettings : EditorScriptableSettings<HandTrackingSimulationEditorSettings>
    {
        private const string Path = "Assets/XR/Settings/";
        
        public bool enabled;
        public DataSource dataSource;

        public enum DataSource {
            SimulationSubsystem
        }
    }
}