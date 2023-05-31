// /******************************************************************************
//  * File: QCHTHandInteractionController.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Core;
using QCHT.Interactions.Hands;
using UnityEngine;

namespace QCHT.Interactions.Proximal
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public class QCHTHandInteractionController : MonoBehaviour, IInteractionController
    {
        public HandType Type;
        public HandPoseDriver Driver;
        public HandPartPresenter Root;
        
        private InteractionResult _result;

        private void OnEnable() => QCHTProximalInteractor.RegisterController(this);

        private void OnDisable()
        {
            // Force releasing interaction if the Controller is disabled
            ReleaseInteraction();

            QCHTProximalInteractor.UnRegisterController((XrHandedness)Type);
        }

        #region IInteractionController

        public XrHandedness Handedness => (XrHandedness)Type;
        public HandPoseDriver PoseDriver => Driver;

        public InteractionResult Process()
        {
            _result.currentGameObject = Root.TriggeredObject;
            return _result;
        }

        public void OnBeginInteraction() => Root.ForceMatching(true);
        public void OnEndInteraction() => Root.ForceMatching(false);
        public void ReleaseInteraction() => Root.ReleaseTrigger();

        #endregion
    }
}