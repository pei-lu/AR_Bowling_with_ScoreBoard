// /******************************************************************************
//  * File: QCHTData.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine;
using QCHT.Core.Extensions;
using QCHT.Interactions.Hands;

namespace QCHT.Core
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public sealed class QCHTData
    {
        public QCHTHand LeftHand { get; } = new QCHTHand {IsLeft = true};
        public QCHTHand RightHand { get; } = new QCHTHand {IsLeft = false};

        public void Update(byte[] buffer, int size, int state, int handCount)
        {
            this.LoadBytesToData(buffer, size, state, handCount);
        }

        public void Update(float[] data, int size)
        {
            this.LoadFloatsToData(data, size);
        }

        public QCHTHand GetHand(bool isLeft)
        {
            return isLeft ? LeftHand : RightHand;
        }
    }

    public sealed class QCHTHand
    {
        // Unity Side Data
        public bool IsDetected;
        public DataSpace DataSpace;

        // QCHT SDK Data
        public float handId;
        public bool IsLeft;
        public readonly Vector3[] points = new Vector3[(int) QCHTPointId.POINT_COUNT]; // In camera space
        public readonly Quaternion[] rotations = new Quaternion[(int) QCHTPointId.POINT_COUNT];
        public GestureId gesture = GestureId.UNKNOWN;
        public float gestureRatio = 0f;
        public float scale = 1f;
        public float flipRatio = 0f;

        public QCHTHand()
        {
            for (var i = 0; i < points.Length; i++)
                points[i] = Vector3.zero;

            for (var i = 0; i < rotations.Length; i++)
                rotations[i] = Quaternion.identity;
        }

        public void Clear()
        {
            IsDetected = false;
            gesture = GestureId.UNKNOWN;
        }
    }
}