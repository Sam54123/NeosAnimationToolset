using System;
using System.Collections.Generic;

using FrooxEngine;

using NeosAnimationToolset.Trackers;

namespace NeosAnimationToolset
{
    public class TrackedRig : SyncObject, ITrackable
    {
        public readonly SyncRef<Rig> rig;
        public readonly Sync<bool> position;
        public readonly Sync<bool> rotation;
        public readonly Sync<bool> scale;
        public readonly Sync<ResultTypeEnum> ResultType;
        public readonly SyncRefList<SkinnedMeshRenderer> meshes;
        public AnimationCapture AnimCapture;


        public void OnStart(AnimationCapture animCapture)
        {
            if (rig.Target == null) return;
            this.AnimCapture = animCapture;
            bool pos = position.Value;
            bool rot = rotation.Value;
            bool scl = scale.Value;
            //bonezs = new Bonez[rig.Target.Bones.Count];
            foreach (Slot bone in rig.Target.Bones)
            {
                SlotTracker s = new SlotTracker(bone, true);
                AnimCapture.RecordedSlots.Add(s);
                s.Position = pos;
                s.Rotation = rot;
                s.Scale = scl;
                foreach (SkinnedMeshRenderer smr in meshes)
                {
                    for (int i = 0; i < smr.Bones.Count; i++)
                    {
                        /*TrackedSlot.SlotListReference slr = s.ListReferences.Add();
                        slr.list.Target = smr.Bones;
                        slr.index.Value = i;*/
                    }
                }
            }
        }
        public void OnUpdate(float T) { }
        public void OnStop() { }
        public void OnReplace(Animator anim) { }
        public void Clean()
        {
            List<SlotTracker> slots = AnimCapture.RecordedSlots;
            foreach (SlotTracker it in slots) { it.Clean(); }
            slots.RemoveAll((it) => { return it.AddedByRig; });
        }
    }
}
