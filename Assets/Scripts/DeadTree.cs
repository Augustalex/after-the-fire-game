﻿using System;
using UnityEngine;


    public class DeadTree : MonoBehaviour
    {
        public Material burningMaterial;
        public Material deadMaterial;
        public MeshRenderer treeMesh;
        public ParticleSystem fireParticles;
        [HideInInspector] public Island island; // Will be set in Island.cs onEnable
        
        [Serializable]
        private enum State
        {
            Burning = 0,
            Dead = 1,
            Planted = 2,
        }

        [SerializeField] private State currentState = State.Burning;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (currentState == State.Burning)
                {
                    // Check if player has enough snow
                    var playerGrower = other.GetComponent<PlayerGrower>();
                    var playerController = other.GetComponent<PlayerController>();
                    playerController.ZeroBoostJuice();
                    if (playerGrower.GrowthProgress() > 0.2f)
                    {
                        playerGrower.ReleaseThirdOfSnow();
                        currentState = State.Dead;
                        treeMesh.material = deadMaterial;
                        fireParticles.Stop();
                        island.OnExstuinguishTree();
                    }
                }
                else if (currentState == State.Dead)
                {
                    Debug.Log("Already dead");
                }
                
            }
        }
    }
