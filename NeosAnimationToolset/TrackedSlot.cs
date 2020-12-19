using FrooxEngine;
using BaseX;

namespace NeosAnimationToolset
{
    public class TrackedSlot : SyncObject
    {
        public class SlotListReference : SyncObject
        {
            public readonly SyncRef<SyncRefList<Slot>> list;
            public readonly Sync<int> index;
        }
        public readonly SyncRef<Slot> slot;
        public readonly Sync<bool> position;
        public readonly Sync<bool> rotation;
        public readonly Sync<bool> scale;
        public readonly SyncRefList<SyncRef<Slot>> references;
        public readonly SyncList<SlotListReference> listReferences;
        public readonly Sync<ResultTypeEnum> ResultType;
        public AnimationCapture AnimCapture;
        public bool addedByRig = false;

    }
}
