// Copyright (C) 2019 Tricky Fast Studios, LLC
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrickyFast.CAT;
using TrickyFast.CAT.Values;
using Cinemachine;

namespace TrickyFast.CAT.Cinemachine
{
    [CATegory("Trigger/Cinemachine")]
    [CATDescription("Fires whenever the Cinemachine Brain cuts from one virtual camera to another with no transition.")]
    public class CinemachineCutTrigger : CATrigger
    {
        CinemachineBrain _brain;

        public override Deferred StartListening(CATContext context, TriggerCallback callback)
        {
            if (IsListening) return base.StartListening(context, callback);
            var dfrd = base.StartListening(context, callback);
            
            _brain = CinemachineCore.Instance.GetActiveBrain(0);
            _brain.m_CameraCutEvent.AddListener(OnCut);

            return dfrd;
        }

        void OnCut(CinemachineBrain brain)
        {
            Fire(true, new List<GameObject> { brain.ActiveVirtualCamera.VirtualCameraGameObject });
        }

        public override void StopListening()
        {
            _brain.m_CameraCutEvent.RemoveListener(OnCut);
            base.StopListening();
        }
    }
}
