// /******************************************************************************
//  * File: QCHTHeadProvider.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;

namespace QCHT.Core
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public sealed class QCHTHeadProvider : MonoBehaviour
    {
        [SerializeField] private Transform head;

        public Transform Head => head == null ? transform : head;
    }
}