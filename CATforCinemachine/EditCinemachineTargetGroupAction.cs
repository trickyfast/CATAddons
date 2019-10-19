// Copyright (C) 2019 Tricky Fast Studios, LLC
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrickyFast.CAT.Values;
using Cinemachine;

namespace TrickyFast.CAT.Cinemachine
{
	[CATegory("Action/Cinemachine")]
    [CATDescription("Add or remove entries from the list in a CinemachineTargetGroup. Use to focus cameras on multiple objects.",
        "cinemachineTargetGroup: The group whose entries to change.",
        "targetedObject: The game object(s) to add or remove from the group.",
        "weight: The weight associated with the added object. If removing an object, this is ignored.",
        "radius: The radius associated with the added object. If removing an object, this is ignored.",
        "operation: Whether to add or remove the object from the group.")]
    public class EditCinemachineTargetGroupAction : CATInstantAction
    {
        [Tooltip("The group whose entries to change.")]
        public CATarget cinemachineTargetGroup;
        [Tooltip("The game object(s) to add or remove from the group.")]
        public CATarget targetedObject;
        [Tooltip("The weight associated with the added object. If removing an object, this is ignored.")]
        public FloatValue weight = new FloatValue(1f);
        [Tooltip("The radius associated with the added object. If removing an object, this is ignored.")]
        public FloatValue radius = new FloatValue(1f);
        [Tooltip("Whether to add or remove the object from the group.")]
        public EditOperation operation;

        protected override void DoAction(CATContext context)
        {
            GameObject gobj = cinemachineTargetGroup.First(context);
            if (gobj == null) return;
            CinemachineTargetGroup group = gobj.GetComponentInChildren<CinemachineTargetGroup>();
            if (group == null) return;
            var all = cinemachineTargetGroup.GetTargets(context);
            if (all == null || all.Count < 1) return;

            float cachedWeight = weight.GetValue(context);
            float cachedRadius = radius.GetValue(context);

            switch (operation)
            {
                case EditOperation.Add:
                    for (var i = 0; i < all.Count; ++i) group.AddMember(all[i].transform, cachedWeight, cachedRadius);
                    break;
                case EditOperation.Remove:
                    for (var i = 0; i < all.Count; ++i) group.RemoveMember(all[i].transform);
                    break;
                default:
                    throw new System.NotImplementedException("Unhandled EditOperation in " + name + ": " + operation);
            }
        }

        public enum EditOperation
        {
            Add,
            Remove
        }
    }
}
