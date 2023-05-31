// /******************************************************************************
//  * File: HandPoseMask.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Hands
{
    [CreateAssetMenu(menuName = "QCHT/Interactions/HandPoseMask")]
    public sealed class HandPoseMask : ScriptableObject
    {
        public enum MaskState
        {
            Required,
            Free
        }

        public MaskState Thumb;
        public MaskState Index;
        public MaskState Middle;
        public MaskState Ring;
        public MaskState Pinky;
    }
}