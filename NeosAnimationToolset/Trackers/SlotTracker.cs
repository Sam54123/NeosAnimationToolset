using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FrooxEngine;
using BaseX;

namespace NeosAnimationToolset.Trackers
{
    /// <summary>
    /// Captures the animation for a slot.
    /// </summary>
    public class SlotTracker : ITrackable
    {
        public class ShotListReference
        {
            public SyncRefList<Slot> list;
            public int index;
        }

        public readonly Slot Slot;
        public bool Position;
        public bool Rotation;
        public bool Scale;
        public readonly List<SyncRef<Slot>> References = new List<SyncRef<Slot>>();
        public readonly List<ShotListReference> ListReferences = new List<ShotListReference>();
        public ResultTypeEnum ResultType;
        public AnimationCapture AnimCapture;
        public readonly bool AddedByRig;

        public CurveFloat3AnimationTrack PositionTrack;
        public CurveFloatQAnimationTrack RotationTrack;
        public CurveFloat3AnimationTrack ScaleTrack;

        public SlotTracker(Slot slot, bool addedByRig)
        {
            this.Slot = slot;
            this.AddedByRig = addedByRig;
        }

        public SlotTracker(TrackedSlot slot)
        {
            this.Slot = slot.Slot;
            this.Position = slot.position.Value;
            this.Rotation = slot.rotation.Value;
            this.Scale = slot.scale.Value;
            // TODO: References and list references.
            this.AnimCapture = slot.AnimCapture;
            this.AddedByRig = slot.addedByRig;
        }

        public void OnStart(AnimationCapture animCapture)
        {
            this.AnimCapture = animCapture;
            if (Position) PositionTrack = AnimCapture.Animation.AddTrack<CurveFloat3AnimationTrack>();
            if (Rotation) RotationTrack = AnimCapture.Animation.AddTrack<CurveFloatQAnimationTrack>();
            if (Scale) ScaleTrack = AnimCapture.Animation.AddTrack<CurveFloat3AnimationTrack>();
        }
        
        public void OnUpdate(float t)
        {
            Slot ruut = AnimCapture.RootSlot;
            PositionTrack?.InsertKeyFrame(ruut.GlobalPointToLocal(Slot?.GlobalPosition ?? float3.Zero), t);
            RotationTrack?.InsertKeyFrame(ruut.GlobalRotationToLocal(Slot?.GlobalRotation ?? floatQ.Identity), t);
            ScaleTrack?.InsertKeyFrame(ruut.GlobalScaleToLocal(Slot?.GlobalScale ?? float3.Zero), t);
        }

        public void OnStop() { }

        public void OnReplace(Animator anim)
        {
            Slot root = AnimCapture.RootSlot;
            ResultTypeEnum rte = ResultType;
            if (ResultType == ResultTypeEnum.DO_NOTHING)
            {
                if (PositionTrack != null) anim.Fields.Add();
                if (RotationTrack != null) anim.Fields.Add();
                if (ScaleTrack != null) anim.Fields.Add();
                return;
            }

            Slot s = root.AddSlot((rte == ResultTypeEnum.CREATE_VISUAL || rte == ResultTypeEnum.CREATE_NON_PERSISTENT_VISUAL) ? "Visual" : "Empty Object", rte != ResultTypeEnum.CREATE_NON_PERSISTENT_VISUAL);
            if (PositionTrack != null) { anim.Fields.Add().Target = s.Position_Field; }
            if (RotationTrack != null) { anim.Fields.Add().Target = s.Rotation_Field; }
            if (ScaleTrack != null) { anim.Fields.Add().Target = s.Scale_Field; }
            if (rte == ResultTypeEnum.CREATE_VISUAL || rte == ResultTypeEnum.CREATE_NON_PERSISTENT_VISUAL)
            {
                CrossMesh mesh = root.GetComponentOrAttach<CrossMesh>();
                mesh.Size.Value = 0.05f;
                mesh.BarRatio.Value = 0.05f;
                PBS_Metallic mat = root.GetComponentOrAttach<PBS_Metallic>();
                mat.EmissiveColor.Value = new color(0.5f, 0.5f, 0.5f);
                MeshRenderer meshRenderer = s.AttachComponent<MeshRenderer>();
                meshRenderer.Mesh.Target = mesh;
                meshRenderer.Materials.Add(mat);
            }
            else if (rte == ResultTypeEnum.CREATE_PARENT_SLOTS)
            {
                Slot old = Slot;
                old.SetParent(s, false);
                if (PositionTrack != null) { old.LocalPosition = new float3(0, 0, 0); }
                if (RotationTrack != null) { old.LocalRotation = floatQ.Identity; }
                if (ScaleTrack != null) { old.LocalScale = new float3(1, 1, 1); }
            }
            else if (rte == ResultTypeEnum.REPLACE_REFERENCES)
            {
                // TODO: Implement this.
            }
        }

        public void Clean()
        {
            PositionTrack = null;
            RotationTrack = null;
            ScaleTrack = null;
        }
    }
}
