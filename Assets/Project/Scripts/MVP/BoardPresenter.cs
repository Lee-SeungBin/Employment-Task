using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 모델과 뷰의 통신을 담당하는 중재자
/// </summary>
public class BoardPresenter
{
    // 기존 BoardController를 MVP패턴으로 변경 BoardController -> BoardPresenter
    private readonly IBoardView view;
    private readonly BoardModel model;
    private readonly StageData[] stageDatas;

    public BoardPresenter(IBoardView view, BoardModel model, StageData[] stageDatas)
    {
        this.view = view;
        this.model = model;
        this.stageDatas = stageDatas;

        this.view.OnCreateBlock += this.model.SetBoardBlock;
        this.view.OnCreateWall += this.model.SetWall;
        this.view.OnCreatePlayingBlock += this.model.SetPlayBlock;
        this.view.OnCreateSingleBlock += this.model.SetPlaySingleBlock;
    }
    /// <summary>
    /// 중재자 초기화 함수
    /// </summary>
    /// <param name="stageIdx"> 스테이지 번호 </param>
    public async void Init(int stageIdx = 0)
    {
        model.Initialize(stageIdx); // 모델 초기화

        await CreateCustomWalls(stageIdx);
        
        await CreateBoardAsync(stageIdx);
        
        await CreatePlayingBlocksAsync(stageIdx);

        CreateMasking();
        foreach (var handler in GameObject.FindObjectsOfType<BlockDragHandler>())
        {
            handler.OnBlockDrop += HandleBlockDropped;
        }
    }
    /// <summary>
    /// 벽 생성 함수, wallData를 받아 view가 생성
    /// </summary>
    /// <param name="stageIdx"> 스테이지 번호 </param>
    private async Task CreateCustomWalls(int stageIdx)
    {
        if (stageIdx < 0 || stageIdx >= stageDatas.Length || stageDatas[stageIdx].Walls == null)
        {
            Debug.LogError($"유효하지 않은 스테이지 인덱스이거나 벽 데이터가 없습니다: {stageIdx}");
            return;
        }
        
        foreach (var wallData in stageDatas[stageIdx].Walls)
        {
            view.CreateWall(wallData);
        }
        
        await Task.Yield();
    }
    /// <summary>
    /// 보드 생성, BoardBlockData를 받아 view가 생성, model이 데이터 관리
    /// </summary>
    /// <param name="stageIdx"></param>
    private async Task CreateBoardAsync(int stageIdx = 0)
    {
        // 보드 블록 생성
        for (int i = 0; i < stageDatas[stageIdx].boardBlocks.Count; i++)
        {
            BoardBlockData data = stageDatas[stageIdx].boardBlocks[i];
            view.CreateBoardBlock(data);
        }
        
        model.SetStandardBlock();
        model.SetCheckBlockGroup();
        model.SetBoardSize();
        
        await Task.Yield();
    }
    /// <summary>
    /// 플레잉 보드 생성, PlayingBlockData를 받아 view가 생성
    /// </summary>
    /// <param name="stageIdx"> 스테이지 번호 </param>
    private async Task CreatePlayingBlocksAsync(int stageIdx = 0)
     {
         for (int i = 0; i < stageDatas[stageIdx].playingBlocks.Count; i++)
         {
             var pbData = stageDatas[stageIdx].playingBlocks[i];
             view.CreatePlayingBlock(pbData);
         }

         await Task.Yield();
     }
    /// <summary>
    /// 마스킹 생성 함수 -> model의 마스킹 위치를 받아 view가 생성
    /// </summary>
    private void CreateMasking()
    {
        var positions = model.GetMaskingPositions();
        view.ShowMasking(positions, model.BlockDistance);
    }
    
    public void GoToPreviousLevel()
    {
        // if (nowStageIndex == 0) return;
        //
        // Destroy(boardParent);
        // Destroy(playingBlockParent.gameObject);
        // Init(--nowStageIndex);
        //
        // StartCoroutine(Wait());
    }

    public void GotoNextLevel()
    {
        // if (nowStageIndex == stageDatas.Length - 1) return;
        //
        // Destroy(boardParent);
        // Destroy(playingBlockParent.gameObject);
        // Init(++nowStageIndex);
        //
        // StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return null;
        
        Vector3 camTr = Camera.main.transform.position;
        Camera.main.transform.position = new Vector3(1.5f + 0.5f * (model.BoardWidth - 4),camTr.y,camTr.z);
    } 
    /// <summary>
    /// 블록 드래그 핸들 함수 -> 드래그 후 호출, model에서 인접 블록 검사, 파괴 가능하면 view에서 파괴 이펙트 생성
    /// </summary>
    /// <param name="handler"> 블록 핸들러 (블록 그룹) </param>
    /// <param name="dropPos"> 드롭 위치 </param>
    private void HandleBlockDropped(BlockDragHandler handler, Vector3 dropPos)
    {
        Ray ray = new Ray(handler.transform.position, Vector3.down);
        var centerPos = Vector3.zero;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 coordinate = hit.transform.position;
            Vector3 targetPos = new Vector3(coordinate.x, handler.transform.position.y, coordinate.z);
            handler.transform.position = targetPos;
            
            centerPos.x = Mathf.Round(handler.transform.position.x / 0.79f);
            centerPos.y = Mathf.Round(handler.transform.position.z / 0.79f);
            
            foreach (var blockObject in handler.blocks)
            {
                blockObject.SetCoordinate(centerPos);
            }

            if (hit.collider.TryGetComponent(out BoardBlockObject boardBlockObject))
            {
                foreach (var blockObject in handler.blocks)
                {
                    model.CheckAdjacentBlock(boardBlockObject, blockObject, targetPos, out var info);
                    if (info != null)
                    {
                        view.CreateDestroyEffect(info.blockObject, info.blockLength, info.rotation, info.startPos, info.endPos);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Nothing Detected");
        }
    }
}