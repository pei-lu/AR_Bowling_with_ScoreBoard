// /******************************************************************************
//  * File: QCHTIHandInputHandler.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEngine.InputSystem;

namespace QCHT.Core
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public interface QCHTIHandInputHandler
    {
        public void OnLeftHandInsert(InputAction.CallbackContext context);
        public void OnRightHandInsert(InputAction.CallbackContext context);
        public void OnHandsRemove(InputAction.CallbackContext context);
        public void OnHandsReleased(InputAction.CallbackContext context);
        public void OnPinch(InputAction.CallbackContext context);
        public void OnGrab(InputAction.CallbackContext context);
        public void OnFlip(InputAction.CallbackContext context);
        public void OnMousePosition(InputAction.CallbackContext context);
        public void OnMouseScroll(InputAction.CallbackContext context);
    }
}