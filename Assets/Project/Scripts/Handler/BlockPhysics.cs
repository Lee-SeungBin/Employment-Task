using UnityEngine;

/// <summary>
/// 블록의 물리 연산을 담당하는 클래스
/// </summary>
public class BlockPhysics : MonoBehaviour
{
    private bool isColliding;
    private Vector3 lastCollisionNormal;
    private readonly float collisionResetTime = 0.1f;
    private readonly float moveSpeed = 25f;           
    private readonly float followSpeed = 30f;        
    private readonly float maxSpeed = 20f;
    
    private float lastCollisionTime;  

    private Rigidbody rb;
    private Outline outline;
    
    private Vector3 followTarget;

    #region MonoBehaviour Events

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 2f;
        outline.enabled = false;
    }

    private void Update()
    {
        // 충돌 상태 자동 해제 검사
        if (isColliding && Time.time - lastCollisionTime > collisionResetTime)
        {
            // 일정 시간 동안 충돌 갱신이 없으면 충돌 상태 해제
            ResetCollisionState();
        }
    }
    
    void FixedUpdate()
    {
        // 물리 이동
        if (!rb.isKinematic)
        {
            ApplyFollowPhysics();
        }
    }

    #endregion

    #region Collsion Events

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);   
    }

    private void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision);   
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.contactCount > 0 && Vector3.Dot(collision.contacts[0].normal, lastCollisionNormal) > 0.8f)
        {
            ResetCollisionState();
        }
    }

    #endregion
    /// <summary>
    /// 물리 연산 가능 함수
    /// </summary>
    public void EnablePhysics()
    {
        rb.isKinematic = false;
        outline.enabled = true;
    }
    /// <summary>
    /// 물리 연산 불가능 함수
    /// </summary>
    public void DisablePhysics()
    {
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        outline.enabled = false;
    }
    /// <summary>
    /// 캐싱된 followTarget을 설정해주는 함수
    /// </summary>
    /// <param name="targetPos"> followTarget </param>
    public void FollowPointer(Vector3 targetPos)
    {
        followTarget = targetPos;
    }
    /// <summary>
    /// 물리 연산을 통해 블록이 움직이는 함수
    /// </summary>
    private void ApplyFollowPhysics()
    {
        Vector3 moveVector = followTarget - transform.position;
        
        // 충돌 상태에서 마우스가 충분히 멀어지면 충돌 상태 해제
        float distanceToMouse = Vector3.Distance(transform.position, followTarget);
        if (isColliding && distanceToMouse > 0.5f)
        {
            if (Vector3.Dot(moveVector.normalized, lastCollisionNormal) > 0.1f)
            {
                ResetCollisionState();
            }
        }
        
        // 속도 계산 개선
        Vector3 velocity;
        if (isColliding)
        {
            // 충돌면에 대해 속도 투영 (실제 이동)
            Vector3 projectedMove = Vector3.ProjectOnPlane(moveVector, lastCollisionNormal);
            
            velocity = projectedMove * moveSpeed;
        }
        else
        {
            velocity = moveVector * followSpeed;
        }
        
        // 속도 제한
        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }

        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, velocity, Time.fixedDeltaTime * 10f);
        }
    }
    /// <summary>
    /// 충돌 체크 함수
    /// </summary>
    /// <param name="collision"> 충돌체 </param>
    private void HandleCollision(Collision collision)
    {
        if (rb.isKinematic) return;
        if (collision.contactCount == 0 || collision.gameObject.layer == LayerMask.NameToLayer("Board")) return;

        Vector3 normal = collision.contacts[0].normal;
        if (Vector3.Dot(normal, Vector3.up) < 0.8f)
        {
            isColliding = true;
            lastCollisionNormal = normal;
            lastCollisionTime = Time.time;
        }
    }
    /// <summary>
    /// 충돌 상태 리셋 함수
    /// </summary>
    private void ResetCollisionState()
    {
        isColliding = false;
        lastCollisionNormal = Vector3.zero;
    }
}
