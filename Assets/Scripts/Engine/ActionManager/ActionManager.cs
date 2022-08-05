using ActionDictionary = System.Collections.Generic.Dictionary<string, UtilityAI.StateScript>;
using CircularBuffer;
using Engine;
using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UtilityAI
{
    public class ActionManager
    {
        public StateMachine SMachine;
        public BlackboardDictionary Blackboard = new BlackboardDictionary();


        private ActionDictionary nameToStateScript = new ActionDictionary();

        // -------------------------------
        public DecisionContext CurrentContext;
        public bool dontRepeatThisTick;
        public DecisionHistory dontRepeatDecision; // Needed to avoid repeating something we deem finished or failed

        // -------------------------------
        public string TempSyncStateValue, TempSyncStateValueNext;
        // -------------------------------

        public float time;
        private float breakTime;

        public ActionManager()
        {
            SMachine = new StateMachine(
                    new Context {
                        data = Blackboard,
                    });
        }

        public void Set (string name, StateScript stateScript)
        {
            Debug.Assert(name != null, "Name is null");
            Debug.Assert(stateScript != null, "StateScript is null");
            nameToStateScript[name] = stateScript;
        }

        private StateScript GetInteraction (DecisionContext current)
        {
            StateScript stateScript;

            if (current.DSEId == 0) {
                return null;
            }

            // Try to find local sequence
            var name = UtilityAILoop.Instance.GetDecisionName(current.DSEId);
            if (nameToStateScript.TryGetValue(name, out stateScript)) {
                return stateScript;
            }

            // Can't find sequence, ask smartobject for its sequence
            if (current.Target != Entity.Null) {
                EntityManager mgr = AIManager.Instance.mgr;

                if (!mgr.Exists(current.Target)) return null;

                if (mgr.HasComponent<SignalAction>(current.Target)) {
                    // Debug.Log($"Got sequence from Signal {current.Target}");
                    stateScript = mgr.GetSharedComponentData<SignalAction>(current.Target).data.Invoke();
                    if (stateScript == null) Debug.LogError($"No SequenceAction to Retrieve from Signal {current.Target}");
                    return stateScript;
                }
            }

            throw new MissingReferenceException($"No action steps found localy for {name} {current} on the Signal ({current.Target}) or Agent");
        }

        // --------------------------------------------------------------------------

        public void DontRepeat (DecisionHistory hist)
        {
            dontRepeatDecision = hist;
        }

        public void Break (float duration)
        {
            if (SMachine.Context.SelfMind.Equals(Entity.Null)) return;

            Bootstrap.world.EntityManager.SetComponentData(SMachine.Context.SelfMind, new ActiveDecision {
                    entity = Entity.Null, dseId = 0, target = Entity.Null
                });

            Mind.EndCurrentDecision(SMachine.Context.SelfMind, time, SMachine.Context.decisionHistory);

            CurrentContext = default;

            SMachine.StopAll();
            breakTime = time + duration;
        }
        // --------------------------------------------------------------------------

        internal void Tick (float time)
        {
            Blackboard.Update();
        }


        public void Update (float time, Entity mindEntity, DecisionContext best)
        {
            this.time = time;

            // ============================================
            var isNewDecision = !best.Equals(CurrentContext);

            if (isNewDecision && time > breakTime) {
                Mind.EndCurrentDecision(mindEntity, time, SMachine.Context.decisionHistory);

                CurrentContext = best;


                SMachine.Context.SelfMind = mindEntity;
                SMachine.Context.decisionHistory = CurrentContext.GetDecisionHistory();

                if (best.DSEId == 0) {
                    Bootstrap.world.EntityManager.SetComponentData(SMachine.Context.SelfMind, new ActiveDecision {
                            entity = Entity.Null, dseId = 0, target = Entity.Null
                        });
                    SMachine.StopAll();
                }else{
                    SMachine.BeginStates(GetInteraction(best));
                    Mind.StartCurrentDecision(mindEntity, time, SMachine.Context.decisionHistory, CurrentContext.Decision);
                    Blackboard.Update();
                }
            }


            // if (isNewDecision && !IsCurrentInteractionForced) {
            //     instance.RequestDeactivateAll();
            //     DebugLog(SubActionDebug.Actions.TryToChangeDecision);// $"Try to change to {best.DSEId} #{best.Target.Index}"
            // }

            var currentToHist = CurrentContext.GetDecisionHistory();

            // ============================================
            Blackboard.Update();
            SMachine.Update(time);

            // ============================================

            // if (instance.NodeCount == 0) {
            //     Mind.EndCurrentDecision(mindEntity, time, currentToHist);

            //     // Second pass in case we already finished with our action
            //     // This removes the delay between actions

            //     if (isNewDecision) {
            //         CurrentContext = best;
            //         EnqeueAction(mindEntity, bestEnt, GetInteraction(best)); // Set new
            //     }else if (!IsCurrentInteractionForced && !currentToHist.Equals(dontRepeatDecision)) {  // Do not repeat forced interactions or if decision was failed
            //         if (!dontRepeatThisTick) {
            //             DebugLog(SubActionDebug.Actions.RepeatAction);
            //             EnqeueAction(mindEntity, bestEnt, CurrentInteraction); // Restart
            //         }
            //     }
            // }

            dontRepeatThisTick = false;
        }


        public void Sync ()
        {
            TempSyncStateValue = TempSyncStateValueNext;
        }

        public void DontRepeatThisTick ()
        {
            dontRepeatThisTick = true;
        }

        public void Dispose ()
        {
            SMachine.StopAll();
            SMachine.Update(Time.time);
        }

        // --------------------------------------------------------------------------

        // [System.Diagnostics.Conditional("UNITY_EDITOR")]
        // private void DebugLog (SubActionDebug.Actions action)
        // {
        // #if UNITY_EDITOR && !NO_DEBUG
        //             DebugHistory.PushFront(new SubActionDebug {
        //                     time = time,
        //                     transition = InteractionInstance.TransitionState.Null,
        //                     interaction = CurrentInteraction,
        //                     decision = CurrentContext.GetDecisionHistory(),
        //                     nodeIndex = (byte)255,
        //                     childIndex = (byte)255,
        //                     action = action
        //                 });
        // #endif
        // }
    }
}