// /******************************************************************************
//  * File: QCHTChronoDriver.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Core.PlaybackRecorder;
using UnityEngine.UI;
using UnityEngine;

namespace QCHT.Core
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public sealed class QCHTChronoDriver : MonoBehaviour
    {
        [SerializeField] private Text chronoLabel;
        [SerializeField] private QCHTRecorderManager QCHTRecorderManager;
        
        private void Update()
        {
            chronoLabel.text = FormatTime(QCHTRecorderManager.RecordTime);
        }

        private static string FormatTime(float timeToDisplay)
        {
            var minutes = Mathf.FloorToInt(timeToDisplay / 60);
            var seconds = Mathf.FloorToInt(timeToDisplay % 60);
            var milliSeconds = timeToDisplay % 1 * 1000;
            return $"{minutes:00} : {seconds:00} : {milliSeconds:000}";
        }
    }
}