// /******************************************************************************
//  * File: HandJointUpdater.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Hands
{
    public class HandJointUpdater : MonoBehaviour, IHandJointUpdater
    {
        public virtual void UpdateJoint(DataSpace space, BoneData data)
        {
            var t = transform;
            
            if (space == DataSpace.World)
            {
                if (data.UpdatePosition)
                    t.position = data.Position;

                if (data.UpdateRotation)
                    t.rotation = data.Rotation;
            }
            else
            {
                if (data.UpdatePosition)
                    t.localPosition = data.Position;

                if (data.UpdateRotation && data.IsRoot)
                    t.rotation = data.Rotation;
                else if (data.UpdateRotation)
                    t.localRotation = data.Rotation;
            }
            
            if (data.UpdateScale)
                t.localScale = data.Scale * Vector3.one;
        }

        public override string ToString()
        {
            return $" world position = {transform.position.ToString("F4")}," +
                   $" world rotation = {transform.rotation.ToString("F4")} " +
                   $" local scale = {transform.localScale.ToString("F4")} ";
        }
    }
}