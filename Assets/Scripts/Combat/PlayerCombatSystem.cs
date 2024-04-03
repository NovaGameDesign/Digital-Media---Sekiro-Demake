using System.Runtime.CompilerServices;
using UnityEngine;
using DigitalMedia.Core;
using DigitalMedia.Interfaces;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace DigitalMedia.Combat
{
    public class PlayerCombatSystem : CoreCombatSystem, ICombatCommunication
    {
        #region Input

        private PlayerInput _playerInput;
        private InputAction attack;
        private InputAction block;
        
        #endregion
        
        public GameObject deathblowTarget = null;
        [SerializeField] protected GameObject deathblowAirSlash;

        private const string ANIM_BLOCK = "Player_Block_Start";

        private int soundLastPlayed;
        
        private void Start()
        {
            InitiateStateChange(State.Idle);
            //Input
            _playerInput = GetComponent<PlayerInput>();
            attack = _playerInput.actions["Attack"];
            block = _playerInput.actions["Block"];
            //Assigning Functionality
            attack.performed += TryToAttack;
            block.performed += TryToBlock;
            block.canceled += TryToBlock;

            _animator = GetComponent<Animator>();
            _audioPlayer = GetComponent<AudioSource>();
        }

        #region Attack Related Functionality

        public virtual void TryToAttack(InputAction.CallbackContext context)
        {
            //Convert this to ability and have functionality for the different deathblow types (ie. boss vs basic enemy). 
            if (deathblowTarget != null)
            {
                Deathblow();
                return;
            }
            
            if (currentState == State.Airborne)
            {
//                Debug.Log(("tried to air attack"));
                TriggerAbility("Attack_Airborne");
                return;
            }
            
            switch (currentAttackIndex)
            {
                case 0:
                    TriggerAbility("Attack_Combo_One");
                    break;
                case 1:
                    TriggerAbility("Attack_Combo_Two");
                    break;
                case 2:
                    TriggerAbility("Attack_Combo_Three");
                    break;
                default:
                    currentAttackIndex = 0;
                    break;
            }
            
        }

        /*
        public void EndAttackSequence()
        {
            InitiateStateChange(State.Idle);
            currentAttackIndex = 0;
            _animator.Play("Idle");
        }
        */

        #endregion


        #region Parry Related Functionality

        private void TryToBlock(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                InitiateStateChange(State.Idle);
                _animator.Play("Player_Block_End");
                blocking = false;
            }
            else if (context.performed)
            {
                if (currentState != State.Idle && !canInterruptState) return;

                InitiateStateChange(State.Blocking);

                _animator.Play(ANIM_BLOCK);
                blocking = true;
            }
        }

        public void StartStopParrying(string shouldParry)
        {
            parrying = shouldParry == "true" ? true : false;;
        }

        public void WasParried()
        {
            throw new System.NotImplementedException();
        }

        public void DidParry()
        {
            _animator.Play("Player_Parry");
            parrying = false;
            
            int randomSound = Random.Range(0, data.CombatData.parrySoundsNormal.Length);
            while (soundLastPlayed == randomSound)
            {
                randomSound = Random.Range(0, data.CombatData.parrySoundsNormal.Length);
            }
            soundLastPlayed = randomSound;
            _audioPlayer.PlayOneShot(data.CombatData.parrySoundsNormal[randomSound]);
            
            GameObject sparks = ObjectPool.SharedInstance.GetPooledObject(); 
            if (sparks != null)
            {
                Transform sparksSpawnLocation = this.transform.Find("ParrySparksLocation").transform;
                sparks.transform.position = sparksSpawnLocation.position;
                sparks.transform.rotation = sparksSpawnLocation.rotation;
                sparks.SetActive(true);
                sparks.GetComponent<ParticleSystem>()?.Play();
            }
        }

        #endregion

        #region Deathblow
        
        public void Deathblow()
        {
            InitiateStateChange(State.Deathblowing);
            /*transform.GetComponent<Rigidbody2D>().simulated = false; The idea here is to maybe let the player "teleport" to their destination and do some sort of flash step quick attack deathblow. Idrk I'll probably do it when I have a bit more time to do afterimages for the player teleporting and stuff.
            transform.position = deathblowTarget.transform.Find("Deathblow Position").position;*/
           
            _animator.Play("Player_Deathblow");
        }

        public void EndDeathblowSequence()
        {
            InitiateStateChange(State.Idle);
            _animator.Play("Idle");
            /*transform.GetComponent<Rigidbody2D>().simulated = true;*/
            //Other stuff
        }
        
        public void SpawnDeathblowSlash()
        {
            //Play animations if we end up having one, otherwise just destroy the target and spawn the slash
            //Instantiate(deathblowAirSlash, deathblowAirSlash.transform);
            
            deathblowTarget.GetComponent<StatsComponent>()?.HandleLives();
            
            deathblowTarget = null;
        }
        
        #endregion

        public void TransformForwardDeathblow()
        {
            transform.position = transform.Find("Deathblow Pos").position;
            Debug.LogWarning("Moved the player forward during the deathblow.");
        }
    }

}