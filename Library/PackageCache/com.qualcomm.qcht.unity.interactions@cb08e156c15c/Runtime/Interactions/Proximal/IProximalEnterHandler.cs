// /******************************************************************************
//  * File: IProximalEnterHandler.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine.EventSystems;

namespace QCHT.Interactions.Proximal
{
    public interface IProximalEnterHandler : IEventSystemHandler
    {
        public void OnProximalEnter(InteractionData eventData);
    }
}