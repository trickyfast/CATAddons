using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TrickyFast.CAT;
using TrickyFast.CAT.Values;

namespace TrickyFast.CAT.Cinemachine
{
    [CATegory("Action/Cinemachine")]
    [CATDescription("Generates a Cinemachine Impulse with a direction of your choosing.")]


    public class CinemachineGenerateImpulse : CATInstantAction
    {
        [Tooltip("The object containing a Cinemachine Impulse Source to trigger.")]
        public CATarget impulseSource;
        [Tooltip("The Amplitude Gain of the Impulse.")]
        public FloatValue amplitudeGain;
        [Tooltip("The Frequency Gain of the Impulse.")]
        public FloatValue frequencyGain;
        [Tooltip("The Impact Radius of the Impulse.")]
        public FloatValue impactRadius;

        private CinemachineImpulseSource myImpulseSource;
        protected override void DoAction(CATContext context)
        {
            impulseSource.WithTargets(context, delegate (GameObject obj)
            {
                myImpulseSource = obj.GetComponent<CinemachineImpulseSource>();
            });

            var impulseDefinition = myImpulseSource.m_ImpulseDefinition;
            impulseDefinition.m_AmplitudeGain = amplitudeGain.GetValue(context);
            impulseDefinition.m_FrequencyGain = frequencyGain.GetValue(context);
            impulseDefinition.m_ImpactRadius = impactRadius.GetValue(context);

            myImpulseSource.GenerateImpulse(Camera.main.transform.forward);
        }

        public override List<ValidationResult> Validate()
        {
            var result = base.Validate();
            if (impulseSource.type == CATargetType.None)
            {
                result.Add(ValidationResult.Warning(this, "impulseSource", "No target Impulse Source is specified, so nothing will happen."));
            }
            return result;
        }
    }
}
