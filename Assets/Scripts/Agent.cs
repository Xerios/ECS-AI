using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;
using UtilityAI;

namespace Engine
{
    [SelectionBase]
    public class Agent : EntityMonoBehaviour, IAgent
    {
        private bool IsInitialized;
        private Mind mind;
        private ActionManager actionMgr;

        public InfluenceMapBuilder localGrid = new InfluenceMapBuilder(3);

        public short JobId = -1;

        private byte _assignmentType;
        public byte AssignmentType {
            get => _assignmentType;
            set{
                _assignmentType = value;
                World.Active.EntityManager.SetComponentData(entity, new AssignmentTypeData { TypeId = value });
            }
        }

        private Entity _assignmentEntity;
        public Entity AssignmentEntity {
            get => _assignmentEntity;
            set{
                _assignmentEntity = value;
                World.Active.EntityManager.SetComponentData(entity, new AssignmentEntityData { AssignedEntity = value });
            }
        }

        [Header("ComponentData Setup")]
        public byte Faction = 0;
        public AccessTags AccessTag;
        public NeedTags NeedsTag;
        public Mindset[] Mindsets;

        [Header("Properties")]
        public CharacterAnimation Animator;
        public NavMeshAgent NavMeshAgent;
        public EnemyDetector Detector;

        public GameObject Weapon, Bullet, Slash;

        protected new Transform transform;

        public bool OverrideRotation;
        public float Rotation;
        private float RotationSmooth;
        private Vector3 VelocitySmooth;
        private bool dead;

        // public Unity.Mathematics.Random randomGenerator;


        public void Awake ()
        {
            transform = this.GetComponent<Transform>();

            // ----------------------- setup seq player ( has to be before Mind )
            // seqPlayer = this.gameObject.AddComponent<SequencePlayer>();

            NavMeshAgent.updateRotation = false;
            Rotation = UnityEngine.Random.value * 360;
        }

        public override void Init ()
        {
            // Debug.Log("Init");
            // ----------------------- setup mind
            actionMgr = new ActionManager();


            // -------------------------------------------------------------------------
            var cmds = AIManager.Instance.buffer;
            this.entity = cmds.CreateEntity();

            cmds.AddSharedComponent(this.entity, new AgentSelf { Value = this });
            cmds.AddSharedComponent(this.entity, new ActionManagerSelf { Value = actionMgr });
            cmds.AddSharedComponent(this.entity, new ScoreEvaluate { Evaluate = EvaluateScore });

            cmds.AddComponent(this.entity, new InitAgent());
            cmds.AddComponent(this.entity, new AgentFaction { LayerFlags = (byte)(1 << Faction) });
            cmds.AddComponent(this.entity, new AccessTagData { Value = (uint)AccessTag });

            cmds.AddComponent(this.entity, new StaminaData { Value = 100f, Max = 100f });
            // cmds.AddComponent(this.entity, new StaminaChangeData { Value = 0f });

            cmds.AddComponent(this.entity, new SignalPosition { Value = transform.position });
            cmds.AddComponent(this.entity, new SignalAbilityAssignment { Ability = (byte)UtilityAI.AbilityTags.Defend });

            cmds.AddComponent(this.entity, AssignmentTypeData.Empty());
            cmds.AddComponent(this.entity, AssignmentEntityData.Empty());

            // cmds.AddComponent(this.entity, new SignalActionType { decisionTags = (uint)UtilityAI.DecisionTags.Agent });
            // cmds.AddComponent(this.entity, new SignalFlagsType { Flags = DecisionFlags.NONE });
            // cmds.AddComponent(this.entity, new SignalBroadcast
            //     {
            //         radius = 20f,
            //         limit = 4,
            //     });
            cmds.AddBuffer<PropertyData>(this.entity);

            cmds.AddBuffer<DamageEvent>(this.entity);
            cmds.AddComponent<HealthStateData>(this.entity, new HealthStateData { health = 100, maxHealth = 100 });

            NeedsTag = new NeedTags
            {
                Water = UnityEngine.Random.Range(10, 100),
                Food = UnityEngine.Random.Range(10, 100),
            };

            cmds.AddComponent(this.entity, new NeedsData { Value = NeedsTag });


            if (Faction == 0) {
                GameResources.Current.Population++;
                GameManager.Instance.Agents.Add(this);
            }
        }

