// /******************************************************************************
//  * File: ProximalInteractionController.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Hands;
using UnityEngine;

namespace QCHT.Interactions.Proximal
{
    public class ProximalInteractionController : MonoBehaviour, IInteractionController
    {
        [SerializeField] private XrHandedness handedness;
        [SerializeField] private HandPoseDriver driver; 
        [SerializeField] private HandPartPresenter root;

        private InteractionResult _result;

        private void OnEnable() => ProximalInteractor.RegisterController(this, handedness);

        private void OnDisable()
        {
            // Force releasing interaction if the Controller is disabled
            ReleaseInteraction();

            ProximalInteractor.UnRegisterController(handedness);
        }

        #region IInteractionController

        public XrHandedness Handedness => handedness;
        public HandPoseDriver PoseDriver => driver;

        public InteractionResult Process()
        {
            _result.currentGameObject = root.TriggeredObject;
            return _result;
        }

        public void OnBeginInteraction() => root.ForceMatching(true);
        public void OnEndInteraction() => root.ForceMatching(false);
        public void ReleaseInteraction() => root.ReleaseTrigger();

        #endregion
    }
}