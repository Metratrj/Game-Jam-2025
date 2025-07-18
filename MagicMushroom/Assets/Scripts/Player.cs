using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class Player : MonoBehaviour
{
    // Movement
    public float MaxMoveSpeed;
    public float MoveSpeedHardCap;
    public float AccelerationDuration;
    public float DecelerationDuration;
    public bool WantsToMove;
    public Vector3 CurrentVelocity;
    public Vector3 CurrentMovementVeloctity;

    public Vector3 MoveInput;

    public Rigidbody rb;

    private float AccelerationPerFixed => 1f / AccelerationDuration * Time.fixedDeltaTime;
    private float DecelerationPerFixed => 1f / DecelerationDuration * Time.fixedDeltaTime;


    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 rawInput = context.ReadValue<Vector2>();
        MoveInput = new Vector3(rawInput.x, 0, rawInput.y);
    }

    private void Move()
    {
        WantsToMove = !Mathf.Approximately(MoveInput.magnitude, 0);
        Movement(AccelerationPerFixed, DecelerationPerFixed);

        //ClampVelocity();
        //ApplyVelocity();

    }

    //private void ApplyVelocity()
    //{
    //    CurrentVelocity = new Vector3(CurrentMovementVeloctity.x, CurrentMovementVeloctity.y, 
    //        CurrentMovementVeloctity.z);
    //    rb.linearVelocity = CurrentVelocity;
    //}
    //
    //private void ClampVelocity()
    //{
    //    if (CurrentMovementVeloctity.magnitude > MaxMoveSpeed)
    //        CurrentMovementVeloctity -= CurrentMovementVeloctity * DecelerationPerFixed;
    //
    //    if (CurrentMovementVeloctity.magnitude > MoveSpeedHardCap)
    //    {
    //        CurrentMovementVeloctity = CurrentMovementVeloctity.normalized;
    //        CurrentMovementVeloctity *= MoveSpeedHardCap;
    //    }
    //}

    private void Movement(float acceleration, float deceleration)
    {
        if (WantsToMove)
            CurrentMovementVeloctity +=
                new UnityEngine.Vector3(MoveInput.x, 0, MoveInput.z) * (MaxMoveSpeed * acceleration);

        else if (CurrentMovementVeloctity.magnitude > 0)
            CurrentMovementVeloctity -= CurrentMovementVeloctity * deceleration;
    }
}
