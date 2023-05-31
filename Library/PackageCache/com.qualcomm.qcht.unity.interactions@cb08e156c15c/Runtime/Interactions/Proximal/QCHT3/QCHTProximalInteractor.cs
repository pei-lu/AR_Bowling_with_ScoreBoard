// /******************************************************************************
//  * File: QCHTProximalInteractor.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Core;
using QCHT.Core.Extensions;
using QCHT.Interactions.Core;
using QCHT.Interactions.Extensions;
using QCHT.Interactions.Hands;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Interactions.Proximal
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public sealed class QCHTProximalInteractor : MonoBehaviour
    {
        private static InteractionData s_leftInteractionData;
        private static InteractionData s_rightInteractionData;
        private static DoubleInteractionData s_doubleInteractionData;

        public static bool IsLeftGrabbing =>
            s_leftInteractionData != null &&
            (s_leftInteractionData.ProximalGrab || s_leftInteractionData.ProximalDoubleGrab);

        public static bool IsRightGrabbing =>
            s_rightInteractionData != null &&
            (s_rightInteractionData.ProximalGrab || s_rightInteractionData.ProximalDoubleGrab);

        private void Update() => ProcessControllers();

        private void ProcessControllers()
        {
            if (s_leftController)
                ProcessController(s_leftController, s_leftInteractionData);

            if (s_rightController)
                ProcessController(s_rightController, s_rightInteractionData);

            if (s_doubleInteractionData != null)
                ProcessDoubleGrab(s_doubleInteractionData);
        }

        private void ProcessController(IInteractionController controller, InteractionData interactionData)
        {
            GetDataFromController(controller, interactionData);
            ProcessHandInput(controller, interactionData);
            ProcessGrab(interactionData);
        }

        private static void GetDataFromController(IInteractionController controller, InteractionData eventData)
        {
            if (controller == null)
                return;

            var results = controller.Process();

            if (!results.currentGameObject)
            {
                if (eventData.ProximalEnter)
                {
                    ExecuteEvents.ExecuteHierarchy(eventData.ProximalEnter, eventData,
                        ExecuteProximalEventsExtensions.exitHandler);
                    eventData.ProximalEnter = null;
                }
            }

            if (!eventData.ProximalEnter || eventData.ProximalEnter != results.currentGameObject)
            {
                eventData.ProximalEnter = results.currentGameObject;

                if (eventData.ProximalEnter && !eventData.ProximalGrab)
                {
                    ExecuteEvents.ExecuteHierarchy(eventData.ProximalEnter, eventData,
                        ExecuteProximalEventsExtensions.enterHandler);
                    var dataProvider = eventData.ProximalEnter.GetComponentInChildren<IInteractionDataProvider>();
                    eventData.Data = dataProvider;
                }
            }
        }

        private void ProcessHandInput(IInteractionController controller, InteractionData eventData)
        {
            if (GetGestureDownThisFrame(eventData))
            {
                if (!eventData.ProximalGrab)
                {
                    var grabbable = ExecuteEvents.GetEventHandler<IProximalGrabHandler>(eventData.ProximalEnter);

                    if (grabbable)
                    {
                        ExecuteEvents.ExecuteHierarchy(grabbable, eventData,
                            ExecuteProximalEventsExtensions.beginGrabHandler);
                        eventData.ProximalGrab = grabbable;
                        controller.OnBeginInteraction();
                    }
                }
            }
            else if (GetReleaseGestureThisFrame(eventData))
            {
                if (eventData.ProximalGrab)
                { 
                    ExecuteEvents.ExecuteHierarchy(eventData.ProximalGrab, eventData,
                        ExecuteProximalEventsExtensions.endGrabHandler);
                    eventData.ProximalGrab = null;
                    eventData.ProximalDoubleGrab = null;
                    controller.OnEndInteraction();
                }
            }
        }

        private static void ProcessDoubleGrab(DoubleInteractionData eventData)
        {
            var leftData = eventData.LeftData;
            var rightData = eventData.RightData;

            // Totally stops double grab when one hand stopped double grabbing
            if (leftData.ProximalDoubleGrab != rightData.ProximalDoubleGrab)
            {
                leftData.ProximalDoubleGrab = null;
                rightData.ProximalDoubleGrab = null;

                // Get back to proximal for the hand which still grabbing
                var data = leftData.ProximalGrab ? leftData : rightData;
                if (data.ProximalGrab)
                {
                    ExecuteEvents.ExecuteHierarchy(data.ProximalGrab, data,
                        ExecuteProximalEventsExtensions.beginGrabHandler);
                }
            }

            // Begin double grabbing
            if (!leftData.ProximalDoubleGrab && leftData.ProximalGrab &&
                leftData.ProximalGrab == rightData.ProximalGrab)
            {
                var doubleGrab = ExecuteEvents.GetEventHandler<IDoubleGrabHandler>(leftData.ProximalGrab);

                if (doubleGrab)
                {
                    ExecuteEvents.ExecuteHierarchy(doubleGrab, s_doubleInteractionData,
                        ExecuteProximalEventsExtensions.beginDoubleGrabHandler);
                    leftData.ProximalDoubleGrab = doubleGrab;
                    rightData.ProximalDoubleGrab = doubleGrab;
                }
            }

            // Double grabbing
            if (leftData.ProximalDoubleGrab)
            {
                var doubleGrab = ExecuteEvents.GetEventHandler<IDoubleGrabHandler>(leftData.ProximalDoubleGrab);

                if (doubleGrab)
                {
                    ExecuteEvents.Execute(doubleGrab, s_doubleInteractionData,
                        ExecuteProximalEventsExtensions.doubleGrabHandler);
                    leftData.ProximalDoubleGrab = doubleGrab;
                    rightData.ProximalDoubleGrab = doubleGrab;
                }
            }
        }

        private static void ProcessGrab(InteractionData eventData)
        {
            if (eventData.ProximalGrab && !eventData.ProximalDoubleGrab)
            {
                ExecuteEvents.Execute(eventData.ProximalGrab, eventData, ExecuteProximalEventsExtensions.grabHandler);
            }
        }

        #region static

        private static bool GetGestureDownThisFrame(InteractionData eventData)
        {
            var isLeft = eventData.Controller.Handedness == XrHandedness.XR_HAND_LEFT;
            var hand = isLeft ? QCHTInput.LeftHand : QCHTInput.RightHand;
            var releaseThreshold = hand.GetMinDistToThumbTip();
            return releaseThreshold < 0.055f;
        }

        private static bool GetReleaseGestureThisFrame(InteractionData eventData)
        {
            var isLeft = eventData.Controller.Handedness == XrHandedness.XR_HAND_LEFT;
            var hand = isLeft ? QCHTInput.LeftHand : QCHTInput.RightHand;
            var releaseThreshold = hand.GetMinDistToThumbTip();
            return releaseThreshold >= 0.055f;
        }

        private static QCHTHandInteractionController s_leftController;
        private static QCHTHandInteractionController s_rightController;

        public static void RegisterController(QCHTHandInteractionController controller)
        {
            switch (controller.Handedness)
            {
                case XrHandedness.XR_HAND_LEFT:
                    s_leftController = controller;
                    s_leftInteractionData = new InteractionData(EventSystem.current) {Controller = s_leftController};
                    break;

                case XrHandedness.XR_HAND_RIGHT:
                    s_rightController = controller;
                    s_rightInteractionData = new InteractionData(EventSystem.current)
                        {Controller = s_rightController};
                    break;
            }

            if (s_leftController && s_rightController)
            {
                s_doubleInteractionData ??= new DoubleInteractionData(EventSystem.current);
                s_doubleInteractionData.LeftData = s_leftInteractionData;
                s_doubleInteractionData.RightData = s_rightInteractionData;
            }
        }

        public static void UnRegisterController(XrHandedness handType)
        {
            ref var controller = ref handType == XrHandedness.XR_HAND_LEFT ? ref s_leftController : ref s_rightController;
            ref var eventData = ref handType == XrHandedness.XR_HAND_LEFT ? ref s_leftInteractionData : ref s_rightInteractionData;

            if (eventData != null)
            {
                // Exit
                if (eventData.ProximalEnter)
                {
                    ExecuteEvents.ExecuteHierarchy(eventData.ProximalEnter, eventData,
                        ExecuteProximalEventsExtensions.exitHandler);
                }

                // Stop grabbing
                if (eventData.ProximalGrab)
                {
                    ExecuteEvents.ExecuteHierarchy(eventData.ProximalGrab, eventData,
                        ExecuteProximalEventsExtensions.endGrabHandler);
                } 
            }
            
            controller = null;
            eventData = null;

            if (s_leftController == null || s_rightController == null)
                s_doubleInteractionData = null;
        }

        #endregion
    }
}