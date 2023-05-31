/******************************************************************************
 * File: SpacesHandsChangedEventArgs.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;

namespace Qualcomm.Snapdragon.Spaces
{
    [Obsolete("This item is now obsolete and will be removed in future release, please consider updating your project. More information: https://docs.spaces.qualcomm.com/unity/setup/SetupGuideUnity.html",false)]
    public class SpacesHandsChangedEventArgs
    {
        public List<SpacesHand> added { get; private set; }
        public List<SpacesHand> updated { get; private set; }
        public List<SpacesHand> removed { get; private set; }

        public SpacesHandsChangedEventArgs(List<SpacesHand> added, List<SpacesHand> updated, List<SpacesHand> removed) {
            this.added = added;
            this.updated = updated;
            this.removed = removed;
        }
    }
}