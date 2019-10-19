// Copyright (C) 2019 Tricky Fast Studios, LLC
#if SPINE
// FOR TESTING. Do not use in production.
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DELETE_ME {
	public class SpineTester : MonoBehaviour {
		public string animationName;
		public bool loop;
		public bool go;
		SkeletonAnimation skel;
		Spine.AnimationState.TrackEntryDelegate act;

		void Start()
		{
			skel = GetComponent<SkeletonAnimation>();
			act = (x) => { Debug.Log("event"); };
			act += (x) => { Debug.Log("Part deux"); };
			skel.AnimationState.Complete += act;
		}

		void Update () {
			if (go)
			{
				go = false;
				skel.AnimationState.SetAnimation(0, animationName, loop);
			}
		}
	}
}
#endif