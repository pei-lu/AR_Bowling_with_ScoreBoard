/******************************************************************************
 * File: SceneUnderstandingFeature.cs
 * Copyright (c) 2022-2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Qualcomm.Snapdragon.Spaces
{
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = FeatureName,
        BuildTargetGroups = new []{ BuildTargetGroup.Android },
        Company = "Qualcomm",
        Desc = "Enables Spatial Meshing feature on Snapdragon Spaces enabled devices",
        DocumentationLink = "",
        OpenxrExtensionStrings = FeatureExtensions,
        Version = "0.13.0",
        Required = false,
        Category = FeatureCategory.Feature,
        FeatureId = FeatureID)]
#endif
    public sealed partial class SpatialMeshingFeature : SpacesOpenXRFeature
    {
        public const string FeatureName = "Spatial Meshing (Experimental)";
        public const string FeatureID = "com.qualcomm.snapdragon.spaces.sceneunderstanding";
        public const string FeatureExtensions = "XR_MSFT_scene_understanding";

        protected override bool IsRequiringBaseRuntimeFeature => true;

        private BaseRuntimeFeature _baseRuntimeFeature;
        private static List<XRMeshSubsystemDescriptor> _meshSubsystemDescriptors = new List<XRMeshSubsystemDescriptor>();

        protected override string GetXrLayersToLoad() {
            return "XR_APILAYER_QTI_scene_understanding";
        }
        
        protected override IntPtr HookGetInstanceProcAddr(IntPtr func) {
            RequestLayers(GetXrLayersToLoad());
            return Internal_GetInterceptedInstanceProcAddr(func);
        }

        protected override bool OnInstanceCreate(ulong instanceHandle) {
            base.OnInstanceCreate(instanceHandle);
            Internal_RegisterMeshingLifecycleProvider();
            Internal_SetInstanceHandle(instanceHandle);

            _baseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();

            var missingExtensions = GetMissingExtensions(FeatureExtensions);
            if (missingExtensions.Any()) {
                Debug.Log(FeatureName + " is missing following extension in the runtime: " + String.Join(",", missingExtensions));
                return false;
            }
            return true;
        }

        protected override void OnSubsystemCreate() => CreateSubsystem<XRMeshSubsystemDescriptor, XRMeshSubsystem>(_meshSubsystemDescriptors, "Spaces-MeshSubsystem");

        protected override void OnSubsystemStop() => StopSubsystem<XRMeshSubsystem>();

        protected override void OnSubsystemDestroy() => DestroySubsystem<XRMeshSubsystem>();

        protected override void OnSessionCreate(ulong sessionHandle) {
            base.OnSessionCreate(sessionHandle);
            Internal_SetSessionHandle(sessionHandle);
        }

        protected override void OnAppSpaceChange(ulong spaceHandle) {
            base.OnAppSpaceChange(spaceHandle);
            Internal_SetSpaceHandle(spaceHandle);
        }
    }
}