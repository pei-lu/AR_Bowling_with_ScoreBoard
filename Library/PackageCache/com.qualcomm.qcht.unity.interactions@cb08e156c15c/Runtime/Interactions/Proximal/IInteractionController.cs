// /******************************************************************************
//  * File: IInteractionController.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Hands;

namespace QCHT.Interactions.Proximal
{
    public interface IInteractionController
    {
        public XrHandedness Handedness { get; }
        public HandPoseDriver PoseDriver { get; }
        public InteractionResult Process();
        public void OnBeginInteraction();
        public void OnEndInteraction();
    }
}