        public override void DeInit ()
        {
            mind.Dispose();
            actionMgr.Dispose();

            if (Faction == 0) {
                GameResources.Current.Population--;
                GameManager.Instance.Agents.Remove(this);
            }
        }

        public void InitSuccess (Entity ent, EntityCommandBuffer cmds)
        {
            // Debug.Log($"InitSuccess {ent}");
            this.entity = ent;

            mind = new Mind(ent, cmds);
            mind.AddMindsets(Mindsets);

            // Bootstrap.world.EntityManager.SetName(this.entity, "Agent " + this.entity.Index);
            this.name = this.name + " #" + this.entity.Index.ToString();

            // randomGenerator = new Unity.Mathematics.Random((uint)ent.Index);

            IsInitialized = true;

            // -------------------------------------------------------------------------

            var buffer = Bootstrap.world.EntityManager.GetBuffer<PropertyData>(this.entity);
            // buffer.Add(new PropertyData(PropertyType.Health, 1000));
            // buffer.Add(new PropertyData(PropertyType.Hunger,1000));

            // -------------------------------------------------------------------------
            actionMgr.Set("Wander", AgentActions.InteractionWander());
            actionMgr.Set("Pickup", AgentActions.InteractionPickup());
            actionMgr.Set("Attack", AgentActions.InteractionAttack());
            actionMgr.Set("Sleep", AgentActions.InteractionSleep());
            actionMgr.Set("Gather", AgentActions.InteractionGather());
            actionMgr.Set("Defend", AgentActions.InteractionDefend());
            actionMgr.Set("Reclaim", AgentActions.InteractionReclaim());
            actionMgr.Set("DragCorpse", AgentActions.InteractionDragCorpse());

            actionMgr.Blackboard.Self = this.entity;

            actionMgr.Blackboard.Set("self_agent", this);

            actionMgr.Blackboard.SetUpdate((data) =>
                {
                    var target = this.actionMgr.CurrentContext.Target;

                    EntityManager mgr = AIManager.Instance.mgr;

                    if (mgr.Exists(target)) {
                        if (mgr.HasComponent<SignalPosition>(target)) {
                            data.SetVec("target_position", mgr.GetComponentData<SignalPosition>(target).Value);
                        }
                        if (mgr.HasComponent<SignalGameObject>(target)) {
                            data.Set("target_gameobject", mgr.GetSharedComponentData<SignalGameObject>(target).Value);
                        }
                        if (mgr.HasComponent<AgentSelf>(target)) {
                            data.Set("target_agent", mgr.GetSharedComponentData<AgentSelf>(target).Value);
                        }
                    }
                });

            // forceHurt = AgentActions.InteractionAnimate("Hurt");
            // var forceDie = AgentActions.InteractionDie(); // ActionAnimate("Die");//

            // Props.onChange = (typ, value, delta) =>
            // {
            //     // Debug.Log($"{typ} => {value}   ({delta})");
            //     if (typ == PropertyType.Health) {
            //         if (value != 0) {
            //             actionMgr.Force(this.entity, forceHurt);
            //         }else{
            //             actionMgr.Force(this.entity, DeathSeat.Instance.InteractionSequence());
            //             mind.RemoveAllMindset();
            //         }
            //     }
            //     if (typ == PropertyType.Weapon) {
            //         // Debug.Log($"---------------------------- #{this.this.entity.Index}");
            //         // actionMgr.Force(ActionAnimate("Happy"));
            //     }
            // };
        }

