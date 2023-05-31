// /******************************************************************************
//  * File: HandTrackingSubsystem.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Hands;
using UnityEngine;

namespace QCHT.Interactions.Core
{
    internal class HandTrackingSubsystem : XRHandTrackingSubsystem
    {
        public const string ID = "Qualcomm-HandTrackingSubsystem";

        private class HandTrackingProvider : XRHandTrackingSubsystem.Provider {

            public override void Start() => QCHTOpenXRPlugin.StartHandTracking();

            public override void Stop() => Destroy();

            public override void Destroy()  => QCHTOpenXRPlugin.StopHandTracking();

            private XrPose[] _leftPoses = new XrPose[(int)XrHandJoint.XR_HAND_JOINT_MAX];
            private XrPose[] _rightPoses = new XrPose[(int)XrHandJoint.XR_HAND_JOINT_MAX];
            private float[] _radius = new float[(int) XrHandJoint.XR_HAND_JOINT_MAX];
            public override void GetHandData(bool isLeft, ref bool isTracked, ref Pose[] joints, ref float[] scales, ref int gesture, ref float gestureRatio, ref float flipRatio) {
                ref var poses = ref isLeft ? ref _leftPoses : ref _rightPoses;
                var handedness = isLeft ? QCHTOpenXRPlugin.XrHandSide.XR_HAND_LEFT : QCHTOpenXRPlugin.XrHandSide.XR_HAND_RIGHT;
                QCHTOpenXRPlugin.TryLocateHandJoints(handedness, ref isTracked, poses, _radius, ref gesture, ref gestureRatio, ref flipRatio);
                for (var i = 0; i < (int) XrHandJoint.XR_HAND_JOINT_MAX; i++) {
                    joints[i] = poses[i].ToPose();
                    scales[i] = _radius[i];
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor() {
            XRHandTrackingSubsystemDescriptor.Create(new XRHandTrackingSubsystemDescriptor.Cinfo {
                id = ID,
                providerType = typeof(HandTrackingProvider),
                subsystemTypeOverride = typeof(HandTrackingSubsystem)
            });
        }
    }
}