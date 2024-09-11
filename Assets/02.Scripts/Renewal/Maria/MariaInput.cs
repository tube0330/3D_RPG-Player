using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MariaInput : MonoBehaviour
{
    public PlayerInput mariaInput;
    //InputActionMap playerMap;
    InputAction moveAction;
    public InputAction fireAction;

    public Vector2 moveInput { get; private set; }

    void Start()
    {
        mariaInput = GetComponent<PlayerInput>();
        //playerMap = mariaInput.actions.FindActionMap("Player");
        moveAction = mariaInput.actions["Move"];
        fireAction = mariaInput.actions["Fire"];    
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
    }
}
