using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TrickyFast.CAT;
using TrickyFast.CAT.Values;


namespace TrickyFast.CAT.Cinemachine
{
    [CATegory("Action/Cinemachine")]
    [CATDescription("Changes the FOV of a chosen FreeLook camera.")]
    public class CinemachineFreeLookSetFOV : CATAction
    {
        [Tooltip("The FreeLook Camera to change the position of.")]
        public CATarget target;
        [Tooltip("The desired FOV of the FreeLook Camera.")]
        public FloatValue desiredFOV;
        [Tooltip("How long the change between old and chosen FOV should take. Set to 0 for instant movement.")]
        public FloatValue duration;
        [Tooltip("The rate of FOV change over the duration.")]
        public AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        [Tooltip("If true, the parameter will be set back to its initial value when this action is stopped.")]
        public bool revertOnStop;

        private List<GameObject> targets;
        private List<CinemachineFreeLook> cinemachines;
        private List<float> originalFOVs;
        private float cachedFOV;
        private float cachedDuration;
        private float elapsed;
        private bool isResetting;
        public override bool IsInstant
        {
            get
            {
                if (duration.value == 0f)
                    return true;
                return false;
            }
        }

        public override Deferred Run(CATContext context)
        {
            if (IsRunning)
                return runDeferred;
            Deferred dfrd = base.Run(context);
            elapsed = 0.0f;

            cinemachines = new List<CinemachineFreeLook>();
            originalFOVs = new List<float>();

            target.WithTargets(context, delegate (GameObject obj)
            {
                cinemachines.Add(obj.GetComponent<CinemachineFreeLook>());
            });

            cachedDuration = duration.GetValue(context);
            cachedFOV = desiredFOV.GetValue(context);

            for (int index = 0; index < cinemachines.Count; ++index)
            {
                cinemachines[index].m_CommonLens = true;
                originalFOVs.Add(cinemachines[index].m_Lens.FieldOfView);
            }


            if (cachedDuration <= 0f)
            {
                for (int index = 0; index < cinemachines.Count; ++index)
                {
                    cinemachines[index].m_Lens.FieldOfView = cachedFOV;
                }
            }
            return dfrd;
        }


        void Update()
        {
            if (IsRunning && cachedDuration > 0f)
            {
                elapsed += Time.deltaTime;
                for (int index = 0; index < cinemachines.Count; ++index)
                {
                    cinemachines[index].m_CommonLens = true;
                    cinemachines[index].m_Lens.FieldOfView = Mathf.Lerp(cinemachines[index].m_Lens.FieldOfView,
                        cachedFOV, curve.Evaluate(elapsed / cachedDuration));
                }
            }
        }

        public override bool Stop()
        {
            if (revertOnStop)
            {
                for (int index = 0; index < cinemachines.Count; ++index)
                {
                    cinemachines[index].m_Lens.FieldOfView = originalFOVs[index];
                }
            }
            return base.Stop();
        }
    }
}

