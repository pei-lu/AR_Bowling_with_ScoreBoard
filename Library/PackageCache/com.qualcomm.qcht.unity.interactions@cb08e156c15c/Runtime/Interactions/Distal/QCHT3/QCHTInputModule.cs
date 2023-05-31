// /******************************************************************************
//  * File: QCHTInputModule.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections.Generic;
using QCHT.Core;
using QCHT.Core.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace QCHT.Interactions.Distal
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public sealed class QCHTInputModule : PointerInputModule
    {
        [SerializeField, Tooltip("Handle click when pinch down instead of pinch up")]
        private bool clickOnDown = true;

        [SerializeField]
        private bool selectionGrab;
        
        [Tooltip("Is left raycast enabled ?")]
        public bool EnableLeftRaycast = true;

        [Tooltip("Override left ray transform.")]
        [CanBeEmpty]
        public Transform LeftRayTransform;

        [Tooltip("Is right raycast enabled ?")]
        public bool EnableRightRaycast = true;

        [Tooltip("Override right ray transform.")]
        [CanBeEmpty]
        public Transform RightRayTransform;
        
        private readonly Dictionary<int, XrPointerEventData> _qchtPointerData = new Dictionary<int, XrPointerEventData>();

        private QCHTDoublePointerEventData _doubleEventData;

        private readonly List<RaycastResult> _qchtRaycastResultCache = new List<RaycastResult>();

        private readonly MouseState _leftMouseState = new MouseState();

        private readonly MouseState _rightMouseState = new MouseState();

        #region InputModule Activation

        public override bool IsModuleSupported() => true;

        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
                return false;

            return QCHTInput.IsHandDetected(true) || QCHTInput.IsHandDetected(false);
        }

        public override void ActivateModule()
        {
            base.ActivateModule();

            var eventData = GetBaseEventData();
            var selectObject = eventSystem.currentSelectedGameObject;
            
            if (selectObject == null)
                selectObject = eventSystem.firstSelectedGameObject;

            eventSystem.SetSelectedGameObject(selectObject, eventData);
        }

        public override void DeactivateModule()
        {
            base.DeactivateModule();
            
            var eventData = GetBaseEventData();

            foreach (var p in _qchtPointerData.Values)
                HandlePointerExitAndEnter(p, null);
            
            foreach (var p in m_PointerData.Values)
                HandlePointerExitAndEnter(p, null);
            
            m_PointerData.Clear();
            eventSystem.SetSelectedGameObject(null, eventData);
        }

        #endregion

        #region Processing Events
        
        public override void Process()
        {
            UpdateQCHTPointerData();
            UpdateMouse();
        }
        
        private void UpdateMouse()
        {
            ProcessMouse(_leftMouseState);
            ProcessMouse(_rightMouseState);
            ProcessDoubleDraggingEvent(_leftMouseState, _rightMouseState);
        }
        
        private void ProcessMouse(MouseState mouseData)
        {
            var eventData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;
            var buttonData = eventData.buttonData;
            ProcessMouseInput(eventData);
            ProcessMove(buttonData);
            ProcessDrag(buttonData);
        }

        private void ProcessDoubleDraggingEvent(MouseState leftMouseData, MouseState rightMouseData)
        {
            var leftEventData = leftMouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;
            var rightEventData = rightMouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            if (!(leftEventData.buttonData is XrPointerEventData qchtLeftData) ||
                !(rightEventData.buttonData is XrPointerEventData qchtRightData))
                return;
            
            if (!qchtLeftData.dragging || !qchtRightData.dragging ||
                qchtLeftData.pointerDrag != qchtRightData.pointerDrag)
            {
                if (_doubleEventData != null)
                    _doubleEventData.doubleDragging = false;
                return;
            }
            
            if (leftEventData.PressedThisFrame() || rightEventData.PressedThisFrame())
            {
                var pointerDragBegin = ExecuteEvents.GetEventHandler<IDoubleDragBeginHandler>(qchtLeftData.pointerDrag);

                if (pointerDragBegin)
                {
                    var pressPosition = Vector3.Lerp(qchtLeftData.pointerPressRaycast.worldPosition,
                        qchtRightData.pointerPressRaycast.worldPosition, 0.5f);

                    _doubleEventData ??= new QCHTDoublePointerEventData(eventSystem);
                    _doubleEventData.doubleDragPressPosition = pressPosition;
                    _doubleEventData.leftData = qchtLeftData;
                    _doubleEventData.rightData = qchtRightData;
                    _doubleEventData.doubleDragging = true;

                    ExecuteEvents.Execute(pointerDragBegin, _doubleEventData,
                        ExecuteEventsExtensions.pointerDoubleDragBeginHandler);
                }
            }

            if (!(_doubleEventData is {doubleDragging: true}))
                return;

            var doubleDragHandler = ExecuteEvents.GetEventHandler<IDoubleDragHandler>(qchtLeftData.pointerDrag);
            if (!doubleDragHandler)
                return;

            var doubleDragPosition = Vector3.Lerp(qchtLeftData.dragPosition, qchtRightData.dragPosition, 0.5f);
            _doubleEventData.doubleDragPosition = doubleDragPosition;

            ExecuteEvents.Execute(doubleDragHandler, _doubleEventData,
                ExecuteEventsExtensions.pointerDoubleDragHandler);
        }
        
        private void ProcessMouseInput(MouseButtonEventData data)
        {
            var qchtEvent = data.buttonData as XrPointerEventData;

            if (qchtEvent == null)
                return;

            var currentOverGo = qchtEvent.pointerCurrentRaycast.gameObject;

            // Pointer Down
            if (data.PressedThisFrame())
            {
                qchtEvent.dragging = false;
                qchtEvent.useDragThreshold = true;
                qchtEvent.eligibleForClick = true;
                qchtEvent.delta = Vector2.zero;
                qchtEvent.pressPosition = qchtEvent.position;
                qchtEvent.pointerPressRaycast = qchtEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, qchtEvent);
                
                var newPressed =
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, qchtEvent, ExecuteEvents.pointerDownHandler);
                
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                var time = Time.unscaledTime;

                if (newPressed == qchtEvent.lastPress)
                {
                    var diffTime = time - qchtEvent.clickTime;

                    if (diffTime < 0.3f)
                        ++qchtEvent.clickCount;
                    else
                        qchtEvent.clickCount = 1;

                    qchtEvent.clickTime = time;
                }
                else
                {
                    qchtEvent.clickCount = 1;
                }

                qchtEvent.pointerPress = newPressed;
                qchtEvent.rawPointerPress = currentOverGo;
                qchtEvent.clickTime = time;
                qchtEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (clickOnDown && qchtEvent.pointerPress != null && qchtEvent.eligibleForClick)
                {
                    ExecuteEvents.Execute(qchtEvent.pointerPress, qchtEvent, ExecuteEvents.pointerClickHandler);
                }

                if (qchtEvent.pointerDrag != null)
                    ExecuteEvents.Execute(qchtEvent.pointerDrag, qchtEvent,
                        ExecuteEvents.initializePotentialDrag);
            }

            // Pointer Up
            if (data.ReleasedThisFrame())
            {
                ExecuteEvents.Execute(qchtEvent.pointerPress, qchtEvent, ExecuteEvents.pointerUpHandler);
                if (!clickOnDown)
                {
                    var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                    if (qchtEvent.pointerPress == pointerUpHandler && qchtEvent.eligibleForClick)
                    {
                        ExecuteEvents.Execute(qchtEvent.pointerPress, qchtEvent,
                            ExecuteEvents.pointerClickHandler);
                    }
                }

                if (qchtEvent.pointerDrag != null)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, qchtEvent, ExecuteEvents.dropHandler);
                }

                qchtEvent.eligibleForClick = false;
                qchtEvent.pointerPress = null;
                qchtEvent.rawPointerPress = null;

                if (qchtEvent.pointerDrag != null && qchtEvent.dragging)
                    ExecuteEvents.Execute(qchtEvent.pointerDrag, qchtEvent, ExecuteEvents.endDragHandler);

                qchtEvent.dragging = false;
                qchtEvent.pointerDrag = null;
                
                if (currentOverGo != qchtEvent.pointerEnter)
                {
                    HandlePointerExitAndEnter(qchtEvent, null);
                    HandlePointerExitAndEnter(qchtEvent, currentOverGo);
                }
            }

            if (currentOverGo != qchtEvent.pointerEnter)
            {
                HandlePointerExitAndEnter(qchtEvent, currentOverGo);
            }
        }

        protected override void ProcessMove(PointerEventData pointerEvent)
        {
            base.ProcessMove(pointerEvent);
            ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent,
                ExecuteEvents.pointerMoveHandler);
        }

        protected override void ProcessDrag(PointerEventData pointerEvent)
        {
            if (!(pointerEvent is XrPointerEventData qchtEvent))
                return;

            if (pointerEvent.pointerDrag != null && !pointerEvent.dragging
                                                 && CanDrag(qchtEvent))
            {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }
            
            if (pointerEvent.dragging && pointerEvent.pointerDrag != null)
            {
                qchtEvent.dragPosition = qchtEvent.worldSpaceRay.origin +
                                         qchtEvent.worldSpaceRay.direction *
                                         qchtEvent.pointerPressRaycast.distance;
                
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }

                if (!(_doubleEventData is {doubleDragging: true}))
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }

            var deltaMagnitude = pointerEvent.scrollDelta.sqrMagnitude;
            if (!Mathf.Approximately(deltaMagnitude, 0f))
            {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(pointerEvent.pointerCurrentRaycast.gameObject);
                ExecuteEvents.ExecuteHierarchy(scrollHandler, pointerEvent, ExecuteEvents.scrollHandler);
            }
        }
        
        private new void HandlePointerExitAndEnter(PointerEventData currentPointerData, GameObject newEnterTarget)
        {
            if (newEnterTarget == null || currentPointerData.pointerEnter == null)
            {
                var hoveredCount = currentPointerData.hovered.Count;

                for (var i = 0; i < hoveredCount; ++i)
                    ExecuteEvents.Execute(currentPointerData.hovered[i], currentPointerData,
                        ExecuteEvents.pointerExitHandler);

                currentPointerData.hovered.Clear();

                if (newEnterTarget == null)
                {
                    currentPointerData.pointerEnter = null;
                    return;
                }
            }
            
            if (currentPointerData.pointerEnter == newEnterTarget && newEnterTarget)
                return;

            var commonRoot = FindCommonRoot(currentPointerData.pointerEnter, newEnterTarget);
            
            if (currentPointerData.pointerEnter != null)
            {
                var t = currentPointerData.pointerEnter.transform;

                while (t)
                {
                    if (commonRoot && commonRoot.transform == t)
                        break;

                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerExitHandler);
                    currentPointerData.hovered.Remove(t.gameObject);
                    t = t.parent;
                }
            }
            
            currentPointerData.pointerEnter = newEnterTarget;

            if (newEnterTarget)
            {
                var t = newEnterTarget.transform;
                while (t != null && t.gameObject != commonRoot)
                {
                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerEnterHandler);
                    currentPointerData.hovered.Add(t.gameObject);
                    t = t.parent;
                }
            }
        }
        
        #endregion
        
        #region QCHTPointerEventData pool

        private void GetPointerData(int id, out XrPointerEventData data, bool create)
        {
            if (_qchtPointerData.TryGetValue(id, out data) || !create)
                return;

            data = new XrPointerEventData(eventSystem)
            {
                pointerId = id,
            };

            _qchtPointerData.Add(id, data);
        }

        #endregion

        private void UpdateQCHTPointerData()
        {
            UpdateMouseState(true, EnableLeftRaycast);
            UpdateMouseState(false, EnableRightRaycast);
        }

        private void UpdateMouseState(bool isLeftHand, bool raycastEnabled)
        {
            var mouseId = isLeftHand ? kMouseLeftId : kMouseRightId;
            GetPointerData(mouseId, out var data, true);
            data.Reset();

            data.button = PointerEventData.InputButton.Left;
            data.useDragThreshold = true;
            data.isLeftHand = isLeftHand;

            var mouseState = isLeftHand ? _leftMouseState : _rightMouseState;
            var buttonState = PointerEventData.FramePressState.NotChanged;
            var pressed = QCHTInput.GestureDown(isLeftHand, GestureId.PINCH);
            pressed |= selectionGrab && QCHTInput.GestureDown(isLeftHand, GestureId.GRAB);
            var released = QCHTInput.GestureUp(isLeftHand, GestureId.PINCH);
            pressed |= selectionGrab && QCHTInput.GestureUp(isLeftHand, GestureId.GRAB);

            if (pressed && released)
                buttonState = PointerEventData.FramePressState.PressedAndReleased;
            if (pressed)
                buttonState = PointerEventData.FramePressState.Pressed;
            if (released)
                buttonState =  PointerEventData.FramePressState.Released;
            
            mouseState.SetButtonState(data.button, buttonState, data);

            var physicsRaycaster = isLeftHand
                ? QCHTPhysicsRaycaster.LeftHandRaycaster
                : QCHTPhysicsRaycaster.RightHandRaycaster;

            var rayTransform = isLeftHand ? LeftRayTransform : RightRayTransform;

            if (rayTransform)
                data.worldSpaceRay = new Ray(rayTransform.position, rayTransform.forward);
            else if (physicsRaycaster != null)
                data.worldSpaceRay = physicsRaycaster.Ray;
            else return;

            if (raycastEnabled)
            {
                eventSystem.RaycastAll(data, _qchtRaycastResultCache);
            }

            var raycast = FindFirstRaycast(_qchtRaycastResultCache);
            data.pointerCurrentRaycast = raycast;
            _qchtRaycastResultCache.Clear();

            var qchtGraphicRaycaster = raycast.module as QCHTGraphicRaycaster;
            if (qchtGraphicRaycaster)
            {
                var position = raycast.screenPosition;
                data.delta = position - data.position;
                data.position = position;
            }

            #region UpdateRay

            QCHTRayData.SetState(isLeftHand, RAY_STATE.RAY_STATE_NONE);

            var isOver = raycast.gameObject;
            var isSelecting = isOver && data.pointerPress;
            var isDragging = _doubleEventData is {doubleDragging: true} || data.dragging;

            if (isOver || isDragging)
            {
                QCHTRayData.AddState(isLeftHand, RAY_STATE.RAY_STATE_REST);
                QCHTRayData.AddState(isLeftHand, RAY_STATE.RAY_STATE_HOVER);

                if (isSelecting)
                    QCHTRayData.AddState(isLeftHand, RAY_STATE.RAY_STATE_SELECTED);
                else
                    QCHTRayData.RemoveState(isLeftHand, RAY_STATE.RAY_STATE_SELECTED);

                if (isDragging)
                    QCHTRayData.AddState(isLeftHand, RAY_STATE.RAY_STATE_DRAG);
                else
                    QCHTRayData.RemoveState(isLeftHand, RAY_STATE.RAY_STATE_DRAG);

                if (isOver && QCHTRayData.GetState(isLeftHand) < RAY_STATE.RAY_STATE_DRAG)
                    QCHTRayData.SetEndPoint(isLeftHand, raycast.worldPosition);
                else if (QCHTRayData.GetState(isLeftHand) >= RAY_STATE.RAY_STATE_DRAG)
                {
                    if (qchtGraphicRaycaster != null)
                        QCHTRayData.SetEndPoint(data.isLeftHand, raycast.worldPosition);
                    else
                        QCHTRayData.SetEndPoint(data.isLeftHand, data.dragPosition);
                }
            }
            else
            {
                QCHTRayData.RemoveState(isLeftHand, RAY_STATE.RAY_STATE_HOVER);
                if (physicsRaycaster != null && physicsRaycaster.enabled)
                    QCHTRayData.AddState(isLeftHand, RAY_STATE.RAY_STATE_REST);
                else
                    QCHTRayData.RemoveState(isLeftHand, RAY_STATE.RAY_STATE_REST);
            }

            var dragObj = data.pointerDrag;
            if (dragObj)
            {
                var rayData = dragObj.GetComponent<IRayDataProvider>();
                if (!ReferenceEquals(rayData, null))
                {
                    QCHTRayData.SetReticleImage(isLeftHand, rayData.SelectedRayData.ReticleSprite);
                    QCHTRayData.SetReticleColor(isLeftHand, rayData.SelectedRayData.ReticleColor);
                    QCHTRayData.SetRayColor(isLeftHand, rayData.SelectedRayData.RayColor);
                    QCHTRayData.SetReticleScaleFactor(isLeftHand, rayData.SelectedRayData.ReticleScaleFactor);
                    return;
                }
            }

            var enterObj = data.pointerEnter;
            if (enterObj)
            {
                var rayData = enterObj.GetComponent<IRayDataProvider>();
                if (!ReferenceEquals(rayData, null))
                {
                    QCHTRayData.SetReticleImage(isLeftHand, rayData.HoverRayData.ReticleSprite);
                    QCHTRayData.SetReticleColor(isLeftHand, rayData.HoverRayData.ReticleColor);
                    QCHTRayData.SetRayColor(isLeftHand, rayData.HoverRayData.RayColor);
                    QCHTRayData.SetReticleScaleFactor(isLeftHand, rayData.HoverRayData.ReticleScaleFactor);
                    return;
                }
            }

            QCHTRayData.SetReticleImage(isLeftHand, null);
            QCHTRayData.SetReticleColor(isLeftHand, null);
            QCHTRayData.SetRayColor(isLeftHand, null);
            QCHTRayData.SetReticleScaleFactor(isLeftHand, 1.0f);

            #endregion
        }
        
        private bool CanDrag(XrPointerEventData pointerEvent)
        {
            if (!pointerEvent.useDragThreshold) return true;
            var hand = pointerEvent.isLeftHand ? QCHTInput.LeftHand : QCHTInput.RightHand;
            var p = (pointerEvent.pointerPressRaycast.worldPosition - hand.GetWristPosition()).normalized;
            var d = pointerEvent.worldSpaceRay.direction.normalized;
            var th = Mathf.Cos(Mathf.Deg2Rad);
            var pdDot = Vector3.Dot(p, d);
            return pdDot < th;
        }

        public override bool IsPointerOverGameObject(int pointerId)
        {
            GetPointerData(pointerId, out var data, false);

            if (data != null)
                return data.pointerEnter != null;

            return false;
        }
    }
}