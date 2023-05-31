// /******************************************************************************
//  * File: SnapData.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions.Hands;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace QCHT.Interactions.Proximal
{
    [Serializable]
    public class SnapData
    {
        public HandPose HandPose;
        public HandPoseMask HandPoseMask;
        public Vector3 LocalOffsetPosition;
        public Vector3 LocalOffsetRotation;

#if UNITY_EDITOR
        public static SnapData DeserializeSnapData(SerializedProperty property)
        {
            var handPoseProperty = property.FindPropertyRelative("HandPose");
            var handMaskProperty = property.FindPropertyRelative("HandPoseMask");
            var offsetPositionProperty = property.FindPropertyRelative("LocalOffsetPosition");
            var offsetRotationProperty = property.FindPropertyRelative("LocalOffsetRotation");
            return new SnapData
            {
                HandPose = handPoseProperty.objectReferenceValue as HandPose,
                HandPoseMask = handMaskProperty.objectReferenceValue as HandPoseMask,
                LocalOffsetPosition = offsetPositionProperty.vector3Value,
                LocalOffsetRotation = offsetRotationProperty.vector3Value
            };
        }

        public static void SerializeSnapData(SerializedProperty property, SnapData snapData)
        {
            var handPoseProperty = property.FindPropertyRelative("HandPose");
            var handMaskProperty = property.FindPropertyRelative("HandPoseMask");
            var offsetPositionProperty = property.FindPropertyRelative("LocalOffsetPosition");
            var offsetRotationProperty = property.FindPropertyRelative("LocalOffsetRotation");
            handPoseProperty.objectReferenceValue = snapData.HandPose;
            handMaskProperty.objectReferenceValue = snapData.HandPoseMask;
            offsetPositionProperty.vector3Value = snapData.LocalOffsetPosition;
            offsetRotationProperty.vector3Value = snapData.LocalOffsetRotation;
            property.serializedObject.ApplyModifiedProperties();
        }
#endif
    }
    
    public class InteractionData : BaseEventData
    {
        public IInteractionController Controller;
        public GameObject ProximalEnter;
        public GameObject ProximalGrab;
        public GameObject ProximalDoubleGrab;
        public IInteractionDataProvider Data;

        public InteractionData(EventSystem eventSystem) : base(eventSystem)
        {
        }
    }

    public class DoubleInteractionData : BaseEventData
    {
        public InteractionData LeftData;
        public InteractionData RightData;

        public DoubleInteractionData(EventSystem eventSystem) : base(eventSystem)
        {
        }
    }

    public struct InteractionResult
    {
        public GameObject currentGameObject;
    }
}