/******************************************************************************
 * File: BaseRuntimeFeature.InterceptWaitFrame.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using AOT;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    public partial class BaseRuntimeFeature
    {
        private static long _predictedDisplayTime = 0;
        public long PredictedDisplayTime => _predictedDisplayTime;

        internal delegate void ReceiveFrameStateCallback(XrFrameState frameState);

        [DllImport(InterceptOpenXRLibrary, EntryPoint = "SetFrameStateCallback")]
        private static extern void SetFrameStateCallback(ReceiveFrameStateCallback callback);

        [MonoPInvokeCallback(typeof(ReceiveFrameStateCallback))]
        private static void OnFrameStateUpdate(XrFrameState frameState) {
            _predictedDisplayTime = frameState.PredictedDisplayTime;
        }
    }
}