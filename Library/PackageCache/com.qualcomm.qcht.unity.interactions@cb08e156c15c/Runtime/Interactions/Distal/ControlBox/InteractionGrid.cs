// /******************************************************************************
//  * File: InteractionGrid.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace QCHT.Interactions.Distal.ControlBox
{
    public class InteractionGrid : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDoubleDragBeginHandler, IDoubleDragHandler,
        IRayDataProvider
    {
        public enum InteractionType
        {
            None,
            Move,
            Rotate,
            Scale
        }

        private const string LightLeftKeyword = "LIGHT_L";
        private const string LightRightKeyword = "LIGHT_R";
        private const string Selected = "SELECTED";

        private static readonly int s_lightPosLeft = Shader.PropertyToID("_LightPosL");
        private static readonly int s_lightPosRight = Shader.PropertyToID("_LightPosR");
        private static readonly int s_lightColor = Shader.PropertyToID("_LightColor");
        
        private Material _material;
        private Collider _collider;

        private InteractionGridControlPoint[] _handlers;

        private Vector3 _localLightPosLeft;
        private Vector3 _localLightPosRight;

        private InteractionGridControlPoint _currentHandle;
        private bool IsInteractingWithAnHandle => _currentHandle != null;

        private Transform ControlBoxTransform => transform.parent;

        private bool _leftSelected;
        private bool _rightSelected;
        private bool IsSelected => _leftSelected | _rightSelected;

        private bool _leftHover;
        private bool _rightHover;
        private bool IsHover => _leftHover || _rightHover;

        private Vector3 _initialControlBoxPosition;
        private Vector3 _initialLocalPositionDeltaLeft;
        private Vector3 _initialLocalPositionDeltaRight;
        private Vector3 _initialControlBoxScale;
        private Vector3 _offsetToControlBoxCenter;

        private float _smoothSpeed;

        private RayData _hoverRayData;
        public RayData HoverRayData => _hoverRayData;

        private RayData _selectedRayData;
        public RayData SelectedRayData => _selectedRayData;

        public event Action onHandled;
        public event Action onReleased;

        private QCHTControlBoxSettings _settings;

        public QCHTControlBoxSettings Settings
        {
            private get => _settings;
            set
            {
                _settings = value;
                foreach (var handler in _handlers)
                    handler.SetStates(Settings.NormalState, Settings.HoverState, Settings.SelectedState);
            }
        }
        
        #region MonoBehaviour Functions

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _material = GetComponent<Renderer>().material;
            _handlers = GetComponentsInChildren<InteractionGridControlPoint>();
        }

        private void OnEnable()
        {
            foreach (var handle in _handlers)
            {
                handle.onStartInteractingHandle += OnStartInteractingWithHandle;
                handle.onStopInteractingHandle += OnStopInteractingWithHandle;
            }
        }

        private void OnDisable()
        {
            foreach (var handle in _handlers)
            {
                handle.onStartInteractingHandle -= OnStartInteractingWithHandle;
                handle.onStopInteractingHandle -= OnStopInteractingWithHandle;
            }
        }

        private void Update()
        {
            _smoothSpeed = Settings.SmoothSpeed;
            _collider.isTrigger = Settings.IsTrigger;
            UpdateLayer();
            UpdateHandles();
            UpdateRayData();
            UpdateLightColor();
            UpdateLightPositionInMaterial();
        }

        public void LateUpdate()
        {
            if (ControlBoxTransform.localScale.x > _settings.MaxScale)
                ControlBoxTransform.localScale = _settings.MaxScale * Vector3.one;
            
            if (ControlBoxTransform.localScale.x < _settings.MinScale)
                ControlBoxTransform.localScale = _settings.MinScale * Vector3.one;
        }

        #endregion

        #region IPointerHandlers

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!(eventData is XrPointerEventData qchtEvent))
                return;

            ref var hover = ref qchtEvent.isLeftHand ? ref _leftHover : ref _rightHover;
            hover = true;

            if (qchtEvent.dragging)
                return;

            AddLight(qchtEvent.isLeftHand);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!(eventData is XrPointerEventData qchtEvent))
                return;

            ref var hover = ref qchtEvent.isLeftHand ? ref _leftHover : ref _rightHover;
            hover = false;

            if (qchtEvent.dragging)
                return;

            RemoveLight(qchtEvent.isLeftHand);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!(eventData is XrPointerEventData qchtEvent) || qchtEvent.dragging)
                return;

            SetLightPosition(qchtEvent.isLeftHand, eventData.pointerCurrentRaycast.worldPosition);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!(eventData is XrPointerEventData qchtEvent))
                return;

            ref var select = ref qchtEvent.isLeftHand ? ref _leftSelected : ref _rightSelected;
            select = true;

            ref var initialLocalPosition = ref qchtEvent.isLeftHand
                ? ref _initialLocalPositionDeltaLeft
                : ref _initialLocalPositionDeltaRight;

            var cbPosition = ControlBoxTransform.position;
            initialLocalPosition = cbPosition - qchtEvent.pointerPressRaycast.worldPosition;
            _initialControlBoxPosition = cbPosition;

            Select();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!(eventData is XrPointerEventData qchtEvent))
                return;

            ref var selected = ref qchtEvent.isLeftHand ? ref _leftSelected : ref _rightSelected;
            selected = false;

            Deselect();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!(eventData is XrPointerEventData qchtEvent))
                return;

            var initialLocalPosition =
                qchtEvent.isLeftHand ? _initialLocalPositionDeltaLeft : _initialLocalPositionDeltaRight;

            var dragPosition = qchtEvent.dragPosition;
            var controlBoxPosition = ControlBoxTransform.position;
            var draggingOffset = initialLocalPosition;

            if (Settings.Constraint == QCHTControlBoxSettings.CameraConstraint.Horizontal)
            {
                dragPosition.y = _initialControlBoxPosition.y;
                draggingOffset.y = 0;
            }
            else if (Settings.Constraint == QCHTControlBoxSettings.CameraConstraint.Vertical)
            {
                dragPosition.x = _initialControlBoxPosition.x;
                draggingOffset.x = 0;
            }

            var targetPosition = dragPosition + draggingOffset;

            var dt = Time.deltaTime * _smoothSpeed;
            controlBoxPosition = Vector3.Lerp(controlBoxPosition, targetPosition, dt);
            ControlBoxTransform.position = controlBoxPosition;

