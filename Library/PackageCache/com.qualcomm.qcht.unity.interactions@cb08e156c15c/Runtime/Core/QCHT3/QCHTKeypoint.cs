// /******************************************************************************
//  * File: QCHTKeypoint.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Collections;
using UnityEngine;

namespace QCHT.Core
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public class QCHTKeypoint : MonoBehaviour
    {
        private const float MaxZDisplayEnd = 0.65f + 0.5f; // 65 cm
        private const float MaxZDisplayStart = 0.62f + 0.5f; // 60 cm

        private Material _material;
        private Renderer _rend;
        private Coroutine _currentFade;
        private bool _isShown;

        private void Start()
        {
            _rend = GetComponent<Renderer>();
            _material = _rend.material;
        }

        public void SetColor(Color c)
        {
            _material.color = c;
        }

        public void Update()
        {
            if (!_isShown)
                return;

            var c = _material.color;

            if (transform.localPosition.z > MaxZDisplayStart)
            {
                var fadeDelta = (transform.localPosition.z - MaxZDisplayStart) /
                                (MaxZDisplayEnd - MaxZDisplayStart);
                c.a = Mathf.Lerp(c.a, 1 - fadeDelta, Time.deltaTime * 5f);
            }
            else
            {
                c.a = 1;
            }

            _material.color = c;
        }

        public void Fade(bool show)
        {
            if (_isShown == show)
                return;

            if (_currentFade != null)
                StopCoroutine(_currentFade);

            _currentFade = StartCoroutine(FadeTo(show ? 1 : 0, 0.1f));
            _isShown = show;
        }

        private IEnumerator FadeTo(float targetOpacity, float duration)
        {
            var color = _material.color;
            var startOpacity = color.a;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                var blend = Mathf.Clamp01(t / duration);
                color.a = Mathf.Lerp(startOpacity, targetOpacity, blend);
                _material.color = color;
                yield return null;
            }
        }

        public void ShowPoint(bool show)
        {
            _rend.enabled = show;
        }
    }
}