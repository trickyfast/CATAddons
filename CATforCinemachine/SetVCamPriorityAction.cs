// Copyright (C) 2019 Tricky Fast Studios, LLC
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrickyFast.CAT.Values;
using Cinemachine;

namespace TrickyFast.CAT.Cinemachine
{
    [CATegory("Action/Cinemachine")]
    [CATDescription("Instantly change the priority of any Cinemachine Virtual Camera.",
    "vCam: The virtual camera whose priority to adjust.",
    "newPriority: The camera's priority will be set to this.")]
    public class SetVCamPriorityAction : CATRevertOnStopAction
    {
        [Tooltip("The virtual camera whose priority to adjust.")]
        public CATarget vcam;
        [Tooltip("The virtual camera's priority will be set to this.")]
        public IntegerValue newPriority;

        int originalPriority;
        CinemachineVirtualCameraBase cam;

        protected override void DoAction(CATContext context)
        {
            var gobj = vcam.First(context);
            if (gobj == null) return;
            cam = gobj.GetComponentInChildren<CinemachineVirtualCameraBase>();
            if (cam == null) return;
            originalPriority = cam.Priority;
            cam.Priority = newPriority.GetValue(context);
        }

        protected override void Revert()
        {
            if (cam != null) cam.Priority = originalPriority;
        }
    }
}
