// /******************************************************************************
//  * File: ProximalInteractable.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Linq;
using QCHT.Interactions.Hands;
using UnityEngine;
using UnityEngine.Events;

namespace QCHT.Interactions.Proximal
{
    public sealed class ProximalInteractable : MonoBehaviour, IProximalEnterHandler, IProximalExitHandler,
        IBeginProximalGrabHandler, IEndProximalGrabHandler, IProximalGrabHandler, IBeginDoubleGrabHandler,
        IDoubleGrabHandler
    {
        [Header("Object"), SerializeField]
        private bool disableCollisionsWhenInteracting = true;

        [SerializeField] private bool smoothTarget;
        
        [Header("Scale"), SerializeField, MinMax(0f, 10f)]
        private Vector2 maxScaleMultiplier = new Vector2(.5f, 2f);
        
        [Header("Callbacks"), Space]
        public UnityEvent<InteractionData> OnProximalEnterEvent = new UnityEvent<InteractionData>();
        public UnityEvent<InteractionData> OnProximalExitEvent = new UnityEvent<InteractionData>();
        public UnityEvent<InteractionData> OnProximalGrabEvent = new UnityEvent<InteractionData>();
        public UnityEvent<InteractionData> OnProximalReleaseEvent = new UnityEvent<InteractionData>();
        
        private Pose _leftPoseOffset = Pose.identity;
        private Pose _rightPoseOffset = Pose.identity;
        private float _startMagnitude;
        private Vector3 _startScale;

        private SnapData _currentSnapData;
        private InteractionData _currentInteractionData;

        private Rigidbody _rigidbody;
        private Collider[] _colliders; // Collider that are not trigger

        public bool IsGrabbed => _currentInteractionData != null;

        private float _damping = 1.05f;
        private readonly float _grabbingSpeed = 5f;

        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private Vector3 _targetDir;

        public void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>(true).Where(col => !col.isTrigger).ToArray();
            _targetPosition = transform.position;
        }

        public void OnEnable()
        {
            Application.onBeforeRender += UpdateSmoothTargetPosition;
        }
        
        public void OnDisable()
        {
            ReleaseGrab();
            Application.onBeforeRender -= UpdateSmoothTargetPosition;
        }

        public void FixedUpdate() => UpdateSmoothTargetPosition();
        
        private Vector3 _lastDir;

        private void UpdateSmoothTargetPosition()
        {
            if (!smoothTarget)
                return;
            var t = transform;
            var dt = Time.deltaTime * _grabbingSpeed;
            var position = t.position;
            position += _targetDir * dt;
            _targetDir /= _damping;
            t.position = position;
            t.rotation = Quaternion.Slerp(t.rotation, _targetRotation, dt);
        } 

        #region Proximal Events

        public void OnProximalEnter(InteractionData eventData)
        {
            OnProximalEnterEvent.Invoke(eventData);
        }

        public void OnProximalExit(InteractionData eventData)
        {
            OnProximalExitEvent.Invoke(eventData);
        }