        void Update ()
        {
            if (!IsInitialized) return;
            if (dead) {
                // AIManager.Instance.mgr.GetBuffer<DamageEvent>(this.entity).Clear();
                return;
            }

            localGrid.Solve();

            var dt = Time.deltaTime;

            Vector3 _velocity = NavMeshAgent.velocity;
            VelocitySmooth = Vector3.Lerp(VelocitySmooth, _velocity, dt * 10f);

            float speed = VelocitySmooth.magnitude;
            Animator.Velocity = VelocitySmooth;
            // Animator.Speed = speed * 0.05f;
            // Animator.X = Vector3.Dot(VelocitySmooth, transform.right);
            // Animator.Y = Vector3.Dot(VelocitySmooth, transform.forward);

            if (!OverrideRotation && speed > 2f) {
                Rotation = VelocitySmooth.normalized.DirectionXZ();
            }

            StaminaData.Add(AIManager.Instance.mgr, this.entity, -0.1f);
            // RotationSmooth = Mathf.LerpAngle(RotationSmooth, Rotation, dt * 3f);
            // this.transform.localRotation = Quaternion.Euler(0f, RotationSmooth, 0f);

            AIManager.Instance.mgr.SetComponentData(this.entity, new SignalPosition { Value = transform.position });

            // var currentNeeds = AIManager.Instance.mgr.GetComponentData<NeedsData>(this.entity).Value;
            // currentNeeds.Water++;
            // currentNeeds.Food++;
            // AIManager.Instance.mgr.SetComponentData(this.entity, new NeedsData { Value = currentNeeds });

            // --------------------------
            var timer = World.Active.EntityManager.GetComponentData<SpatialDetectionTimer>(entity);
            var state = World.Active.EntityManager.GetComponentData<SpatialDetectionState>(entity);
            Detector.SetDetectionLevel(timer.timer, state.state);

            // var proximities = World.Active.EntityManager.GetBuffer<EntityInProximity>(entity);
            // for (int i = 0; i < proximities.Length; i++) {
            //     byte myfaction = (byte)(1 << Faction);
            //     var isFriendly = (proximities[i].faction & myfaction) == myfaction;
            //     Debug.DrawLine(this.transform.position, proximities[i].position, isFriendly ? Color.green : Color.red);
            // }

            // --------------------------
            var healthState = AIManager.Instance.mgr.GetComponentData<HealthStateData>(this.entity);
            // healthState.Heal(0.0001f * Time.timeScale);

            // --------------------------

            var isDamaged = false;
            var impulseVec = Vector3.zero;
            var damage = 0.0f;
            var damageVec = Vector3.zero;

            // Apply hitcollider damage events
            var damageBuffer = AIManager.Instance.mgr.GetBuffer<DamageEvent>(this.entity);
            for (var eventIndex = 0; eventIndex < damageBuffer.Length; eventIndex++) {
                isDamaged = true;

                var damageEvent = damageBuffer[eventIndex];

                // GameDebug.Log(string.Format("ApplyDamage. Target:{0} Instigator:{1} Dam:{2}", healthState.name, m_world.GetGameObjectFromEntity(damageEvent.instigator), damageEvent.damage ));
                healthState.ApplyDamage(ref damageEvent);
                AIManager.Instance.mgr.SetComponentData(this.entity, healthState);

                impulseVec += damageEvent.direction * damageEvent.impulse;
                damageVec += damageEvent.direction * damageEvent.damage;
                damage += damageEvent.damage;

                // damageHistory.ApplyDamage(ref damageEvent, m_world.worldTime.tick);

                // if (damageBuffer[eventIndex].instigator != Entity.Null && AIManager.Instance.mgr.Exists(damageEvent.instigator) &&
                //     AIManager.Instance.mgr.HasComponent<DamageHistoryData>(damageEvent.instigator)) {
                //     var instigatorDamageHistory = AIManager.Instance.mgr.GetComponentData<DamageHistoryData>(damageEvent.instigator);
                //     if (m_world.worldTime.tick > instigatorDamageHistory.inflictedDamage.tick) {
                //         instigatorDamageHistory.inflictedDamage.tick = m_world.worldTime.tick;
                //         instigatorDamageHistory.inflictedDamage.lethal = 0;
                //     }
                //     if (healthState.health <= 0)
                //         instigatorDamageHistory.inflictedDamage.lethal = 1;

                //     AIManager.Instance.mgr.SetComponentData(damageEvent.instigator, instigatorDamageHistory);
                // }

                // collOwner.collisionEnabled = healthState.health > 0 ? 1 : 0;
                // AIManager.Instance.mgr.SetComponentData(this.entity, collOwner);
            }
            damageBuffer.Clear();


            var damageImpulse = impulseVec.magnitude;
            var damageDir = damageImpulse > 0 ? impulseVec.normalized : damageVec.normalized;

            if (healthState.health == 0) {
                Detector.SetDetectionLevel(0f);
                if (NavMeshAgent.isOnNavMesh) NavMeshAgent.isStopped = true;
                NavMeshAgent.enabled = false;
                LeanTween.cancel(this.gameObject);
                Animator.Die(damageDir);
                dead = true;

                // AIManager.Instance.mgr.RemoveComponent<SpatialTracked>(this.entity);
                AIManager.Instance.mgr.RemoveComponent<SpatialTrackProximity>(this.entity);
                AIManager.Instance.mgr.RemoveComponent<ActionManagerSelf>(this.entity);
                AIManager.Instance.mgr.RemoveComponent<MindReference>(this.entity);
                // AIManager.Instance.mgr.RemoveComponent<MindInitSystem.MindInitialized>(this.entity);
                mind.Dispose();
                actionMgr.Dispose();

                // DestroyEntity();
                // Destroy(this.gameObject, 5f);
            }else if (isDamaged) {
                AIManager.Instance.mgr.SetComponentData(this.entity, new SpatialDetectionTimer { timer = SpatialDetectionTimer.MAX_LIMIT });

                if (damageImpulse > 0) {
                    if (NavMeshAgent.enabled) NavMeshAgent.ResetPath();
                    actionMgr.Break(1f);
                    Animator.Hit(damageDir, true);
                    LeanTween.cancel(this.gameObject);
                    LeanTween.value(this.gameObject, (value) => {
                            if (NavMeshAgent.enabled) NavMeshAgent.Move(value * Time.timeScale);
                        }, impulseVec, Vector3.zero, 0.7f)
                    .setEaseOutExpo();
                    LeanTween.delayedCall(this.gameObject, 0.7f, () => Animator.Play("KnockedDownUp", speed: 15f));
                }else{
                    actionMgr.Break(0.2f);
                    Animator.Hit(damageDir, false);
                }

                // var charPredictedState = AIManager.Instance.mgr.GetComponentData<CharacterPredictedData>(this.entity);
                // // charPredictedState.damageTick = m_world.worldTime.tick;
                // charPredictedState.damageDirection = damageDir;
                // charPredictedState.damageImpulse = damageImpulse;
                // AIManager.Instance.mgr.SetComponentData(this.entity, charPredictedState);

                // if (healthState.health <= 0) {
                //     var ragdollState = AIManager.Instance.mgr.GetComponentData<RagdollStateData>(this.entity);
                //     ragdollState.ragdollActive = 1;
                //     ragdollState.impulse = impulseVec;
                //     AIManager.Instance.mgr.SetComponentData(this.entity, ragdollState);
                // }
            }
        }

