using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbnoSkeleton_MOD
{
    public class AbnoSkeleton : CreatureBase
    {
        private List<AgentModel> markedAgents = new List<AgentModel>();
        private AgentModel mindfulAgent;
        private AgentModel sightfulAgent;

        public override void OnFinishWork(UseSkill skill)
        {
            base.OnFinishWork(skill);

            if (skill?.agent == null) return;

            AgentModel agent = skill.agent;

            if (markedAgents.Contains(agent)) return;

            markedAgents.Add(agent);

            if (markedAgents.Count == 1)
            {
                mindfulAgent = agent;
                agent.additionalDef.B *= 0.1f;
                agent.SetAgentCommand(new AbnoSkeletonAttackCommand(agent, null));
            }
            else if (markedAgents.Count == 2)
            {
                sightfulAgent = agent;
                agent.additionalDef.P *= 0.1f;
                
                // Link them to attack each other
                if (mindfulAgent != null && sightfulAgent != null)
                {
                    mindfulAgent.SetAgentCommand(new AbnoSkeletonAttackCommand(mindfulAgent, sightfulAgent));
                    sightfulAgent.SetAgentCommand(new AbnoSkeletonAttackCommand(sightfulAgent, mindfulAgent));
                }
            }
            else if (markedAgents.Count >= 3)
            {
                foreach (AgentModel marked in markedAgents)
                {
                    marked.Die();
                }
                markedAgents.Clear();
                mindfulAgent = null;
                sightfulAgent = null;
            }
        }
    }

    public class AbnoSkeletonAttackCommand : WorkerCommand
    {
        private AgentModel target;
        private Timer attackTimer = new Timer();
        private const float ATTACK_RANGE = 5f;
        private const float ATTACK_DAMAGE = 10f;

        public AbnoSkeletonAttackCommand(AgentModel actor, AgentModel target)
        {
            this.actor = actor;
            this.target = target;
        }

        public override void Execute()
        {
            base.Execute();

            if (this.target == null || this.target.IsDead())
            {
                this.Finish();
                return;
            }

            PassageObjectModel passage = this.target.GetMovableNode().GetPassage();
            if (passage == null || passage != this.actor.GetMovableNode().GetPassage())
            {
                this.Finish();
                return;
            }

            float distance = MovableObjectNode.GetDistance(this.actor.GetMovableNode(), this.target.GetMovableNode());
            float range = distance - this.actor.radius - this.target.radius;

            if (range <= ATTACK_RANGE)
            {
                // Close enough to attack
                if (this.attackTimer.RunTimer())
                {
                    this.GiveDamage();
                    this.attackTimer.StartTimer(2f);
                }
            }
            else
            {
                // Move toward target
                if (!this.actor.GetMovableNode().IsMoving())
                {
                    this.actor.GetMovableNode().MoveToMovableNode(this.target.GetMovableNode(), false);
                }
            }
        }

        private void GiveDamage()
        {
            if (this.target.IsDead())
            {
                return;
            }

            this.target.TakeDamage(this.actor, new DamageInfo(RwbpType.R, ATTACK_DAMAGE));
            this.target.UnderAttack(this.actor);
        }

        public override void OnDestroy()
        {
            base.OnStop();
            this.actor.GetMovableNode().StopMoving();
        }
    }
}