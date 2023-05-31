// /******************************************************************************
//  * File: HandRiggedVisualizer.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using QCHT.Interactions.Hands;
using UnityEngine;

namespace QCHT.Interactions.Core
{
    public class HandRiggedVisualizer : MonoBehaviour, IHandProvider {
        
        [SerializeField] private XrHandedness handType;
        
        public XrHandedness HandType => handType;
        public HandPose HandPose => _handPose;
        public Pose Pose => new Pose(transform.position, transform.rotation);
        public XrHandGesture GestureId => (XrHandGesture) _gesture;
        
        private HandPose _handPose;
        private int _gesture;

        public void Awake() {
            _handPose ??= CreateHandPose();
            _propertyBlock ??= new MaterialPropertyBlock();
        }
        
        public void UpdateData(IReadOnlyList<Pose> joints, IReadOnlyList<float> scales) {
            if (joints != null) {
                UpdatePoses(joints);
            }

            if (scales != null) {
                UpdateScales(scales);
            }
        }
        
        private void UpdatePoses(IReadOnlyList<Pose> joints) {
            _handPose ??= CreateHandPose();
            _handPose.Index.BaseData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_PROXIMAL].position;
            _handPose.Index.BaseData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_PROXIMAL].rotation;
            _handPose.Index.MiddleData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_INTERMEDIATE].position;
            _handPose.Index.MiddleData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_INTERMEDIATE].rotation;
            _handPose.Index.TopData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_DISTAL].position;
            _handPose.Index.TopData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_INDEX_DISTAL].rotation;
            
            _handPose.Middle.BaseData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_PROXIMAL].position;
            _handPose.Middle.BaseData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_PROXIMAL].rotation;
            _handPose.Middle.MiddleData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_INTERMEDIATE].position;
            _handPose.Middle.MiddleData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_INTERMEDIATE].rotation;
            _handPose.Middle.TopData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_DISTAL].position;
            _handPose.Middle.TopData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_DISTAL].rotation;
            
            _handPose.Ring.BaseData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_RING_PROXIMAL].position;      
            _handPose.Ring.BaseData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_RING_PROXIMAL].rotation;      
            _handPose.Ring.MiddleData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_RING_INTERMEDIATE].position;
            _handPose.Ring.MiddleData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_RING_INTERMEDIATE].rotation;
            _handPose.Ring.TopData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_RING_DISTAL].position;         
            _handPose.Ring.TopData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_RING_DISTAL].rotation;
            
            _handPose.Pinky.BaseData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_PROXIMAL].position;      
            _handPose.Pinky.BaseData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_PROXIMAL].rotation;      
            _handPose.Pinky.MiddleData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_INTERMEDIATE].position;
            _handPose.Pinky.MiddleData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_INTERMEDIATE].rotation;
            _handPose.Pinky.TopData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_DISTAL].position;         
            _handPose.Pinky.TopData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_DISTAL].rotation;         
            
            _handPose.Thumb.BaseData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_METACARPAL].position;      
            _handPose.Thumb.BaseData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_METACARPAL].rotation;      
            _handPose.Thumb.MiddleData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_PROXIMAL].position;
            _handPose.Thumb.MiddleData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_PROXIMAL].rotation;
            _handPose.Thumb.TopData.Position = joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_DISTAL].position;         
            _handPose.Thumb.TopData.Rotation = joints[(int)XrHandJoint.XR_HAND_JOINT_THUMB_DISTAL].rotation;
        }
        
        private void UpdateScales(IReadOnlyList<float> scales) {
            _handPose ??= CreateHandPose();
            _handPose.Root.Scale = scales[(int) XrHandJoint.XR_HAND_JOINT_WRIST];
            _handPose.Palm.Scale = scales[(int) XrHandJoint.XR_HAND_JOINT_PALM];
            
            _handPose.Index.BaseData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_INDEX_PROXIMAL];
            _handPose.Index.MiddleData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_INDEX_INTERMEDIATE];
            _handPose.Index.TopData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_INDEX_DISTAL];
            
            _handPose.Middle.BaseData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_PROXIMAL];
            _handPose.Middle.MiddleData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_INTERMEDIATE];
            _handPose.Middle.TopData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_MIDDLE_DISTAL];
            
            _handPose.Ring.BaseData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_RING_PROXIMAL];      
            _handPose.Ring.MiddleData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_RING_INTERMEDIATE];
            _handPose.Ring.TopData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_RING_DISTAL];         
            
            _handPose.Pinky.BaseData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_PROXIMAL];      
            _handPose.Pinky.MiddleData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_INTERMEDIATE];
            _handPose.Pinky.TopData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_LITTLE_DISTAL];         
            
            _handPose.Thumb.BaseData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_THUMB_METACARPAL];      
            _handPose.Thumb.MiddleData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_THUMB_PROXIMAL];
            _handPose.Thumb.TopData.Scale = scales[(int)XrHandJoint.XR_HAND_JOINT_THUMB_DISTAL];         
        }

        private HandPose CreateHandPose() {
            var handPose = ScriptableObject.CreateInstance<HandPose>();
            handPose.Type = handType;
            handPose.Space = DataSpace.World;
            return handPose;
        }
        
        #region Presenter
        
        private static readonly int s_globalAlpha = Shader.PropertyToID("_Alpha");

        [Header("Presenter")]
        [SerializeField] private SkinnedMeshRenderer meshRenderer;
        [SerializeField] private float fadeDuration = 1f;

        private MaterialPropertyBlock _propertyBlock;

        private bool _isVisible;
        private Coroutine _fadeCoroutine;
        
        private HandSkin _handSkin;

        public HandSkin HandSkin {
            set { 
                if (_handSkin == value) return;
                ApplySkin(value);
            }

            get => _handSkin;
        }
        
        private void ApplySkin(HandSkin skin) {
            _handSkin = skin;

            if (meshRenderer == null)
                return;
            
            if (_handSkin == null) {
                meshRenderer.material = null;
                meshRenderer.sharedMesh = null;
                return;
            }

            meshRenderer.material = _handSkin.MainMaterial;
            meshRenderer.sharedMesh = _handSkin.MainMesh;
        }
        
        public void Show() {
            if (_fadeCoroutine != null) 
                StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(Fade(1f));
        }

        public void Hide() => SetAlpha(0f);

        private IEnumerator Fade(float targetAlpha) {
            var startAlpha = GetAlpha();
            float time = 0;
            while (time < fadeDuration) {
                time += Time.deltaTime;
                var dt = time / fadeDuration;
                var alpha = Mathf.Lerp(startAlpha, targetAlpha, dt);
                SetAlpha(alpha);
                yield return null; // Wait for next frame
            }
        }

        private float GetAlpha() {
            _propertyBlock ??= new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(_propertyBlock);
            return _propertyBlock.GetFloat(s_globalAlpha);
        }

        private void SetAlpha(float alpha) {
            _propertyBlock ??= new MaterialPropertyBlock();
            _propertyBlock.SetFloat(s_globalAlpha, alpha);
            meshRenderer.SetPropertyBlock(_propertyBlock);
        }
        
        #endregion
    }
}