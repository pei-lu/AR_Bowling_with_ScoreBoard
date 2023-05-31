// /******************************************************************************
//  * File: IHandJointUpdater.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

namespace QCHT.Interactions.Hands
{
    public interface IHandJointUpdater
    {
        public void UpdateJoint(DataSpace space, BoneData data);
    }
}