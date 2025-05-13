using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보드에서 블록을 생성하고 머테리얼을 변경하는 뷰
/// </summary>
public class BoardView : MonoBehaviour, IBoardView
{
    [Header("Prefabs")]
    [SerializeField] private GameObject boardBlockPrefab;
    [SerializeField] private GameObject blockGroupPrefab;
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private GameObject[] wallPrefabs;
    [SerializeField] private GameObject quadPrefab;

    [Header("Materials")]
    [SerializeField] private Material[] blockMaterials;
    [SerializeField] private Material[] testBlockMaterials;
    [SerializeField] private Material[] wallMaterials;

    [Header("Transforms")]
    [SerializeField] private Transform spawnerTr;

    [SerializeField] private Transform quadTr;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem destroyParticlePrefab;

    private GameObject boardParent;
    private GameObject playingBlockParent;
    private GameObject wallsParent;
    private float yoffset = 0.625f;
    // private float wallOffset = 0.225f;
    private List<GameObject> quads = new();

    public event IBoardView.CreateBoardBlockDelegate OnCreateBlock;
    public event IBoardView.CreateWallDelegate OnCreateWall;
    public event IBoardView.CreatePlayingBlockDelegate OnCreatePlayingBlock;
    public event IBoardView.CreateSingleBlockDelegate OnCreateSingleBlock;

    private void Awake()
    {
        boardParent = new GameObject("BoardParent");
        boardParent.transform.SetParent(transform);
        playingBlockParent = new GameObject("PlayingBlockParent");
        playingBlockParent.transform.SetParent(transform);
        wallsParent = new GameObject("WallsParent");
        wallsParent.transform.SetParent(transform);
    }
    /// <summary>
    /// 블록 생성 함수 > 중재자에서 호출, OnCreateBlock Invoke(생성된 보드 블록, 보드 블록 데이터)
    /// </summary>
    /// <param name="boardBlockData"> 보드 블록 데이터 </param>
    public void CreateBoardBlock(BoardBlockData boardBlockData)
    {
        GameObject blockObj = Instantiate(boardBlockPrefab, boardParent.transform);

        if (blockObj.TryGetComponent(out BoardBlockObject boardBlock))
        {
            OnCreateBlock?.Invoke(boardBlock, boardBlockData);
        }
        else
        {
            Debug.LogWarning("boardBlockPrefab에 BoardBlockObject 컴포넌트가 필요합니다!");
        }
    }
    /// <summary>
    /// 플레이 블록 생성 함수 > 중재자에서 호출, OnCreatePlayingBlock Invoke (생성된 블록 그룹, 플레이 블록 데이터)
    /// 플레이 블록 데이터의 shape수 만큼 OnCreateSingleBlock Invoke (생성된 싱글 블록, 플레이 블록 데이터, shape 데이터)
    /// </summary>
    /// <param name="pbData"> 플레이 블록 데이터 </param>
    public void CreatePlayingBlock(PlayingBlockData pbData)
    {
        var blockGroupObject = Instantiate(blockGroupPrefab, playingBlockParent.transform);
        var dragHandler = blockGroupObject.GetComponent<BlockDragHandler>();
        OnCreatePlayingBlock?.Invoke(dragHandler, pbData);
        foreach (var shape in pbData.shapes)
        {
            GameObject singleBlock = Instantiate(blockPrefab, blockGroupObject.transform);
            var renderer = singleBlock.GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer != null && pbData.colorType >= 0)
            {
                renderer.material = testBlockMaterials[(int)pbData.colorType];
            }

            if (singleBlock.TryGetComponent(out BlockObject blockObj))
            {
                OnCreateSingleBlock?.Invoke(blockObj, pbData, shape);
            }
        }
    }
    /// <summary>
    /// 벽 생성 함수 -> OnCreateWall Invoke (생성된 벽, 벽 데이터)
    /// </summary>
    /// <param name="wallData"></param>
    public void CreateWall(Project.Scripts.Data_Script.WallData wallData)
    {
        var canWallCreate = wallData.length - 1 >= 0 && wallData.length - 1 < wallPrefabs.Length;
        if (!canWallCreate)
        {
            return;
        }

        GameObject wallObj = Instantiate(wallPrefabs[wallData.length - 1], wallsParent.transform);
        WallObject wall = wallObj.GetComponent<WallObject>();
        wall.SetWall(wallMaterials[(int)wallData.wallColor], wallData.wallColor != ColorType.None);
        OnCreateWall?.Invoke(wallObj, wallData);
    }
    /// <summary>
    /// 파괴 효과 생성 함수
    /// </summary>
    /// <param name="block"> 파괴될 블록 </param>
    /// <param name="length"> 블록의 길이 </param>
    /// <param name="rotation"> 블록의 방향 </param>
    /// <param name="centerPos"> 시작 포지션 </param>
    /// <param name="endPos"> 끝 포지션 </param>
    public void CreateDestroyEffect(BlockObject block, int length, Quaternion rotation, Vector3 centerPos,
        Vector3 endPos)
    {
        ParticleSystem[] pss = destroyParticlePrefab
            .GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in pss)
        {
            ParticleSystemRenderer psrs = ps.GetComponent<ParticleSystemRenderer>();
            psrs.material = GetTargetMaterial((int)block.colorType);
        }

        ParticleSystem particle = Instantiate(destroyParticlePrefab,
            transform.position, rotation);
        particle.transform.position = centerPos;
        particle.transform.localScale = new Vector3(length * 0.4f, 0.5f, length * 0.4f);
        block.dragHandler.DestroyMove(endPos, particle);
    }
    /// <summary>
    /// 쿼드 생성 함수
    /// </summary>
    /// <param name="positions"> 마스킹 포지션 리스트 </param>
    /// <param name="blockDistance"> 블록 거리 </param>
    public void ShowMasking(List<Vector2> positions, float blockDistance)
    {
        foreach (var quad in quads)
        {
            Destroy(quad);
        }

        quads.Clear();

        foreach (var pos in positions)
        {
            GameObject quad = Instantiate(quadPrefab, quadTr);
            quad.transform.position = blockDistance * new Vector3(pos.x, yoffset, pos.y);
            quads.Add(quad);
        }
    }
    /// <summary>
    /// 벽의 머테리얼을 가져옴
    /// </summary>
    /// <param name="index"> 가져올 인덱스 </param>
    /// <returns> 저장되어 있는 벽 머테리얼[인덱스번호] </returns>
    private Material GetTargetMaterial(int index)
    {
        return wallMaterials[index];
    }
}