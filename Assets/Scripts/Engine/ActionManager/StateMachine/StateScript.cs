using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UtilityAI
{
    public class StateScript
    {
        public string Name;
        public State[] States;

        public State CurrentState;
        internal Context ContextCopy;

        public StateScript(string name, params StateDefinition[] stateDefs)
        {
            this.Name = name;
            States = new State[stateDefs.Length];
            for (int i = 0; i < stateDefs.Length; i++) States[i] = new State(stateDefs[i]);
        }

        public void Process (float time)
        {
            // Debug.Log($"--- Update: {name}");
            for (int i = 0; i < States.Length; i++) {
                CurrentState = States[i];
                // Debug.Log($"--- Update Script {CurrentState.meta.Id}");
                CurrentState.Process(ContextCopy, time);
            }
        }

        internal void Repeat ()
        {
            CurrentState.CurrentTrack.Suspend();
            CurrentState.Begin();
        }

        internal void Go (string newState)
        {
            CurrentState.TriggerEvent(StateDefinition.EventTypes.End); // NECESSARY?
            CurrentState.CurrentTrack.Suspend();
            // Debug.Log($"[{name}] [{CurrentState.meta.Id}.{CurrentState.CurrentEvent}] GO TO [{newState}.Begin]");
            Debug.AssertFormat(States.Any(x => x.meta.Id == newState), "No states in '{0}' with name '{1}' found", Name, newState);
            States.First(x => x.meta.Id == newState).Begin();
        }

        internal void StopExternal ()
        {
            // Debug.Log($"[{Name}] External Exit: {CurrentState.meta.Id}");
            for (int i = 0; i < States.Length; i++) {
                if (States[i].IsActive()) States[i].TriggerEvent(StateDefinition.EventTypes.Exit);
            }
        }

        internal bool IsActive ()
        {
            // Debug.Log($"[{name}] External Exit: {CurrentState.meta.Id}");
            for (int i = 0; i < States.Length; i++) {
                if (States[i].IsActive()) return false;
            }
            return true;
        }

        internal void Begin ()
        {
            // Debug.Log($"Begin {name}");
            States[0].Begin();
        }

        public static StateDefinition State (string name) => new StateDefinition(name);
    }
}