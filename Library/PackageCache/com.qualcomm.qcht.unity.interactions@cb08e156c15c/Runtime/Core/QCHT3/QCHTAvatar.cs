// /******************************************************************************
//  * File: QCHTAvatar.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using QCHT.Core;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QCHT.Interactions.Core
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public sealed class QCHTAvatar : MonoBehaviour
    {
        [SerializeField]
        private MonoBehaviour headProvider;

        [SerializeField, Tooltip("Should the avatar follow the HMD?")]
        private bool relativeToHead = true;

        private IHeadProvider _headProvider;

        #region MonoBehaviour Functions

        public void Awake()
        {
            if (!TryToGetHeadProvider())
            {
                enabled = false;
            }
        }

        public void Start()
        {
            IgnoreAllCollisions();
        }

        public void Update()
        {
            if (!relativeToHead) return;

            var t = transform;
            t.position = _headProvider.Head.position;
            t.rotation = _headProvider.Head.rotation;
        }

        private bool TryToGetHeadProvider() 
        {
            _headProvider = headProvider as IHeadProvider ?? QCHTManager.Instance;
            return _headProvider != null;
        }
        
        /// <summary>
        /// Removes all collisions detections between parts from the same avatar
        /// </summary>
        private void IgnoreAllCollisions()
        {
            var colliders = GetComponentsInChildren<Collider>();

            foreach (var colA in colliders)
            {
                foreach (var colB in colliders)
                {
                    Physics.IgnoreCollision(colA, colB);
                }
            }
        }

        #endregion
    }
}