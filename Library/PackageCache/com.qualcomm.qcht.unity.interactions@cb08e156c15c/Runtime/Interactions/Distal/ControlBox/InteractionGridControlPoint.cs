// /******************************************************************************
//  * File: InteractionGridControlPoint.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace QCHT.Interactions.Distal.ControlBox
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(ParticleSystem))]
    public class InteractionGridControlPoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IRayDataProvider
    {
        public InteractionGrid.InteractionType Type;

        [SerializeField] private bool isCorner;

        [SerializeField] private Vector3 axis = Vector3.one;

        private Collider _pointCollider;
        private ParticleSystem _particleSystem;

        private QCHTControlBoxSettings.HandleState _normalState;
        private QCHTControlBoxSettings.HandleState _hoverState;
        private QCHTControlBoxSettings.HandleState _selectedState;
        private ParticleSystem.MainModule _psMainModule;

        private Vector3 _initialScale = Vector3.zero;
        private Vector3 _opposite;
        private Vector3 _originalPos;
        private Vector3 _originalGizmoPos;
        private Vector3 _diagDir;
        private Quaternion _originalGizmoRot;

        private bool _leftSelected;
        private bool _rightSelected;
        private bool IsSelected => _leftSelected || _rightSelected;

        private bool _leftHover;
        private bool _rightHover;
        private bool IsHovered => _leftHover || _rightHover;

        public delegate void HandleEventHandler(InteractionGridControlPoint handle);

        public event HandleEventHandler onStartInteractingHandle;
        public event HandleEventHandler onStopInteractingHandle;

        private Transform GridTransform => transform.parent;
        private Transform ControlBoxTransform => transform.parent.parent;

        private RayData _hoverRayData;
        public RayData HoverRayData => _hoverRayData;

        private RayData _selectedRayData;
        public RayData SelectedRayData => _selectedRayData;

        #region MonoBehaviour Functions

        private void Awake()
        {
            _pointCollider = GetComponent<Collider>();
            _particleSystem = GetComponent<ParticleSystem>();
            _psMainModule = _particleSystem.main;
        }

        private void Update()
        {
            if (IsSelected)
            {
                _psMainModule.startColor = _selectedState?.color ?? _psMainModule.startColor;
                _psMainModule.startSize = _selectedState?.size ?? _psMainModule.startSize;
                return;
            }

            if (IsHovered)
            {
                _psMainModule.startColor = _hoverState?.color ?? _psMainModule.startColor;
                _psMainModule.startSize = _hoverState?.size ?? _psMainModule.startSize;
                return;
            }

            _psMainModule.startColor = _normalState?.color ?? _psMainModule.startColor;
            _psMainModule.startSize = _normalState?.size ?? _psMainModule.startSize;
        }

        #endregion

        #region IPointerHandlers

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!(eventData is XrPointerEventData qchtEvent))
                return;

            ref var hover = ref qchtEvent.isLeftHand ? ref _leftHover : ref _rightHover;
            hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!(eventData is XrPointerEventData qchtEvent))
                return;

            ref var hover = ref qchtEvent.isLeftHand ? ref _leftHover : ref _rightHover;
            hover = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!(eventData is XrPointerEventData qchtEvent))
                return;

            ref var select = ref qchtEvent.isLeftHand ? ref _leftSelected : ref _rightSelected;
            select = true;

            var t = transform;
            var localHandleBoxPosition = t.localPosition;
            _initialScale = ControlBoxTransform.localScale;
            _originalGizmoPos = ControlBoxTransform.position;
            _originalGizmoRot = ControlBoxTransform.rotation;
            _originalPos = GridTransform.TransformPoint(localHandleBoxPosition);
            _opposite = GridTransform.TransformPoint(-localHandleBoxPosition);
            _diagDir = (_opposite - _originalPos).normalized;

            onStartInteractingHandle?.Invoke(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!(eventData is XrPointerEventData qchtEvent))
                return;

            ref var select = ref qchtEvent.isLeftHand ? ref _leftSelected : ref _rightSelected;
            select = false;

            onStopInteractingHandle?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!(eventData is XrPointerEventData qchtEvent))
                return;

            switch (Type)
            {
                case InteractionGrid.InteractionType.Scale:
                    var initialDist = Vector3.Dot(qchtEvent.pointerPressRaycast.worldPosition - _opposite, _diagDir);
                    var currentDist = Vector3.Dot(qchtEvent.dragPosition - _opposite, _diagDir);
                    var scaleFactor = 1 + (currentDist - initialDist) / initialDist;
                    var targetScale = Vector3.one * Mathf.Clamp(0f, scaleFactor, scaleFactor);
                    var finalScale = _initialScale.Multiply(targetScale);
                    var dir = ControlBoxTransform.InverseTransformDirection(_originalGizmoPos - _opposite);
                    var newPosition = _opposite + ControlBoxTransform.TransformDirection(dir.Multiply(targetScale));
                    ControlBoxTransform.localScale = finalScale;
                    ControlBoxTransform.position = newPosition;
                    break;

                case InteractionGrid.InteractionType.Rotate:
                    var controlBoxPosition = ControlBoxTransform.position;
                    var initDir = Vector3
                        .ProjectOnPlane(qchtEvent.pointerPressRaycast.worldPosition - controlBoxPosition, axis)
                        .normalized;
                    var currentDir = Vector3.ProjectOnPlane(qchtEvent.dragPosition - controlBoxPosition, axis)
                        .normalized;
                    var goal = Quaternion.FromToRotation(initDir, currentDir) * _originalGizmoRot;
                    ControlBoxTransform.rotation = goal;
                    break;
            }

            // Forces end point to handler position
#pragma warning disable CS0618
            QCHTRayData.SetEndPoint(qchtEvent.isLeftHand, transform.position);
#pragma warning restore CS0618
        }

        #endregion

        public void SetStates(QCHTControlBoxSettings.HandleState normal, QCHTControlBoxSettings.HandleState hover,
            QCHTControlBoxSettings.HandleState selected)
        {
            _normalState = normal;
            _hoverState = hover;
            _selectedState = selected;
        }

        public void SetHoverRayData(RayData rayData)
        {
            _hoverRayData = rayData;
        }

        public void SetSelectedRayData(RayData rayData)
        {
            _selectedRayData = rayData;
        }

        public void ToggleVisibility(bool visible)
        {
            if (visible)
            {
                float particleSize;

                if (IsSelected)
                    particleSize = _selectedState.size;
                else if (IsHovered)
                    particleSize = _hoverState.size;
                else
                    particleSize = _normalState.size;

                _psMainModule.startSize = particleSize;
                _particleSystem.Play();
            }
            else
            {
                _particleSystem.Stop();
            }
        }

        public void ToggleActivation(bool on)
        {
            _pointCollider.enabled = on;
        }

        public bool IsHandleFacingUser()
        {
            var t = transform.position;
            var h = Camera.main.transform.position;
            var ht = (t - h).normalized;
            if (Vector3.Dot(ht, transform.forward) < 0) return true;
            if (Vector3.Dot(ht, transform.right) < 0) return true;
            if (isCorner && Vector3.Dot(ht, transform.up) < 0) return true;
            return false;
        }
    }
}