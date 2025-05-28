using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

// Custom Physics Engine
public class PlayerMovement : MonoBehaviour
{
    public float jumpForce = 500f;
    public float moveForce = 100000f;
    public float rotateSpeed = 5f;
    public bool debug = false;
    public Vector3 spawnPoint;

    private Transform _transform;
    private Rigidbody _rb;
    private Vector3 _movementDirection;
    private bool _isGrounded = false;
    
    // Start is called before the first frame update
    void Start()
    {
        _transform = GetComponent<Transform>();
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        _transform.position = spawnPoint;
    }

    private void HandleJump()
    {
        if (!_isGrounded)
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _isGrounded = false;
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void CalculateMovement(float x, float z)
    {
        _movementDirection = (transform.right * x + transform.forward * z).normalized;
    }

    private void HandleRotation(float x)
    {
    }
    
    // Update is called once per frame
    void Update()
    {
        // Handle inputs
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        HandleJump();
        CalculateMovement(x, z);
    }
    
    void HandleDebugGizmos()
    {
        if (_transform != null)
        {
            Gizmos.DrawRay(_transform.position, _transform.forward);
        }
    }

    void OnDrawGizmos()
    {
        if (debug)
        {
            HandleDebugGizmos();
        }
    }

    void HandleMovement()
    {
        _rb.AddForce(_movementDirection * (moveForce * Time.fixedDeltaTime), ForceMode.Force);
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void CalculateGrounding(Collision collision)
    {
        if (collision.gameObject.CompareTag("Jumpable"))
        {
            _isGrounded = true;
        }
    }

    private void HandlePotentialDeath(Collision collision)
    {
        if (collision.gameObject.CompareTag("Fatal"))
        {
            // Reset the level
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        CalculateGrounding(collision);   
        HandlePotentialDeath(collision);
    }
}
