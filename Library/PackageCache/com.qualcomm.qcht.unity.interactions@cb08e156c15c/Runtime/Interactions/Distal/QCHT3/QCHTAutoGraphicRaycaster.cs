// /******************************************************************************
//  * File: QCHTAutoGraphicRaycaster.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections.Generic;
using QCHT.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QCHT.Interactions.Distal
{
    /// <summary>
    /// Custom graphic raycaster for QCHT.
    /// Largely based on the official Unity's GraphicRaycaster. 
    /// </summary>
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    [RequireComponent(typeof(Canvas))]
    public class QCHTAutoGraphicRaycaster : QCHTGraphicRaycaster
    {
        [SerializeField, Range(0, 1f)] private float maxDot = 1f;
        private List<Graphic> _predefinedGraphics = new List<Graphic>();
        private Vector3 _initialPositionLeft = Vector3.zero;
        private Vector3 _initialPositionRight = Vector3.zero;
        private RaycastHit _lastHitPredefinedLeft = new RaycastHit();
        private RaycastHit _lastHitPredefinedRight = new RaycastHit();

        #region Graphic Raycaster

        /// <summary>
        /// Performs the raycast against the list of graphics associated with the Canvas.
        /// </summary>
        /// <param name="eventData">Current event data</param>
        /// <param name="resultAppendList">List of hit objects to append new results to.</param>
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (!(eventData is XrPointerEventData qchtEvent))
                return;

            if (eventData.dragging)
            {
                UpdateDraggingPosition(resultAppendList, eventData.GetRay(), eventData.pointerDrag,
                    qchtEvent.isLeftHand);
            }
            else
            {
                AutoRaycast(resultAppendList, eventData.GetRay(), qchtEvent.isLeftHand);
            }
        }

        #endregion

        /// <summary>
        /// Auto raycasts
        /// </summary>
        /// <param name="resultAppendList"></param>
        /// <param name="isLeft"></param>
        private void AutoRaycast(List<RaycastResult> resultAppendList, Ray ray, bool isLeft)
        {
            if (canvas == null)
                return;

            _predefinedGraphics.Clear();
            _predefinedGraphics.AddRange(GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas));

            _raycastHitResults.Clear();

            var pointer = isLeft ? _pointerLeft : _pointerRight;
            if (pointer != null) pointer.SetActive(false);

            ref var lastHit = ref isLeft ? ref _lastHitPredefinedLeft : ref _lastHitPredefinedRight;
            ref var initialPosition = ref isLeft ? ref _initialPositionLeft : ref _initialPositionRight;
            GraphicRaycastPredefined(isLeft, ray, ref lastHit, ref initialPosition);
            if (lastHit.Graphic == null)
                return;

            var castResult = new RaycastResult
            {
                gameObject = lastHit.Graphic.gameObject,
                module = this,
                distance = Vector3.Distance(lastHit.WorldPos, ray.origin),
                index = 0,
                depth = lastHit.Graphic.depth,
                worldPosition = lastHit.WorldPos,
                worldNormal = transform.forward,
                screenPosition = eventCamera.WorldToScreenPoint(lastHit.WorldPos, Camera.MonoOrStereoscopicEye.Mono)
            };

            if (pointer != null)
            {
                var sp = isLeft ? _pointerLeftSpriteRenderer : _pointerRightSpriteRenderer;
                var t = pointer.transform;
                t.localScale = Vector3.one * pointerSize;
                SetPointerPosition(pointer, castResult);
                pointer.SetActive(true);
                sp.sortingOrder = canvas.sortingOrder + pointerSortingOrder;
            }

            resultAppendList.Add(castResult);
        }

        protected virtual void UpdateDraggingPosition(List<RaycastResult> resultAppendList, Ray ray,
            GameObject dragObject,
            bool isLeft)
        {
            var pointer = isLeft ? _pointerLeft : _pointerRight;
            if (pointer != null) pointer.SetActive(false);

            ref var lastHit = ref isLeft ? ref _lastHitPredefinedLeft : ref _lastHitPredefinedRight;

            if (lastHit.Graphic == null)
                return;

            var canvasRect = canvas.GetComponent<RectTransform>();

            if (RayIntersectsRectTransform(canvasRect, ray, out var worldPos))
            {
                lastHit.WorldPos = worldPos;
            }

            var castResult = new RaycastResult
            {
                gameObject = dragObject,
                module = this,
                distance = Vector3.Distance(dragObject.transform.position, ray.origin),
                index = resultAppendList.Count,
                depth = lastHit.Graphic.depth,
                worldPosition = lastHit.Graphic.transform.position,
                worldNormal = transform.forward,
                screenPosition = eventCamera.WorldToScreenPoint(lastHit.WorldPos, Camera.MonoOrStereoscopicEye.Mono)
            };

            if (pointer != null)
            {
                var sp = isLeft ? _pointerLeftSpriteRenderer : _pointerRightSpriteRenderer;
                var t = pointer.transform;
                t.localScale = Vector3.one * pointerSize;
                SetPointerPosition(pointer, castResult);
                pointer.SetActive(true);
                sp.sortingOrder = canvas.sortingOrder + pointerSortingOrder;
            }

            resultAppendList.Add(castResult);
        }

        private void GraphicRaycastPredefined(bool isLeftHand, Ray ray, ref RaycastHit hit, ref Vector3 initialPosition)
        {
            var closest = GetClosestElement(isLeftHand, ray);
            hit.Graphic = closest;
            hit.WorldPos = closest ? closest.transform.position : Vector3.zero;
            initialPosition = hit.WorldPos;
        }

        private Graphic GetClosestElement(bool isLeftHand, Ray ray)
        {
            Graphic nearestObject = null;
            if (!QCHTInput.IsHandDetected(isLeftHand))
                return null;

            var shoulderPos = GetShoulderPosition(isLeftHand);
            var dist = 0f;

            foreach (var p in _predefinedGraphics)
            {
                var hTp = (p.transform.position - shoulderPos).normalized;
                var hTh = (ray.origin - shoulderPos).normalized;
                var d = Mathf.Max(Vector3.Dot(hTp, hTh), 0);
                if (d < maxDot)
                    continue;

                if (d > dist)
                {
                    nearestObject = p;
                    dist = d;
                }
            }

            return nearestObject;
        }

        private Vector3 GetShoulderPosition(bool isLeftHand, float offset = 0.15f)
        {
            var head = Camera.main.transform;
            var bottomHead = -head.up;
            var shoulderOffset = isLeftHand ? -head.right : head.right;
            return head.position + head.rotation * ((bottomHead + shoulderOffset) * offset);
        }

        private static void SetPointerPosition(GameObject pointer, RaycastResult raycastResult)
        {
            if (pointer == null)
                return;

            pointer.transform.position = raycastResult.worldPosition;
            pointer.transform.rotation = Quaternion.LookRotation(raycastResult.worldNormal);
        }
    }
}