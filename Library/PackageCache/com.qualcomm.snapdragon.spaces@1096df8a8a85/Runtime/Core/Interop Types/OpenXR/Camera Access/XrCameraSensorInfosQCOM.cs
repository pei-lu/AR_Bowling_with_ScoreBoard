/****************************************************************
 * File: XrCameraSensorInfosQCOM.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ****************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrCameraSensorInfosQCOM
    {
        private XrStructureType _type;
        private IntPtr _next;
        private uint _sensorPropertyCount;
        private IntPtr /* XrCameraSensorPropertiesQCOM[] */ _sensorProperties;

        public XrCameraSensorInfosQCOM(uint sensorPropertyCount, IntPtr /* XrCameraSensorPropertiesQCOM[] */ sensorProperties) {
            _type = XrStructureType.XR_TYPE_CAMERA_SENSOR_INFOS_QCOMX;
            _next = IntPtr.Zero;
            _sensorPropertyCount = sensorPropertyCount;
            _sensorProperties = sensorProperties;
        }

        public override string ToString() {
	        return String.Join("\n",
		        "[XrCameraSensorInfosQCOM]",
		        $"Type:\t{_type}",
		        $"Next:\t{_next}",
		        $"SensorPropertyCount:\t{_sensorPropertyCount}",
		        $"SensorProperties:\t{_sensorProperties}"
	        );
        }
    }
}
