// /******************************************************************************
//  * File: QCHTHandData.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;

namespace QCHT.Core
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public enum HandType
    {
        Left,
        Right
    }

    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public enum QCHTPointId
    {
        THUMB_PIP = 0,
        THUMB_DIP = 1,
        THUMB_TIP = 2,
        INDEX_MCP = 3,
        INDEX_PIP = 4,
        INDEX_DIP = 5,
        INDEX_TIP = 6,
        MIDDLE_MCP = 7,
        MIDDLE_PIP = 8,
        MIDDLE_DIP = 9,
        MIDDLE_TIP = 10,
        RING_MCP = 11,
        RING_PIP = 12,
        RING_DIP = 13,
        RING_TIP = 14,
        PINKY_MCP = 15,
        PINKY_PIP = 16,
        PINKY_DIP = 17,
        PINKY_TIP = 18,
        PALM_CENTER = 19,
        WRIST_START = 20,
        WRIST_END = 21,
        WRIST_CENTER = 22,
        POINT_COUNT = 23
    }

    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public enum KnuckleId
    {
        BASE = 0,
        PIP = 1,
        DIP = 2,
        TIP = 3
    }

    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public enum FingerId
    {
        THUMB = 0,
        INDEX = 1,
        MIDDLE = 2,
        RING = 3,
        PINKY = 4,
        COUNT = 5
    }

    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public enum GestureId
    {
        UNKNOWN = -1,
        OPEN_HAND = 0,
        GRAB = 2,
        PINCH = 7,
        POINT = 8
    }
}