// /******************************************************************************
//  * File: CanBeEmptyAttribute.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions
{
    /// <summary>
    /// Used on a SerializedField surfaces the expectation that this field can remain empty.
    /// </summary>
    public class CanBeEmptyAttribute : PropertyAttribute
    {
        public CanBeEmptyAttribute()
        {
        }
    }
}