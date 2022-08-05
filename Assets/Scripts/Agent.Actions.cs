using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;
using UtilityAI;

namespace Engine
{
    public static partial class AgentActions
    {
        [StateScript]
        public static StateScript InteractionAttack ()
        {
            return new StateScript("Attack(Slash)",
                       new StateDefinition("move"){
                           {
                               StateDefinition.____BEGIN____,
                               // (context) => context.data.GetVec("target_position").Around(context.data.Get<Agent>("self_agent").GetPosition(), 3f),
                               (context) => {
                                   var targetpos = context.data.GetVec("target_position");
                                   var mypos = context.data.Get<Agent>("self_agent").GetPosition();
                                   context.data.SetVec("destination", targetpos.Around(mypos, 3f));
                                   context.SetComponent(context.data.Self, new SpatialDetectionState { state = SpatialDetectionState.AGGRO });
                               },
                               ActionsTest.HasArrivedToDestination,
                               (context) => context.Go("attack")
                           },
                           {
                               StateDefinition.____UPDATE____,
                               //    (context) => context.data.GetVec("target_position").Around(context.data.Get<Agent>("self_agent").GetPosition(), 3f),
                               (context) => {
                                   var targetpos = context.data.GetVec("target_position");
                                   var mypos = context.data.Get<Agent>("self_agent").GetPosition();
                                   context.data.SetVec("destination", targetpos.Around(mypos, 3f));
                               },
                               ActionsTest.MoveToDestination
                           }
                       },
                       new StateDefinition("attack"){
                           {
                               StateDefinition.____BEGIN____,
                               (context) => {
                                   var agent = context.data.Get<Agent>("self_agent");

                                   var rot = Quaternion.LookRotation(context.data.GetVec("target_position") - agent.transform.position, Vector3.up);
                                   var dir = rot * Vector3.forward;

                                   if (agent.Animator.Has("Attack")) {
                                       agent.Animator.Play("Attack", true);
                                   }else{
                                       Bootstrap.world
                                       .GetExistingSystem<PrefabSpawnSystem>()
                                       .Add(agent.Slash, agent.GetPosition() + Vector3.up * 3f + dir * 0.5f, dir.QuaternionY(), Vector3.zero);
                                       StaminaData.Add(context.data.Self, -10f);
                                   }

                                   if (AIManager.Instance.mgr.HasComponent<DamageEvent>(context.GetTarget())) {
                                       var damageEventBuffer = AIManager.Instance.mgr.GetBuffer<DamageEvent>(context.GetTarget());
                                       if (UnityEngine.Random.value > 0.8f) {
                                           DamageEvent.AddEvent(damageEventBuffer, Entity.Null, 70, dir, 1f);
                                       }else{
                                           DamageEvent.AddEvent(damageEventBuffer, Entity.Null, 50, dir, 0f);
                                       }
                                   }
                               },
                               ActionsTest.ShortWait,
                               ActionsTest.Repeat
                           }
                       }
                       );
        }

        [StateScript]
        public static StateScript InteractionWander ()
        {
            void RandomDestination (Context context)
            {
                const float scale = 10f;
                const float dist = 5f;
                var agent = context.data.Get<Agent>("self_agent");

                float t = UnityEngine.Random.value * (Mathf.PI * 2);
                var rotation = agent.Rotation * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Sin(rotation), 0, Mathf.Cos(rotation)) * dist;
                var randomDestination = agent.transform.position + dir + new Vector3(Mathf.Sin(t), 0, Mathf.Cos(t)) * scale;

                agent.localGrid
                .Start(agent.GetPosition(), context.GetStateTrackHash())
                // .AddMultipliedMap((byte)(10 * agent.Faction), 0.1f)// InfluenceMapSystem.InfluenceMapTypes.FACTION_x_UNITS
                .AddFalloff(randomDestination, 10f, 1f)
                .AddMultipliedMap((byte)InfluenceMapSystem.InfluenceMapTypes.WORLD_OBSTACLES, -100f)
                .AddReversedMultipliedMap((byte)InfluenceMapSystem.InfluenceMapTypes.BUILD_SPACE, -20f)
                .AddPositiveClampedMap((byte)(10 * agent.Faction), -1f)
                // .AddPositiveFalloff(Vector3.zero, 100f, 0.5f)
                // .ReduceValuesAroundSelf(25f)
                .End();
            }

