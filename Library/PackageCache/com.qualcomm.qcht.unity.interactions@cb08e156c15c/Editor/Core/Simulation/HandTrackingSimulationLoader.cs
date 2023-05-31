// /******************************************************************************
//  * File: HandTrackingSimulationLoader.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace QCHT.Interactions.Core.Editor
{
    public static class HandTrackingSimulationLoader
    {
        private static readonly ISubsystem s_subsystem;
        private static readonly List<XRHandTrackingSubsystemDescriptor> s_handTrackingSubsystemDescriptors = new List<XRHandTrackingSubsystemDescriptor>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration), Preserve]
        public static void Initialize() {
            // Will execute the static constructor as a side effect.
        }

        static HandTrackingSimulationLoader() {
            if (!Application.isEditor || !HandTrackingSimulationEditorSettings.Instance.enabled)
                return;
            
            if(HandTrackingSimulationEditorSettings.Instance.dataSource == HandTrackingSimulationEditorSettings.DataSource.SimulationSubsystem)
                s_subsystem = CreateSubsystem<XRHandTrackingSubsystemDescriptor, HandTrackingSimulationSubsystem>(s_handTrackingSubsystemDescriptors, HandTrackingSimulationSubsystem.ID);
            
            s_subsystem?.Start();
        }

        private static ISubsystem CreateSubsystem<TDescriptor, TSubsystem>(List<TDescriptor> descriptors, string id)
            where TDescriptor : ISubsystemDescriptor
            where TSubsystem : ISubsystem
        {
            if (descriptors == null)
                throw new ArgumentNullException(nameof(descriptors));

            SubsystemManager.GetSubsystemDescriptors<TDescriptor>(descriptors);

            if (descriptors.Count > 0) {
                foreach (var descriptor in descriptors) {
                    if (string.Compare(descriptor.id, id, StringComparison.OrdinalIgnoreCase) == 0) {
                        return descriptor.Create();
                    }
                }
            }

            return null;
        }
    }
}