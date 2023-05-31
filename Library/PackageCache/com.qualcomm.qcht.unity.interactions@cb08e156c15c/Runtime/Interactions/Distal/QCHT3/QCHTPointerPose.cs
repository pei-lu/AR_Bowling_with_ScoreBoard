// /******************************************************************************
//  * File: QCHTPointerPose.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;

namespace QCHT.Interactions.Distal
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public sealed class QCHTPointerPose : MonoBehaviour
    {
        public enum LaserPosition
        {
            Wrist,
            IndexTip,
            IndexDip,
            MiddleOfIndexTipAndIndexDip,
            MiddleOfIndexTipAndThumbTip,
            MiddleOfIndexTipAndThumbTipAndPalm
        }

        public bool IsLeftHand;
        public LaserPosition Position;

        [SerializeField] private Vector3 positionOffset;
        [SerializeField] private Vector3 rotationOffset;

        [Space]
        [SerializeField] private Transform wristTransform;

        [SerializeField] private Transform indexTipTransform;
        [SerializeField] private Transform indexDipTransform;
        [SerializeField] private Transform thumbTipTransform;
        
        public void LateUpdate()
        {
            var t = transform;
            Vector3 posOffset = wristTransform.localToWorldMatrix * positionOffset;
            t.position = GetHandPoint(Position) + posOffset;
            var shoulderPoint = GetShoulderPosition(0.15f);
            var position = t.position;
            var direction = Vector3.Normalize(position - shoulderPoint);
            t.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(rotationOffset);
        }

        private Vector3 GetShoulderPosition(float offset)
        {
            var head = Camera.main.transform;
            var bottomHead = head.position - head.up * offset;
            var shoulderOffset = IsLeftHand ? -head.right : head.right;
            return bottomHead + shoulderOffset * offset;
        }

        private Vector3 GetHandPoint(LaserPosition laserPosition)
        {
            return laserPosition switch
            {
                LaserPosition.Wrist => wristTransform.position,
                LaserPosition.IndexTip => indexTipTransform.position,
                LaserPosition.IndexDip => indexDipTransform.position,
                LaserPosition.MiddleOfIndexTipAndIndexDip => (indexTipTransform.position + indexDipTransform.position) *
                                                             .5f,
                LaserPosition.MiddleOfIndexTipAndThumbTip => (thumbTipTransform.position + indexTipTransform.position) *
                                                             0.5f,
                LaserPosition.MiddleOfIndexTipAndThumbTipAndPalm => (thumbTipTransform.position +
                                                                     indexTipTransform.position +
                                                                     wristTransform.position) * 0.333f,
                _ => wristTransform.position
            };
        }
    }
}