            return new StateScript("Wander",
                       new StateDefinition("move"){
                           {
                               StateDefinition.____BEGIN____,
                               //    new StateTrackDefinition("first",
                               (context) => {
                                   RandomDestination(context);
                                   context.Wait();
                               },
                               (context) => {
                                   var agent = context.data.Get<Agent>("self_agent");
                                   if (!agent.localGrid.IsGenerated) {
                                       context.Suspend();
                                       return;
                                   }
                                   context.data.SetVec("destination", agent.localGrid.highestPosition);
                               },
                               ActionsTest.MoveToDestination,
                               (context) => {
                                   var agent = context.data.Get<Agent>("self_agent");
                                   agent.NavMeshAgent.speed = 5;
                               },
                               ActionsTest.HasArrivedToDestination,
                               ActionsTest.Wait,
                               ActionsTest.Repeat
                               //    ),
                               //    new StateTrackDefinition("second",
                               //    (context) => Debug.Log("Hello world, this is my custom track"),
                               //    ActionsTest.Wait,
                               //    (context) => Debug.Log("After wait message is now, today !")
                               //    )
                           }
                       }
                       );
        }

        internal static StateScript InteractionDragCorpse ()
        {
            void RandomDestination (Context context)
            {
                const float scale = 30f;
                const float dist = 5f;
                var agent = context.data.Get<Agent>("self_agent");

                float t = UnityEngine.Random.value * (Mathf.PI * 2);
                var rotation = agent.Rotation * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Sin(rotation), 0, Mathf.Cos(rotation)) * dist;
                var randomDestination = agent.transform.position + dir + new Vector3(Mathf.Sin(t), 0, Mathf.Cos(t)) * scale;

                agent.localGrid
                .Start(agent.GetPosition(), context.GetStateTrackHash())
                // .AddMultipliedMap((byte)(10 * agent.Faction), 0.1f)// InfluenceMapSystem.InfluenceMapTypes.FACTION_x_UNITS
                .AddFalloff(randomDestination, 10f, -1f)
                .AddMultipliedMap((byte)InfluenceMapSystem.InfluenceMapTypes.WORLD_OBSTACLES, -100f)
                .AddReversedMultipliedMap((byte)InfluenceMapSystem.InfluenceMapTypes.BUILD_SPACE, 20f)
                .AddPositiveClampedMap((byte)(10 * agent.Faction), -1f)
                // .AddPositiveFalloff(Vector3.zero, 100f, 0.5f)
                // .ReduceValuesAroundSelf(25f)
                .End();
            }

