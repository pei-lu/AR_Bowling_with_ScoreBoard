// /******************************************************************************
//  * File: HandTrackingFeature.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

#if XR_OPENXR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace QCHT.Interactions.Core
{
#if UNITY_EDITOR
    [OpenXRFeature(
        FeatureId = FeatureId,
        UiName = FeatureName,
        Desc = FeatureDescription,
        Company = "Qualcomm",
        BuildTargetGroups = new []{ BuildTargetGroup.Android },
#if !SPACES
        CustomRuntimeLoaderBuildTargets = new []{ BuildTarget.Android },
#endif
        DocumentationLink = "",
        OpenxrExtensionStrings = "",
        Version = "0.11.0",
        Required = false,
        Category = FeatureCategory.Feature)]
#endif
    public sealed class HandTrackingFeature : OpenXRFeature
    {
#if SPACES
        public const string FeatureId = "com.qualcomm.snapdragon.spaces.handtracking";
        public const string FeatureName = "Hand Tracking";
        public const string FeatureDescription = "Enables Hand Tracking feature on Snapdragon Spaces enabled devices";
#else
        public const string FeatureId = "com.qualcomm.snapdragon.handtracking";
        public const string FeatureName = "Qualcomm Hand Tracking";
        public const string FeatureDescription = "Enables Hand Tracking and gestures feature on Snapdragon enabled devices.";
#endif
        // public bool InitOnLoad;

        private static readonly List<XRHandTrackingSubsystemDescriptor> s_handTrackingSubsystemDescriptors = new List<XRHandTrackingSubsystemDescriptor>();
        protected override bool OnInstanceCreate(ulong xrInstance) {
            return base.OnInstanceCreate(xrInstance);
            
#if UNITY_ANDROID && !UNITY_EDITOR
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            var runtimeChecker = new AndroidJavaClass("com.qualcomm.snapdragon.spaces.unityserviceshelper.RuntimeChecker");

            if ( !runtimeChecker.CallStatic<bool>("CheckCameraPermissions", new object[] { activity }) ) {
                Debug.LogError("Snapdragon Spaces Services has no camera permissions! Hand Tracking feature disabled.");
                return false;
            }
#endif
        }

        protected override IntPtr HookGetInstanceProcAddr(IntPtr func) => QCHTOpenXRPlugin.GetInterceptedInstanceProcAddr(func);
        protected override void OnSubsystemCreate() => CreateSubsystem<XRHandTrackingSubsystemDescriptor, XRHandTrackingSubsystem>(s_handTrackingSubsystemDescriptors, HandTrackingSubsystem.ID);
        protected override void OnSubsystemStop() => StopSubsystem<XRHandTrackingSubsystem>();
        protected override void OnSubsystemDestroy() => DestroySubsystem<XRHandTrackingSubsystem>();
    }
}
#endif
