// /******************************************************************************
//  * File: QCHTManager.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections;
using QCHT.Interactions;
using QCHT.Interactions.Core;
using QCHT.Interactions.Hands;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Core
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public enum QCHTSimulationMode
    {
        MODE_EDITOR,
        MODE_REPLAY
    }

    [Obsolete]
    [DisallowMultipleComponent]
#if XR_SDK_SVR
    public sealed class QCHTManager : MonoBehaviour, IHeadProvider, SvrManager.SvrEventListener
#else
    public sealed class QCHTManager : MonoBehaviour, IHeadProvider
#endif
    {
        #region Singleton

        private static QCHTManager s_instance;

        public static QCHTManager Instance
        {
            get
            {
                if (!s_instance)
                {
                    var instance = FindObjectOfType<QCHTManager>();

                    if (!instance)
                    {
                        var obj = new GameObject("QCHTManager");
                        instance = obj.AddComponent<QCHTManager>();
                        instance._simulationMode = QCHTSimulationMode.MODE_EDITOR;
                    }

                    s_instance = instance;
                }

                return s_instance;
            }
        }

        #endregion

        [SerializeField]
        [Tooltip("By default, the camera position is in 0,0,0.")]
        private Transform cameraPlaceHolder;

        [SerializeField, HideInInspector]
        private string _cameraName = "QCHTCamera";

        [Tooltip(
            "EDITOR (default mode) allows you to control the hands when in play mode. \n \nREPLAY means that you can replay a previously recorded sequence by dragging and dropping hand's data.")]
        [SerializeField, HideInInspector] private QCHTSimulationMode _simulationMode;
        [SerializeField, HideInInspector] private bool _applyHeadPose;

        [CanBeEmpty, SerializeField, HideInInspector]
        private Transform _recordedAnchor;

        private Coroutine _startCoroutine;
        private QCHTHeadProvider _headProvider;

        public Transform Head => _headProvider ? _headProvider.Head : QchtCameraObject.transform;

        private GameObject _qchtCameraObj;

        private GameObject QchtCameraObject
        {
            get
            {
                if (!_qchtCameraObj)
                {
                    if (Camera.main != null)
                    {
                        Debug.LogWarning("[QCHTManager] MainCamera found!");
                        _qchtCameraObj = Camera.main.gameObject;
                        return _qchtCameraObj;
                    }

                    var qchtCam = Resources.Load<GameObject>(_cameraName);

                    if (!qchtCam)
                    {
                        Debug.LogWarning("[QCHTManager] QCHTCamera not found! Please check the qcht packages.");
                        _qchtCameraObj = new GameObject("QCHTCamera");
                        _qchtCameraObj.AddComponent<Camera>();
                        _qchtCameraObj.AddComponent<AudioListener>();
                        _qchtCameraObj.tag = "MainCamera";
                    }
                    else
                    {
                        _qchtCameraObj = Instantiate(qchtCam);
                    }

                    DontDestroyOnLoad(_qchtCameraObj);
                    
                    _headProvider = _qchtCameraObj.GetComponent<QCHTHeadProvider>();
                }

                return _qchtCameraObj;
            }
        }

        public bool IsRunning { get; private set; }

        public bool ApplyHeadPose => _applyHeadPose;

        #region MonoBehaviour Functions

        private void Start()
        {
            QchtCameraObject.SetActive(true);

            if (cameraPlaceHolder != null)
            {
                QchtCameraObject.transform.position = cameraPlaceHolder.position;
                QchtCameraObject.transform.rotation = cameraPlaceHolder.rotation;
            }
            
#if XR_SDK_SVR
            SvrManager.Instance.AddEventListener(this);
#endif

            if (!IsRunning)
                StartQchtOrStartQchtWithDelayIfRequired();
        }

        private void OnApplicationPause(bool paused)
        {
            if (!paused && !IsRunning)
            {
                StartQchtOrStartQchtWithDelayIfRequired();
            }
            else if (paused && IsRunning)
            {
                StopQcht();
            }
        }
        
        private void OnApplicationQuit()
        {
            if (IsRunning)
            {
                StopQcht();
            }
        }

        public void OnEnable()
        {
            Application.onBeforeRender += UpdateData;
        }

        public void OnDisable()
        {
            Application.onBeforeRender -= UpdateData;
        }

        public void Update() => UpdateData();
        
#if XR_SDK_SVR
        public void OnSvrEvent(SvrManager.SvrEvent ev)
        {
            if (ev.eventType != SvrManager.svrEventType.kEventProximity) return;
            switch (ev.eventData.proximity.distance)
            {
                case 0 when !IsRunning:
                    StartQchtOrStartQchtWithDelayIfRequired();
                    break;
                case > 0 when IsRunning:
                    StopQcht();
                    break;
            }
        }
#endif

#if UNITY_EDITOR
        private void FixedUpdate()
        {
            if (!IsRunning)
                return;

            if (_simulationMode == QCHTSimulationMode.MODE_REPLAY)
            {
                QCHTSDK.Instance.UpdateData();

                if (_applyHeadPose)
                {
                    var headPose = QCHTSDKDataCsvReader.Instance.CurrentHeadPose;
                    Head.position = headPose.position;
                    Head.rotation = headPose.rotation;
                }

                if (!_recordedAnchor)
                    return;

                var anchorPose = QCHTSDKDataCsvReader.Instance.CurrentAnchorPose;
                _recordedAnchor.position = anchorPose.position;
                _recordedAnchor.rotation = anchorPose.rotation;
            }
            
            QCHTInput.OnPreUpdate();
        }
#endif

        private void UpdateData()
        {
#if DEBUG
            QCHTTimeProfiler.DumpProfilingData();
#endif
            if (!IsRunning)
                return;
            
            if (_simulationMode != QCHTSimulationMode.MODE_REPLAY)
            {
                QCHTSDK.Instance.UpdateData();
            }

            QCHTInput.Update();
        }

        #endregion

        private void StartQchtOrStartQchtWithDelayIfRequired()
        {
            QCHTSDK.simulationMode = _simulationMode;

            if (QCHTSDK.Instance.NeedDelay)
            {
                if (_startCoroutine != null)
                {
                    StopCoroutine(_startCoroutine);
                    _startCoroutine = null;
                }

                _startCoroutine = StartCoroutine(StartQchtWithDelay(3f));
            }
            else
            {
                StartQcht();
            }
        }

        private IEnumerator StartQchtWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartQcht();
        }

        private void StartQcht()
        {
            QCHTSDK.Instance.StartQcht();
            IsRunning = true;
        }

        private void StopQcht()
        {
            QCHTSDK.Instance.StopQcht();
            IsRunning = false;
        }
    }