        public void OnBeginGrab(InteractionData eventData)
        {
            if (eventData.Data is {Type: InteractableType.Snap})
            {
                // Release snapping when already grabbed
                if (_currentInteractionData != eventData)
                    ReleaseGrab();

                var snapData = eventData.Data?.SnapData;

                try
                {
                    var data = snapData?.First(x => x.HandPose && x.HandPose.Type == eventData.Controller.Handedness);
                    if (data != null)
                    {
                        var pose = data.HandPose;
                        pose.Type = eventData.Controller.Handedness;
                        eventData.Controller.PoseDriver.HandPose = pose;
                        eventData.Controller.PoseDriver.HandPoseMask = data.HandPoseMask;
                        _currentSnapData = data;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            else
            {
                var t = transform;
                ref var poseOffset = ref eventData.Controller.Handedness == XrHandedness.XR_HAND_LEFT
                    ? ref _leftPoseOffset
                    : ref _rightPoseOffset;
                poseOffset.position = eventData.Controller.PoseDriver.RootTransform.InverseTransformPoint(t.position);
                poseOffset.rotation = Quaternion.Inverse(eventData.Controller.PoseDriver.RootTransform.rotation) * t.rotation;
            }

            if (disableCollisionsWhenInteracting)
                ToggleObjectCollision(false);

            OnProximalGrabEvent.Invoke(eventData);
            _currentInteractionData = eventData;
        }

        public void OnGrab(InteractionData eventData)
        {
            if (eventData.Data is {Type: InteractableType.Snap} && _currentSnapData != null)
            {
                var pose = _currentSnapData.HandPose;
                pose.Root.Position = eventData.Controller.PoseDriver.RootTransform.localPosition;
                pose.Root.Rotation = eventData.Controller.PoseDriver.RootTransform.localRotation;

                var t = transform;
                t.rotation = eventData.Controller.PoseDriver.RootTransform.rotation *
                                  Quaternion.Inverse(Quaternion.Euler(_currentSnapData.LocalOffsetRotation));
                t.position = eventData.Controller.PoseDriver.RootTransform.position -
                                  t.rotation * _currentSnapData.LocalOffsetPosition;
                UpdateSmoothTargetPosition();
            }
            else
            {
                ref var poseOffset = ref eventData.Controller.Handedness == XrHandedness.XR_HAND_LEFT
                    ? ref _leftPoseOffset
                    : ref _rightPoseOffset;

                var rRot = eventData.Controller.PoseDriver.RootTransform.rotation;
                var finalRotation = rRot * poseOffset.rotation;
                var finalPosition = eventData.Controller.PoseDriver.RootTransform.position + rRot * poseOffset.position;
                
                if (smoothTarget)
                {
                    _targetRotation = finalRotation;
                    _targetPosition = finalPosition;
                    var dir = _targetPosition - transform.position;
                    _targetDir = dir;
                }
                else
                {
                    var t = transform;
                    t.rotation = finalRotation;
                    t.position = finalPosition;
                }
            }
        }

        public void OnEndGrab(InteractionData eventData)
        {
            if (disableCollisionsWhenInteracting)
                ToggleObjectCollision(true);

            if (_currentInteractionData != null)
            {
                _currentInteractionData.ProximalGrab = null;
                _currentInteractionData.Controller.OnEndInteraction();
            }
            
            _currentInteractionData = null;
            _currentSnapData = null;

            if (eventData.Controller != null)
            {
                //eventData.Controller.Driver.HandPose = null;
                eventData.Controller.PoseDriver.HandPoseMask = null;
                ref var poseOffset = ref eventData.Controller.Handedness == XrHandedness.XR_HAND_LEFT
                    ? ref _leftPoseOffset
                    : ref _rightPoseOffset;
                poseOffset = Pose.identity;
            }
            
            OnProximalReleaseEvent.Invoke(eventData);
        }

        public void OnBeginDoubleGrab(DoubleInteractionData eventData)
        {
            _startMagnitude = Vector3.Distance(eventData.LeftData.Controller.PoseDriver.RootTransform.position,
                eventData.RightData.Controller.PoseDriver.RootTransform.position);
            _startScale = transform.localScale;

            if (eventData.LeftData.Data is {Type: InteractableType.Snap})
            {
                var grabData = _currentInteractionData.Controller.Handedness == XrHandedness.XR_HAND_LEFT
                    ? eventData.LeftData
                    : eventData.RightData;
                OnBeginGrab(grabData);
            }
        }

        public void OnDoubleGrab(DoubleInteractionData eventData)
        {
            if (eventData.LeftData.Data is {Type: InteractableType.Snap})
            {
                // Do nothing
            }
            else
            {
                var t = transform;
                var lPos = eventData.LeftData.Controller.PoseDriver.RootTransform.position;
                var rPos = eventData.RightData.Controller.PoseDriver.RootTransform.position;
                var lrRot = eventData.LeftData.Controller.PoseDriver.RootTransform.rotation;
                var leftPos = lPos + lrRot * _leftPoseOffset.position;
                var rrRot = eventData.RightData.Controller.PoseDriver.RootTransform.rotation;
                var rightPos = rPos + rrRot * _rightPoseOffset.position;
                var m = Vector3.Distance(lPos, rPos);
                t.position = Vector3.Lerp(leftPos, rightPos, .5f);
                t.localScale = _startScale *
                               Mathf.Clamp(m / _startMagnitude, maxScaleMultiplier.x, maxScaleMultiplier.y);
            }
        }

        #endregion

        public void ReleaseGrab()
        {
            if (_currentInteractionData == null)
                return;
            
            OnEndGrab(_currentInteractionData);
        }

        private void ToggleObjectCollision(bool enable)
        {
            if (_rigidbody)
                _rigidbody.isKinematic = !enable;

            foreach (var col in _colliders)
            {
                col.isTrigger = !enable;
            }
        }
    }
}