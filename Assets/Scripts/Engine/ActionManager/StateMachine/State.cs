using System;
using UnityEngine;

namespace UtilityAI
{
    public class State
    {
        public readonly StateDefinition meta;
        private StateDefinition.EventTypes currentEvent, scheduledEvent;

        private Track[] tracks;
        private int currentTrack;

        public StateDefinition.EventTypes CurrentEvent { get => currentEvent; }
        public int CurrentTrackId { get => currentTrack; }
        public Track CurrentTrack { get => tracks[currentTrack]; }

        private bool HasUpdate;
        private StateEventDefinition UpdateState;

        public State(StateDefinition state)
        {
            this.meta = state;
            this.currentEvent = StateDefinition.EventTypes.Null;
            this.scheduledEvent = StateDefinition.EventTypes.Null;
            this.tracks = null;

            if (this.meta.Has(StateDefinition.EventTypes.Update)) {
                UpdateState = this.meta.Get(StateDefinition.EventTypes.Update);
                HasUpdate = true;
            }
        }

        public void Begin () => TriggerEvent(StateDefinition.EventTypes.Begin);
        public void Stop () => TriggerEvent(StateDefinition.EventTypes.End);

        public void TriggerEvent (StateDefinition.EventTypes @event)
        {
            // Debug.Log($"{meta.Id}: Trigger event {@event}");

            if (currentEvent == @event) {
                for (var i = 0; i < tracks.Length; i++) tracks[i].Reset();
                return;
            }

            if (@event == StateDefinition.EventTypes.Exit) {
                currentEvent = StateDefinition.EventTypes.Null;
            }

            if (this.meta.Contains(@event)) {
                scheduledEvent = @event;
                return;
            }

            if (@event == StateDefinition.EventTypes.End) {
                scheduledEvent = StateDefinition.EventTypes.Null;
            }else if (@event == StateDefinition.EventTypes.Exit) {
                scheduledEvent = StateDefinition.EventTypes.Null;
                // }else if (@event == "Update") {
                //     if (this.meta.Contains("End")) {
                //         scheduledEvent = "End";
                //     }else{
                //         scheduledEvent = null;
                //     }
            }else{
                scheduledEvent = StateDefinition.EventTypes.Null;
                Debug.LogError($"No event with name '{@event}' exists");
            }
        }

        public bool IsActive () => currentEvent != StateDefinition.EventTypes.Null || scheduledEvent != StateDefinition.EventTypes.Null;

        public void Suspend () => tracks[currentTrack].Suspend();

        public void Wait (float until) => tracks[currentTrack].Wait(until);

        public void Process (Context context, float time)
        {
            // Add scheduled event if current is empty
            if (currentEvent == StateDefinition.EventTypes.Null && scheduledEvent != StateDefinition.EventTypes.Null) {
                currentEvent = scheduledEvent;
                scheduledEvent = StateDefinition.EventTypes.Null;

                tracks = CopyTracks(currentEvent);
            }

            if (currentEvent == StateDefinition.EventTypes.Null) return;

            // Debug.Log($"Process {currentEvent} ({tracks?.Length})");

            int finishedCount = 0;

            for (currentTrack = 0; currentTrack < tracks.Length; currentTrack++) {
                tracks[currentTrack].Process(context, time);
                if (tracks[currentTrack].IsFinished) finishedCount++;
            }

            // All tracks finished playing?
            if (finishedCount >= tracks.Length) {
                if (currentEvent == StateDefinition.EventTypes.Begin) TriggerEvent(StateDefinition.EventTypes.End);
                currentEvent = StateDefinition.EventTypes.Null;
            }
        }

        private Track[] CopyTracks (StateDefinition.EventTypes eventName)
        {
            var @event = this.meta.Get(eventName);
            var len = @event.Tracks.Length;

            if (HasUpdate) {
                len += UpdateState.Tracks.Length;
                // Debug.Log("HasUpdate: " + len);
            }

            var tracks = new Track[len];

            for (int i = 0; i < @event.Tracks.Length; i++) {
                // Debug.Log("AddTrack: " + i + "/" + tracks.Length + " --- " + @event.Tracks.Length);
                tracks[i] = new Track(@event.Tracks[i]);
            }

            if (HasUpdate) {
                // Debug.Log("Update added!");
                for (int i = 0; i < UpdateState.Tracks.Length; i++) {
                    // Debug.Log("Update added on top!" + UpdateState.Tracks[i].Id);
                    tracks[tracks.Length - 1 + i] = new Track(UpdateState.Tracks[i], true);
                }
            }

            return tracks;
        }

        public Track[] GetTracks () => tracks;
    }
}