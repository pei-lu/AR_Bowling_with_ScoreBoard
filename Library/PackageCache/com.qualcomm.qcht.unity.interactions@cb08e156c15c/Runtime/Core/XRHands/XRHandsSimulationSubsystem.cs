// /******************************************************************************
//  * File: XRHandsSimulationSubsystem.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

#if XR_HANDS
using UnityEngine;
using UnityEngine.XR.Hands;

namespace QCHT.Interactions.Core.XRHands
{
    public class XRHandsSimulationSubsystem : XRHandSubsystem
    {
        public const string ID = "Qualcomm-XRHandsSimulationSubsystem";

        protected override void OnCreate() {
            base.OnCreate();
            Debug.Log("[XRHandsSimulationSubsystem:OnCreate]");
        }

        protected override void OnStart() {
            base.OnStart();
            Debug.Log("[XRHandsSimulationSubsystem:OnStart]");
        }

        protected override void OnStop() {
            base.OnStop();
            Debug.Log("[XRHandsSimulationSubsystem:OnStop]");
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            Debug.Log("[XRHandsSimulationSubsystem:OnDestroy]");
        }
    }
}
#endif