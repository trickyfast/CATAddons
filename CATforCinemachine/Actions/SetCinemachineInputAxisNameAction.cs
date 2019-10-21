// Copyright (C) 2019 Tricky Fast Studios, LLC
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrickyFast.CAT.Values;
using Cinemachine;

namespace TrickyFast.CAT.Cinemachine
{
    [CATegory("Action/Cinemachine")]
    [CATDescription("Change the axis to which a specified Cinemachine Virtual Camera will respond. " +
    "Useful customizing control schemes or switching input devices.",
    "cinemachineFreeLookCam: The Cinemachine Free Look Cam which will be affected.",
    "setX: Whether to change the name of the x-axis.",
    "newXAxisName: The name of the axis to which the camera will now listen. Ensure that it is set in the Input project setting.",
    "setY: Whether to change the name of the y-axis.",
    "newYAxisName: The name of the axis to which the camera will now listen. Ensure that it is set in the Input project setting.")]
    public class SetCinemachineInputAxisName : CATRevertOnStopAction
    {
        [Tooltip("The Cinemachine Free Look Cam which will be affected.")]
        public CATarget cinemachineFreeLookCam;
        [Tooltip("Whether to change the name of the x-axis.")]
        public BoolValue setX = new BoolValue(true);
        [Tooltip("The name of the axis to which the camera will now listen. Ensure that it is set in the Input project setting.")]
        public StringValue newXAxisName;
        [Tooltip("Whether to change the name of the y-axis.")]
        public BoolValue setY = new BoolValue(true);
        [Tooltip("The name of the axis to which the camera will now listen. Ensure that it is set in the Input project setting.")]
        public StringValue newYAxisName;

        string originalXAxisName, originalYAxisName;
        bool setXCached, setYCached;

        CinemachineFreeLook cam;

        protected override void DoAction(CATContext context)
        {
            var gobj = cinemachineFreeLookCam.First(context);
            if (gobj == null) return;
            cam = gobj.GetComponentInChildren<CinemachineFreeLook>();
            if (cam == null) return;
            setXCached = setX.GetValue(context);
            setYCached = setY.GetValue(context);
            
            if (setXCached)
            {
                originalXAxisName = cam.m_XAxis.m_InputAxisName;
                cam.m_XAxis.m_InputAxisName = newXAxisName.GetValue(context);
            }
            if (setYCached)
            {
                originalYAxisName = cam.m_YAxis.m_InputAxisName;
                cam.m_YAxis.m_InputAxisName = newYAxisName.GetValue(context);
            }
        }

        protected override void Revert()
        {
            if (setXCached && cam) cam.m_XAxis.m_InputAxisName = originalXAxisName;
            if (setYCached && cam) cam.m_YAxis.m_InputAxisName = originalYAxisName;
        }

        public override List<ValidationResult> Validate()
        {
            var result = base.Validate();
            if (setX.IsReference && setY.IsReference && !setX.value && !setY.value)
                result.Add(new ValidationResult(ValidationResultType.Warning, "setX", "If both Set X and Set Y are turned off, this action will do nothing.", this));
            return result;
        }
    }
}
