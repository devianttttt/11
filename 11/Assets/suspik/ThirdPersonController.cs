using System;
using System.Collections;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotattionSpeed = 10f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Attack Settings")]
    [SerializeField] private float attackComboResetTime = 1f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float cameraDistance = 5f;
    [SerializeField] private float cameraHeight = 2f;
    [SerializeField] private float cameraSmoothSpeed = 10f;

    private CharacterController controller;
    private Animator animator;

    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;
    private Vector3 moveDirection;

    private int currentAttack = 0;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    private bool isCrouching = false;
    private float originalHeight;
    private Vector3 originalCenter;

    private Vector3 cameraOffset;
    private float mouseX;
    private float mouseY;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        originalHeight = controller.height;
        originalCenter = controller.center;

        if(cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        cameraOffset = new Vector3(0,cameraHeight, -cameraDistance);
    }

    private void Update()
    {
        HandleMovement();
        HandleCrouch();
        HandleAttack();
        UpdateCamera();
        ApplyGravity();

        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        float speedPercent = moveDirection.magnitude > 0 ? (currentSpeed / runSpeed) : 0;

        animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);

        animator.SetBool("IsCrouching", isCrouching);

        animator.SetBool("IsGrounded", controller.isGrounded);

        animator.SetBool("IsAttacking", isAttacking);
    }
    private void OawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position + transform.forward * 1f, attackRange);
    }


    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = ! isCrouching;
            if (isCrouching)
            {
                controller.height = originalHeight * 0.5f;
                controller.center = originalCenter * 0.5f;
            }
            else
            {
                controller.height = originalHeight;
                controller.center = originalCenter;
            }
        }
    }

    private void HandleAttack()
    {
        if(Time.time - lastAttackTime > attackComboResetTime)
        {
            currentAttack = 0;
        }
        if(Input.GetMouseButton(1) && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }
    }
    private IEnumerator PerformAttack()
    {
        isAttacking = true;

        string attackTrigger = currentAttack == 0 ? "Attack1" : "Attack2";

        animator.SetTrigger(attackTrigger);

        yield return new WaitForSeconds(0.1f);

        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;

        DetectEnemies();

        yield return new WaitForSeconds(animationLength * 0.8f);

        currentAttack = (currentAttack + 1) % 2;
        lastAttackTime = Time.time;
        isAttacking = false;
    }

    private void DetectEnemies()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward * 1f, attackRange, enemyLayer);
        foreach(Collider enemy in hitEnemies)
        {
            Debug.Log($"Hit enemy: {enemy.name}");
        }
    }
    private void UpdateCamera()
    {
        if(cameraTransform == null) return;

        mouseX += Input.GetAxis("Mouse X");
        mouseY -= Input.GetAxis("Mouse Y");
        mouseY = Mathf.Clamp(mouseY, -30, -60);

        Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0);

        Vector3 targetPosition = transform.position + rotation * cameraOffset;

        RaycastHit hit;
        if(Physics.Linecast(transform.position + Vector3.up * 1.5f, targetPosition, out hit))
        {
            targetPosition = hit.point + hit.normal * 0.3f;
        }

        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraSmoothSpeed * Time.deltaTime);

        cameraTransform.LookAt(transform.position + Vector3.up * 1.5f);
    }

    private void ApplyGravity()
    {
        if(controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }




    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * vertical 
        + right * horizontal).normalized;

        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        if(moveDirection.magnitude > 0.1f && !isAttacking)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotattionSpeed * Time.deltaTime);

            controller.Move(moveDirection * currentSpeed * Time.deltaTime); 
        }
    }

}
