// /******************************************************************************
//  * File: IHeadProvider.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Core
{
    public interface IHeadProvider
    {
        public Transform Head { get; }
    }
}