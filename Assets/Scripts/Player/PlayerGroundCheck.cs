using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    #region Serialized Private Fields

    [Tooltip("Distance from feet position to check for ground")]
    [SerializeField] private float checkDistance = 1f;

    #endregion
    
    #region Private Fields

    private PlayerController playerControllerInstance;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        playerControllerInstance = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        // CheckGrounded();
    }

    private void OnTriggerEnter(Collider other)
    {
        playerControllerInstance.SetGroundedState(true);
    }

    private void OnTriggerStay(Collider other)
    {
        playerControllerInstance.SetGroundedState(true);
    }

    private void OnTriggerExit(Collider other)
    {
        playerControllerInstance.SetGroundedState(false);
    }

    #endregion

    #region Private Methods

    private void CheckGrounded()
    {
        playerControllerInstance.SetGroundedState(Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, checkDistance));
    }

    #endregion
}
