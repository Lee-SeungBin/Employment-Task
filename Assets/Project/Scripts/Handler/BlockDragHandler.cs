using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 블록 드래그 핸들러
/// </summary>
public class BlockDragHandler : MonoBehaviour
{
    public int horizon = 1;
    public int vertical = 1;
    public int uniqueIndex;
    public List<ObjectPropertiesEnum.BlockGimmickType> gimmickType;
    public List<BlockObject> blocks = new();
    public List<Vector2> blockOffsets = new();
    public bool Enabled = true;
    
    private Camera mainCamera;
    private Vector3 offset;
    private float zDistanceToCamera;
    private bool isDragging;
    
    // 블록 드랍(Mouse Up)시 해당 이벤트 발생
    public event BlockDropDelegate OnBlockDrop;
    
    private BlockPhysics blockPhysics;

    #region MonoBehaviour Events

    private void Start()
    {
        mainCamera = Camera.main;
        blockPhysics = GetComponent<BlockPhysics>();
    }
    
    private void Update()
    {
        if (isDragging)
        {
            Vector3 target = GetMouseWorldPosition() + offset;
            blockPhysics.FollowPointer(target);
        }
    }
        
    private void OnDisable()
    {
        transform.DOKill(true);
    }
    
    private void OnDestroy()
    {
        transform.DOKill(true);
    }
    
    #endregion

    #region Mouse Events

    private void OnMouseDown()
    {
        if (!Enabled) return;
        
        isDragging = true;
        blockPhysics.EnablePhysics();
        
        // 카메라와의 z축 거리 계산
        zDistanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
        
        // 마우스와 오브젝트 간의 오프셋 저장
        offset = transform.position - GetMouseWorldPosition();
    }

    private void OnMouseUp()
    {
        if (!isDragging)
        {
            return;
        }
        
        isDragging = false;
        blockPhysics.DisablePhysics();
        Vector3 dropPos = GetMouseWorldPosition() + offset;
        OnBlockDrop?.Invoke(this, dropPos);
    }

    #endregion
    
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = zDistanceToCamera;
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }
    
    public Vector3 GetCenterX()
    {
        if (blocks == null || blocks.Count == 0)
        {
            return Vector3.zero; // Return default value if list is empty
        }
    
        float minX = float.MaxValue;
        float maxX = float.MinValue;
    
        foreach (var block in blocks)
        {
            float blockX = block.transform.position.x;
        
            if (blockX < minX)
            {
                minX = blockX;
            }
        
            if (blockX > maxX)
            {
                maxX = blockX;
            }
        }
    
        // Calculate the middle value between min and max
        return new Vector3((minX + maxX) / 2f, transform.position.y, 0);
    }
    
    public Vector3 GetCenterZ()
    {
        if (blocks == null || blocks.Count == 0)
        {
            return Vector3.zero; // Return default value if list is empty
        }
    
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;
    
        foreach (var block in blocks)
        {
            float blockZ = block.transform.position.z;
        
            if (blockZ < minZ)
            {
                minZ = blockZ;
            }
        
            if (blockZ > maxZ)
            {
                maxZ = blockZ;
            }
        }
    
        return new Vector3(transform.position.x, transform.position.y, (minZ + maxZ) / 2f);
    }

    public void DestroyMove(Vector3 pos, ParticleSystem particle)
    {
        transform.DOMove(pos, 1f).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                Destroy(particle.gameObject);
                Destroy(gameObject);
                //block.GetComponent<BlockShatter>().Shatter();
            });
    }
    
    public delegate void BlockDropDelegate(BlockDragHandler blockDragHandler, Vector3 pos);
}