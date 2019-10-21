// Copyright (C) 2019 Tricky Fast Studios, LLC
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrickyFast.CAT.Values;
using Cinemachine;

namespace TrickyFast.CAT.Cinemachine
{
    [CATegory("Action/Cinemachine")]
    [CATDescription("Use this to adjust the radius and/or height on any number of the three rigs associated with a Cinemachine FreeLookCam.",
        "cinemachineFreeLookCam: The camera whose rigs to adjust.",
        "additive: If checked, will add the specified numbers to the current values on the rigs. If unchecked, will simply replace the current values.",
        "setRadius: The rig(s) on which to adjust the radius.",
        "newRadius: The value that will replace (or be added to) the current value of the specified rig(s)' radius.",
        "setHeight: The rig(s) on which to adjust the height.",
        "newHeight: The value that will replace (or be added to) the current value of the specified rig(s)' height.")]
    public class SetCinemachineFreeLookOrbitRigsAction : CATRevertOnStopAction
    {
        [Tooltip("The camera whose rigs to adjust.")]
        public CATarget cinemachineFreeLookCam;
        [Tooltip("If checked, will add the specified numbers to the current values on the rigs. If unchecked, will simply replace the current values.")]
        public BoolValue additive;
        [Tooltip("The rig(s) on which to adjust the radius.")]
        public RigSelection setRadius = RigSelection.Top;
        [Tooltip("The value that will replace (or be added to) the current value of the specified rig(s)' radius.")]
        public FloatValue newRadius = new FloatValue(1f);
        [Tooltip("The rig(s) on which to adjust the height.")]
        public RigSelection setHeight = RigSelection.Top;
        [Tooltip("The value that will replace (or be added to) the current value of the specified rig(s)' height.")]
        public FloatValue newHeight = new FloatValue(1f);

        CinemachineFreeLook cam;
        float[] originalRadius = { 0f, 0f, 0f };
        float[] originalHeight = { 0f, 0f, 0f };

        protected override void DoAction(CATContext context)
        {
            var gobj = cinemachineFreeLookCam.First(context);
            if (gobj == null) return;

            cam = gobj.GetComponentInChildren<CinemachineFreeLook>();
            if (cam == null) return;

            bool add = additive.GetValue(context);
            for (var i = 0; i < 3; ++i)
            {
                originalRadius[i] = cam.m_Orbits[i].m_Radius;
                originalHeight[i] = cam.m_Orbits[i].m_Height;

                if ((setRadius & IndexToSelection(i)) != 0)
                {
                    if (add)
                        cam.m_Orbits[i].m_Radius += newRadius.GetValue(context);
                    else
                        cam.m_Orbits[i].m_Radius = newRadius.GetValue(context);

                }
                if ((setHeight & IndexToSelection(i)) != 0)
                {
                    if (add)
                        cam.m_Orbits[i].m_Height = newHeight.GetValue(context);
                    else
                        cam.m_Orbits[i].m_Height += newHeight.GetValue(context);
                }
            }
        }

        // Find the index in Cinemachine's internal m_Orbits array of the first provided selection, or -1 if None provided.
        static int SelectionToIndex(RigSelection sel)
        {
            for (var i = 1; i < 5; i *= 2)
            {
                if (((int)sel & i) != 0) return i;
            }
            return -1;
        }

        static RigSelection IndexToSelection(int index)
        {
            switch (index)
            {
                case 0: return RigSelection.Top;
                case 1: return RigSelection.Middle;
                case 2: return RigSelection.Bottom;
                default: return RigSelection.None;
            }
        }

        public override List<ValidationResult> Validate()
        {
            var result = base.Validate();
            if (setRadius == RigSelection.None && setHeight == RigSelection.None)
                result.Add(new ValidationResult(ValidationResultType.Warning, "rigSelection", "If none of the rigs are selected, this action will have no effect.", this));
            return result;
        }

        protected override void Revert()
        {
            for (var i = 0; i < 3; ++i)
            {
                if ((setRadius & IndexToSelection(i)) != 0)
                {
                    cam.m_Orbits[i].m_Radius = originalRadius[i];
                }
                if ((setHeight & IndexToSelection(i)) != 0)
                {
                    cam.m_Orbits[i].m_Height = originalHeight[i];
                }
            }
        }
    }

    [System.Flags]
    public enum RigSelection
    {
        None = 0,
        Top = 1,
        Middle = 1 << 1,
        Bottom = 1 << 2
    }
}
