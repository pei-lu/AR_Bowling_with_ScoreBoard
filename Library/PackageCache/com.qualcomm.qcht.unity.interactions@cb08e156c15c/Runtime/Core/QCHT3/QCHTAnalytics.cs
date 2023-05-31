// /******************************************************************************
//  * File: QCHTAnalytics.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
#if QCHT_ANALYTICS
using QCHT.Analytics;
#endif

namespace QCHT.Core
{
    [Obsolete("This item is now obsolete and will be remove in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public class QCHTAnalytics
    {
        
#if QCHT_ANALYTICS
        private readonly QCHT.Analytics.QCHTAnalytics _analytics = new Analytics.QCHTAnalytics();
#endif
        
        #region Singleton

        private static QCHTAnalytics s_instance;

        private static QCHTAnalytics Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new QCHTAnalytics();
                }
                return s_instance;
            }
        }

        #endregion
        
        
        #region public APIs

        public static void StartSession(string sessionName)
        {
#if QCHT_ANALYTICS
            Instance._analytics.StartSession(sessionName);
#endif
        }
        
        public static void StartEvent(string name, string id)
        {
#if QCHT_ANALYTICS
            Instance._analytics.StartEvent(name, id);
#endif
        }

        public static void StopEvent(string name, string id, int failureCount)
        {
#if QCHT_ANALYTICS
            Instance._analytics.StopEvent(name, id, failureCount);
#endif
        }

        public static void StopSession()
        {
#if QCHT_ANALYTICS
            Instance._analytics.StopSession();
#endif
        }
        
        #endregion
    }
}