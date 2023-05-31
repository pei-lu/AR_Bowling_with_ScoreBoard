// /******************************************************************************
//  * File: QCHTHandLabel.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace QCHT.Interactions.Hands
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public class QCHTHandLabel : MonoBehaviour
    {
        public Text label;

        public void UpdateLabel(string text)
        {
            label.text = text;
        }

        public void ToggleDisplay(bool on)
        {
            label.enabled = on;
        }
    } 
}