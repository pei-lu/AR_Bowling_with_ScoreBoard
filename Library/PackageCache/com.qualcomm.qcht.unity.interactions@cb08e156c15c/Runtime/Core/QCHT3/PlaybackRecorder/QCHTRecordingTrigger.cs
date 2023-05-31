// /******************************************************************************
//  * File: QCHTRecordingTrigger.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions;
using UnityEngine;

namespace QCHT.Core
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public class QCHTRecordingTrigger : MonoBehaviour
    {
        [SerializeField] private CustomEvent StartRecordingEvent;
        [SerializeField] private CustomEvent StopRecordingEvent;

        private Material _m;
        private bool _isRecording;
        private float _timer;
        private readonly float _lockDelay = 3f;

        private void Start()
        {
            _m = GetComponent<MeshRenderer>().material;
            UpdateRecordingState();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.time > _timer + _lockDelay)
            {
                _timer = Time.time;
                _isRecording = !_isRecording;
                UpdateRecordingState();
            }
        }

        private void UpdateRecordingState()
        {
            if (_isRecording)
            {
                StartRecordingEvent.Invoke();
                _m.color = Color.red;
            }
            else
            {
                StopRecordingEvent.Invoke();
                _m.color = Color.green;
            }
        }
    }
}