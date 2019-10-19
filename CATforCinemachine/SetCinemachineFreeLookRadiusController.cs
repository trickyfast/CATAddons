// Copyright (C) 2019 Tricky Fast Studios, LLC
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrickyFast.CAT.Values;
using Cinemachine;

namespace TrickyFast.CAT.Cinemachine
{
    [CATegory("Action/Cinemachine")]
    [CATDescription("Use this to adjust the radius on any number of the three rigs associated with a Cinemachine FreeLookCam." +
    "Will continuously adjust the perspective in response to an event, such as input.",
    "cinemachineFreeLookCam: The camera whose rigs to adjust.",
    "catEventManager: The object, such as a StateMachine, that will receive events to which this action responds. " +
        "This will receive the event, such as 'OnCameraZoom,' from your input device. Usually should be paired with an AxisInputAction",
    "inputEvent: This action is a controller, so it will operate continuously until stopped, and react to the event specified here.",
    "setRadius: The rig(s) on which to adjust the radius.",
    "rigSelection: Specify which rigs on which to continuously adjust the radius",
    "inputSpeed: Speed multiplier for the input event, such as mouse movement or joystick tilt.",
    "minimumDistance: Radius will not fall below this value.",
    "maxiumumDistance: Radius will not exceed this value.")]
    public class SetCinemachineFreeLookRadiusController : CATAction
    {
        [Tooltip("The camera whose rigs to adjust.")]
        public CATarget cinemachineFreeLookCam;
        [Tooltip("The object, such as a StateMachine, that will receive events to which this action responds. " +
            "This will receive the event, such as 'OnCameraZoom,' from your input device. Usually should be paired with an AxisInputAction")]
        public CATarget catEventManager;
        [Tooltip("This action is a controller, so it will operate continuously until stopped, and react to the event specified here.")]
        public InputEventType inputEvent = InputEventType.OnCameraZoom;
        [Tooltip("The rig(s) on which to adjust the radius.")]
        public RigSelection rigSelection = (RigSelection)((int)RigSelection.Bottom + (int)RigSelection.Middle + (int)RigSelection.Top);
        [Tooltip("Speed multiplier for the input event, such as mouse movement or joystick tilt.")]
        public FloatValue inputSpeed = new FloatValue(0.5f);
        [Tooltip("Radius will not fall below this value.")]
        public FloatValue minimumDistance = new FloatValue(1f);
        [Tooltip("Radius will not exceed this value.")]
        public FloatValue maximumDistance = new FloatValue(15f);

        public override bool IsContinuous { get { return true; } }

        CinemachineFreeLook cam;
        CATEventManager evtMan;
        EventSubscription subscription;
        float speed, min, max;

        public override Deferred Run(CATContext context)
        {
            if (IsRunning) return base.Run(context);
            var result = base.Run(context);
            
            // Connect camera and event listener, or quit
            var gobj = cinemachineFreeLookCam.First(context);
            if (gobj == null)
            {
                Stop();
                return result;
            }
            cam = gobj.GetComponentInChildren<CinemachineFreeLook>();
            if (cam == null)
            {
                Stop();
                return result;
            }
            gobj = catEventManager.First(context);
            if (gobj == null)
            {
                Stop();
                return result;
            }
            evtMan = gobj.GetComponentInChildren<CATEventManager>();
            if (evtMan == null)
            {
                Stop();
                return result;
            }

            // Sanitize distance minimum and maximum
            speed = inputSpeed.GetValue(context);
            min = minimumDistance.GetValue(context);
            if (min < 0f) min = 0.0001f;
            max = maximumDistance.GetValue(context);
            if (max < min) max = min;

            // Subscribe to event
            subscription = evtMan.Subscribe(inputEvent.ToString(), OnInput);

            return result;
        }

        // The rigs are stored in an array: {Top, Middle, Bottom}
        private void OnInput(CATEvent evt)
        {
            if (!IsRunning) return;
            float val;
            float data = (float)evt.data;
            if ((rigSelection & RigSelection.Top) != 0)
            {
                val = cam.m_Orbits[0].m_Radius + data * speed;
                val = Mathf.Clamp(val, min, max);
                cam.m_Orbits[0].m_Radius = val;
            }
            if ((rigSelection & RigSelection.Middle) != 0)
            {
                val = cam.m_Orbits[1].m_Radius + data * speed;
                val = Mathf.Clamp(val, min, max);
                cam.m_Orbits[1].m_Radius = val;
            }
            if ((rigSelection & RigSelection.Bottom) != 0)
            {
                val = cam.m_Orbits[2].m_Radius + data * speed;
                val = Mathf.Clamp(val, min, max);
                cam.m_Orbits[2].m_Radius = val;
            }
        }

        public override bool Stop()
        {
            if (evtMan != null) evtMan.Unsubscribe(inputEvent.ToString(), subscription);
            return base.Stop();
        }

        public override List<ValidationResult> Validate()
        {
            var result = base.Validate();
            if (rigSelection == RigSelection.None)
                result.Add(new ValidationResult(ValidationResultType.Warning, "rigSelection", "If none of the rigs are selected, this action will have no effect.", this));
            if (minimumDistance.IsReference && minimumDistance.value <= 0f)
                result.Add(new ValidationResult(ValidationResultType.Warning, "minimumDistance", "Minimum Distance should be greater than zero and less than Maximum Distance.", this));
            if (maximumDistance.IsReference && (maximumDistance.value <= 0f || (minimumDistance.IsReference && maximumDistance.value < minimumDistance.value) ))
                result.Add(new ValidationResult(ValidationResultType.Warning, "maximumDistance", "Maximum Distance should be greater Minimum Distance. Both should be greater than zero.", this));
            return result;
        }
    }
}
