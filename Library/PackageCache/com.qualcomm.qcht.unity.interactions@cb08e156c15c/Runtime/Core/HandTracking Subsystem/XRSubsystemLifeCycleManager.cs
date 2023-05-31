// /******************************************************************************
//  * File: XRSubsystemLifeCycleManager.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.Management;

namespace QCHT.Interactions.Core
{
    public class XRSubsystemLifeCycleManager<TSubsystem, TSubsystemDescriptor, TProvider> : MonoBehaviour
        where TSubsystem : SubsystemWithProvider<TSubsystem, TSubsystemDescriptor, TProvider>, new()
        where TSubsystemDescriptor : SubsystemDescriptorWithProvider<TSubsystem, TProvider>
        where TProvider : SubsystemProvider<TSubsystem>
    {
        protected static TSubsystem subsystem { get; set; }
        
        public TSubsystemDescriptor descriptor => subsystem?.subsystemDescriptor;
        
        protected virtual void OnEnable() {
            subsystem ??= GetSubsystemInLoader() ?? GetSubsystemInManager();
            subsystem?.Start();
        }
        
        protected virtual void OnDisable() => subsystem?.Stop();

        protected virtual void OnDestroy() => subsystem = null;

        protected static TSubsystem GetSubsystemInLoader() {
            if (XRGeneralSettings.Instance == null || XRGeneralSettings.Instance.Manager == null) 
                return null;
            
            TSubsystem system = null;
            var loader = XRGeneralSettings.Instance.Manager.activeLoader;
            if (loader != null) 
                system = loader.GetLoadedSubsystem<TSubsystem>();
            
            return system;
        }
        
        protected static TSubsystem GetSubsystemInManager() {
            var subsystems = new List<TSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            return subsystems.FirstOrDefault();
        }
    }
}