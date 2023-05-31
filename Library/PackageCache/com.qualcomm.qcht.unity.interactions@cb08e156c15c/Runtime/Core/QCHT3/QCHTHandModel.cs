// /******************************************************************************
//  * File: QCHTHandModel.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Core.Extensions;
using UnityEngine;
using QCHT.Interactions.Hands;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Core.Hands
{
    /// <summary>
    /// Converts QCHT hand input data to Hand pose data and update bones transforms.
    /// </summary>
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public sealed class QCHTHandModel : MonoBehaviour, IHandProvider
    {
        // From Thumb mcp used in QCHTSDK
        private static readonly Vector3 s_leftThumpMcp = new Vector3(0.0270322636f, 0.00485388096f, 0.00566395884f);
        private static readonly Vector3 s_rightThumpMcp = new Vector3(-0.0270322636f, 0.00485388096f, 0.00566395884f);

        [SerializeField]
        public HandType handType;

        public bool IsTracked { get; private set; }
        public Pose Pose { get; private set; }
        public XrHandedness HandType => (XrHandedness) handType;
        public HandPose HandPose { get; private set; }
        public XrHandGesture GestureId { get; private set; }
        public float GestureRatio { get; private set; }
        public float FlipRatio { get; private set; }

        #region MonoBehaviour Functions

        private void Awake()
        {
            HandPose = InstantiateHandPose();
        }
        
        private void Update()
        {
            var hand = handType == QCHT.Core.HandType.Left ? QCHTSDK.Instance.Data.LeftHand : QCHTSDK.Instance.Data.RightHand;
            IsTracked = hand.IsDetected;
            Pose = new Pose(hand.GetWristPosition(), hand.GetWristRotation());
            GestureId = (XrHandGesture)hand.gesture;
            GestureRatio = hand.gestureRatio;
            FlipRatio = hand.flipRatio;
            UpdateHandPose(hand);
        }

        #endregion

        private HandPose InstantiateHandPose()
        {
            var handPose = ScriptableObject.CreateInstance<HandPose>();
            handPose.Type = (XrHandedness)handType;
            return handPose;
        }

        private void UpdateHandPose(QCHTHand hand)
        {
            HandPose.Space = hand.DataSpace;
            
            var rootPosition = hand.GetWristPosition();
            var rootRotation = QCHTManager.Instance.Head. rotation * hand.GetWristRotation();

            UpdateBoneData(ref HandPose.Root, rootPosition, rootRotation);
            UpdateFingerData(hand, ref HandPose.Thumb, FingerId.THUMB);
            UpdateFingerData(hand, ref HandPose.Index, FingerId.INDEX);
            UpdateFingerData(hand, ref HandPose.Middle, FingerId.MIDDLE);
            UpdateFingerData(hand, ref HandPose.Ring, FingerId.RING);
            UpdateFingerData(hand, ref HandPose.Pinky, FingerId.PINKY);

            HandPose.Scale = Vector3.one * hand.scale;
        }

        private static void UpdateFingerData(QCHTHand hand, ref FingerData fingerData, FingerId fingerId)
        {
            var joints = QCHTFinger.GetFingerJointsId(fingerId);
            var rot = hand.rotations;
            var pos = hand.points;

            if (fingerId == FingerId.THUMB)
            {
                var thumbMcp = hand.IsLeft ? s_leftThumpMcp : s_rightThumpMcp;
                UpdateBoneData(ref fingerData.BaseData, thumbMcp, rot[(int) joints[0]]);
                UpdateBoneData(ref fingerData.MiddleData, pos[(int) joints[0]], rot[(int) joints[1]]);
                UpdateBoneData(ref fingerData.TopData, pos[(int) joints[1]], rot[(int) joints[2]]);
            }
            else
            {
                UpdateBoneData(ref fingerData.BaseData, pos[(int) joints[0]], rot[(int) joints[0]] * rot[(int) joints[1]]);
                UpdateBoneData(ref fingerData.MiddleData, pos[(int) joints[1]], rot[(int) joints[2]]);
                UpdateBoneData(ref fingerData.TopData, pos[(int) joints[2]], rot[(int) joints[3]]);
            }
        }

        private static void UpdateBoneData(ref BoneData boneData, Vector3 position, Quaternion rotation)
        { 
            boneData.Position = position;
            boneData.Rotation = rotation;
        }
    }

#if UNITY_EDITOR
    [Obsolete]
    [CustomEditor(typeof(QCHTHandModel))]
    public class HandModelEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // if (!Application.isPlaying)
            //     return;
            //
            // if (GUILayout.Button("Save current model as pose asset"))
            // { 
            //     // TODO: Implement saving hand model as pose asset
            // }
        }
    }
#endif
}