// Copyright (C) 2019 Tricky Fast Studios, LLC
using System.Collections.Generic;
using UnityEngine;
using TrickyFast.CAT.Values;
#if SPINE
using Spine.Unity;
//not using Spine base namespace because there are lots of name conflicts with UnityEngine

namespace TrickyFast.CAT
{
	[CATegory("Action/Spine")]
	[CATDescription("Direct Spine's SkeletonAnimation objects to play a named animation",
	"target: GameObject containing a SpineAnimation component or SpineAnimation child",
	"trackNumber: Layer this animation onto the skeleton at a specific track?",
	"animationName: Name of the animation, as defined in the Spine skeleton data asset",
	"loop: Should the animation loop after finishing?",
	"includeChildren: Whether to affect all Spine animations in the target's hierarchy",
	"resetPoseOnStop: if false, use Spine's ClearTrack. If true, use SetEmptyTrack.")]
	public class SkeletonAnimationPlayAction : CATAction
	{
		[Tooltip("GameObject containing a SpineAnimation component or SpineAnimation child")]
		public CATarget target;
		[Tooltip("Layer this animation onto the skeleton at a specific track?\nLeave at 0 if not sure. See Spine docs for details")]
		public IntegerValue trackNumber;
		[Tooltip("Name of the animation, as defined in the Spine skeleton data asset")]
		public StringValue animationName;
		[Tooltip("Should the animation loop after finishing? If checked, this will be a continuous action.")]
		public BoolValue loop;
		[Tooltip("Affect all Spine animations in the target's hierarchy,\n" +
			"or just a component on the target GameObject itself?")]
		public BoolValue includeChildren = new BoolValue(true);
		[Tooltip("If checked, the animation track will be cleared and the pose will reset when this action is stopped.\n" +
			"Otherwise, the pose will not be reset and the animation will freeze in place if not replaced.")]
		public BoolValue resetPoseOnStop = new BoolValue(true);
		//TODO mixing time on Stop? mixing time on Run?
		/*TODO for CAT release: 
		1) Define stopping condition if neither instant nor continuous. 
		Do all targets' animation need to finish or only some? Should this be an option left to the user?
		2) Time offset for running animation on multiple targets? Animation is synchronized otherwise. */

		public override bool IsContinuous { get { return loop.value; } }

		SkeletonAnimation[] targetSkeletons;
		Spine.AnimationState.TrackEntryDelegate animationEnd;
		Spine.AnimationState.TrackEntryDelegate animationComplete;
		bool stopCalled; // In some situations Spine will fire multiple events when the animation is done (End / Complete distinction) but we only need one

