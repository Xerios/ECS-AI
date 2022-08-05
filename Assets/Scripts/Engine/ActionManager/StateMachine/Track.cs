using UnityEngine;

namespace UtilityAI
{
    public struct Track
    {
        private readonly StateTrackDefinition meta;
        private int currentIndex;

        private StateCommand[] Commands;

        private float WaitUntil;
        private bool Suspended;
        private bool Finished;
        private bool Infinite;

        public StateTrackDefinition Meta { get => meta; }
        public int CurrentIndex { get => currentIndex; }
        public int Length { get => Commands.Length; }

        public bool IsFinished { get => Finished; }
        public float IsWaiting { get => WaitUntil; }

        public Track(StateTrackDefinition track, bool infinite = false)
        {
            this.meta = track;
            currentIndex = 0;
            WaitUntil = -1;
            Suspended = false;
            Finished = false;
            Infinite = infinite;
            Commands = track.Commands;
        }

        public void Reset ()
        {
            currentIndex = 0;
            Suspended = true; // Break out of loop if we're in one ( otherwise it adds currentIndex +1 adn everything goes mayham )
            Finished = false;
            // Debug.Log("Retry Track = " + currentIndex);
        }

        public void Suspend () // EXECUTING THIS FROM .CurrentState.CurrentTrack breaks things due to struct
        {
            Suspended = true;
        }

        public void Wait (float until) // EXECUTING THIS FROM .CurrentState.CurrentTrack breaks due to struct
        {
            WaitUntil = until;
            // Debug.Log($"Wait until {WaitUntil} > {Time.time} ");
            Suspended = true;
            currentIndex++;
        }

        public void Process (Context context, float time)
        {
            if (Finished && !Infinite) return;
            if (WaitUntil > time) {
                // Debug.Log($"Waiting... until {WaitUntil} > {Time.time}");
                return;
            }

            Suspended = false;
            while (currentIndex < Commands.Length) {
                Commands[currentIndex].Invoke(context);
                // Debug.Log(
                //     $"[{context.machine.Current.Name}] [{context.machine.Current.CurrentState.meta.Id}.{context.machine.Current.CurrentState.CurrentEvent}] [Track {meta.Id ?? "DEFAULT"}]: {currentIndex} {Commands[currentIndex].Method.Name}" +
                //     (Suspended ?  " [SUSPEND]" : ""));
                if (Suspended) break;
                currentIndex++;
            }
            if (currentIndex == Commands.Length) {
                if (Infinite) {
                    currentIndex = 0;
                }

                Finished = true;
                // Debug.Log(
                //     $"[{context.machine.Current.name}] [{context.machine.Current.CurrentState.meta.Id}.{context.machine.Current.CurrentState.CurrentEvent}] [Track {meta.Id ?? "DEFAULT"}]: [FINISHED]");
            }
        }
    }
}