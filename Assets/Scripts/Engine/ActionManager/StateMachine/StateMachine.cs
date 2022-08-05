using Engine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace UtilityAI
{
    public class StateMachine
    {
        public Context Context;
        public StateScript Current;

        private StringDictionary<StateScript> states;
        private LinkedList<StateScript> states_sorted;

        public StateMachine(Context context)
        {
            context.machine = this;
            this.Context = context;
            this.states = new StringDictionary<StateScript>(3);
            this.states_sorted = new LinkedList<StateScript>();
        }

        public void StopAll ()
        {
            foreach (var item in states) {
                Current = item.Value;
                Current.StopExternal();
            }
        }


        public void BeginStates (StateScript stateScript)
        {
            StopAll();
            if (states.ContainsKey(stateScript.Name)) {
                states_sorted.Remove(states[stateScript.Name]);
                states[stateScript.Name] = stateScript;
            }else{
                states.Add(stateScript.Name, stateScript);
            }
            states_sorted.AddLast(stateScript);
            stateScript.ContextCopy = Context;
            stateScript.Begin();
        }

        public void Update (float time)
        {
            foreach (var item in states_sorted) {
                Profiler.BeginSample(item.Name);
                Current = item;
                Current.Process(time);
                Profiler.EndSample();
            }
        }

        public IEnumerable<StateScript> GetStates () => states_sorted;
    }
}