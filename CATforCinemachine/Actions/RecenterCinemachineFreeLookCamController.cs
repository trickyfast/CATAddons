// Copyright (C) 2019 Tricky Fast Studios, LLC
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrickyFast.CAT.Values;
using Cinemachine;
using TrickyFast.CAT.Extensions;

namespace TrickyFast.CAT.Cinemachine
{
    [CATegory("Action/Cinemachine")]
    [CATDescription("Continuously waits for a specified input, and, when pressed, causes the camera to swing back to its heading direction " +
        "and default distance, if desired. For example, can be used to have the camera swing behind the player when the camera control stick is pressed in.",
        "cinemachineFreeLookCam: The FreeLookCamera to move. Must be the live camera when this action runs.",
        "centerTime: How quickly the recentering should happen.",
        "centerXAxis: Whether to recenter the orbit of the camera behind the player",
        "centerYAxis: Whether to recenter the camera progress on the three rigs to its default value. Controls tilt and track in/out.",
        "inputAxisName: The name of the button press or other input that will trigger this action. See project input settings for options.",
        "bailOnRotate: If Bail On Rotate is selected, how much rotation (in degrees) to allow before canceling camera recenter.",
        "subjectRotateTolerance: ",
        "bailOnMove: If the follow target moves while in the middle of recentering the camera, cancel the recentering.",
        "SubjectMoveTolerance: If Bail On Move is selected, distance to allow before canceling camera recenter (ignoring height).")]
    public class RecenterCinemachineFreeLookCamController : CATAction
    {
        [Tooltip("The FreeLookCamera to move. Must be the live camera when this action runs.")]
        public CATarget cinemachineFreeLookCam;
        [Tooltip("How quickly the recentering should happen.")]
        public FloatValue centerTime = new FloatValue(0.5f);
        [Tooltip("Whether to recenter the orbit of the camera behind the player")]
        public BoolValue centerXAxis = new BoolValue(true);
        [Tooltip("Whether to recenter the camera progress on the three rigs to its default value. Controls tilt and track in/out.")]
        public BoolValue centerYAxis = new BoolValue(true);
        [Tooltip("The name of the button press or other input that will trigger this action. See project input settings for options.")]
        public StringValue inputAxisName;
        [Tooltip("If the follow target rotates while in the middle of recentering the camera, cancel the recentering.")]
        public BoolValue bailOnRotate = new BoolValue(true);
        [Tooltip("If Bail On Rotate is selected, how much rotation (in degrees) to allow before canceling camera recenter.")]
        public FloatValue subjectRotateTolerance = new FloatValue(15f);
        [Tooltip("If the follow target moves while in the middle of recentering the camera, cancel the recentering.")]
        public BoolValue bailOnMove = new BoolValue(true);
        [Tooltip("If Bail On Move is selected, distance to allow before canceling camera recenter (ignoring height).")]
        public FloatValue subjectMoveTolerance = new FloatValue(4f);

        CinemachineFreeLook cam;
        CATContext cachedContext;
        Coroutine waitToResetCoroutine;
        const float resetTolerance = 1.5f;
        float originalCenterTimeX, originalCenterTimeY, originalWaitTimeX, originalWaitTimeY;
        float rotToleranceDot, moveToleranceSqr;
        Vector3 subjectStartForward, subjectStartPos;
        bool centerX, centerY, originalXActive, originalYActive;
        bool lastFrameInput, bailRot, bailMov;
        string axis;

        public override bool IsContinuous { get { return false; } }

        public override Deferred Run(CATContext context)
        {
            if (IsRunning) return base.Run(context);
            Deferred result = base.Run(context);

            axis = inputAxisName.GetValue(context);
            if (string.IsNullOrEmpty(axis))
            {
                Stop();
                return result;
            }
            GameObject gobj = cinemachineFreeLookCam.First(context);
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
            if (!CinemachineCore.Instance.IsLive(cam))
            {
                Stop();
                return result;
            }

            bailRot = bailOnRotate.GetValue(context);
            bailMov = bailOnMove.GetValue(context);
            rotToleranceDot = subjectRotateTolerance.GetValue(context).AngleToDot();
            moveToleranceSqr = subjectMoveTolerance.GetValue(context);
            moveToleranceSqr *= moveToleranceSqr;
            //camTransform = CinemachineCore.Instance.GetActiveBrain(0).OutputCamera.transform;

            centerX = centerXAxis.GetValue(context);
            centerY = centerYAxis.GetValue(context);

            originalXActive = cam.m_XAxis.m_Recentering.m_enabled;
            originalYActive = cam.m_YAxis.m_Recentering.m_enabled;
            originalCenterTimeX = cam.m_XAxis.m_Recentering.m_RecenteringTime;
            originalCenterTimeY = cam.m_YAxis.m_Recentering.m_RecenteringTime;
            originalWaitTimeX = cam.m_XAxis.m_Recentering.m_WaitTime;
            originalWaitTimeY = cam.m_YAxis.m_Recentering.m_WaitTime;

            if (!(centerX && centerY)) Stop();

            return result;
        }
        
