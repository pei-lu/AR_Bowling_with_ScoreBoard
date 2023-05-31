// /******************************************************************************
//  * File: HandGhost.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Interactions.Hands
{
    [ExecuteInEditMode]
    public sealed class HandGhost : MonoBehaviour
    {
        private HandPose _handPose;

        public HandPose HandPose
        {
            set
            {
                if (ReferenceEquals(_handPose, value))
                    return;

                _handPose = value;
                UpdatePose();
            }

            get => _handPose;
        }

        // Thumb
        public Transform thumbBase;
        public Transform thumbMiddle;
        public Transform thumbTop;

        // Index
        public Transform indexBase;
        public Transform indexMiddle;
        public Transform indexTop;

        // Middle
        public Transform middleBase;
        public Transform middleMiddle;
        public Transform middleTop;

        // Ring
        public Transform ringBase;
        public Transform ringMiddle;
        public Transform ringTop;

        // Pinky
        public Transform pinkyBase;
        public Transform pinkyMiddle;
        public Transform pinkyTop;

        public void UpdatePose()
        {
            if (!_handPose)
                return;

            if (_handPose.Space == DataSpace.Local)
            {
                // Thumb
                thumbBase.localRotation = _handPose.Thumb.BaseData.Rotation;
                thumbMiddle.localRotation = _handPose.Thumb.MiddleData.Rotation;
                thumbTop.localRotation = _handPose.Thumb.TopData.Rotation;

                // Index
                indexBase.localRotation = _handPose.Index.BaseData.Rotation;
                indexMiddle.localRotation = _handPose.Index.MiddleData.Rotation;
                indexTop.localRotation = _handPose.Index.TopData.Rotation;

                // Middle
                middleBase.localRotation = _handPose.Middle.BaseData.Rotation;
                middleMiddle.localRotation = _handPose.Middle.MiddleData.Rotation;
                middleTop.localRotation = _handPose.Middle.TopData.Rotation;

                // Ring
                ringBase.localRotation = _handPose.Ring.BaseData.Rotation;
                ringMiddle.localRotation = _handPose.Ring.MiddleData.Rotation;
                ringTop.localRotation = _handPose.Ring.TopData.Rotation;

                // Pinky
                pinkyBase.localRotation = _handPose.Pinky.BaseData.Rotation;
                pinkyMiddle.localRotation = _handPose.Pinky.MiddleData.Rotation;
                pinkyTop.localRotation = _handPose.Pinky.TopData.Rotation;
            }
            else
            {
                // Thumb
                thumbBase.rotation = _handPose.Thumb.BaseData.Rotation;
                thumbMiddle.rotation = _handPose.Thumb.MiddleData.Rotation;
                thumbTop.rotation = _handPose.Thumb.TopData.Rotation;

                // Index
                indexBase.rotation = _handPose.Index.BaseData.Rotation;
                indexMiddle.rotation = _handPose.Index.MiddleData.Rotation;
                indexTop.rotation = _handPose.Index.TopData.Rotation;

                // Middle
                middleBase.rotation = _handPose.Middle.BaseData.Rotation;
                middleMiddle.rotation = _handPose.Middle.MiddleData.Rotation;
                middleTop.rotation = _handPose.Middle.TopData.Rotation;

                // Ring
                ringBase.rotation = _handPose.Ring.BaseData.Rotation;
                ringMiddle.rotation = _handPose.Ring.MiddleData.Rotation;
                ringTop.rotation = _handPose.Ring.TopData.Rotation;

                // Pinky
                pinkyBase.rotation = _handPose.Pinky.BaseData.Rotation;
                pinkyMiddle.rotation = _handPose.Pinky.MiddleData.Rotation;
                pinkyTop.rotation = _handPose.Pinky.TopData.Rotation;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HandGhost))]
    public sealed class GhostEditor : UnityEditor.Editor
    {
        public HandPose Pose;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            var pose = EditorGUILayout.ObjectField("Test Hand pose", Pose, typeof(HandPose), false) as HandPose;
            EditorGUILayout.EndHorizontal();

            if (!Equals(pose, Pose))
            {
                var ghost = target as HandGhost;

                if (!ghost)
                    return;

                ghost.HandPose = pose;
            }

            DrawDefaultInspector();
        }
    }
#endif
}