        // --------------------------------------------
        public float EvaluateScore (Entity target)
        {
            // World.Active.EntityManager.GetBuffer.IsZero(PropertyType.Health) ? 0f : 1f;
            return 1f;
        }

        public int GetFaction () => Faction;
        public Vector3 GetPosition () => transform.position;
        public Transform GetTransform () => transform;

        public void Toggle (bool value)
        {
            NavMeshAgent.enabled = value;
            Animator.gameObject.SetActive(value);
        }
        public void Show () => Toggle(true);
        public void Hide () => Toggle(false);
        // --------------------------------------------

        public Mind GetMind () => mind;

        // #if UNITY_EDITOR
        public ActionManager GetActionManager () => actionMgr;
        // #endif
        // --------------------------------------------

        private void OnDrawGizmosSelected ()
        {
            if (!Application.isPlaying) return;

            // localGrid
            // .Start(GetPosition())
            // // .SetAllValues(1f)
            // .SelfFalloff()
            // // .AddMultipliedMap((byte)InfluenceMapSystem.InfluenceMapTypes.WORLD_OBSTACLES, -100f)
            // .Solve()
            // .DebugGizmos()
            // .End();

            // localGrid.Start(CurrentPosition.ToVector2Int())
            //       .AddPositiveMemory(mind.Memories, DecisionTags.Protection, 20f);

            // localGrid.DebugGizmos();

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(this.GetPosition(), Quaternion.Euler(0, Rotation, 0) * Vector3.forward * 10);

            // float t = UnityEngine.Random.value * (Mathf.PI * 2);
            // const float scale = 10f;
            // const float dist = 5f;

            // var dir = new Vector3(Mathf.Sin(Rotation * Mathf.Deg2Rad), 0, Mathf.Cos(Rotation * Mathf.Deg2Rad)) * dist;
            // var randomDestination = transform.position + dir;// + new Vector3(Mathf.Cos(t), 0, Mathf.Sin(t)) * scale;

            // Gizmos.DrawWireSphere(randomDestination, scale);
            // Gizmos.DrawWireSphere(randomDestination + new Vector3(Mathf.Sin(t), 0, Mathf.Cos(t)) * scale, 0.2f);
        }
    }
}