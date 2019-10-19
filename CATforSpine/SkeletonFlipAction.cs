// Copyright (C) 2019 Tricky Fast Studios, LLC
using System.Collections.Generic;
using UnityEngine;
using TrickyFast.CAT.Values;
#if SPINE
using Spine.Unity;

namespace TrickyFast.CAT
{
	[CATegory("Action/Spine")]
	[CATDescription("Cause a Spine skeleton to flip.",
		"setFlipX: Whether to adjust the flipX setting or not",
		"flipXNewValue: Value to which flipX will be set, if set",
		"setFlipY: Whether to adjust the flipY setting or not",
		"flipYNewValue: Value to which flipY will be set, if set")]
	public class SkeletonFlipAction : CATInstantAction {
		[Tooltip("GameObject with a Spine skeleton component or child.")]
		public CATarget target;
		[Tooltip("Affect all Spine animations in the target's hierarchy,\n" +
			"or just a component on the target GameObject itself?")]
		public BoolValue includeChildren = new BoolValue(true);
		[Tooltip("Whether to adjust the flipX setting or not")]
		public BoolValue setFlipX = new BoolValue(true);
		[Tooltip("Value to which flipX will be set, if set")]
		public BoolValue flipXNewValue = new BoolValue(true);
		[Tooltip("Whether to adjust the flipY setting or not")]
		public BoolValue setFlipY = new BoolValue(false);
		[Tooltip("Value to which flipY will be set, if set")]
		public BoolValue flipYNewValue = new BoolValue(false);

		protected override void DoAction(CATContext context)
		{
			// Get unique skeletons
			var ors = new List<SkeletonAnimator>();
			var tions = new List<SkeletonAnimation>();
			var uniqueSkeletons = new HashSet<Spine.Skeleton>();
			if (includeChildren.GetValue(context))
			{
				target.WithTargets(context, gobj => {
					ors.AddRange(gobj.GetComponentsInChildren<SkeletonAnimator>());
					tions.AddRange(gobj.GetComponentsInChildren<SkeletonAnimation>());
				});
			}
			else
			{
				target.WithTargets(context, gobj => {
					ors.Add(gobj.GetComponent<SkeletonAnimator>());
					tions.Add(gobj.GetComponent<SkeletonAnimation>());
				});
			}
			for (var i = 0; i < ors.Count; ++i) { uniqueSkeletons.Add(ors[i].skeleton); }
			for (var i = 0; i < tions.Count; ++i) { uniqueSkeletons.Add(tions[i].skeleton); }

			// Flip all collected skeletons
			Spine.Skeleton[] skeletons = new Spine.Skeleton[uniqueSkeletons.Count];
			uniqueSkeletons.CopyTo(skeletons);
			for (var i = 0; i < uniqueSkeletons.Count; ++i)
			{
				var skel = skeletons[i];
				if (setFlipX.GetValue(context)) skel.FlipX = flipXNewValue.GetValue(context);
				if (setFlipY.GetValue(context)) skel.FlipY = flipYNewValue.GetValue(context);
			}
		}
	}
}
#endif