using System;
using BaseX;
using FrooxEngine;
using CodeX;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FrooxEngine.UIX;

namespace NeosAnimationToolset
{
    public enum ResultTypeEnum
    {
        REPLACE_REFERENCES, CREATE_VISUAL, CREATE_NON_PERSISTENT_VISUAL, CREATE_EMPTY_SLOT, CREATE_PARENT_SLOTS, DO_NOTHING
    }

    [Category("Tools/Tooltips")]
    public partial class RecordingTool : ToolTip
    {
        public readonly SyncRef<User> recordingUser;

        public readonly Sync<int> state;

        public readonly Sync<double> _startTime;

        public AnimX animation;

        public readonly SyncRef<Slot> rootSlot;

        public readonly SyncList<TrackedSkinnedMeshRenderer> recordedSMR;
        public readonly SyncList<TrackedMeshRenderer> recordedMR;
        public readonly SyncList<TrackedSlot> recordedSlots;
        public readonly SyncList<FieldTracker> recordedFields;

        public readonly SyncRef<StaticAnimationProvider> _result;

        protected override void OnAttach()
        {
            base.OnAttach();
            Slot visual = Slot.AddSlot("Visual");

            visual.LocalRotation = floatQ.Euler(90f, 0f, 0f);
            visual.LocalPosition = new float3(0, 0, 0);

            PBS_Metallic material = visual.AttachComponent<PBS_Metallic>();

            visual.AttachComponent<SphereCollider>().Radius.Value = 0.025f;

            ValueMultiplexer<color> vm = visual.AttachComponent<ValueMultiplexer<color>>();
            vm.Target.Target = material.EmissiveColor;
            vm.Values.Add(new color(0, 0.5f, 0, 1));
            vm.Values.Add(new color(0.5f, 0, 0, 1));
            vm.Values.Add(new color(0.5f, 0.5f, 0, 1));
            vm.Values.Add(new color(0, 0, 0.5f, 1));
            vm.Index.DriveFrom<int>(state);

            CylinderMesh mesh = visual.AttachMesh<CylinderMesh>(material);
            mesh.Radius.Value = 0.015f;
            mesh.Height.Value = 0.05f;
        }

        public override void OnPrimaryPress()
        {
            if (state.Value == 3)
            {
                Animator animator = rootSlot.Target.AttachComponent<Animator>();
                animator.Clip.Target = _result.Target;
                foreach (ITrackable it in recordedSMR) { it.OnReplace(animator); it.Clean(); }
                foreach (ITrackable it in recordedMR) { it.OnReplace(animator); it.Clean(); }
                foreach (ITrackable it in recordedSlots) { it.OnReplace(animator); it.Clean(); }
                foreach (ITrackable it in recordedFields) { it.OnReplace(animator); it.Clean(); }
                state.Value = 0;
            }
            else if (state.Value == 1)
            {
                state.Value = 2;
                StartTask(bakeAsync);
            }
            else if (state.Value == 0)
            {
                animation = new AnimX(1f);
                recordingUser.Target = LocalUser;
                state.Value = 1;
                _startTime.Value = base.Time.WorldTime;
                foreach (ITrackable it in recordedSMR) { it.OnStart(this); it.OnUpdate(0);}
                foreach (ITrackable it in recordedMR) { it.OnStart(this); it.OnUpdate(0);}
                foreach (ITrackable it in recordedSlots) { it.OnStart(this); it.OnUpdate(0);}
                foreach (ITrackable it in recordedFields) { it.OnStart(this); it.OnUpdate(0);}
            }
        }

        protected override void OnCommonUpdate()
        {
            base.OnCommonUpdate();

            if (state.Value != 1) return;
            User usr = recordingUser.Target;
            if (usr == LocalUser)
            {
                float t = (float)(base.Time.WorldTime - _startTime);
                foreach (ITrackable it in recordedSMR) { it.OnUpdate(t); }
                foreach (ITrackable it in recordedMR) { it.OnUpdate(t); }
                foreach (ITrackable it in recordedSlots) { it.OnUpdate(t); }
                foreach (ITrackable it in recordedFields) { it.OnUpdate(t); }
            }
        }

        protected async Task bakeAsync()
        {
            Slot root = rootSlot.Target;
            float t = (float)(base.Time.WorldTime - _startTime);
            animation.GlobalDuration = t;

            foreach (ITrackable it in recordedSMR) { it.OnUpdate(t); it.OnStop(); }
            foreach (ITrackable it in recordedMR) { it.OnUpdate(t); it.OnStop(); }
            foreach (ITrackable it in recordedSlots) { it.OnUpdate(t); it.OnStop(); }
            foreach (ITrackable it in recordedFields) { it.OnUpdate(t); it.OnStop(); }
            await default(ToBackground);

            string tempFilePath = Engine.LocalDB.GetTempFilePath("animx");
            animation.SaveToFile(tempFilePath);
            Uri uri = Engine.LocalDB.ImportLocalAsset(tempFilePath, LocalDB.ImportLocation.Move);

            await default(ToWorld);
            _result.Target = (root ?? Slot).AttachComponent<StaticAnimationProvider>();
            _result.Target.URL.Value = uri;
            state.Value = 3;
        }
    }
}
