// /******************************************************************************
//  * File: ExecuteEventsExtensions.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine.EventSystems;

namespace QCHT.Interactions.Distal
{
    public static class ExecuteEventsExtensions
    {
        /*** IDoubleDragHandler ***/
        private static readonly ExecuteEvents.EventFunction<IDoubleDragHandler> s_pointerDoubleDragHandler =
            ExecuteDoubleDrag;

        private static void ExecuteDoubleDrag(IDoubleDragHandler handler, BaseEventData eventData)
        {
            if (ExecuteEvents.ValidateEventData<BaseEventData>(eventData) is QCHTDoublePointerEventData qchtEventData)
                handler.OnDoubleDrag(qchtEventData);
        }

        public static ExecuteEvents.EventFunction<IDoubleDragHandler> pointerDoubleDragHandler =>
            s_pointerDoubleDragHandler;


        /*** IDoubleDragHandler ***/
        private static readonly ExecuteEvents.EventFunction<IDoubleDragBeginHandler> s_pointerDoubleDragBeginHandler =
            ExecuteDoubleDragBegin;

        private static void ExecuteDoubleDragBegin(IDoubleDragBeginHandler handler, BaseEventData eventData)
        {
            if (ExecuteEvents.ValidateEventData<BaseEventData>(eventData) is QCHTDoublePointerEventData
                qchtDoublePointerEventData)
                handler.OnBeginDoubleDrag(qchtDoublePointerEventData);
        }

        public static ExecuteEvents.EventFunction<IDoubleDragBeginHandler> pointerDoubleDragBeginHandler =>
            s_pointerDoubleDragBeginHandler;
    }
}