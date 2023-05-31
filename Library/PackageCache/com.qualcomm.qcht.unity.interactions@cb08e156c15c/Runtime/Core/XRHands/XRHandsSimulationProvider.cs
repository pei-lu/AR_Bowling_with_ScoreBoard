// /******************************************************************************
//  * File: XRHandsSimulationProvider.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

#if XR_HANDS
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace QCHT.Interactions.Core.XRHands
{
    public class XRHandsSimulationProvider : XRHandSubsystemProvider
    {
        public override void Start() {
            Debug.Log("[XRHandsSimulationProvider:Start]");
        }

        public override void Stop() {
            Debug.Log("[XRHandsSimulationProvider:Stop]");
        }

        public override void Destroy() {
            Debug.Log("[XRHandsSimulationProvider:Destroy]");;
        }

        public override void GetHandLayout(NativeArray<bool> handJointsInLayout) { }

        public override XRHandSubsystem.UpdateSuccessFlags TryUpdateHands(XRHandSubsystem.UpdateType updateType,
            ref Pose leftHandRootPose, NativeArray<XRHandJoint> leftHandJoints,
            ref Pose rightHandRootPose, NativeArray<XRHandJoint> rightHandJoints)
        {
            return XRHandSubsystem.UpdateSuccessFlags.None;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor() {
            var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo {
                id = XRHandsSimulationSubsystem.ID,
                providerType = typeof(XRHandsSimulationProvider),
                subsystemTypeOverride = typeof(XRHandsSimulationSubsystem)
            };
            XRHandSubsystemDescriptor.Register(handsSubsystemCinfo);
        }
    }
}
#endif