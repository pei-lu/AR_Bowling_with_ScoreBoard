// /******************************************************************************
//  * File: CustomEvent.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace QCHT.Interactions
{
    [CreateAssetMenu(menuName = "QCHT/CustomEvent")]
    public sealed class CustomEvent : ScriptableObject
    {
        [FormerlySerializedAs("Event")] [SerializeField]
        private UnityEvent m_Event = new UnityEvent();

        public void Invoke() => m_Event?.Invoke();
        public void AddListener(UnityAction call) => m_Event?.AddListener(call);
        public void RemoveListener(UnityAction call) => m_Event?.RemoveListener(call);
        public void RemoveAllListener() => m_Event?.RemoveAllListeners();

        [Obsolete("Use Invoke() instead.")]
        public void SendEvent() => Invoke();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CustomEvent))]
    public sealed class CustomEventEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Send Event"))
            {
                var customEvent = target as CustomEvent;
                if (customEvent) customEvent.Invoke();
            }
        }
    }
#endif
}