// /******************************************************************************
//  * File: QCHTOpenXRPlugin.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using QCHT.Interactions.Hands;

namespace QCHT.Interactions.Core
{
    internal class QCHTOpenXRPlugin
    {
        public enum XrHandSide
        {
            XR_HAND_LEFT = 1,
            XR_HAND_RIGHT = 2,
            XR_HAND_MAX_ENUM = 0x7FFFFFFF
        }
        
        private const string DllName = "QCHTOpenXRPlugin";

        [DllImport(DllName, EntryPoint = "GetInterceptedInstanceProcAddr")]
        internal static extern IntPtr GetInterceptedInstanceProcAddr(IntPtr func);

        [DllImport(DllName, EntryPoint = "StartHandTracking")]
        internal static extern int StartHandTracking();

        [DllImport(DllName, EntryPoint = "StopHandTracking")]
        internal static extern int StopHandTracking();

        [DllImport(DllName, EntryPoint = "TryLocateHandJoints")]
        internal static extern int TryLocateHandJoints(XrHandSide handedness,
                                                            ref bool isTracked,
                                                            [MarshalAs(UnmanagedType.LPArray, SizeConst = (int)XrHandJoint.XR_HAND_JOINT_MAX)] XrPose[] handPoses,
                                                            [MarshalAs(UnmanagedType.LPArray, SizeConst = (int)XrHandJoint.XR_HAND_JOINT_MAX)] float[] radius,
                                                            ref int gesture,
                                                            ref float gestureRatio,
                                                            ref float flipRatio);
    }
}