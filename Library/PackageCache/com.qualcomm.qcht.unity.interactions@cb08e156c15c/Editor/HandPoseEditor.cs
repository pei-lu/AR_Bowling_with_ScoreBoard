// /******************************************************************************
//  * File: HandPoseEditor.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Interactions.Hands;
using UnityEditor;
using UnityEngine;

namespace QCHT.Interactions.Editor
{
    [CustomEditor(typeof(HandPose))]
    public sealed class HandPoseEditor : UnityEditor.Editor
    {
        private HandPose _handPose;
        private HandGhost _handGhost;

        private Vector2 _previewDir;

        private void OnEnable() => _handPose = target as HandPose;

        private void OnDisable() => CleanupPreviewRenderUtility();
        
        #region Inspector

        public override void OnInspectorGUI()
        {
            var handSide = (XrHandedness) EditorGUILayout.EnumPopup("Hand Side", _handPose.Type);
            if (handSide != _handPose.Type)
            {
                _handPose.Type = handSide;
                UnityEditor.EditorUtility.SetDirty(_handPose);
                SetDirty(true);
                AssetDatabase.SaveAssetIfDirty(_handPose);
            }            
            
            var handSpace = (DataSpace) EditorGUILayout.EnumPopup("Hand Side", _handPose.Space);
            if (handSpace != _handPose.Space)
            {
                _handPose.Space = handSpace;
                UnityEditor.EditorUtility.SetDirty(_handPose);
                SetDirty(true);
                AssetDatabase.SaveAssetIfDirty(_handPose);
            }
        }

        #endregion

        #region Preview

        private const string PreviewGhostInstanceName = "ghostPreview";

        private PreviewRenderUtility _previewRenderUtility;

        public override bool HasPreviewGUI() => true;

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            base.OnInteractivePreviewGUI(r, background);

            if (_previewRenderUtility == null || _isDirty)
            {
                CleanupPreviewRenderUtility();

                _previewRenderUtility = new PreviewRenderUtility();
                InitPreviewCamera();
                ReloadGhostPreview();
                SetDirty(false);
            }

            _previewRenderUtility.BeginPreview(r, background);
            DoRenderPreview(r);
            _previewRenderUtility.EndAndDrawPreview(r);
        }

        private void InitPreviewCamera()
        {
            var camera = _previewRenderUtility.camera;
            var cameraTransform = camera.transform;
            cameraTransform.position = new Vector3(0f, .07f, -1f);
            cameraTransform.LookAt(new Vector3(0f, .07f, 0f));
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = new Color(.2f, .2f, .2f, 1f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10f;
        }

        private void DoRenderPreview(Rect r)
        {
            _previewDir = PreviewGUI.Drag2D(_previewDir, r);

            if (_handGhost)
            {
                _handGhost.transform.rotation = Quaternion.Euler(_previewDir.y, 0.0f, 0.0f) *
                                                Quaternion.Euler(0.0f, _previewDir.x, 0.0f);

                _handGhost.UpdatePose();
            }

            _previewRenderUtility.Render();
        }

        private void ReloadGhostPreview()
        {
            if (!_handPose.Ghost)
                return;

            var obj = _handPose.Type == XrHandedness.XR_HAND_LEFT ? _handPose.Ghost.LeftGhost : _handPose.Ghost.RightGhost;
            if (!obj)
                return;

            var ghost = _previewRenderUtility.InstantiatePrefabInScene(obj.gameObject);
            ghost.name = PreviewGhostInstanceName;
            ghost.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

            if (!ghost)
            {
                OnDisable();
                return;
            }

            _handGhost = ghost.GetComponent<HandGhost>();
            _handGhost.HandPose = _handPose;

            var ghostTransform = _handGhost.transform;
            ghostTransform.position = Vector3.zero;
            ghostTransform.rotation = _handGhost.HandPose.Root.Rotation;
        }

        private void CleanupPreviewRenderUtility()
        {
            if (_previewRenderUtility == null)
                return;

            _previewRenderUtility.Cleanup();
            _previewRenderUtility = null;
        }

        #endregion

        #region Dirty

        private bool _isDirty = true;

        private void SetDirty(bool value)
        {
            _isDirty = value;
        }

        #endregion

        #region Asset

        public static HandPose CreateNewHandPoseAsset()
        {
            var handPose = CreateInstance<HandPose>();
            AssetUtils.CreateAssetInSettingsFromObj(handPose, "HandPoses", "NewHandPose");
            return handPose;
        }

        public static HandPose DuplicateHandPoseAsset(HandPose handPose)
        {
            var newPose = Instantiate(handPose);
            AssetUtils.CreateAssetInSettingsFromObj(newPose, "HandPoses", handPose.name);
            return newPose;
        }

        #endregion
    }

    internal static class PreviewGUI
    {
        private static readonly int s_sliderHash = "Slider".GetHashCode();

        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        {
            var idControl = GUIUtility.GetControlID(s_sliderHash, FocusType.Passive);
            var cEvent = Event.current;

            switch (cEvent.GetTypeForControl(idControl))
            {
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == idControl)
                    {
                        scrollPosition -= cEvent.delta * (cEvent.shift ? 3f : 1f) /
                            Mathf.Min(position.width, position.height) * 140f;
                        cEvent.Use();
                        GUI.changed = true;
                    }

                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == idControl)
                    {
                        GUIUtility.hotControl = 0;
                    }

                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;

                case EventType.MouseDown:
                    if (position.Contains(cEvent.mousePosition) && position.width > 50.0f)
                    {
                        GUIUtility.hotControl = idControl;
                        EditorGUIUtility.SetWantsMouseJumping(1);
                        cEvent.Use();
                    }

                    break;
            }

            return scrollPosition;
        }
    }
}