#pragma warning disable CS0618
            QCHTRayData.SetEndPoint(qchtEvent.isLeftHand, controlBoxPosition - initialLocalPosition);
#pragma warning restore CS0618
        }

        public void OnBeginDoubleDrag(QCHTDoublePointerEventData qchtEvent)
        {
            _initialControlBoxScale = ControlBoxTransform.localScale;
            _offsetToControlBoxCenter =
                ControlBoxTransform.InverseTransformVector(qchtEvent.doubleDragPressPosition -
                                                           ControlBoxTransform.position);
        }

        public void OnDoubleDrag(QCHTDoublePointerEventData qchtDoubleEventData)
        {
            var pressPosLeft = qchtDoubleEventData.leftData.pointerPressRaycast.worldPosition;
            var pressPosRight = qchtDoubleEventData.rightData.pointerPressRaycast.worldPosition;
            var dragPosLeft = qchtDoubleEventData.leftData.dragPosition;
            var dragPosRight = qchtDoubleEventData.rightData.dragPosition;
            var initDistance = Vector3.Distance(pressPosLeft, pressPosRight);
            var currentDistance = Vector3.Distance(dragPosLeft, dragPosRight);

            var scaleFactor = 1 + (currentDistance - initDistance) / initDistance;
            var targetScale = _initialControlBoxScale.Multiply(scaleFactor * Vector3.one);
            ControlBoxTransform.localScale = targetScale;
            ControlBoxTransform.position = Vector3.Lerp(ControlBoxTransform.position,
                qchtDoubleEventData.doubleDragPosition - ControlBoxTransform.TransformVector(_offsetToControlBoxCenter),
                Time.deltaTime * _smoothSpeed);

            // Force scale reticle
            QCHTRayData.SetReticleImage(true, Settings.Resize);
            QCHTRayData.SetReticleImage(false, Settings.Resize);
        }

        #endregion

        #region InteractionGridControlPoint callbacks

        private void OnStartInteractingWithHandle(InteractionGridControlPoint handle)
        {
            _currentHandle = handle;
            Select();
        }

        private void OnStopInteractingWithHandle(InteractionGridControlPoint handle)
        {
            _currentHandle = null;
            Deselect();
        }

        #endregion

        private void Select()
        {
            _material.EnableKeyword(Selected);
            onHandled?.Invoke();
        }

        private void Deselect()
        {
            _material.DisableKeyword(Selected);
            onReleased?.Invoke();
        }

        private void AddLight(bool isLeft)
        {
            _material.EnableKeyword(isLeft ? LightLeftKeyword : LightRightKeyword);
        }

        private void RemoveLight(bool isLeft)
        {
            _material.DisableKeyword(isLeft ? LightLeftKeyword : LightRightKeyword);
        }

        private void UpdateLightColor()
        {
            var color = IsHover ? Settings.HoverColor : Color.white;
            color = IsSelected ? Settings.SelectedColor : color;
            _material.SetColor(s_lightColor, color);
        }

        private void SetLightPosition(bool isLeft, Vector3 position)
        {
            ref var localPos = ref isLeft ? ref _localLightPosLeft : ref _localLightPosRight;
            localPos = ControlBoxTransform.InverseTransformPoint(position);
        }

        private void UpdateLayer()
        {
            if (Settings.Layer == gameObject.layer) return;
            gameObject.layer = Settings.Layer;
            foreach (var handler in _handlers)
                handler.gameObject.layer = Settings.Layer;
        }

        private void UpdateRayData()
        {
            var hoverData = new RayData
            {
                ReticleSprite = Settings.Move,
                ReticleColor = Settings.HoverReticleColor,
                ReticleScaleFactor = Settings.HoverReticleSize,
                RayColor = Settings.HoverRayColor
            };

            _hoverRayData = hoverData;

            var selectedData = new RayData
            {
                ReticleSprite = Settings.Move,
                ReticleColor = Settings.SelectedReticleColor,
                ReticleScaleFactor = Settings.SelectedReticleSize,
                RayColor = Settings.SelectedRayColor
            };

            _selectedRayData = selectedData;
        }

        private void UpdateHandles()
        {
            var type = Settings.DisplayType;
            foreach (var handler in _handlers)
            {
                if (type == QCHTControlBoxSettings.ControlBoxDisplayType.Never)
                {
                    handler.ToggleActivation(false);
                    handler.ToggleVisibility(false);
                    continue;
                }

                if (IsInteractingWithAnHandle)
                {
                    // Interacting show only selected
                    var eq = _currentHandle.Equals(handler);
                    handler.ToggleActivation(eq);
                    handler.ToggleVisibility(eq);
                }
                else if (IsSelected)
                {
                    handler.ToggleActivation(false);
                    handler.ToggleVisibility(false);
                }
                else
                {
                    handler.ToggleActivation(type == QCHTControlBoxSettings.ControlBoxDisplayType.Always || IsHover);
                    handler.ToggleVisibility(handler.IsHandleFacingUser() &&
                                             (type == QCHTControlBoxSettings.ControlBoxDisplayType.Always || IsHover));
                }

                // Ray data
                var handlerType = handler.Type;

                var sprite = handlerType switch
                {
                    InteractionType.Move => Settings.Move,
                    InteractionType.Scale => Settings.Resize,
                    InteractionType.Rotate => Settings.Rotate,
                    _ => null
                };

                var hoverRayData = new RayData
                {
                    ReticleSprite = sprite,
                    ReticleColor = Settings.HoverReticleColor,
                    ReticleScaleFactor = Settings.HoverReticleSize,
                    RayColor = Settings.HoverRayColor
                };

                handler.SetHoverRayData(hoverRayData);

                var selectedRayData = new RayData
                {
                    ReticleSprite = sprite,
                    ReticleColor = Settings.SelectedReticleColor,
                    ReticleScaleFactor = Settings.SelectedReticleSize,
                    RayColor = Settings.SelectedRayColor
                };

                handler.SetSelectedRayData(selectedRayData);
            }
        }

        private void UpdateLightPositionInMaterial()
        {
            if (_material.IsKeywordEnabled(LightLeftKeyword))
            {
                var pos = ControlBoxTransform.TransformPoint(_localLightPosLeft);
                _material.SetVector(s_lightPosLeft, pos);
            }

            if (_material.IsKeywordEnabled(LightRightKeyword))
            {
                var pos = ControlBoxTransform.TransformPoint(_localLightPosRight);
                _material.SetVector(s_lightPosRight, pos);
            }
        }
    }
}