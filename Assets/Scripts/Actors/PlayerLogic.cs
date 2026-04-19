/*******************************************************************************
File:      PlayerLogic.cs
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      11/11/2022
Course:    DES 214

Description:
    Handles all player logic and stats.

*******************************************************************************/
using System;
using UnityEngine;
using static PCG;

public class PlayerLogic : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////////////
    // DESIGNER ADJUSTABLE VALUES
    //////////////////////////////////////////////////////////////////////////

    //Starting health for the level
    public int StartingHealth = 1;
    //Starting speeed before any speed boosts
    public int StartingSpeed = 1;

    //////////////////////////////////////////////////////////////////////////

    //Maximum health, only increased by health boosts
    [HideInInspector]
    public int MaxHealth
    {
        get { return _MaxHealth; }
        set { PlayerHealthBar.MaxHealth = value; _MaxHealth = value; }
    }
    private int _MaxHealth;

    //Current health
    [HideInInspector]
    public int Health
    {
        get { return _Health; }
        set { PlayerHealthBar.Health = value; _Health = value; }
    }
    private int _Health;

    //Player health bar
    private HealthBar PlayerHealthBar;

    //Current speed, including speed boosts
    [HideInInspector]
    public int Speed
    {
        get { return _Speed; }
        set { _Speed = value; if (value > MaximumSpeed) _Speed = MaximumSpeed;}
    }
    private int _Speed;
    //Maximum speed, even with speed boosts
    private int MaximumSpeed = 10; //If this were higher, the player could outrun their own bullets

    //Number of keys currently held
    [HideInInspector]
    public int SilverKeys = 0;
	[HideInInspector]
    public int GoldKeys = 0;

    [HideInInspector]
    public bool CinematicMode = false; //Don't do anything because a cinematic is occuring

    public static event Action OnPlayerDied;

    // Start is called before the first frame update
    void Start()
    {
        //Spawn canvas for the player's UI in the upper left corner
        var canvas = Instantiate(PCGObject.Prefabs["uicanvas"]);
        PlayerHealthBar = canvas.transform.Find("HeartDisplayPanel").GetComponent<HealthBar>();

        //Initialize stats
        MaxHealth = StartingHealth;
        Health = MaxHealth;
        Speed = StartingSpeed;
    }

    //Update is called once per frame
    void Update()
    {
        //Don't do anything if in cinematic mode
        if (CinematicMode)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            return;
        }

        UpdatePlayerMovement();
    }

    void UpdatePlayerMovement()
    {
        //Rotate player towards mouse position
        var worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);
        transform.up = (worldMousePos - transform.position).normalized;

        //Reset direction every frame
        Vector2 dir = Vector2.zero;

        // if we are NOT holding down C we can use the arrow keys and wasd to move
        if (Input.GetKey(KeyCode.C) == false)
        {
            //Determine movement direction based on input
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                dir += Vector2.up;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                dir += Vector2.left;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                dir += Vector2.down;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                dir += Vector2.right;
        }
        else // if we ARE holding down C, we can ONLY use wasd to move
        {
            //Determine movement direction based on input
            if (Input.GetKey(KeyCode.W))
                dir += Vector2.up;
            if (Input.GetKey(KeyCode.A))
                dir += Vector2.left;
            if (Input.GetKey(KeyCode.S))
                dir += Vector2.down;
            if (Input.GetKey(KeyCode.D))
                dir += Vector2.right;
        }
        //Apply velocity
        GetComponent<Rigidbody2D>().velocity = dir.normalized * Speed;
    }

    //Find any child weapons and increment their number of shots by the given amount
    public void IncrementWeaponShots(int increase = 1)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            WeaponLogic weapon = transform.GetChild(i).GetComponent<WeaponLogic>();
            if (weapon != null)
            {
                weapon.BulletsPerShot += increase;
                break;
            }
        }
    }

    //Find any child weapons and increment their range by the given amount
    public void IncrementWeaponRange(float increase = 1.0f)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            WeaponLogic weapon = transform.GetChild(i).GetComponent<WeaponLogic>();
            if (weapon != null)
            {
                weapon.BulletRange += increase;
                break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Check for collision against enemy bullets
        var bullet = collision.GetComponent<BulletLogic>();
        if (bullet != null && bullet.Team == Teams.Enemy)
        {
            Health -= 1;
            if (Health <= 0) //We are dead, so reset the level
            {
                // fire an event when the player dies
                OnPlayerDied?.Invoke();

                Health = StartingHealth;

                transform.position = new Vector3(-19.25f, 18.13f, 0.0f);

                //gameObject.SetActive(false);
                //PCGObject.ResetLevel(1.5f); //Reset after a 1.5 second delay
            }
        }
    }
}
