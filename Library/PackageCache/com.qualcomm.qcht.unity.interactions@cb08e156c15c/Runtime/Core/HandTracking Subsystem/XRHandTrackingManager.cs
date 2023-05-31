// /******************************************************************************
//  * File: XRHandTrackingManager.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections.Generic;
using QCHT.Interactions.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR;
using QCHT.Interactions.Hands;

#if UNITY_AR_FOUNDATION_LEGACY
using UnityEngine.XR.ARFoundation;
#endif

using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using CommonUsages = UnityEngine.InputSystem.CommonUsages;
using InputDevice = UnityEngine.InputSystem.InputDevice;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Interactions.Core
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_Controllers)]
    public sealed class XRHandTrackingManager : XRSubsystemLifeCycleManager<XRHandTrackingSubsystem,
        XRHandTrackingSubsystemDescriptor, XRHandTrackingSubsystem.Provider>
    {
        private const string kResourcesHandLeft = "QualcommHandLeft";
        private const string kResourcesHandRight = "QualcommHandRight";
        private const string kTrackableLeftName = "QC Hand Left";
        private const string kTrackableRightName = "QC Hand Right";

        [SerializeField] private GameObject leftHandPrefab;
        [SerializeField] private GameObject rightHandPrefab;
        
        [Space]
        
        private GameObject _leftHand;
        private GameObject _rightHand;

        private HandRiggedVisualizer _leftHandDataUpdater;
        private HandRiggedVisualizer _rightHandDataUpdater;
        
        private HandTrackingDevice _leftHandDevice;
        private HandTrackingDevice _rightHandDevice;

        private HandTrackingInputState _leftState;
        private HandTrackingInputState _rightState;

        public Pose[] _leftPoses = new Pose[(int)XrHandJoint.XR_HAND_JOINT_MAX];
        public Pose[] _rightPoses = new Pose[(int)XrHandJoint.XR_HAND_JOINT_MAX];
        public Pose toDisplay;
        private float[] _leftScales = new float[(int)XrHandJoint.XR_HAND_JOINT_MAX];
        private float[] _rightScales = new float[(int)XrHandJoint.XR_HAND_JOINT_MAX];
        
        private readonly Pose[] _convertedLeftPoses = new Pose[(int) XrHandJoint.XR_HAND_JOINT_MAX];
        private readonly Pose[] _convertedRightPoses = new Pose[(int) XrHandJoint.XR_HAND_JOINT_MAX];

#if UNITY_AR_FOUNDATION_LEGACY
        private ARSessionOrigin _arOrigin;
#endif
        private XROrigin _xrOrigin;

        private Transform OriginTransform
        {
            get
            {
#if UNITY_AR_FOUNDATION_LEGACY
                if (_arOrigin != null)
                    return _arOrigin.transform;
#endif
                return _xrOrigin ? _xrOrigin.transform : transform;
            }
        }

        private Transform CameraTransform
        {
            get
            {
                Camera cam = null;
#if UNITY_AR_FOUNDATION_LEGACY
                if (_arOrigin != null)
                    cam = _arOrigin.camera;
#endif
                if (cam == null && _xrOrigin != null) {
                    cam = _xrOrigin.Camera;
                }

                if(cam == null)
                    cam = Camera.main;
                
                return cam ? cam.transform : OriginTransform;
            }
        }

        /// <summary>
        /// Returns the current left hand game object instance.
        /// </summary>
        public GameObject LeftHand => _leftHand;
        
        /// <summary>
        /// Returns the current right hand game object instance.
        /// </summary>
        public GameObject RightHand => _rightHand;
        
        /// <summary>
        /// Left prefab object that will be instantiated.
        /// If the prefab changed after Instantiation time, it can be refreshed by calling RefreshLeftHand
        /// </summary>
        public GameObject LeftHandPrefab {
            get => leftHandPrefab;
            set => leftHandPrefab = value;
        }
        
        /// <summary>
        /// Right prefab object that will be instantiated.
        /// If the prefab changed after Instantiation time, it can be refreshed by calling RefreshRightHand.
        /// </summary>
        public GameObject RightHandPrefab {
            get => rightHandPrefab;
            set => rightHandPrefab = value;
        }
        
        private void Awake() {
#if UNITY_AR_FOUNDATION_LEGACY
            _arOrigin = GetComponent<ARSessionOrigin>();
#endif
            _xrOrigin = GetComponent<XROrigin>();
        }
        
        protected override void OnEnable() {
            base.OnEnable();
            StartDevices();
        }

        protected override void OnDisable() {
            base.OnDisable();
            StopDevices();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            RemoveLeftHand();
            RemoveRightHand();
            if (_leftHand != null) DestroyImmediate(_leftHand);
            if (_rightHand != null) DestroyImmediate(_rightHand);
        }

        private void Start() {
            SetParentTrackable(_leftHand);
            SetParentTrackable(_rightHand);
        }
        
        private void Update() => UpdateHands();
        
        #region Input devices
        
        /// <summary>
        /// Adds hand tracking devices in the input devices list.
        /// Devices are in the input devices list during all the lifetime of the Hand Tracking Manager.
        /// See StartDevices and StopDevices to 
        /// </summary>
        private void AddDevices() {
            _leftHandDevice ??= AddHandDevice(true) as HandTrackingDevice;
            _rightHandDevice ??= AddHandDevice(false) as HandTrackingDevice;
        }

        /// <summary>
        /// Removes hand tracking devices from the input system.
        /// </summary>
        private void RemoveDevices() {
            RemoveHandDevice(_leftHandDevice);
            _leftHandDevice = null;
            RemoveHandDevice(_rightHandDevice);
            _rightHandDevice = null;
        }
        
        /// <summary>
        /// Enables hand tracking devices in the input system.
        /// </summary>
        public void StartDevices() {
            if (subsystem == null) 
                return;
            
            InstantiateRightHand();
            InstantiateLeftHand();
            ToggleLeftHand(false);
            ToggleRightHand(false);
            
            AddDevices();
            
            if (_leftHandDevice != null)
                InputSystem.EnableDevice(_leftHandDevice);
            
            if (_rightHandDevice != null)
                InputSystem.EnableDevice(_rightHandDevice);
        }
        
        /// <summary>
        /// Disables hand tracking devices in the input system.
        /// </summary>
        public void StopDevices() {
            ToggleLeftHand(false);
            ToggleRightHand(false);
            _leftState.isTracked = false;
            _rightState.isTracked = false;
            
            if (_leftHandDevice != null && _leftHandDevice.added)
                InputSystem.DisableDevice(_leftHandDevice);
            
            if (_rightHandDevice != null && _rightHandDevice.added)
                InputSystem.DisableDevice(_rightHandDevice);
            
            RemoveDevices();
        }
        
        /// <summary>
        /// Updates hand tracking devices.
        /// </summary>
        [BeforeRenderOrder(XRInteractionUpdateOrder.k_Controllers)]
        private void UpdateHands() {
            UpdateHand(ref _leftState, true, _leftHandDataUpdater, _leftHandDevice);
            UpdateHand(ref _rightState, false, _rightHandDataUpdater, _rightHandDevice);
        } 
        
        #endregion
        
        /// <summary>
        /// Update the hand device and objects,
        /// It will update the InputState struct for QCHT hand tracked device.
        /// That should be done by OpenXR layer and retrieved as a controller profile in future versions
        /// </summary>
        private void UpdateHand(ref HandTrackingInputState state, bool isLeft, HandRiggedVisualizer handRiggedVisualizer, HandTrackingDevice device) {
            ref var joints = ref isLeft ? ref _leftPoses : ref _rightPoses;
            ref var scales = ref isLeft ? ref _leftScales : ref _rightScales;
            var isTracked = false;  
            var gestureId = 0;
            var gestureRatio = 0f;
            var flipRatio = 1f;
            
            // Get hand tracking data
            subsystem?.GetHandData(isLeft, ref isTracked, ref joints, ref scales, ref gestureId, ref gestureRatio, ref flipRatio);
            
            // Update hand tracked objects
            if (handRiggedVisualizer != null) {
                var convertedJoints = ConvertJoints(isLeft, joints);
                handRiggedVisualizer.UpdateData(convertedJoints, scales);
            }

            if (isTracked != state.isTracked) {
                if (isLeft) ToggleLeftHand(isTracked);
                else ToggleRightHand(isTracked);
            }

            if (device == null) return;
            
            state.isTracked = isTracked;
            if (state.isTracked) {
                state.trackingState = (int) (InputTrackingState.Position | InputTrackingState.Rotation);
                var oT = OriginTransform;
                var camT = CameraTransform;
                var cameraPose = new Pose(camT.localPosition, camT.localRotation);  
                var devicePose = Unity.XR.CoreUtils.TransformExtensions.InverseTransformPose(OriginTransform, joints[(int)XrHandJoint.XR_HAND_JOINT_WRIST]);
                //debug display
                toDisplay = devicePose;
                var baseOrientation = Quaternion.AngleAxis(90f, Vector3.right);
                devicePose.position = oT.TransformPoint(devicePose.position); 
                devicePose.rotation = oT.rotation * devicePose.rotation * baseOrientation;

                // Device pose
                state.devicePosition = devicePose.position;
                state.deviceRotation = devicePose.rotation;
                
                // Pointer pose
                const float pointerSmoothValue = 7.5f;
                const float shoulderOffset = 0.08f;
                var positionOffset = isLeft ? new Vector3(0.02f, 0.09f, 0.1f) : new Vector3(-0.02f, 0.09f, 0.1f);
                var posOffset = devicePose.rotation * positionOffset;
                var shoulder = isLeft ? -cameraPose.right : cameraPose.right;
                var shoulderPosition = cameraPose.position + (-cameraPose.up + shoulder) * shoulderOffset;
                var pointerPose = Pose.identity;
                pointerPose.position = devicePose.position + posOffset;
                pointerPose.rotation = Quaternion.LookRotation((pointerPose.position - shoulderPosition).normalized);
                
                var dt = Time.deltaTime * pointerSmoothValue;
                state.pointerPosition = Vector3.Lerp(state.pointerPosition, pointerPose.position, dt);
                state.pointerRotation = Quaternion.Slerp(state.pointerRotation, pointerPose.rotation, dt);

                // Trigger pressed
                state.selectPressed = (XrHandGesture) gestureId == XrHandGesture.XR_HAND_PINCH;
                
                // Grip pressed
                if (handRiggedVisualizer) {
                    var releaseThreshold = handRiggedVisualizer.HandPose.GetMinDistToThumbTip();
                    state.gripPressed = releaseThreshold < 0.055f;
                }
                else {
                    state.gripPressed = (XrHandGesture) gestureId == XrHandGesture.XR_HAND_GRAB;
                }
            }
            
            InputState.Change(device, state);
        }

        private IReadOnlyList<Pose> ConvertJoints(bool isLeft, IReadOnlyList<Pose> joints) {
            var convertedJoints = isLeft ? _convertedLeftPoses : _convertedRightPoses;
            var baseOrientation = Quaternion.AngleAxis(90f, Vector3.right);
            for (var i = 0; i < joints.Count; i++) {
                var openXRPose = joints[i];
                var origin = OriginTransform;
                openXRPose.position = origin.TransformPoint(openXRPose.position);
                openXRPose.rotation = origin.rotation * openXRPose.rotation * baseOrientation;
                convertedJoints[i] = openXRPose;
            }
            return convertedJoints;
        }
        
        #region Hand Objects

        /// <summary>
        /// Forces regenerating the left hand trackable object.
        /// </summary>
        public void RefreshLeftHand() {
            RemoveLeftHand();
            InstantiateLeftHand();
        }
        
        /// <summary>
        /// Forces regenerating the right hand trackable object.
        /// </summary>
        public void RefreshRightHand() {
            RemoveRightHand();
            InstantiateRightHand();
        }
        
        /// <summary>
        /// Instantiates left hand trackable object.
        /// </summary>
        private void InstantiateLeftHand() =>
            InstantiateHand(kTrackableLeftName, leftHandPrefab, ref _leftHand, ref _leftHandDataUpdater);
        
        /// <summary>
        /// Instantiates right hand trackable object.
        /// </summary>
        private void InstantiateRightHand() =>
            InstantiateHand(kTrackableRightName, rightHandPrefab, ref _rightHand, ref _rightHandDataUpdater);

        /// <summary>
        /// Instantiate a hand object.
        /// </summary>
        private void InstantiateHand(string objectName, GameObject prefab, ref GameObject hand, ref HandRiggedVisualizer dataUpdater) {
            if (hand == null) {
                hand = InstantiateHandTrackable(objectName, prefab);
            }

            if (hand != null) {                
                hand.TryGetComponent(out dataUpdater);
                SetParentTrackable(hand);
            }
        }

        /// <summary>
        /// Destroys left hand trackable object.
        /// </summary>
        private void RemoveLeftHand() => RemoveHand(ref _leftHand);

        /// <summary>
        /// Destroys right hand trackable object.
        /// </summary>
        private void RemoveRightHand() => RemoveHand(ref _rightHand);

        /// <summary>
        /// Destroys a hand object.
        /// </summary>
        private static void RemoveHand(ref GameObject hand) {
            if (hand != null) Destroy(hand);
            hand = null;
        }

        /// <summary>
        /// Sets the left hand skin.
        /// Do nothing if the hand object is not ISkinnable.
        /// </summary>
        /// <param name="skin"> Skin to set. </param>
        public void SetLeftHandSkin(HandSkin skin) => SetSkin(_leftHandDataUpdater, skin);
        
        /// <summary>
        /// Sets the right hand skin.
        /// Do nothing if the hand object is not ISkinnable.
        /// </summary>
        /// <param name="skin"> Skin to set. </param>
        public void SetRightHandSkin(HandSkin skin) => SetSkin(_rightHandDataUpdater, skin);

        private static void SetSkin(HandRiggedVisualizer hand, HandSkin skin) {
            if (hand == null) return;
            hand.HandSkin = skin;
        }
        
        /// <summary>
        /// Toggles the left hand visibility.
        /// Performs fading if the hand object is a IHandFadable. 
        /// </summary>
        /// <param name="visible"> Is the object visible? </param>
        public void ToggleLeftHand(bool visible) => ToggleHand(_leftHandDataUpdater, visible);

        /// <summary>
        /// Toggles the right hand visibility.
        /// Performs fading if the hand object is a IHandFadable. 
        /// </summary>
        /// <param name="visible"> Is the object visible? </param>
        public void ToggleRightHand(bool visible) => ToggleHand(_rightHandDataUpdater, visible);

        /// <summary>
        /// Toggles a hand game object visibility.
        /// Performs fading if the hand object is a IHandFadable. 
        /// </summary>
        private void ToggleHand(HandRiggedVisualizer hand, bool visible) {
            if (hand == null) return;
            if (visible) {
                hand.gameObject.SetActive(true);
                hand.Show();
            }
            else {
                hand.Hide();
                hand.gameObject.SetActive(false);
            }
        }

        private void SetParentTrackable(GameObject hand) {
            if (hand == null) return;
            Transform trackablesParent = null;
#if UNITY_AR_FOUNDATION_LEGACY
            if (_arOrigin != null)
                trackablesParent = _arOrigin.trackablesParent;
#endif
            if (trackablesParent == null && _xrOrigin != null)
                trackablesParent = _xrOrigin.TrackablesParent;

            trackablesParent = trackablesParent ? trackablesParent : transform;
            var handTransform = hand.transform;
            handTransform.SetParent(trackablesParent);
            handTransform.localPosition = Vector3.zero;
            handTransform.localRotation = Quaternion.identity;
        }
        
        #endregion

        #region static

        public static GameObject DefaultLeftHandPrefab => Resources.Load<GameObject>(kResourcesHandLeft);
        public static GameObject DefaultRightHandPrefab => Resources.Load<GameObject>(kResourcesHandRight);

        /// <summary>
        /// Instantiates a hand tracking manager with default hands prefabs.
        /// </summary>
        [Obsolete("InstantiateHandTrackingManager is deprecated. Please use GetOrCreate() instead.")]
        public static void InstantiateHandTrackingManager() {
            GetOrCreate(DefaultLeftHandPrefab, DefaultRightHandPrefab);
        }
        
        /// <summary>
        /// Gets existing or creates a Hand Tracking Manager if it doesn't exist.
        /// </summary>
        /// <returns> New or existing hand tracking manager instance. </returns>
        public static XRHandTrackingManager GetOrCreate(GameObject leftPrefab = null, GameObject rightPrefab = null) {
            if (TryFindOrigin(out var origin) && origin != null) {
                var manager = origin.gameObject.AddComponent<XRHandTrackingManager>();
                var needRefreshL = true;
                var needRefreshR = true;
                if (manager == null) { // if already exists
                    manager = origin.gameObject.GetComponent<XRHandTrackingManager>();
                    needRefreshL = leftPrefab != manager.LeftHandPrefab;
                    needRefreshR = rightPrefab != manager.rightHandPrefab;
                }
                manager.leftHandPrefab = leftPrefab;
                manager.rightHandPrefab = rightPrefab;
                if (needRefreshL) {
                    manager.RefreshLeftHand();
                    manager.ToggleLeftHand(false);
                }
                if (needRefreshR) {
                    manager.RefreshRightHand();
                    manager.ToggleRightHand(false);
                }
#if UNITY_EDITOR   
                Selection.activeGameObject = origin.gameObject;
#endif
                return manager;
            }

            Debug.LogWarning("[XRHandTrackingManager] Hand tracking manager may not work correctly because it is not related to a ARSession/XROrigin");
            var go = new GameObject("HandTrackingManager");
            return go.AddComponent<XRHandTrackingManager>();
        }
        
        /// <summary>
        /// Destroys Hand Tracking Manager instance if it does exist.
        /// </summary>
        public static void Destroy(XRHandTrackingManager manager = null) {
            manager = manager ? manager : FindObjectOfType<XRHandTrackingManager>();
            if (manager == null) return;
            GameObject.Destroy(manager);
        }
        
        private static GameObject InstantiateHandTrackable(string handName, GameObject prefab) {
            if (prefab == null) {
                return null;
            }

            GameObject instance;
            if (prefab.scene.rootCount == 0 || !prefab.activeInHierarchy) {
                instance = Instantiate(prefab);
                instance.name = handName;
            }
            else {
                instance = prefab;
            }
            return instance;
        }
        
        private static InputDevice AddHandDevice(bool isLeft) {
            var usage = isLeft ? CommonUsages.LeftHand : CommonUsages.RightHand;
            var device = InputSystem.AddDevice<HandTrackingDevice>($"{nameof(HandTrackingDevice)} - {usage}");
            if (device != null) InputSystem.SetDeviceUsage(device, usage);
            return device;
        }

        private static void RemoveHandDevice(InputDevice device) {
            if (device == null) return;
            InputSystem.RemoveDevice(device);
        }
        
        private static bool TryFindOrigin(out Transform parent) {
#if UNITY_AR_FOUNDATION_LEGACY
            var arOrigin = FindObjectOfType<ARSessionOrigin>(true);
            if (arOrigin != null) {
                parent = arOrigin.transform;
                return true;
            }
#endif
            var xrOrigin = FindObjectOfType<XROrigin>(true);
            if (xrOrigin != null) {
                parent = xrOrigin.transform;
                return true;
            }
            
            parent = null;
            return false;
        }

        #endregion 
    }
}