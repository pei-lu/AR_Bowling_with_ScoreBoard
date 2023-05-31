// /******************************************************************************
//  * File: XrPose.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Runtime.InteropServices;
using UnityEngine;

namespace QCHT.Interactions.Core
{
    
    [StructLayout(LayoutKind.Sequential)]
    public struct XrPose
    {
        private XrQuaternion _orientation;
        private XrVector3 _position;

        public XrPose(Pose pose) {
            _orientation = new XrQuaternion(pose.rotation);
            _position = new XrVector3(pose.position);
        }

        public XrPose(XrQuaternion orientation, XrVector3 position) {
            _orientation = orientation;
            _position = position;
        }

        public Pose ToPose() {
            return new Pose(_position.ToVector3(), _orientation.ToQuaternion());
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct XrQuaternion
    {
        private float _x;
        private float _y;
        private float _z;
        private float _w;

        public XrQuaternion(Quaternion quaternion) {
            _x = quaternion.x;
            _y = quaternion.y;
            _z = -quaternion.z;
            _w = -quaternion.w;
        }

        public static XrQuaternion identity => new XrQuaternion(new Quaternion(0, 0, -0, -1));

        public Quaternion ToQuaternion() {
            return new Quaternion(_x, _y, -_z, -_w);
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct XrVector3
    {
        private float _x;
        private float _y;
        private float _z;

        public XrVector3(Vector3 position) {
            _x = position.x;
            _y = position.y;
            _z = -position.z;
        }

        public static XrVector3 zero => new XrVector3(new Vector3(0, 0, -0));

        public Vector3 ToVector3() {
            return new Vector3(_x, _y, -_z);
        }
    }
}