#if UNITY_EDITOR

    [Obsolete]
    [CustomEditor(typeof(QCHTManager))]
    public sealed class QCHTManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _cameraNameProperty;
        private SerializedProperty _simulationModeProperty;
        private SerializedProperty _applyHeadPoseProperty;
        private SerializedProperty _anchorTransformProperty;

        private void OnEnable()
        {
            _cameraNameProperty = serializedObject.FindProperty("_cameraName");
            _simulationModeProperty = serializedObject.FindProperty("_simulationMode");
            _applyHeadPoseProperty = serializedObject.FindProperty("_applyHeadPose");
            _anchorTransformProperty = serializedObject.FindProperty("_recordedAnchor");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(_simulationModeProperty);

            var mode = (QCHTSimulationMode) _simulationModeProperty.enumValueIndex;
            switch (mode)
            {
                case QCHTSimulationMode.MODE_EDITOR:
                    _cameraNameProperty.stringValue = "QCHTCamera";
                    break;
                case QCHTSimulationMode.MODE_REPLAY:
                    WriteCSVFilePath(EditorGUILayout.TextField("CSV File path", ReadCSVFilePath()));
                    _cameraNameProperty.stringValue = "ReplayCamera";
                    _applyHeadPoseProperty.boolValue =
                        EditorGUILayout.Toggle("Apply Recorded head pose", _applyHeadPoseProperty.boolValue);
                    EditorGUILayout.PropertyField(_anchorTransformProperty);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static string ReadCSVFilePath()
        {
            return EditorPrefs.GetString("QCHTDataCsvFilePath", string.Empty);
        }

        private static void WriteCSVFilePath(string filePath)
        {
            EditorPrefs.SetString("QCHTDataCsvFilePath", filePath);
        }
    }
#endif
}