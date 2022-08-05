using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace UtilityAI
{
    public delegate void StateCommand (Context context);

    public struct StateDefinition : IEnumerable
    {
        public string Id;
        public StateEventDefinition[] Events;

        public const EventTypes ____BEGIN____ = EventTypes.Begin;
        public const EventTypes ____UPDATE____ = EventTypes.Update;
        public const EventTypes ____END____ = EventTypes.End;
        public const EventTypes ____EXIT____ = EventTypes.Exit;

        public enum EventTypes: byte {
            Null,
            Begin,
            Update,
            End,
            Exit,

            __CUSTOM_EVENT_0,
            __CUSTOM_EVENT_1,
            __CUSTOM_EVENT_2,
            // ....
        }

        public StateDefinition(string id)
        {
            this.Id = id;
            this.Events = new StateEventDefinition[0];
        }

        public void AddTracks (EventTypes @event, params StateTrackDefinition[] tracks)
        {
            Array.Resize(ref this.Events, this.Events.Length + 1);
            this.Events[this.Events.Length - 1] = new StateEventDefinition(@event, tracks);
        }

        public void Add (EventTypes @event, params StateCommand[] commands)
        {
            Array.Resize(ref this.Events, this.Events.Length + 1);
            this.Events[this.Events.Length - 1] = new StateEventDefinition(@event, new [] { new StateTrackDefinition(commands) });
        }

        IEnumerator IEnumerable.GetEnumerator (){ yield break; }

        public StateDefinition On (EventTypes @event, params StateTrackDefinition[] commands)
        {
            AddTracks(@event, commands);
            return this;
        }

        public StateDefinition On (EventTypes @event, params StateCommand[] commands) => On(@event, new StateTrackDefinition(commands));

        public bool Contains (EventTypes @event) => this.Events.Any(x => x.Id == @event);

        public bool Has (EventTypes @event)
        {
            for (int i = 0; i < this.Events.Length; i++) {
                if (this.Events[i].Id == @event) {
                    return true;
                }
            }
            return false;
        }

        public StateEventDefinition Get (EventTypes @event)
        {
            for (int i = 0; i < this.Events.Length; i++) {
                if (this.Events[i].Id == @event) {
                    return this.Events[i];
                }
            }
            Debug.LogError($"[{Id}] Event with name {@event} does not exist!");
            return default;
        }
    }

    public struct StateEventDefinition
    {
        public StateDefinition.EventTypes Id;
        public StateTrackDefinition[] Tracks;

        public StateEventDefinition(StateDefinition.EventTypes id, StateTrackDefinition[] tracks)
        {
            this.Id = id;
            this.Tracks = tracks;
        }
    }

    public struct StateTrackDefinition
    {
        public string Id;
        public StateCommand[] Commands;

        public StateTrackDefinition(params StateCommand[] commands)
        {
            this.Id = null;
            this.Commands = commands;
        }

        public StateTrackDefinition(string id, params StateCommand[] commands) : this(commands)
        {
            this.Id = id;
        }
    }
}