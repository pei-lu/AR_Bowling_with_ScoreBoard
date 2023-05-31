// /******************************************************************************
//  * File: QCHTSDKEditor.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Interactions.Hands;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QCHT.Core
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public sealed class QCHTSDKEditor : QCHTSDK
    {
#if UNITY_EDITOR
        private QCHTSDKUnityEmulator _emulator;
        private readonly InputActionAsset _inputActions;

        public QCHTSDKEditor()
        {
            _inputActions = Resources.Load<InputActionAsset>("HandSimulationInput"); 
        }
        
        public override void StartQcht()
        {
            switch (simulationMode)
            {
                case QCHTSimulationMode.MODE_EDITOR:
                    _emulator = new QCHTSDKUnityEmulator();
                    ToggleInputSystem(true);
                    HookInputs(_emulator);
                    return;

                case QCHTSimulationMode.MODE_REPLAY:
                    QCHTSDKDataCsvReader.StartReplay(EditorPrefs.GetString("QCHTDataCsvFilePath"),
                        EditorPrefs.GetFloat("QCHTDataReplaySpeed", 1f));
                    return;
            }
        }

        public override void StopQcht()
        {
            switch (simulationMode)
            {
                case QCHTSimulationMode.MODE_EDITOR:
                    ToggleInputSystem(false);
                    UnHookInputs(_emulator);
                    return;

                case QCHTSimulationMode.MODE_REPLAY:
                    QCHTSDKDataCsvReader.StopReplay();
                    return;
            }
        }

        public override void UpdateData()
        {
            switch (simulationMode)
            {
                case QCHTSimulationMode.MODE_EDITOR:
                    _emulator.UpdateData(_data);
                    break;

                case QCHTSimulationMode.MODE_REPLAY:
                    QCHTSDKDataCsvReader.UpdateReplay(_data);
                    break;

                default:
                    base.UpdateData();
                    break;
            }
        }

        private void ToggleInputSystem(bool on)
        {
            if (!_inputActions)
                return;

            foreach (var actionAsset in _inputActions)
            {
                if (actionAsset == null) 
                    continue;
                
                if(on)
                    actionAsset.Enable();
                else
                    actionAsset.Disable();
            }
        }

        private void HookInputs(QCHTIHandInputHandler inputHandler)
        {
            if (!_inputActions)
                return;
            
            var handMap = _inputActions.FindActionMap("HandEmulation");
            var leftHandInsertAction = handMap.FindAction("InsertLeftHand");
            var rightHandInsertAction = handMap.FindAction("InsertRightHand");
            var removeHandsAction = handMap.FindAction("RemoveHands");
            var releaseHandsAction = handMap.FindAction("ReleaseHands");
            var pinchAction = handMap.FindAction("Pinch");
            var grabAction = handMap.FindAction("Grab");
            var flipAction = handMap.FindAction("Flip");
            var mousePosition = handMap.FindAction("MousePosition");
            var mouseScroll = handMap.FindAction("MouseScroll");

            leftHandInsertAction.performed += inputHandler.OnLeftHandInsert;
            rightHandInsertAction.performed += inputHandler.OnRightHandInsert;
            removeHandsAction.performed += inputHandler.OnHandsRemove;
            releaseHandsAction.performed += inputHandler.OnHandsReleased;
            pinchAction.performed += inputHandler.OnPinch;
            grabAction.performed += inputHandler.OnGrab;
            flipAction.performed += inputHandler.OnFlip;
            mousePosition.performed += inputHandler.OnMousePosition;
            mouseScroll.performed += inputHandler.OnMouseScroll;
        }


        private void UnHookInputs(QCHTIHandInputHandler inputHandler)
        {
            if (!_inputActions)
                return;
            
            var handMap = _inputActions.FindActionMap("HandEmulation");

            var leftHandInsertAction = handMap.FindAction("InsertLeftHand");
            var rightHandInsertAction = handMap.FindAction("InsertRightHand");
            var removeHandsAction = handMap.FindAction("RemoveHands");
            var releaseHandsAction = handMap.FindAction("ReleaseHands");
            var pinchAction = handMap.FindAction("Pinch");
            var grabAction = handMap.FindAction("Grab");
            var flipAction = handMap.FindAction("Flip");
            var mousePosition = handMap.FindAction("MousePosition");
            var mouseScroll = handMap.FindAction("MouseScroll");

            leftHandInsertAction.performed -= inputHandler.OnLeftHandInsert;
            rightHandInsertAction.performed -= inputHandler.OnRightHandInsert;
            removeHandsAction.performed -= inputHandler.OnHandsRemove;
            releaseHandsAction.performed -= inputHandler.OnHandsReleased;
            pinchAction.performed -= inputHandler.OnPinch;
            grabAction.performed -= inputHandler.OnGrab;
            flipAction.performed -= inputHandler.OnFlip;
            mousePosition.performed -= inputHandler.OnMousePosition;
            mouseScroll.performed -= inputHandler.OnMouseScroll;
        }
#endif
    }
}