        private void Update()
        {
            if (!IsRunning) return;
            if (cam != null && !CinemachineCore.Instance.IsLive(cam))
            {
                Stop();
            }

            bool thisFrameInput = !Mathf.Approximately(0f, Input.GetAxis(axis));
            if (thisFrameInput)
            {
                if (waitToResetCoroutine != null)
                {
                    StopCoroutine(waitToResetCoroutine);
                    waitToResetCoroutine = null;
                }
                if (centerX)
                {
                    cam.m_RecenterToTargetHeading.m_enabled = true;
                    cam.m_RecenterToTargetHeading.m_RecenteringTime = centerTime.GetValue(cachedContext);
                    cam.m_RecenterToTargetHeading.m_WaitTime = 0f;
                    cam.m_RecenterToTargetHeading.RecenterNow();
                }
                if (centerY)
                {
                    cam.m_YAxisRecentering.m_enabled = true;
                    cam.m_YAxisRecentering.m_RecenteringTime = centerTime.GetValue(cachedContext);
                    cam.m_YAxisRecentering.m_WaitTime = 0f;
                    cam.m_YAxisRecentering.RecenterNow();
                }
            }
            else if (lastFrameInput)    
            {
                if (centerX && waitToResetCoroutine == null)
                {
                    subjectStartForward = Vector3.ProjectOnPlane(cam.Follow.forward, Vector3.up);
                    subjectStartPos = cam.Follow.position.Geoposition();
                    waitToResetCoroutine = StartCoroutine(WaitToReset());
                }
                else if (centerY) ResetCamSettings();
            }

            lastFrameInput = thisFrameInput;
        }

        public override bool Stop()
        {
            if (waitToResetCoroutine != null)
            {
                StopCoroutine(waitToResetCoroutine);
                waitToResetCoroutine = null;
            }
            if (cam != null)
            {
                ResetCamSettings();
            }
            cachedContext = null;
            return base.Stop();
        }

        void ResetCamSettings()
        {
            cam.m_RecenterToTargetHeading.m_enabled = originalXActive;
            cam.m_RecenterToTargetHeading.m_WaitTime = originalWaitTimeX;
            cam.m_RecenterToTargetHeading.m_RecenteringTime = originalCenterTimeX;

            cam.m_YAxisRecentering.m_enabled = originalYActive;
            cam.m_YAxisRecentering.m_WaitTime = originalWaitTimeY;
            cam.m_YAxisRecentering.m_RecenteringTime = originalCenterTimeY;
        }

        // Since the reset isn't instant, we don't want to interrupt the orbit when input stops unless the player explicitly cancels.
        IEnumerator WaitToReset()
        {
            if (centerX)
            {
                float xAngle;
                bool bailNowMov, bailNowRot;
                // When input stops, don't interrupt rotation while: Center X option is selected, 
                // and the camera is outside the tolerance for lining up with the goal,
                // and the goal doesn't rotate more than the rotation tolerance.
                // and the goal doesn't move more than the movement tolerance.
                do
                {
                    xAngle = Vector3.Angle(Vector3.forward, Vector3.ProjectOnPlane(cam.Follow.forward, Vector3.up)) - cam.m_XAxis.Value;
                    bailNowRot = bailRot && Vector3.Dot(subjectStartForward, Vector3.ProjectOnPlane(cam.Follow.forward, Vector3.up)) < rotToleranceDot;
                    bailNowMov = bailMov && Vector3.SqrMagnitude(cam.Follow.position.Geoposition() - subjectStartPos) > moveToleranceSqr;
                    yield return null;
                } while (centerX && 
                    (xAngle > resetTolerance || xAngle < -resetTolerance) && 
                    !bailNowRot && !bailNowMov);
                ResetCamSettings();
                waitToResetCoroutine = null;
            }
        }

        public override List<ValidationResult> Validate()
        {
            var result = base.Validate();
            if (!inputAxisName.IsReference && string.IsNullOrEmpty(inputAxisName.value))
                result.Add(new ValidationResult(ValidationResultType.Warning, "inputAxisName",
                "Must provide an input to detect in order for this controller to do anything.", this));
            if ((!centerXAxis.IsReference && !centerXAxis.value) && (!centerYAxis.IsReference && !centerYAxis.value))
                result.Add(new ValidationResult(ValidationResultType.Warning, "centerXAxis",
                "If neither X nor Y axes are centered, this controller will do nothing.", this));
            if (!centerTime.IsReference && centerTime.value < 0f)
                result.Add(new ValidationResult(ValidationResultType.Error, "centerTime",
                "Time value must not be negative.", this));
            if (!subjectRotateTolerance.IsReference && (subjectRotateTolerance.value >= 180f || subjectRotateTolerance.value <= 0f))
                result.Add(new ValidationResult(ValidationResultType.Error, "subjectRotateTolerance",
                "Should be a smallish angle, but values greater than 180 are nonsensical", this));
            if (!subjectMoveTolerance.IsReference && (subjectMoveTolerance.value < 0f))
                result.Add(new ValidationResult(ValidationResultType.Warning, "subjectRotateTolerance",
                "Must be positive or zero. If negative, will be treated as positive.", this));
            return result;
        }
    }
}
