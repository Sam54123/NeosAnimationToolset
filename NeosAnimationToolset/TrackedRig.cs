using System;
using System.Collections.Generic;

using FrooxEngine;

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
                TrackedSlot s = new TrackedSlot();
                AnimCapture.RecordedSlots.Add(s);
                s.slot.Target = bone;
                s.position.Value = pos;
                s.rotation.Value = rot;
                s.addedByRig = true;
                s.scale.Value = scl;
                foreach (SkinnedMeshRenderer smr in meshes)
                {
                    for (int i = 0; i < smr.Bones.Count; i++)
                    {
                        TrackedSlot.SlotListReference slr = s.listReferences.Add();
                        slr.list.Target = smr.Bones;
                        slr.index.Value = i;
                    }
                }
            }
        }
        public void OnUpdate(float T) { }
        public void OnStop() { }
        public void OnReplace(Animator anim) { }
        public void Clean()
        {
            List<TrackedSlot> slots = AnimCapture.RecordedSlots;
            foreach (TrackedSlot it in slots) { it.Clean(); }
            slots.RemoveAll((it) => { return it.addedByRig; });
        }
    }
}
