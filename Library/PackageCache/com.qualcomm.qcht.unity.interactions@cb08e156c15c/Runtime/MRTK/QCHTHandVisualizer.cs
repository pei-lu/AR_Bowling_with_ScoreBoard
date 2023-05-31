// /******************************************************************************
//  * File: QCHTHandVisualizer.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

#if UNITY_MRTK_3
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Subsystems;

namespace QCHTHandVisualiser
{
    public class QCHTHandVisualizer : MonoBehaviour
    {
        [SerializeField] private XRNode handNode = XRNode.LeftHand; 
        
        [SerializeField]
        [Tooltip("Renderer of the hand mesh")]
        private SkinnedMeshRenderer handRenderer = null;
        [SerializeField] private Transform _handRoot;
        [SerializeField] private Transform _thumb001;
        [SerializeField] private Transform _thumb002;
        [SerializeField] private Transform _thumb003;
        [SerializeField] private Transform _index001;
        [SerializeField] private Transform _index002;
        [SerializeField] private Transform _index003;
        [SerializeField] private Transform _middle001;
        [SerializeField] private Transform _middle002;
        [SerializeField] private Transform _middle003;
        [SerializeField] private Transform _ring001;
        [SerializeField] private Transform _ring002;
        [SerializeField] private Transform _ring003;
        [SerializeField] private Transform _pinky001;
        [SerializeField] private Transform _pinky002;
        [SerializeField] private Transform _pinky003;

        private readonly Transform[] _riggedVisualJointsArray = new Transform[(int)TrackedHandJoint.TotalJoints];
        private HandJointPose[] _convertedHandPose = new HandJointPose[(int)TrackedHandJoint.TotalJoints];
        private HandsAggregatorSubsystem _handsSubsystem;
        
        
        void Start()
        {
            LoadNodes();
        }

        
        protected void OnEnable()
        {
            handRenderer.enabled = false;
            Debug.Assert(handNode == XRNode.LeftHand || handNode == XRNode.RightHand,
                $"HandVisualizer has an invalid XRNode ({handNode})!");

            _handsSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();

            if (_handsSubsystem == null)
            {
                StartCoroutine(WaitForSubsystem());
            }
        }

        protected void OnDisable()
        {
            handRenderer.enabled = false;
        }
        
        private IEnumerator WaitForSubsystem()
        {
            yield return new WaitUntil(() => XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>() != null);
            OnEnable();
        }


        private void LoadNodes()
        {
            for (int i = 0; i < (int)TrackedHandJoint.TotalJoints; i++)
            {
                _riggedVisualJointsArray[i] = (TrackedHandJoint) i switch
                {
                    TrackedHandJoint.Wrist => _handRoot,
                    TrackedHandJoint.ThumbMetacarpal => _thumb001,
                    TrackedHandJoint.ThumbProximal => _thumb002,
                    TrackedHandJoint.ThumbDistal => _thumb003,
                    TrackedHandJoint.IndexProximal => _index001,
                    TrackedHandJoint.IndexIntermediate => _index002,
                    TrackedHandJoint.IndexDistal => _index003,
                    TrackedHandJoint.MiddleProximal => _middle001,
                    TrackedHandJoint.MiddleIntermediate => _middle002,
                    TrackedHandJoint.MiddleDistal => _middle003,
                    TrackedHandJoint.RingProximal => _ring001,
                    TrackedHandJoint.RingIntermediate => _ring002,
                    TrackedHandJoint.RingDistal => _ring003,
                    TrackedHandJoint.LittleProximal => _pinky001,
                    TrackedHandJoint.LittleIntermediate => _pinky002,
                    TrackedHandJoint.LittleDistal => _pinky003,
                    _ => _riggedVisualJointsArray[i]
                };
            }
        }

        private void Update()
        {
            if (!ShouldRenderHand() ||
                !_handsSubsystem.TryGetEntireHand(handNode, out IReadOnlyList<HandJointPose> joints))
            {
                handRenderer.enabled = false;
                return;
            }

            handRenderer.enabled = true;
            UpdateQCHTAvatar(ConvertHandJoints(joints));
        }

        private IReadOnlyList<HandJointPose> ConvertHandJoints(IReadOnlyList<HandJointPose> joints)
        {
            var baseOrientation = Quaternion.AngleAxis(90f, Vector3.right);
           
            for (var i = 0; i < (int) TrackedHandJoint.TotalJoints; i++)
            {
                var openXRPose = joints[i];
                openXRPose.Rotation *= baseOrientation;
                _convertedHandPose[i] = openXRPose;
            }
            
            return _convertedHandPose;
        }

        void UpdateQCHTAvatar(IReadOnlyList<HandJointPose> joints)
        {
            for (int i = 0; i < (int) TrackedHandJoint.TotalJoints; i++)
            {
                var jointTransform = _riggedVisualJointsArray[i];
                var jointPose = joints[i];

                if (jointTransform != null)
                {
                    switch ((TrackedHandJoint) i)
                    {
                        case TrackedHandJoint.Wrist:
                            jointTransform.position = jointPose.Position;
                            jointTransform.rotation = jointPose.Rotation;
                            break;
                        case TrackedHandJoint.IndexMetacarpal:
                        case TrackedHandJoint.MiddleMetacarpal:
                        case TrackedHandJoint.RingMetacarpal:
                        case TrackedHandJoint.LittleMetacarpal:
                        case TrackedHandJoint.Palm:
                            break;
                        default:
                            jointTransform.rotation = jointPose.Rotation;
                            break;
                    }
                }
            }
        }
        
        private bool ShouldRenderHand()
        {
            return _handsSubsystem != null && _handRoot != null && handRenderer != null;
        }
    }
}
#endif