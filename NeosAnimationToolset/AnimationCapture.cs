using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaseX;
using FrooxEngine;

namespace NeosAnimationToolset
{
    public class AnimationCapture
    {
        public readonly World World;
        public enum RecordingState { Idle, Recording, Saving, Cached }

        /// <summary>
        /// The last animation that was recorded. Make sure to back up before recording again.
        /// </summary>
        public AnimX Animation;

        public readonly List<TrackedRig> RecordedRigs;
        public readonly List<TrackedSlot> RecordedSlots;
        public readonly List<FieldTracker> RecordedFields;

        /// <summary>
        /// The Static Animation Provider to put the animation into when complete.
        /// </summary>
        public StaticAnimationProvider Output;

        public Slot RootSlot;

        public RecordingState State { get; private set; }

        /// <summary>
        /// The time at which the recording was started.
        /// </summary>
        public double StartTime { get; private set; }

        public AnimationCapture(World world)
        {
            this.World = world;
        }

        public void StartRecording()
        {
            if (State == RecordingState.Recording || State == RecordingState.Saving) { return; }

            Animation = new AnimX();
            State = RecordingState.Recording;
            StartTime = World.Time.WorldTime;

            foreach (TrackedRig it in RecordedRigs) { it.OnStart(this); }
            foreach (TrackedSlot it in RecordedSlots) { it.OnStart(this); }
            foreach (FieldTracker it in RecordedFields) { it.OnStart(this); }
        }

        public void StopRecording()
        {
            if (State == RecordingState.Recording)
            {
                State = RecordingState.Saving;
                Task.Run(BakeAsync);
            }
        }

        /// <summary>
        /// Deploy the recorded animation back onto the components it came from.
        /// </summary>
        public void Deploy()
        {
            if (State == RecordingState.Cached)
            {
                Animator animator = RootSlot.AttachComponent<Animator>();
                animator.Clip.Target = Output;
                foreach (TrackedRig it in RecordedRigs) { it.OnReplace(animator); it.Clean(); }
                foreach (TrackedSlot it in RecordedSlots) { it.OnReplace(animator); it.Clean(); }
                foreach (FieldTracker it in RecordedFields) { it.OnReplace(animator); it.Clean(); }
                State = RecordingState.Idle;
            }
        }

        /// <summary>
        /// Must be called every frame while recording.
        /// </summary>
        public void Update()
        {
            if (State == RecordingState.Recording)
            {
                float t = (float)(World.Time.WorldTime - StartTime);
                foreach (TrackedRig it in RecordedRigs) { it.OnUpdate(t); }
                foreach (TrackedSlot it in RecordedSlots) { it.OnUpdate(t); }
                foreach (FieldTracker it in RecordedFields) { it.OnUpdate(t); }
            }
        }

        protected async Task BakeAsync()
        {
            float t = (float)(World.Time.WorldTime - StartTime);
            Animation.GlobalDuration = t;

            foreach (TrackedRig rig in RecordedRigs) { rig.OnUpdate(t); rig.OnStop(); }
            foreach (TrackedSlot slot in RecordedSlots) { slot.OnUpdate(t); slot.OnStop(); }
            foreach (FieldTracker field in RecordedFields) { field.OnUpdate(t); field.OnStop(); }

            await default(ToBackground);

            string tempFilePath = World.Engine.LocalDB.GetTempFilePath("animx");
            Animation.SaveToFile(tempFilePath);
            Uri uri = World.Engine.LocalDB.ImportLocalAsset(tempFilePath, LocalDB.ImportLocation.Move);

            await default(ToWorld);
            if (Output != null) { Output.URL.Value = uri; }
            State = RecordingState.Cached;
        }
    }
}
