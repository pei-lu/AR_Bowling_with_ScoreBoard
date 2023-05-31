﻿/******************************************************************************
 * File: XrSceneComponentLocationMSFT.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XrSceneComponentLocationMSFT
    {
        private XrSpaceLocationFlags _flags;
        private XrPosef _pose;
    }
}