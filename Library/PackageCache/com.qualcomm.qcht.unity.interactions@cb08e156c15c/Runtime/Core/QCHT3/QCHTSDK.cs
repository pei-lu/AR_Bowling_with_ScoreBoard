// /******************************************************************************
//  * File: QCHTSDK.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using QCHT.Interactions.Hands;
using UnityEngine;

namespace QCHT.Core
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public class QCHTSDK
    {
        #region Instance

        private static QCHTSDK s_instance;

        public static QCHTSDK Instance
        {
            get
            {
                if (s_instance != null)
                    return s_instance;

                s_instance = Application.isEditor ? (QCHTSDK) new QCHTSDKEditor() : new QCHTSDKAndroid();

                return s_instance;
            }

            set => s_instance = value;
        }

        #endregion

        private const int StructSize = sizeof(int) + // Hand ID
                                       sizeof(int) + // Is Left
                                       sizeof(float) * 7 * 23 + // Points and Rotations
                                       sizeof(int) + // Gesture
                                       sizeof(float) + // Gesture Ratio
                                       sizeof(float) + // Scale
                                       sizeof(float); // Flip Ratio

        private static readonly byte[] s_buffer = new byte[StructSize * 2];

        public static QCHTSimulationMode simulationMode;

        protected readonly QCHTData _data = new QCHTData();
        public QCHTData Data => _data;

        public bool NeedDelay;

        public virtual void StartQcht()
        {
            Data.LeftHand.DataSpace = DataSpace.Local;
            Data.RightHand.DataSpace = DataSpace.Local;
        }

        public virtual void StopQcht()
        {
        }

        public virtual void UpdateData()
        {
            var sdkStatus = ClaySDKGetStatus();
            if (sdkStatus < 0) return;

            var handCount = 0;
            var dataPtr = IntPtr.Zero;
            var success = ClaySDKGetData(ref dataPtr, ref handCount);
            if (!success) return;

            Marshal.Copy(dataPtr, s_buffer, 0, StructSize * handCount);
            _data.Update(s_buffer, StructSize, sdkStatus, handCount);
        }

        [DllImport("claysdk")]
        private static extern bool ClaySDKGetData(ref IntPtr outData, ref int outSize);

        [DllImport("claysdk")]
        private static extern int ClaySDKGetStatus();
    }
}