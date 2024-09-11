using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MariaMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    MariaInput mariaInput;
    CharacterController controller;

    void Start()
    {
        mariaInput = GetComponent<MariaInput>();
        controller = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        Vector2 moveDir = mariaInput.moveInput;
        Vector3 move = new Vector3(moveDir.x, 0, moveDir.y); // 입력값을 3D 공간의 Vector3로 변환 (Y축은 0으로 고정)
        controller.Move(move * Time.deltaTime * moveSpeed);
    }
}