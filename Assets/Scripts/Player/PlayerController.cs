using System;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController i;
    
    #region Public Fields
    
    [HideInInspector]
    public float movementSpeedMofifier = 1f;

    #endregion
    
    #region Serialized Private Fields

    [Header("Player Attributes:")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float smoothTime;

    [Header("Object references:")]
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private GameObject glasses;
    [SerializeField] private TextMeshPro nicknameText;
    
    #endregion

    #region Private Fields

    // Movement
    private bool isGrounded;
    private float verticalLookRotation;
    private Vector3 smoothMoveVelocity;
    private Vector3 moveAmount;
    private Vector3 movementDir;
    
    // Objects
    private Rigidbody rb;
    private Camera cam;
    private PhotonView PV;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        if (!i)
        {
            i = this;
        }
    }

    private void Start()
    {
        nicknameText.text = PV.Controller.NickName;
        cam = GetComponentInChildren<Camera>();
        
        if (!PV.IsMine)
        {
            cam.enabled = false;
            GetComponent<AudioListener>().enabled = false;
            // Destroy(rb);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            glasses.SetActive(false);
        }
    }

    private void Update()
    {
        if(!PV.IsMine) return;
        if(UIManager.instance.isPaused) return;
        
        // Movement
        GetInput();
        LookRotation();
        if(Input.GetButtonDown("Jump")) Jump();
    }

    private void FixedUpdate()
    {
        if(!PV.IsMine) return;
        if(UIManager.instance.isPaused) return;

        ProcessInput();
    }

    #endregion

    #region Public Methods

    public void SetGroundedState(bool _state)
    {
        isGrounded = _state;
    }

    public void SetSensitivity(float _value)
    {
        mouseSensitivity = _value;
    }

    #endregion
    
    #region Private Methods

    private void LookRotation()
    {
        transform.Rotate(Vector3.up * (Input.GetAxisRaw("Mouse X") * mouseSensitivity));

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    private void GetInput()
    {
        movementDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        moveAmount = Vector3.SmoothDamp(moveAmount, movementDir * ((Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed) * movementSpeedMofifier), ref smoothMoveVelocity, smoothTime);
    }

    private void ProcessInput()
    {
        // Vector3 movedir = new Vector3(moveAmount.x, rb.velocity.y, moveAmount.z);
        // rb.velocity = transform.TransformDirection(movedir);
        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(transform.up * (jumpForce * 1.5f), ForceMode.Impulse);
        }
    }

    #endregion
}
