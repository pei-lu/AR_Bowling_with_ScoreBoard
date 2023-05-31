// /******************************************************************************
//  * File: HandPoseDriver.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.XR;

namespace QCHT.Interactions.Hands
{
    public sealed class HandPoseDriver : MonoBehaviour
    {
        [FormerlySerializedAs("HandModel"), SerializeField, Interface(typeof(IHandProvider))]
        private MonoBehaviour poseProvider;

        private IHandProvider _poseProvider;

        public IHandProvider Provider => _poseProvider;

        private HandPose _handPose;

        public HandPose HandPose
        {
            set => _handPose = value ? value : _poseProvider.HandPose;
            get => _handPose;
        }

        private HandPoseMask _handPoseMask;

        public HandPoseMask HandPoseMask
        {
            set => _handPoseMask = value;
            get => _handPoseMask;
        }

        [FormerlySerializedAs("palmPart")]
        [Header("Joints")]
        [SerializeField] private HandJointUpdater rootPart;

        [Space]

        // Thumb
        [SerializeField] private HandJointUpdater thumbBase;
        [SerializeField] private HandJointUpdater thumbMiddle;
        [SerializeField] private HandJointUpdater thumbTop;

        [Space]

        // Index
        [SerializeField] private HandJointUpdater indexBase;
        [SerializeField] private HandJointUpdater indexMiddle;
        [SerializeField] private HandJointUpdater indexTop;

        [Space]

        // Middle
        [SerializeField] private HandJointUpdater middleBase;
        [SerializeField] private HandJointUpdater middleMiddle;
        [SerializeField] private HandJointUpdater middleTop;

        [Space]

        // Ring
        [SerializeField] private HandJointUpdater ringBase;
        [SerializeField] private HandJointUpdater ringMiddle;
        [SerializeField] private HandJointUpdater ringTop;

        [Space]

        // Pinky
        [SerializeField] private HandJointUpdater pinkyBase;
        [SerializeField] private HandJointUpdater pinkyMiddle;
        [SerializeField] private HandJointUpdater pinkyTop;

        [Space]
        
        [SerializeField] private Transform scale;

        public Transform RootTransform => rootPart.transform;

        private float _scaleMultiplier = 1f;

        public float ScaleMultiplier
        {
            get => _scaleMultiplier;
            set => _scaleMultiplier = value;
        }

        #region MonoBehaviour Functions

        public void Awake()
        {
            _poseProvider = poseProvider as IHandProvider;
            
            if(poseProvider != null)
                Assert.IsNotNull(_poseProvider);
        }

        private void Start() => _handPose = _poseProvider.HandPose;
        
        private void LateUpdate()
        {
            // Update hand scale 
            if (scale)
                scale.localScale = (HandPose ? HandPose.Scale : _poseProvider.HandPose.Scale) * _scaleMultiplier;

            rootPart.UpdateJoint(HandPose.Space, _poseProvider.HandPose.Root);
            UpdateFingerJoints(XrFinger.XR_HAND_THUMB);
            UpdateFingerJoints(XrFinger.XR_HAND_INDEX);
            UpdateFingerJoints(XrFinger.XR_HAND_MIDDLE);
            UpdateFingerJoints(XrFinger.XR_HAND_RING);
            UpdateFingerJoints(XrFinger.XR_HAND_PINKY);
        }

        #endregion

        private void UpdateFingerJoints(XrFinger id)
        {
            var maskState = GetMaskStateForFinger(id);
            var handPose = maskState == HandPoseMask.MaskState.Required ? HandPose : _poseProvider.HandPose;

            if (handPose == null)
                return;
            
            var (bottom, middle, top) = GetJointsOfAFinger(id);
            var fingerData = GetFingerDataFromPose(id, handPose);

            bottom?.UpdateJoint(handPose.Space, fingerData.BaseData);
            middle?.UpdateJoint(handPose.Space, fingerData.MiddleData);
            top?.UpdateJoint(handPose.Space, fingerData.TopData);
        }

        private HandPoseMask.MaskState GetMaskStateForFinger(XrFinger id)
        {
            if (HandPoseMask == null)
                return HandPoseMask.MaskState.Required;

            return id switch
            {
                XrFinger.XR_HAND_THUMB => HandPoseMask.Thumb,
                XrFinger.XR_HAND_INDEX => HandPoseMask.Index,
                XrFinger.XR_HAND_MIDDLE => HandPoseMask.Middle,
                XrFinger.XR_HAND_RING => HandPoseMask.Ring,
                XrFinger.XR_HAND_PINKY => HandPoseMask.Pinky,
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };
        }

        private FingerData GetFingerDataFromPose(XrFinger id, HandPose pose)
        {
            return id switch
            {
                XrFinger.XR_HAND_THUMB => pose.Thumb,
                XrFinger.XR_HAND_INDEX => pose.Index,
                XrFinger.XR_HAND_MIDDLE => pose.Middle,
                XrFinger.XR_HAND_RING => pose.Ring,
                XrFinger.XR_HAND_PINKY => pose.Pinky,
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };
        }

        private (IHandJointUpdater, IHandJointUpdater, IHandJointUpdater) GetJointsOfAFinger(XrFinger id)
        {
            return id switch
            {
                XrFinger.XR_HAND_THUMB => (thumbBase, thumbMiddle, thumbTop),
                XrFinger.XR_HAND_INDEX => (indexBase, indexMiddle, indexTop),
                XrFinger.XR_HAND_MIDDLE => (middleBase, middleMiddle, middleTop),
                XrFinger.XR_HAND_RING => (ringBase, ringMiddle, ringTop),
                XrFinger.XR_HAND_PINKY => (pinkyBase, pinkyMiddle, pinkyTop),
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"WRIST : [ {rootPart} ]\n");
            sb.Append($"THUMB PROXIMAL : [ {thumbMiddle} ]\n");
            sb.Append($"THUMB DISTAL : [ {thumbTop} ]\n");
            sb.Append($"INDEX PROXIMAL : [ {indexBase} ]\n");
            sb.Append($"INDEX INTERMEDIATE : [ {indexMiddle} ]\n");
            sb.Append($"INDEX DISTAL : [ {indexTop} ]\n");
            sb.Append($"MIDDLE PROXIMAL : [ {middleBase} ]\n");
            sb.Append($"MIDDLE INTERMEDIATE : [ {middleMiddle} ]\n");
            sb.Append($"MIDDLE DISTAL : [ {middleTop} ]\n");
            sb.Append($"RING PROXIMAL : [ {ringBase} ]\n");
            sb.Append($"RING INTERMEDIATE : [ {ringMiddle} ]\n");
            sb.Append($"RING DISTAL : [ {ringTop} ]\n");
            sb.Append($"PINKY PROXIMAL : [ {pinkyBase} ]\n");
            sb.Append($"PINKY INTERMEDIATE : [ {pinkyMiddle} ]\n");
            sb.Append($"PINKY DISTAL : [ {pinkyTop} ]\n");
            return sb.ToString();
        }
    }
}