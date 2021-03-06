﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyTiles : MonoBehaviour
{
    public Material DefaultMaterial;        // glow up when a player steps on it
    public Material GlowingMaterial;
    private Renderer TileMaterial;
    public Texture MainTex;

    static Dictionary<GameObject, Color> PlayerMat = new Dictionary<GameObject, Color>();     // to avoid creating too many materials. Make one material for each player.
    static Dictionary<Color, string> StringColors = new Dictionary<Color, string>();
    // dictionary between player and material
    public float ResetTimer = 1.5f;
    private float CurrentTimer;
    private void Awake()
    {
        TileMaterial = GetComponentInParent<Renderer>();
        TileMaterial.material = DefaultMaterial;
    }
    // play VFX?

    private void OnTriggerEnter(Collider other)         // not synced. Movement is synced so we do not need to sync this script online at all.
    {    
        if(other.tag == Tags.PLAYER_TAG && PlayerMat.ContainsKey(other.gameObject))
        {
            Color PlayerCol = PlayerMat[other.gameObject];
            TileMaterial.material = GlowingMaterial;
            TileMaterial.material.color = PlayerCol;
            TileMaterial.material.SetColor("_EmissionColor", PlayerCol * Player_Colors.GetColorIntensity(StringColors[PlayerCol])); // change this multiplier.
            InitiateTimer();
        }
    }

    private void InitiateTimer()
    {
        CurrentTimer = ResetTimer;
    }
    private void ResetMaterial()
    {
        TileMaterial.material = DefaultMaterial;
        CurrentTimer = -1;
    }

    private void Update()           // player stepping on platform again resets the timer.
    {
        if(CurrentTimer >= 0)
        {
            CurrentTimer -= BoltNetwork.FrameDeltaTime;
            if(CurrentTimer <= 0)
            {
                ResetMaterial();
            }
        }
    }

    public static void AddPlayer(GameObject obj,Color playerColor,string userColor)      // adds the player so we can quickly grab an the color without using getcomponent.
    {
        if(!PlayerMat.ContainsKey(obj))
            PlayerMat.Add(obj, playerColor);
        if (!StringColors.ContainsKey(playerColor)) 
            StringColors.Add(playerColor, userColor);
    }

    public static void ResetStaticLists()
    {
        PlayerMat.Clear();
        StringColors.Clear();
    }
}
