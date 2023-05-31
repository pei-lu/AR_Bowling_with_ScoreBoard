// /******************************************************************************
//  * File: HandTrackingSimulationSubsystem.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Linq;
using QCHT.Interactions.Hands;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace QCHT.Interactions.Core
{ 
  public class HandTrackingSimulationSubsystem : XRHandTrackingSubsystem {

    public const string ID = "Qualcomm-HandTrackingSimulationSubsystem";

    private class HandTrackingSimulationProvider : XRHandTrackingSubsystem.Provider
    {
      private static void AssignPose(ref Pose[] joints, HandPose poseAsset, Pose wrist) {
        var poses = ConvertHandPoseToOpenXRData(poseAsset); 
        for (var i = 0; i < poses.Length; i++) { 
          var pose = poses[i]; 
          pose.rotation = wrist.rotation * pose.rotation * Quaternion.AngleAxis(-90f, Vector3.right); 
          pose.position += wrist.position; 
          joints[i] = pose;
        }
      }

      private static HandPose GetPoseAsset(XrHandGesture gesture, bool isLeft) {
        return gesture switch {
          XrHandGesture.XR_HAND_PINCH => isLeft ? XRHandSimulationHandPosesSettings.Instance.leftPinchHand : XRHandSimulationHandPosesSettings.Instance.rightPinchHand,
          XrHandGesture.XR_HAND_GRAB => isLeft ? XRHandSimulationHandPosesSettings.Instance.leftGrabHand : XRHandSimulationHandPosesSettings.Instance.rightGrabHand,
          _ => isLeft ? XRHandSimulationHandPosesSettings.Instance.leftOpenHand : XRHandSimulationHandPosesSettings.Instance.rightOpenHand
        };
      }
      
      public override void GetHandData(bool isLeft, ref bool isTracked, ref Pose[] joints, ref float[] scales, ref int gesture, ref float gestureRatio, ref float flipRatio) {
        var ctrl = isLeft ? InputSystem.GetDevice<XRSimulatedController>(CommonUsages.LeftHand) : InputSystem.GetDevice<XRSimulatedController>(CommonUsages.RightHand);
        if (ctrl == null) {
          // Shows and apply hand pose
          AssignPose(ref joints, GetPoseAsset(XrHandGesture.XR_HAND_OPEN_HAND, isLeft), Pose.identity);
          isTracked = true;
          return;
        }
        isTracked = ctrl.isTracked.IsPressed();
        if (ctrl.trigger.IsPressed()) gesture = (int) XrHandGesture.XR_HAND_PINCH;
        else if (ctrl.grip.IsPressed()) gesture = (int) XrHandGesture.XR_HAND_GRAB;
        else gesture = (int) XrHandGesture.XR_HAND_OPEN_HAND;
        var pose = new Pose(ctrl.devicePosition.ReadValue(), ctrl.deviceRotation.ReadValue());
        AssignPose(ref joints, GetPoseAsset((XrHandGesture) gesture, isLeft), pose);
        scales = s_scales;
      }

      private static Pose[] ConvertHandPoseToOpenXRData(HandPose pose) {
        if (pose == null) return new Pose [(int) XrHandJoint.XR_HAND_JOINT_MAX];
        return new Pose[] {
          new Pose(pose.Palm.Position, pose.Palm.Rotation),
          new Pose(pose.Root.Position, pose.Root.Rotation),
          new Pose(pose.Thumb.BaseData.Position, pose.Thumb.BaseData.Rotation),
          new Pose(pose.Thumb.MiddleData.Position, pose.Thumb.MiddleData.Rotation),
          new Pose(pose.Thumb.TopData.Position, pose.Thumb.TopData.Rotation),
          new Pose(), //fake 
          new Pose(), //fake
          new Pose(pose.Index.BaseData.Position, pose.Index.BaseData.Rotation),
          new Pose(pose.Index.MiddleData.Position, pose.Index.MiddleData.Rotation),
          new Pose(pose.Index.TopData.Position, pose.Index.TopData.Rotation),
          new Pose(), //fake 
          new Pose(), //fake
          new Pose(pose.Middle.BaseData.Position, pose.Middle.BaseData.Rotation),
          new Pose(pose.Middle.MiddleData.Position, pose.Middle.MiddleData.Rotation),
          new Pose(pose.Middle.TopData.Position, pose.Middle.TopData.Rotation),
          new Pose(), //fake 
          new Pose(), //fake
          new Pose(pose.Ring.BaseData.Position, pose.Ring.BaseData.Rotation),
          new Pose(pose.Ring.MiddleData.Position, pose.Ring.MiddleData.Rotation),
          new Pose(pose.Ring.TopData.Position, pose.Ring.TopData.Rotation),
          new Pose(), //fake 
          new Pose(), //fake
          new Pose(pose.Pinky.BaseData.Position, pose.Pinky.BaseData.Rotation),
          new Pose(pose.Pinky.MiddleData.Position, pose.Pinky.MiddleData.Rotation),
          new Pose(pose.Pinky.TopData.Position, pose.Pinky.TopData.Rotation),
          new Pose() //fake 
        };
      }

      private static readonly float[] s_scales = Enumerable.Repeat(1f, (int) XrHandJoint.XR_HAND_JOINT_MAX).ToArray();
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterDescriptor() {
      XRHandTrackingSubsystemDescriptor.Create(new XRHandTrackingSubsystemDescriptor.Cinfo {
        id = ID,
        providerType = typeof(HandTrackingSimulationProvider),
        subsystemTypeOverride = typeof(HandTrackingSubsystem)
      });
    }
  }
}