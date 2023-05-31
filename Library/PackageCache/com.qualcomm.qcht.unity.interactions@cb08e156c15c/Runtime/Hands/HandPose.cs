// /******************************************************************************
//  * File: HandPose.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions.Extensions;
using UnityEngine;

namespace QCHT.Interactions.Hands
{
    [Serializable]
    public enum DataSpace
    {
        Local,
        World
    }
    
    [Serializable]
    public struct BoneData
    {
        public bool IsRoot;
        public bool IsThumb;
        public bool UpdatePosition;
        public bool UpdateRotation;
        public bool UpdateScale;
        public Vector3 Position;
        public Quaternion Rotation;
        public float Scale;

        public static BoneData DefaultRoot => new BoneData
        {
            IsRoot = true,
            UpdateScale = true,
            Scale = 1f
        };

        public static BoneData Default => new BoneData
        {
            UpdateRotation = true,
            UpdateScale = false,
            Scale = 1f
        };
        
        public static BoneData DefaultThumb => new BoneData
        {
            IsThumb = true,
            UpdateRotation = true,
            UpdateScale = false,
            Scale = 1f
        };

        public void Flip()
        {
            if (IsRoot)
            {
                // Ignored
            }
            else if (IsThumb)
            {
                Rotation = Rotation.FlipZAxis();
                Rotation = Rotation.FlipXZAxis();
            }
            else
            {
                Rotation = Rotation.FlipXAxis();
                Rotation = Rotation.FlipZXAxis();
            }
        }
    }

    [Serializable]
    public struct FingerData
    {
        public BoneData BaseData;
        public BoneData MiddleData;
        public BoneData TopData;

        public static FingerData Default => new FingerData
        {
            BaseData = BoneData.Default,
            MiddleData = BoneData.Default,
            TopData = BoneData.Default
        };

        public static FingerData DefaultThumb => new FingerData
        {
            BaseData = BoneData.DefaultThumb,
            MiddleData = BoneData.DefaultThumb,
            TopData = BoneData.DefaultThumb
        };

        public void Flip()
        {
            BaseData.Flip();
            MiddleData.Flip();
            TopData.Flip();
        }
    }

    /// <summary>
    /// Stores hand pose.
    /// All position and rotation are stored local.
    /// </summary>
    [CreateAssetMenu(menuName = "QCHT/Interactions/HandPose")]
    public sealed class HandPose : ScriptableObject, ICloneable
    {
        [SerializeField] private XrHandedness _type;
        
        public XrHandedness Type
        {
            set
            {
                if (_type == value)
                    return;

                _type = value;
                Flip();
            }
            get => _type;
        }
        
        public bool IsLeft => Type == XrHandedness.XR_HAND_LEFT;
        
        [SerializeField] private DataSpace space;
        
        public DataSpace Space
        {
            get => space;
            set => space = value;
        }
        
        public Vector3 Scale = Vector3.one;
        
        [Space]
        public BoneData Root;
        public BoneData Palm;
        
        public FingerData Thumb;
        public FingerData Index;
        public FingerData Middle;
        public FingerData Ring;
        public FingerData Pinky;

        [CanBeEmpty, HideInInspector]
        public HandPose HandPoseRefLeft;

        [CanBeEmpty, HideInInspector]
        public HandPose HandPoseRefRight;

        [CanBeEmpty]
        public HandGhostSO Ghost;

        public HandPose()
        {
            Root = BoneData.DefaultRoot;
            Palm = BoneData.Default;
            Thumb = FingerData.DefaultThumb;
            Index = FingerData.Default;
            Middle = FingerData.Default;
            Ring = FingerData.Default;
            Pinky = FingerData.Default;
        }

        public object Clone()
        {
            var handPose = CreateInstance<HandPose>();
            handPose.CopyFrom(this);
            return handPose;
        }

        public void Reset()
        {
            var handPoseRef = IsLeft ? HandPoseRefLeft : HandPoseRefRight;

            if (handPoseRef)
            {
                Root.CopyFrom(handPoseRef.Root);
                Thumb.CopyFrom(handPoseRef.Thumb);
                Index.CopyFrom(handPoseRef.Index);
                Middle.CopyFrom(handPoseRef.Middle);
                Ring.CopyFrom(handPoseRef.Ring);
                Pinky.CopyFrom(handPoseRef.Pinky);
                return;
            }

            Root = BoneData.DefaultRoot;
            Thumb = FingerData.Default;
            Index = FingerData.Default;
            Middle = FingerData.Default;
            Ring = FingerData.Default;
            Pinky = FingerData.Default;
        }

        private void Flip()
        {
            Root.Flip();
            Thumb.Flip();
            Index.Flip();
            Middle.Flip();
            Ring.Flip();
            Pinky.Flip();
        }

        public BoneData GetFingerTip(XrFinger fingerId)
        {
            return fingerId switch
            {
                XrFinger.XR_HAND_THUMB => Thumb.TopData,
                XrFinger.XR_HAND_INDEX => Index.TopData,
                XrFinger.XR_HAND_MIDDLE => Middle.TopData,
                XrFinger.XR_HAND_RING => Ring.TopData,
                XrFinger.XR_HAND_PINKY => Pinky.TopData,
                _ => throw new ArgumentOutOfRangeException(nameof(fingerId), fingerId, null)
            };
        }
    }

    public static class BoneDataExtensions
    {
        public static void CopyFrom(this BoneData boneData, BoneData data)
        {
            boneData.IsRoot = data.IsRoot;
            boneData.IsThumb = data.IsThumb;
            boneData.Position = data.Position;
            boneData.Rotation = data.Rotation;
        }
    }

    public static class FingerDataExtensions
    {
        public static void CopyFrom(this FingerData fingerData, FingerData finger)
        {
            fingerData.BaseData.CopyFrom(finger.BaseData);
            fingerData.MiddleData.CopyFrom(finger.MiddleData);
            fingerData.TopData.CopyFrom(finger.TopData);
        }
    }

    public static class HandPoseExtensions
    {
        public static void CopyFrom(this HandPose handPose, HandPose pose)
        {
            handPose.Root.CopyFrom(pose.Root);
            handPose.Thumb.CopyFrom(pose.Thumb);
            handPose.Index.CopyFrom(pose.Index);
            handPose.Middle.CopyFrom(pose.Middle);
            handPose.Ring.CopyFrom(pose.Ring);
            handPose.Pinky.CopyFrom(pose.Pinky);
        }
    }
}