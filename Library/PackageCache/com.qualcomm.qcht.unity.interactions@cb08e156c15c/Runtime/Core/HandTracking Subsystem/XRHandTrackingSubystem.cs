// /******************************************************************************
//  * File: XRHandTrackingSubystem.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using UnityEngine.SubsystemsImplementation;

namespace QCHT.Interactions.Core
{
    public class XRHandTrackingSubsystem : SubsystemWithProvider<XRHandTrackingSubsystem, XRHandTrackingSubsystemDescriptor, XRHandTrackingSubsystem.Provider>
    {
        public void GetHandData(bool isLeft, ref bool isTracked, ref Pose[] joints, ref float[] scales, ref int gesture, ref float gestureRatio, ref float flipRatio)
            => provider.GetHandData(isLeft, ref isTracked, ref joints, ref scales, ref gesture, ref gestureRatio, ref flipRatio);

        public abstract class Provider : SubsystemProvider<XRHandTrackingSubsystem>
        {
            public abstract void GetHandData(bool isLeft, ref bool isTracked, ref Pose[] joints, ref float[] scales, ref int gesture, ref float gestureRatio, ref float flipRatio);
            public override void Start() { }
            public override void Stop() { }
            public override void Destroy() { }
        }
    }
}