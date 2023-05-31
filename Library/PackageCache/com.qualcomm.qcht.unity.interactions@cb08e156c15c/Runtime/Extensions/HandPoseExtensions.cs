// /******************************************************************************
//  * File: HandPoseExtensions.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Hands;
using UnityEngine;

namespace QCHT.Interactions.Extensions
{
    public static class HandPoseExtensions
    {
        /// <summary>
        /// Returns the distance from the thumb tip to closest other finger tip 
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <returns></returns>
        public static float GetMinDistToThumbTip(this HandPose hand)
        {
            var minDist = float.MaxValue;
            for (var i = 1; i < 5; i++)
            {
                var thumbTip = hand.Thumb.TopData.Position;
                var dist = Vector3.Distance(thumbTip, hand.GetFingerTip((XrFinger)i).Position);
                if (dist < minDist) minDist = dist;
            }

            return minDist;
        }

        /// <summary>
        /// Returns the flexion degree of a finger.
        /// It corresponds to the signed angle value between the palm and the finger first bone. 
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <param name="fingerId">The finger id to perform calculation</param>
        /// <returns>0f flexion opened - 1f flexion closed</returns>
        public static float GetFingerFlexionValue(this HandPose hand, XrFinger fingerId)
        {
            // TODO
            /*var root = hand.GetWristPosition();
            var joints = QCHTFinger.GetFingerJointsId(fingerId);
            var bone1 = root - hand.points[(int) joints[0]];
            var bone2 = hand.points[(int) joints[1]] - hand.points[(int) joints[0]];
            var angle = Vector3.SignedAngle(bone1, bone2, hand.rotations[(int) joints[0]] * Vector3.right);
            if (angle < 0f) angle += 360f;
            return Mathf.Clamp01(Mathf.Abs((angle - 180f) / 90f));*/
            return 0f;
        }

        /// <summary>
        /// Returns the curl value of a finger.
        /// It corresponds to the distal to tip composed flexion value.
        /// For example a fist gestures occurs when all the finger flexion values and the curl values return 1f. 
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <param name="fingerId">The finger id to perform calculation</param>
        /// <returns>0f when distal/tip rotation is open and 1f when distal/tip rotation is closed</returns>
        public static float GetFingerCurlValue(this HandPose hand, XrFinger fingerId)
        {
            // TODO
            /*var joints = QCHTFinger.GetFingerJointsId(fingerId);

            // First bone joint angle
            var pos1 = hand.points[(int) joints[0]];
            var pos2 = hand.points[(int) joints[1]];
            var pos3 = hand.points[(int) joints[2]];
            var rot1 = hand.rotations[(int) joints[1]];
            var bone1 = pos1 - pos2;
            var bone2 = pos3 - pos2;
            var a1 = Vector3.SignedAngle(bone1, bone2, rot1 * Vector3.right);
            if (a1 < 0f) a1 += 360f;
            var sum = a1;

            if (fingerId > FingerId.THUMB)
            {
                // Second bone joint angle
                var pos4 = hand.points[(int) joints[3]];
                var rot2 = hand.rotations[(int) joints[2]];
                var bone3 = pos4 - pos3;
                bone2 = -bone2; // reflected bone2
                var a2 = Vector3.SignedAngle(-bone2, bone3, rot2 * Vector3.right);
                if (a2 < 0f) a2 += 360f;
                sum += a2;
                sum *= .5f;
            }

            return 1f - Mathf.Clamp01(Mathf.Abs((sum - 180f) / 90f));*/
            return 0f;
        }

        /// <summary>
        /// Returns the abduction value of a finger.
        /// It corresponds to the angle value between the finger and the next finger.
        /// For example the abduction value of the index will return the angle between the base bone of the index and the base bone of the middle finger.
        /// The abduction value of the pinky finger will always return 0. 
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <param name="fingerId">The finger id to perform calculation</param>
        /// <returns>Signed angle of abduction, 0f for adduction >0f for abduction</returns>
        public static float GetFingerAbductionValue(this HandPose hand, XrFinger fingerId)
        {
            // TODO
            // if (fingerId >= FingerId.PINKY)
            //     return 0f;
            //
            // var nextFingerId = fingerId + 1;
            // var fJoints = QCHTFinger.GetFingerJointsId(fingerId);
            // var nfJoints = QCHTFinger.GetFingerJointsId(nextFingerId);
            // var fBase = hand.points[(int) fJoints[0]];
            // var nfBase = hand.points[(int) nfJoints[0]];
            // var fTip = hand.points[(int) fJoints[fJoints.Length - 1]];
            // var nfTip = hand.points[(int) nfJoints[nfJoints.Length - 1]];
            // var baseMidPoint = Vector3.Lerp(fBase, nfBase, 0.5f);
            //
            // Vector3 n1;
            // if (fingerId == FingerId.THUMB)
            // {
            //     n1 = fTip - fBase;
            // }
            // else
            // {
            //     n1 = fTip - baseMidPoint;
            // }
            //
            // var n2 = nfTip - baseMidPoint;
            // var axis = Vector3.Cross(n1, n2);
            // return Vector3.SignedAngle(n1, n2, axis);

            return 0f;
        }

        /// <summary>
        /// Returns the opposition value of a finger from the thumb.
        /// It corresponds to the distance between the thumb tip and the performed finger tip.
        /// The opposition value of the thumb will therefore always return 0.
        /// </summary>
        /// <param name="hand">Self. hand data</param>
        /// <param name="fingerId">The finger id to perform calculation</param>
        /// <param name="normalized">Is normalized by max distance (0f, 1f). see s_maxDistanceThumbTipToFingerTip</param>
        /// <returns>The distance between thumb tip and finger tip</returns>
        public static float GetFingerOppositionValue(this HandPose hand, XrFinger fingerId, bool normalized = false)
        {
            if (fingerId <= XrFinger.XR_HAND_THUMB || fingerId >= XrFinger.XR_HAND_COUNT)
                return 0f;

            var thumbTip = hand.Thumb.TopData.Position;
            var fingerTip = hand.GetFingerTip(fingerId).Position;
            var distance = Vector3.Magnitude(fingerTip - thumbTip);
            return normalized ? distance / s_maxDistanceThumbTipToFingerTip[(int) fingerId] : distance;
        }
        
        /// <summary>
        /// Max distance from thumb tip to finger tip
        /// Based on the open hand pose as reference
        /// </summary>
        private static float[] s_maxDistanceThumbTipToFingerTip = new float[]
        {
            0f, // THUMB
            0.07374594f, // INDEX
            0.09536739f, // MIDDLE
            0.1099648f, // RING
            0.1198689f, // PINKY
        };
    }
}