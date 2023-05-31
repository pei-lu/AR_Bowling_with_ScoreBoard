// /******************************************************************************
//  * File: QCHTHandAudioClips.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;

namespace QCHT.Interactions.Hands
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    [CreateAssetMenu(menuName = "QCHT/Audio/HandFeedbackClips", order = 1)]
    public sealed class QCHTHandAudioClips : ScriptableObject
    {
        public AudioClip Select;
        public AudioClip Unselect;
        public AudioClip Reset;
    }
}