            return new StateScript("Drag Corpse",
                       new StateDefinition("go_to_corpse"){
                           {
                               StateDefinition.____BEGIN____,
                               (context) => context.data.SetVec("destination", context.data.GetVec("target_position")),
                               ActionsTest.MoveToDestination,
                               ActionsTest.HasArrivedToDestination,
                               (context) => context.Go("drag")
                           }
                       },
                       new StateDefinition("drag"){
                           {
                               StateDefinition.____BEGIN____,
                               (context) => {
                                   RandomDestination(context);
                                   context.Wait();
                               },
                               (context) => {
                                   var agent = context.data.Get<Agent>("self_agent");
                                   if (!agent.localGrid.IsGenerated) {
                                       context.Suspend();
                                       return;
                                   }
                                   context.data.SetVec("destination", agent.localGrid.highestPosition);
                               },
                               ActionsTest.MoveToDestination,
                               ActionsTest.HasArrivedToDestination,
                               (context) => {
                                   Debug.Log("Done!");
                               }
                           },
                           {
                               StateDefinition.____UPDATE____,
                               (context) => {
                                   var data = context.data;
                                   var agent = data.Get<Agent>("self_agent");

                                   var gameObj = AIManager.Instance.mgr.GetSharedComponentData<AgentSelf>(context.decisionHistory.GetSignalId()).Value;

                                   gameObj.transform.position = agent.GetPosition();
                                   Debug.Log("Dragging...");
                               }
                           }
                       }
                       );
        }

        [StateScript]
        public static StateScript InteractionPickup ()
        {
            return new StateScript("Pickup",
                       new StateDefinition("go_to_item"){
                           {
                               StateDefinition.____BEGIN____,
                               (context) => context.data.SetVec("destination", context.data.GetVec("target_position")),
                               ActionsTest.MoveToDestination,
                               ActionsTest.HasArrivedToDestination,
                               (context) => context.Go("pickup")
                           }
                       },
                       // ------------------
                       new StateDefinition("pickup"){
                           {
                               StateDefinition.____BEGIN____,
                               (context) => {
                                   var data = context.data;
                                   var agent = data.Get<Agent>("self_agent");
                                   agent.Animator.Play("Pickup", false);
                               },
                               ActionsTest.Wait,
                               (context) => {
                                   var data = context.data;
                                   var agent = data.Get<Agent>("self_agent");
                                   var go = data.Get<GameObject>("target_gameobject");


                                   // Two actions can execute at the exact same moment causing two get the pickup
                                   // IS THIS STILL RELEVANT WITH NEW STATESCRIPT SYSTEM???????????????????????????????
                                   if (go == null) return;
                                   var goEntity = go.GetComponent<EntityMonoBehaviour>();
                                   if (goEntity.IsDestroyed) return;

                                   if (!agent.GetMind().Mindsets.Contains(go.GetComponent<WeaponTest>().MindsetToAddOnWeaponPickup)) {
                                       agent.GetMind().RemoveMindset(agent.GetMind().Mindsets[1]);
                                       agent.GetMind().AddMindset(go.GetComponent<WeaponTest>().MindsetToAddOnWeaponPickup);
                                   }

                                   go.GetComponent<Rigidbody>().isKinematic = true;

                                   goEntity.DestroyEntity();
                                   GameObject.Destroy(go, 0.5f);
                               }
                           }
                       }
                       );
        }


        [StateScript]
        public static StateScript InteractionSleep ()
        {
            return new StateScript("Go Inside House",
                       new StateDefinition("find_house"){
                           {
                               StateDefinition.____BEGIN____,
                               (context) => {
                                   var found = false;
                                   SO_House house = null;
                                   Entity entity;
                                   NativeMultiHashMapIterator<int> it;
                                   for (bool success = GameManager.Instance.Storage.TryGetFirstValue(0, out entity, out it);
                                   success; success = GameManager.Instance.Storage.TryGetNextValue(out entity, ref it)) {
                                       // ...
                                       var gameObj = AIManager.Instance.mgr.GetSharedComponentData<SignalGameObject>(entity).Value;
                                       // ...
                                       house = gameObj.GetComponent<SO_House>();
                                       var canUse = house.Participants.CanUse(context.data.Self);
                                       if (canUse) {
                                           found = true;
                                           //    Debug.Log("Add participant");
                                           house.Participants.Add(context.data.Self);
                                           break;
                                       }
                                   }
                                   if (found) {
                                       //    Debug.Log("Engage participant");
                                       house.Participants.Engage(context.data.Self, (house.transform.position - context.data.Get<Agent>("self_agent").GetPosition()).sqrMagnitude);
                                       context.data.Set("house", house);
                                   }else{
                                       context.data.Remove("house");
                                       //    ActionsTest.Fail(context);
                                   }
                               },
                               (context) => {
                                   //    Debug.Log("Engage participant2");
                                   if (context.data.Has("house") && context.data.Get<SO_House>("house").Participants.IsFirstEngaged(context.data.Self)) {
                                       //    Debug.Log("Add participant");
                                       context.data.Get<SO_House>("house").Participants.RemoveEngaged(context.data.Self);
                                   }
                                   context.Go("get_to_house");
                               }
                           },
                           {
                               StateDefinition.____EXIT____,
                               (context) => {
                                   //    Debug.Log("-- RemoveEngaged participant");
                                   if (context.data.Has("house")) {
                                       context.data.Get<SO_House>("house").Participants.RemoveEngaged(context.data.Self);
                                       context.data.Get<SO_House>("house").Participants.Remove(context.data.Self);
                                   }
                               }
                           }
                       },
                       new StateDefinition("get_to_house"){
                           {
                               StateDefinition.____BEGIN____,
                               (context) => {
                                   //    Debug.Log("get_to_house participant");
                                   if (context.data.Has("house")) {
                                       context.data.Get<SO_House>("house").Participants.Add(context.data.Self);
                                       context.data.SetVec("destination", context.data.Get<SO_House>("house").transform.position);
                                   }else{
                                       var agent = context.data.Get<Agent>("self_agent");

                                       agent.localGrid
                                       .Start(agent.GetPosition(), context.GetStateTrackHash())
                                       .SelfFalloff()
                                       .AddMultipliedMap((byte)InfluenceMapSystem.InfluenceMapTypes.WORLD_OBSTACLES, -100f)
                                       // .AddPositiveClampedMap((byte)InfluenceMapSystem.InfluenceMapTypes.FACTION_0_UNITS, -1f) // CHANGE THIS LATER !!!!!!!!!!!!!!!!!!!!!!!!!
                                       // .AddPositiveClampedMap((byte)InfluenceMapSystem.InfluenceMapTypes.BUILD_SPACE, -100f) // CHANGE THIS LATER !!!!!!!!!!!!!!!!!!!!!!!!!
                                       .End();
                                       context.Wait();
                                   }
                               },
                               (context) => {
                                   var agent = context.data.Get<Agent>("self_agent");
                                   if (!agent.localGrid.IsGenerated) {
                                       context.Suspend();
                                       return;
                                   }
                                   if (!context.data.Has("house")) {
                                       context.data.SetVec("destination", agent.localGrid.highestPosition);
                                   }
                               },
                               ActionsTest.MoveToDestination,
                               ActionsTest.HasArrivedToDestination,
                               (context) => {
                                   if (context.data.Has("house")) {
                                       context.data.Get<SO_House>("house").ParticipantsInside++;
                                   }
                                   context.data.Get<Agent>("self_agent").Hide();
                                   context.data.Get<Agent>("self_agent").Detector.SetSleep();
                               },
                               (context) => context.Go("inside")
                           },
                           {
                               StateDefinition.____EXIT____,
                               (context) => {
                                   //    Debug.Log("-- RemoveEngaged participant22");
                                   if (context.data.Has("house")) {
                                       context.data.Get<SO_House>("house")?.Participants.Remove(context.data.Self);
                                   }

                                   context.data.Get<Agent>("self_agent").Show();
                                   context.data.Get<Agent>("self_agent").Detector.Reset();
                               }
                           }
                       },
                       new StateDefinition("inside"){
                           {
                               StateDefinition.____BEGIN____,
                               ActionsTest.Wait,
                               ActionsTest.Wait,
                               ActionsTest.Wait,
                               (context) => {
                                   StaminaData.SetMax(context.data.Self);
                                   if (context.data.Has("house")) {
                                       HealthStateData.SetMax(context.data.Self);
                                   }
                               },
                               (context) => context.Repeat()
                           },
                           {
                               StateDefinition.____END____,
                               (context) => {
                                   //    Debug.Log("Remove participant2");
                                   context.data.Get<Agent>("self_agent").Show();
                                   context.data.Get<Agent>("self_agent").Detector.Reset();
                                   if (context.data.Has("house")) {
                                       context.data.Get<SO_House>("house").ParticipantsInside--;
                                       context.data.Get<SO_House>("house").Participants.Remove(context.data.Self);
                                   }
                               }
                           },
                           {
                               StateDefinition.____EXIT____,
                               (context) => {
                                   //    Debug.Log("Force exit");
                                   //    Debug.Log("Remove participant3");
                                   context.data.Get<Agent>("self_agent").Show();
                                   context.data.Get<Agent>("self_agent").Detector.Reset();
                                   if (context.data.Has("house")) {
                                       context.data.Get<SO_House>("house").ParticipantsInside--;
                                       context.data.Get<SO_House>("house").Participants.Remove(context.data.Self);
                                   }
                               }
                           }
                       }
                       );
        }

        [StateScript]
        public static StateScript InteractionGather ()
        {
            return new StateScript("Go To Resource",
                       new StateDefinition("get_to_ressource"){
                           {
                               StateDefinition.____BEGIN____,
                               (context) => {
                                   var assignedToEntity = context.GetComponent<AssignmentEntityData>(context.data.Self).AssignedEntity;
                                   var assignedPosition = context.GetComponent<SignalPosition>(assignedToEntity).Value;
                                   context.data.SetVec("destination", assignedPosition);
                               },
                               ActionsTest.MoveToDestination,
                               ActionsTest.HasArrivedToDestination,
                               (context) => context.Go("take")
                           }
                       },
                       new StateDefinition("take"){
                           {
                               StateDefinition.____BEGIN____,
                               ActionsTest.StopAvoidance,
                               ActionsTest.ShortWait,
                               (context) => {
                                   //    Debug.Log("Resource taken");
                                   var data = context.data;
                                   var agent = data.Get<Agent>("self_agent");
                                   agent.Animator.Play("Pickup", false);
                                   GameResources.Current.Wood++;
                                   context.Go("get_to_stash");
                                   agent.NavMeshAgent.enabled = true;
                                   StaminaData.Add(context.data.Self, -10f);
                                   // if (this.gameObject != null) Destroy(this.gameObject, 0.1f);
                               },
                               ActionsTest.ShortWait,
                               ActionsTest.ResumeAvoidance
                           },
                           {
                               StateDefinition.____EXIT____,
                               ActionsTest.ResumeAvoidance
                           }
                       },
                       new StateDefinition("get_to_stash"){
                           {
                               StateDefinition.____BEGIN____,
                               (context) => context.data.SetVec("destination", GameManager.Instance.StorageSpace),
                               ActionsTest.MoveToDestination,
                               ActionsTest.HasArrivedToDestination,
                               ActionsTest.Repeat
                           }
                       }
                       );
        }


        [StateScript]
        public static StateScript InteractionDefend ()
        {
            return new StateScript("Defend",
                       new StateDefinition("walk around"){
                           {
                               StateDefinition.____BEGIN____,
                               (context) => {
                                   var assignedToEntity = context.GetComponent<AssignmentEntityData>(context.data.Self).AssignedEntity;
                                   if (assignedToEntity == Entity.Null) { ActionsTest.Suspend(context); return; }

                                   var assignedPosition = context.GetComponent<SignalPosition>(assignedToEntity).Value;
                                   assignedPosition += new Vector3(UnityEngine.Random.Range(-15, 15), 0, UnityEngine.Random.Range(-15, 15));
                                   context.data.SetVec("destination", assignedPosition);
                               },
                               ActionsTest.MoveToDestination,
                               ActionsTest.HasArrivedToDestination,
                               ActionsTest.Wait,
                               ActionsTest.Repeat
                           },
                           {
                               StateDefinition.____UPDATE____,
                               (context) => {
                                   var agent = context.data.Get<Agent>("self_agent");
                                   context.SetComponent(context.data.Self, new SpatialDetectionTimer { timer = SpatialDetectionTimer.DEFEND });
                                   var isCloseBy = Vector2.SqrMagnitude(context.data.GetVec("destination").XZ() - agent.GetPosition().XZ()) < (15 * 15);
                                   if (isCloseBy) {
                                       agent.NavMeshAgent.speed = 5;
                                   }else{
                                       agent.NavMeshAgent.speed = 15;
                                   }
                               }
                           }
                       });
        }


        [StateScript]
        public static StateScript InteractionReclaim ()
        {
            return new StateScript("Reclaim",
                       new StateDefinition("Go to reclaim"){
                           {
                               StateDefinition.____BEGIN____,
                               (context) => {
                                   var assignedToEntity = context.GetComponent<AssignmentEntityData>(context.data.Self).AssignedEntity;
                                   if (assignedToEntity == Entity.Null) { ActionsTest.Suspend(context); return; }

                                   var assignedPosition = context.GetComponent<SignalPosition>(assignedToEntity).Value;

                                   context.data.SetVec("destination", assignedPosition.Around(context.data.Get<Agent>("self_agent").GetPosition(), 3f));
                               },
                               ActionsTest.MoveToDestination,
                               ActionsTest.HasArrivedToDestination,
                               ActionsTest.Wait,
                               (context) => {
                                   JobListUI.Instance.ReclaimAndSetDefaultJob(context.data.Get<Agent>("self_agent"));
                                   GameManager.Instance.AgentJobChange.Execute();
                               }
                           }
                       }
                       );
        }


        // static Vector3 AttackDestination (Context context)
        // {
        //     var targetPos = context.data.GetVec("target_position");
        //     var agent = context.data.Get<Agent>("self_agent");

        //     // var mgr = AIManager.Instance.mgr;
        //     // var faction = agent.Faction;

        //     // if ((targetPos - agent.GetPosition()).sqrMagnitude > 10 * 10) {
        //     //     return targetPos.Around(agent.GetPosition(), 5f);
        //     // }

        //     Vector3 pos;

        //     agent.localGrid
        //     .Start(agent.GetPosition(), context.GetStateTrackHash())
        //     .SetAllValues(1)
        //     .AddNegativeClampedMap((byte)(10 * agent.Faction)) // InfluenceMapSystem.InfluenceMapTypes.FACTION_x_UNITS
        //     .AddFalloff(targetPos, 10f, -0.5f)
        //     .ReduceValuesAroundSelf(27f)
        //     .AddMultipliedMap((byte)InfluenceMapSystem.InfluenceMapTypes.WORLD_OBSTACLES, -100f)
        //     .Solve()
        //     .GetHighestPosition(out pos)
        //     .End();

        //     return pos;
        // }

        // [StateScript]
        // public static StateScript InteractionShoot ()
        // {
        //     return new StateScript("Attack",
        //                new StateDefinition("move")
        //                .OnBegin(
        //                ActionsTest.HasArrivedToDestination,
        //                (context) => context.Go("circle")
        //                )
        //                .OnUpdate(
        //                (context) => context.data.SetVec("destination", context.data.GetVec("target_position").Around(context.data.Get<Agent>("self_agent").GetPosition(), 5f)),
        //                ActionsTest.MoveToDestination
        //                ),
        //                // ------------------
        //                new StateDefinition("circle")
        //                .OnBegin(
        //                (context) => context.data.SetVec("destination", AttackDestination(context.data)),
        //                ActionsTest.MoveToDestination,
        //                ActionsTest.HasArrivedToDestination,
        //                ActionsTest.Wait,
        //                (context) => {
        //                    var agent = context.data.Get<Agent>("self_agent");

        //                    var rot = Quaternion.LookRotation(context.data.GetVec("target_position") - agent.transform.position, Vector3.up);
        //                    var dir = rot * Vector3.forward;

        //                    Bootstrap.world.GetExistingSystem<PrefabSpawnSystem>().Add(
        //                    agent.Bullet,
        //                    agent.GetPosition() + Vector3.up * 3.5f + dir * 5,
        //                    Quaternion.identity,
        //                    dir * 200
        //                    );
        //                }
        //                ).OnEnd(ActionsTest.Repeat)
        //                );
        // }
    }
}