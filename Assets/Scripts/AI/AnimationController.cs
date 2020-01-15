using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract partial class Character
{
    public class AnimationController
    {
        private readonly Character _owner;

        public Animator Animator { get; private set; }


        private const string FLEE_ANIMATION_BOOL = "Fleeing";
        private const string DEATH_ANIMATION_BOOL = "Dead";
        private const string ATTACK_ANIMATION_BOOL = "Attacking";
        private const string RANGED_ATTACK_ANIMATION_BOOL = "ArcherAttack";
        private const string MOVE_ANIMATION_BOOL = "Walking";
        private const string IDLE_ANIMATION_BOOL = "Idling";
        private const string RUN_ANIMATION_BOOL = "Running";
        private const string HIDE_ANIMATION_BOOL = "Hiding";
        private const string CHEER_ANIMATION_BOOL = "Cheering";
        private const string SURPRISE_ANIMATION_BOOL = "Surprised";
        private const string EAT_ANIMATION_BOOL = "Eating";
        private const string PROVOKE_ANIMATION_BOOL = "Provoking";
        private const string PICKUP_ANIMATION_BOOL = "PickUp";

        public AnimationController(Character owner, Animator animator)
        {
            _owner = owner;
            Animator = animator;
        }

        //TODO: override in goblin class.
        public void HandleAnimation()
        {
            if (!Animator)
            {
                return;
            }

            if (_owner.navMeshAgent)
            {
                Animator.SetFloat("Speed", _owner.navMeshAgent.speed);
            }

            if (!_owner.Alive())
            {
                Animate(DEATH_ANIMATION_BOOL);
            }
            else if (_owner.Fleeing())
            {
                Animate(FLEE_ANIMATION_BOOL);
            }
            else if (_owner.attackAnimation && _owner.Attacking())
            {
                Animate(_owner.Equipped.Values.Any(e => e && e.Type == Equipment.EquipmentType.Bow)
                    ? RANGED_ATTACK_ANIMATION_BOOL
                    : ATTACK_ANIMATION_BOOL);
            }
            else if (_owner.Searching() && _owner.LootTarget && Vector3.Distance(_owner.transform.position, _owner.LootTarget.transform.position) < 2f)
            {
                Animate(PICKUP_ANIMATION_BOOL);
            }
            else if (_owner.AgentVelocity > _owner.SpeedAnimationThreshold)
            {
                Animate(_owner.Walking ? MOVE_ANIMATION_BOOL : RUN_ANIMATION_BOOL);
            }
            else if (_owner.Provoking() && _owner as Goblin && Vector3.Distance(_owner.transform.position, (_owner as Goblin).ProvokeTarget.transform.position) < 5f)
            {
                Animate(PROVOKE_ANIMATION_BOOL);
            }
            else if (_owner.Hiding())
            {
                Animate(HIDE_ANIMATION_BOOL);
            }
            else if (_owner.Watching() && !_owner.IsChief() && !_owner.IsChallenger())
            {
                //Animator.SetLookAtPosition(Team.Leader.transform.position);
                _owner.transform.LookAt(_owner.Team.Leader.transform);
                Animate(CHEER_ANIMATION_BOOL);
            }
            else if (_owner.Surprised())
            {
                Animate(SURPRISE_ANIMATION_BOOL);
            }
            else if (_owner.Resting())
            {
                Animate(EAT_ANIMATION_BOOL);
            }
            else
            {
                Animate(IDLE_ANIMATION_BOOL);
            }
        }

        private void Animate(string boolName)
        {
            DisableOtherAnimations(Animator, boolName);
            Animator.SetBool(boolName, true);
        }

        private void DisableOtherAnimations(Animator animator, string animation)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name != animation && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(parameter.name, false);
                }
            }
        }

    }

    internal void SetAnimationRandom()
    {
        lastAnimationRandom = Random.Range(0, 4);
        _animationController?.Animator.SetInteger("AnimationRandom", lastAnimationRandom);

    }
}
