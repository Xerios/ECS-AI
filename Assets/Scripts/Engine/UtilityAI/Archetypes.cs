using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace UtilityAI
{
    public static class UtilityAIArchetypes
    {
        public static EntityArchetype MindArchetype;
        public static EntityArchetype DecisionArchetype, DecisionTargetArchetype, ConsiderationArchetype;
        public static EntityArchetype SignalArchetype;

        [RuntimeInitializeOnLoadMethod]
        public static void InitArchetypes ()
        {
            var mgr = World.Active.EntityManager;

            MindArchetype = mgr.CreateArchetype(
                    typeof(MindBelongsTo),
                    typeof(BestDecision),
                    typeof(ActiveDecision),
                    typeof(SelectBestDecision),
                    typeof(DecisionOption),
                    typeof(DecisionHistoryRecord),
                    typeof(AddNewTargets),
                    typeof(AcceptedDecisions),
#if UNITY_EDITOR
                    typeof(DebugConsideration),
#endif
                    typeof(DecisionInternal)
                    );

            SignalArchetype = mgr.CreateArchetype(
                    typeof(SignalPosition),
                    typeof(SignalActionType),
                    typeof(SignalFlagsType),
                    typeof(SignalGameObject));

            DecisionArchetype = mgr.CreateArchetype(
                    typeof(DecisionToAdd),
                    typeof(DecisionScore),
                    typeof(DecisionSelfEntity),
                    typeof(DecisionMindEntity),
                    typeof(DecisionWeight),
                    typeof(DecisionId),
                    typeof(DecisionPreferred),
                    typeof(DecisionTarget),
                    typeof(DecisionNoTarget)
                    );
            DecisionTargetArchetype = mgr.CreateArchetype(
                    typeof(DecisionToAdd),
                    typeof(DecisionScore),
                    typeof(DecisionSelfEntity),
                    typeof(DecisionMindEntity),
                    typeof(DecisionWeight),
                    typeof(DecisionId),
                    typeof(DecisionPreferred),
                    typeof(DecisionTarget),
                    typeof(DecisionLastSeen)
                    );

            ConsiderationArchetype = mgr.CreateArchetype(
                    // typeof(ConsiderationDisabled),
                    typeof(ConsiderationType), typeof(ConsiderationDecisionParent), typeof(ConsiderationMindParent),
                    typeof(ConsiderationData), typeof(ConsiderationModfactor),
                    typeof(ConsiderationCurve), typeof(ConsiderationScore));
        }
    }
}