		public override Deferred Run(CATContext context)
		{
			if (IsRunning)
				return base.Run(context);
			stopCalled = false;
			Deferred dfrd = base.Run(context);

			HashSet<SkeletonAnimation> uniqueSkeletons = new HashSet<SkeletonAnimation>();
			target.WithTargets(context, delegate (GameObject obj)
			{
				var skels = includeChildren.value ?
					obj.GetComponentsInChildren<SkeletonAnimation>() :
					new SkeletonAnimation[] { obj.GetComponent<SkeletonAnimation>() };
				for (var i = 0; i < skels.Length; ++i)
				{
					if (skels[i] == null) continue;
					uniqueSkeletons.Add(skels[i]);
				}
			});

			targetSkeletons = new SkeletonAnimation[uniqueSkeletons.Count];
			uniqueSkeletons.CopyTo(targetSkeletons);

			for (var i = 0; i < targetSkeletons.Length; ++i)
			{
				targetSkeletons[i].AnimationState.SetEmptyAnimation(trackNumber.value, 0);
				targetSkeletons[i].AnimationState.SetAnimation(trackNumber.value, animationName.value, loop.value);
			}

			
			/* Stop this action in animation callbacks. See http://esotericsoftware.com/spine-unity-events
			TODO: Presently, this game doesn't need to be able to account for multiple targets with animations potentially finishing at different times,
			so I am going to just stop on the first one's callback. This should be changed for a release of CAT. Also should consider the case that the 
			skeletons could be destroyed before the callback is called. Also, might the provided animationName change while the CAT is running?
			Not sure about that last one.
			*/
			var aniState = targetSkeletons[0].AnimationState;
			animationEnd = (trackEntry) =>
			{
				if (stopCalled) return;
				Debug.Log("animationEnd entered. " + trackEntry.Animation.Name);
				// Ignore events on non-targeted tracks
				if (trackEntry.TrackIndex != trackNumber.value) return;

				// See above TODO; for CAT release, don't just stop based on the first one
				if (targetSkeletons[0] == null)
				{
					//Debug.Log("Stop! No SkeletonAnimator on target.");
					Stop();
				}
				else if (trackEntry.Animation.Name == animationName.value)
				{
					//Debug.Log("Stop! Animation ended.");
					Stop();
				}
			};
			animationEnd += (trackentry) =>
			{
				aniState.End -= animationEnd;
			};
			animationComplete = (trackEntry) =>
			{
				if (stopCalled) return;
				Debug.Log("animationComplete entered. " + trackEntry.Animation.Name);
				if (trackEntry.TrackIndex != trackNumber.value) return;

				//Debug.Log("Stop! Animation complete.");
				Stop();
			};
			animationComplete += (trackEntry) =>
			{
				aniState.Complete -= animationComplete;
			};

			aniState.End += animationEnd;
			if (!loop.value)
			{
				aniState.Complete += animationComplete;
			}
			
			
			return dfrd;
		}

		public override bool Stop()
		{
			if (!IsRunning) return false;
			stopCalled = true;
			for (var i = 0; i < targetSkeletons.Length; ++i)
			{
				Spine.Animation anim = targetSkeletons[i].skeleton.Data.FindAnimation(animationName.value);
				if (anim == null || anim != targetSkeletons[i].state.GetCurrent(trackNumber.value).Animation)   // chosen animation not playing
				{ 
					continue;
				}
				if (resetPoseOnStop.value)
				{
					targetSkeletons[i].AnimationState.SetEmptyAnimation(trackNumber.value, 0);
				}
				else
				{
					targetSkeletons[i].AnimationState.ClearTrack(trackNumber.value);
				}
					
			}
			return base.Stop();
		}

		public override List<ValidationResult> Validate()
		{
			var result = base.Validate();
			result.AddRange(ValidateField("target"));
			result.AddRange(ValidateField("trackNumber"));
			result.AddRange(ValidateField("animationName"));
			result.AddRange(ValidateField("loop"));
			result.AddRange(ValidateField("includeChildren"));
			result.AddRange(ValidateField("fireAndForget"));
			// TODO: Uncomment to search target for valid SkeletonAnimator objects and verify that the name is a valid animation attached to it.
			// Will require CAT library upgrade including targeting at edit time.
			/*
			if (target != null && target.type != CATargetType.None)
			{
				
				bool found = false;
				var targets = target.GetTargets();
				for (var i = 0; i < targets.Count; ++i)
				{
					if (includeChildren.value)
					{
						var skels = targets[i].GetComponentsInChildren<SkeletonAnimation>();
						if (skels.Length == 0) continue;
						for (var j = 0; j < skels.Length; ++j)
						{
							if (skels[j].Skeleton.Data.FindAnimation(animationName.value) != null)
							{
								found = true;
								break;
							}
						}
					}
					else
					{
						var skel = targets[i].GetComponent<SkeletonAnimation>();
						if (skel == null) continue;
						if (skel.Skeleton.Data.FindAnimation(animationName.value) != null)
						{
							found = true;
							break;
						}
					}
				}
				if (!found) { result.Add(ValidationResult.Warning(this, "", "No SkeletonAnimations found in target(s) with an animation named \"" + animationName + "\"");  }
			}
			/**/
			return result;
		}
	}
}
#endif