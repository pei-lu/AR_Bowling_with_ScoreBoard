// /******************************************************************************
//  * File: GrabPoint.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using QCHT.Interactions.Hands;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Proximal
{
    public enum SnapGestureId
    {
        None = XrHandGesture.XR_HAND_UNKNOWN,
        Pinch = XrHandGesture.XR_HAND_PINCH,
        Grab = XrHandGesture.XR_HAND_GRAB,
    }

    public sealed class GrabPoint : MonoBehaviour, IInteractionDataProvider
    {
        [SerializeField] private InteractableType interactionType = InteractableType.Free;
        [SerializeField] private SnapGestureId mainGesture = SnapGestureId.Grab;
        [SerializeField, NonReorderable] private SnapData[] snapData;
        public IEnumerable<SnapData> SnapData => snapData;
        public InteractableType Type => interactionType;
        public XrHandGesture GrabGesture => (XrHandGesture) mainGesture;

        [SerializeField] private XRGrabInteractable _xrInteractable;
        [SerializeField] private ProximalInteractable _proximalInteractable;

        private bool _wasDynamicAttached;

        public void OnEnable() {
            _xrInteractable = _xrInteractable ? _xrInteractable : GetComponentInParent<XRGrabInteractable>();
            
            if (_xrInteractable != null) {
                _xrInteractable.selectEntered.AddListener(OnSelectEntered);
                _xrInteractable.selectExited.AddListener(OnSelectExited);
            }

            _proximalInteractable = _proximalInteractable ? _proximalInteractable : GetComponentInParent<ProximalInteractable>();
            
            if (_proximalInteractable != null) {
                _proximalInteractable.OnProximalGrabEvent.AddListener(OnSelectEntered);
                _proximalInteractable.OnProximalReleaseEvent.AddListener(OnSelectExited);
            }
        }

        public void OnDisable() {
            if (_xrInteractable != null) {
                _xrInteractable.selectEntered.RemoveListener(OnSelectEntered);
                _xrInteractable.selectExited.RemoveListener(OnSelectExited);
            }

            if (_proximalInteractable != null) {
                _proximalInteractable.OnProximalGrabEvent.RemoveListener(OnSelectEntered);
                _proximalInteractable.OnProximalReleaseEvent.RemoveListener(OnSelectExited);
            }
        }

        #region XR Grab Interactable

        private void OnSelectEntered(SelectEnterEventArgs args) {
            var interactor = args.interactorObject as XRDirectInteractor;
            if (interactor == null)
                return;

            var data = OnSelectEntered(interactor.gameObject);
            if (data != null) {
                interactor.attachTransform.localPosition = Quaternion.Inverse(Quaternion.Euler(data.LocalOffsetRotation)) * -data.LocalOffsetPosition;
                interactor.attachTransform.localRotation = Quaternion.Inverse(Quaternion.Euler(data.LocalOffsetRotation));
            }
        }

        private void OnSelectExited(SelectExitEventArgs args) {
            var interactor = args.interactorObject as XRDirectInteractor;
            if (interactor == null)
                return;

            OnSelectExited(interactor.gameObject);
        }

        #endregion

        #region Proximal Interactable

        private void OnSelectEntered(InteractionData args) {
            var interactor = args.Controller as ProximalInteractionController;
            if (interactor == null)
                return;

            OnSelectEntered(interactor.gameObject);
        }

        private void OnSelectExited(InteractionData args) {
            var interactor = args.Controller as ProximalInteractionController;
            if (interactor == null)
                return;

            OnSelectExited(interactor.gameObject);
        }

        #endregion

        private SnapData OnSelectEntered(GameObject interactorObject) {
            var driver = interactorObject.GetComponent<HandPoseDriver>();
            if (driver == null)
                return null;

            try {
                var data = SnapData.First(x => x.HandPose.Type == driver.Provider.HandType);
                driver.HandPose = data.HandPose;
                driver.HandPoseMask = data.HandPoseMask;
                
                if (_xrInteractable != null) {
                    // force no dynamic attach
                    _wasDynamicAttached = _xrInteractable.useDynamicAttach;
                    _xrInteractable.useDynamicAttach = false;
                }
                
                return data;
            }
            catch (Exception) {
                // ignored
            }

            return null;
        }

        private void OnSelectExited(GameObject interactorObject) {
            var driver = interactorObject.GetComponent<HandPoseDriver>();
            if (driver == null)
                return;

            driver.HandPose = null;
            driver.HandPoseMask = null;
            driver.RootTransform.localPosition = Vector3.zero;
            driver.RootTransform.localRotation = Quaternion.identity;
            
            if (_xrInteractable != null) {
                _xrInteractable.useDynamicAttach = _wasDynamicAttached;
            }
        }
    }
}