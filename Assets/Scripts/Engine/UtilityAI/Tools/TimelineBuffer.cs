using CircularBuffer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI
{
    public class TimelineBuffer<T>: CircularBuffer<TimelineEvent<T> >  where T : IEquatable<T>
    {
        public bool lastEventFinished = true;
        public TimelineEvent<T> LastEvent;

        public TimelineBuffer(int capacity) : base(capacity) {}

        public void DontMergeNext ()
        {
            lastEventFinished = true;
        }

        public void Add (T reference, float time)
        {
            // Debug.Log($"{time} - {reference}");

            // if (reference.Equals(default(T))) return;
            if (!lastEventFinished && LastEvent.Reference.Equals(reference)) {
                var lastEventRef = Front();
                lastEventRef.Reference = reference;
                lastEventRef.EndTime = time;
                this[0] = lastEventRef;
            }else{
                LastEvent = new TimelineEvent<T>(reference, time);
                PushFront(LastEvent);
                lastEventFinished = false;
            }
        }

        public float GetEventDuration (T reference, float beforeTime)
        {
            float duration = 0f;

            if (this._size == 0) return 0;

            for (int i = 0; i < _size; i++) {
                var item = _buffer[InternalIndex(i)];
                if (item.EndTime < beforeTime) break;
                if (reference.Equals(item.Reference)) {
                    if (item.StartTime < beforeTime) {
                        duration += (item.EndTime - beforeTime); // Doesn't make sense? Investigate later why I did this
                        break;
                    }else{
                        duration += item.Duration;
                    }
                }
            }
            // UnityEngine.Debug.Log(duration);
            return duration;
        }

        public int GetEventRepeats (T reference, float beforeTime)
        {
            int repeats = 0;

            if (this._size == 0) return 0;

            for (int i = 0; i < _size; i++) {
                var item = _buffer[InternalIndex(i)];
                if (item.EndTime < beforeTime) break;
                // If (item.EndTime < beforeTime || item.StartTime < beforeTime) break; // Old way of doing it, might be wrong
                if (item.Reference.Equals(reference) && (i != 0 || (i == 0 && lastEventFinished))) repeats++;   // (item != LastEvent) => Ignore current event ( not counted as a repeat )
            }

            return repeats;
        }

        public IEnumerable<TimelineEvent<T> > GetEventsRange (float start, float end)
        {
            if (_size == 0) yield break;
            foreach (var item in this) {
                if (item.StartTime > start && item.StartTime < end) continue;
                yield return item;
            }
        }
    }
}