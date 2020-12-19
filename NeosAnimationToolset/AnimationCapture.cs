using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaseX;
using FrooxEngine;

namespace NeosAnimationToolset
{
    public enum RecordingState { Idle, Recording, Saving, Cached }
    public class AnimationCapture : SyncObject
    {
        /*
         * Note: this class is synced because it's required that all clients know if it's recording and who's doing it.
         * State and Recording user are the ONLY values that are synced, because they're the only values that matter to
         * all clients during recording. All other synced values are handled by the parent component 
         */

        /// <summary>
        /// The last animation that was recorded. Make sure to back up before recording again.
        /// </summary>
        public AnimX Animation;

        public readonly List<TrackedRig> RecordedRigs = new List<TrackedRig>();
        public readonly List<TrackedSlot> RecordedSlots = new List<TrackedSlot>();
        public readonly List<FieldTracker> RecordedFields = new List<FieldTracker>();

        /// <summary>
        /// The Static Animation Provider to put the animation into when complete.
        /// </summary>
        public StaticAnimationProvider Output;

        public Slot RootSlot;

        public readonly Sync<RecordingState> _state;
        public readonly SyncRef<User> _recordingUser;

        /// <summary>
        /// The time at which the recording was started.
        /// </summary>
        public double StartTime { get; private set; }

        public bool CanRecord
        {
            get
            {
                return _state.Value == RecordingState.Idle || _state.Value == RecordingState.Cached;
            }
            
        }

        public void StartRecording()
        {
            if (!CanRecord) return;

            Animation = new AnimX();
            _state.Value = RecordingState.Recording;
            _recordingUser.Target = LocalUser;
            StartTime = World.Time.WorldTime;

            foreach (TrackedRig it in RecordedRigs) { it.OnStart(this); }
            foreach (TrackedSlot it in RecordedSlots) { it.OnStart(this); }
            foreach (FieldTracker it in RecordedFields) { it.OnStart(this); }
        }

        public void StopRecording()
        {
            if (_state.Value == RecordingState.Recording && _recordingUser.Target == LocalUser)
            {
                _state.Value = RecordingState.Saving;
                StartTask(BakeAsync);
            }
        }

        /// <summary>
        /// Deploy the recorded animation back onto the components it came from.
        /// </summary>
        public void Deploy()
        {
            if (_state.Value == RecordingState.Cached && _recordingUser.Target == LocalUser)
            {
                Animator animator = RootSlot.AttachComponent<Animator>();
                animator.Clip.Target = Output;
                foreach (TrackedRig it in RecordedRigs) { it.OnReplace(animator); }
                foreach (TrackedSlot it in RecordedSlots) { it.OnReplace(animator); }
                foreach (FieldTracker it in RecordedFields) { it.OnReplace(animator); }
                Clean();
            } 
            else
            {
                Clean();
            }
        }

        public void Clean()
        {
            if (_state.Value == RecordingState.Cached && _recordingUser.Target == LocalUser)
            {
                Animator animator = RootSlot.AttachComponent<Animator>();
                animator.Clip.Target = Output;
                foreach (TrackedRig it in RecordedRigs) { it.Clean(); }
                foreach (TrackedSlot it in RecordedSlots) { it.Clean(); }
                foreach (FieldTracker it in RecordedFields) { it.Clean(); }
                _state.Value = RecordingState.Idle;
            }
        }

        /// <summary>
        /// Must be called every frame while recording.
        /// </summary>
        public void Update()
        {
            if (_state.Value == RecordingState.Recording && _recordingUser.Target == LocalUser)
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
            _state.Value = RecordingState.Cached;
        }
    }
}
