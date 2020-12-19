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
        public AnimX animation;

        public readonly SyncRef<Slot> RootSlot;
        public readonly SyncList<TrackedRig> RecordedRigs;
        public readonly SyncList<TrackedSlot> RecordedSlots;
        public readonly SyncList<FieldTracker> RecordedFields;
        public readonly SyncRef<StaticAnimationProvider> Output;

        public readonly AnimationCapture AnimCapture;

        protected override void OnAwake()
        {
            base.OnAwake();

        }

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

            CylinderMesh mesh = visual.AttachMesh<CylinderMesh>(material);
            mesh.Radius.Value = 0.015f;
            mesh.Height.Value = 0.05f;
        }

        public override void OnPrimaryPress()
        {
            if (AnimCapture._state.Value == RecordingState.Idle)
            {
                StartRecording();
            }
            else if (AnimCapture._state.Value == RecordingState.Recording) 
            {
                AnimCapture.StopRecording();
            }
            else if (AnimCapture._state.Value == RecordingState.Cached)
            {
                AnimCapture.Deploy();
            }
        }

        public void StartRecording()
        {
            if (AnimCapture.CanRecord)
            {
                // Load recorded items.
                AnimCapture.RootSlot = RootSlot.Target;
                AnimCapture.Output = Output.Target;

                AnimCapture.RecordedRigs.Clear();
                foreach (TrackedRig rig in RecordedRigs) { AnimCapture.RecordedRigs.Add(rig); }

                AnimCapture.RecordedSlots.Clear();
                foreach (TrackedSlot slot in RecordedSlots) { AnimCapture.RecordedSlots.Add(slot); }

                AnimCapture.RecordedFields.Clear();
                foreach (FieldTracker field in RecordedFields) { AnimCapture.RecordedFields.Add(field); }

                AnimCapture.StartRecording();
            }
        }

        protected override void OnCommonUpdate()
        {
            base.OnCommonUpdate();
            AnimCapture.Update();
        }
    }
}
