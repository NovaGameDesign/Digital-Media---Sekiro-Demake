using System;
using DigitalMedia.Combat.Abilities;
using UnityEngine;
using DigitalMedia.Core;
using DigitalMedia.Interfaces;
using DigitalMedia.Misc;
using UnityEngine.InputSystem;
using UnityEngine.U2D.Animation;
using Random = UnityEngine.Random;

namespace DigitalMedia.Combat
{
    public class PlayerCombatSystem : CoreCombatSystem, ICombatCommunication
    {
        #region Input

        private PlayerInput _playerInput;
        private InputAction attack;
        private InputAction block;
       
        private InputAction swapElement;
        
        #endregion
        
        public GameObject deathblowTarget = null;
        
        private int soundLastPlayed;
        
        private float ElementPrecentage;


        public GameObject[] sparks;
        private new PlayerStats stats;
        
        private void Start()
        {
            InitiateStateChange(State.Idle);
            
            //Get Input
            _playerInput = GetComponent<PlayerInput>();
            attack = _playerInput.actions["Attack"];
            block = _playerInput.actions["Block"];
            swapElement = _playerInput.actions["Swap Element"];
            
            //Assigning Functionality
            attack.performed += TryToAttack;
            block.performed += TryToBlock;
            block.canceled += TryToBlock;
            swapElement.performed += SwapElements;

            _animator = GetComponent<Animator>();
            _audioPlayer = GetComponent<AudioSource>();

            spriteLibrary = GetComponent<SpriteLibrary>();

            stats = GetComponent<PlayerStats>();
        }

        private void OnEnable()
        {
            _playerInput = GetComponent<PlayerInput>();
            attack = _playerInput.actions["Attack"];
            block = _playerInput.actions["Block"];
            swapElement = _playerInput.actions["Swap Element"];
            
            //Assigning Functionality
            attack.Enable();
            attack.performed += TryToAttack;
            
            block.Enable();
            block.performed += TryToBlock;
            block.canceled += TryToBlock;
            
            block.Enable();
            swapElement.performed += SwapElements;;
        }

        private void OnDisable()
        {
            attack.performed -= TryToAttack;
            attack.Disable();
            
            block.performed -= TryToBlock;
            block.canceled -= TryToBlock;
            block.Disable();
            
            swapElement.performed -= SwapElements;
            swapElement.Disable();
        }

        #region Input Activation

        private void SwapElements(InputAction.CallbackContext context)
        {
            currentElementIndex++;
            switch (currentElementIndex)
            {
                case 1:
                {
                    currentElement = Elements.Fire;
                    stats.SwapHealthbarUI(1);
                    break;
                }
                case 2:
                {
                    currentElement = Elements.Ice;
                    stats.SwapHealthbarUI(2);
                    break;
                }
                case 3:
                {
                    currentElement = Elements.Lightning;
                    stats.SwapHealthbarUI(3);
                    break;
                }
                default:
                {
                    currentElementIndex = 0;
                    currentElement = Elements.Default;
                    stats.SwapHealthbarUI(0);
                    break;
                }
            }
            
            spriteLibrary.spriteLibraryAsset = elementSprites[currentElementIndex];
        }
        
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
        
        private void TryToBlock(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                if(currentState != State.Blocking) return;
                InitiateStateChange(State.Idle);
                _animator.Play("Player_Block_End");
                blocking = false;
            }
            else if (context.performed)
            {
                if (currentState != State.Idle && !canInterruptState) return;

                InitiateStateChange(State.Blocking);

                _animator.Play("Player_Block_Start");
                blocking = true;
            }
        }

        #endregion

        #region Parry Related Functionality
        

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
            
            int randomSound = Random.Range(0, data.CombatData.parrySoundsNormal.Length - 1);
            while (soundLastPlayed == randomSound)
            {
                randomSound = Random.Range(0, data.CombatData.parrySoundsNormal.Length - 1);
            }
            soundLastPlayed = randomSound;
            _audioPlayer.PlayOneShot(data.CombatData.parrySoundsNormal[randomSound]);

           
            /*GameObject sparks = ObjectPool.SharedInstance.GetPooledObject(); */
            if (sparks[currentElementIndex] != null)
            {
                Transform sparksSpawnLocation = this.transform.Find("ParrySparksLocation").transform;
                Instantiate(sparks[currentElementIndex], sparksSpawnLocation.position, sparksSpawnLocation.rotation);
                /*sparks.transform.position = sparksSpawnLocation.position;
                sparks.transform.rotation = sparksSpawnLocation.rotation;
                sparks.SetActive(true);*/
                /*sparks[currentElementIndex].GetComponent<ParticleSystem>().PL*/
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
