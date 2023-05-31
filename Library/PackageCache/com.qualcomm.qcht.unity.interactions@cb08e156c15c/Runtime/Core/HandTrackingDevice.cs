// /******************************************************************************
//  * File: HandTrackingDevice.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace QCHT.Interactions.Core
{
    public struct HandTrackingInputState : IInputStateTypeInfo
    {
        public FourCC format => new FourCC('Q', 'C', 'H', 'T');
        
        [Preserve, InputControl(name = "trackingState")]
        public int trackingState;
        
        [Preserve, InputControl(name = "isTracked")]
        public bool isTracked;
        
        [Preserve, InputControl(name = "devicePosition")]
        public Vector3 devicePosition;

        [Preserve, InputControl(name = "deviceRotation")]
        public Quaternion deviceRotation;
        
        [Preserve,InputControl(name = "trigger", usage = "Trigger", layout = "Axis", aliases = new[] {"Primary", "select"})]
        public float trigger;
        
        [Preserve, InputControl(name = "triggerPressed", layout = "Button", aliases = new[] {"PrimaryButton", "selectPressed"})]
        public bool selectPressed;
        
        [Preserve, InputControl(name = "grip", usage = "Trigger", layout = "Axis", aliases = new[] {"Secondary", "grip"})]
        public float grip;
        
        [Preserve, InputControl(name = "gripPressed", layout = "Button", aliases = new[] {"SecondaryButton", "gripPressed"})]
        public bool gripPressed;
        
        [InputControl(name = "pointerPosition")]
        public Vector3 pointerPosition;

        [InputControl(name = "pointerRotation")]
        public Quaternion pointerRotation;
    }
    
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [InputControlLayout(displayName = "Qualcomm Hand", commonUsages = new[] {"LeftHand", "RightHand"},
        stateType = typeof(HandTrackingInputState))]
    public class HandTrackingDevice : XRController
    {
        public const string kDeviceName = "Qualcomm Hand";
        
        public AxisControl select { get; private set; }
        public ButtonControl selectPressed { get; private set; }
        public AxisControl grip { get; private set; }
        public ButtonControl gripPressed { get; private set; }
        public Vector3Control pointerPosition { get; private set; }
        public QuaternionControl pointerRotation { get; private set; }

        static HandTrackingDevice() {
            InputSystem.RegisterLayout<HandTrackingDevice>(
                matches:
                new InputDeviceMatcher()
                    .WithProduct(kDeviceName));
        }

        protected override void FinishSetup() {
            base.FinishSetup();
            select = GetChildControl<AxisControl>("trigger");
            selectPressed = GetChildControl<ButtonControl>("triggerPressed");
            grip = GetChildControl<AxisControl>("grip");
            gripPressed = GetChildControl<ButtonControl>("gripPressed");
            pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
            pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeInPlayer() {
        }
    }
}