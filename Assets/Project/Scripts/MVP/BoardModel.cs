using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 보드의 생성된 블록, 벽 등을 관리하고 연산해주는 모델
/// </summary>
public class BoardModel
{
    public Dictionary<(int x, int y), BoardBlockObject> BoardBlockDic { get; private set; }
    public Dictionary<int, List<BoardBlockObject>> CheckBlockGroupDic { get; private set; }
    public Dictionary<(int, bool), BoardBlockObject> StandardBlockDic { get; private set; }

    public Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>> WallColorInfoDic
    {
        get;
        private set;
    }

    public int NowStageIndex { get; private set; }
    public int BoardWidth { get; private set; }
    public int BoardHeight { get; private set; }
    public float BlockDistance { get; } = 0.79f;

    private readonly List<GameObject> walls = new();
    int standardBlockIndex = -1;

    public void Initialize(int stageIdx)
    {
        NowStageIndex = stageIdx;
        BoardBlockDic = new Dictionary<(int x, int y), BoardBlockObject>();
        CheckBlockGroupDic = new Dictionary<int, List<BoardBlockObject>>();
        StandardBlockDic = new Dictionary<(int, bool), BoardBlockObject>();
        WallColorInfoDic = new Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>>();
    }
    /// <summary>
    /// Check 블록 그룹을 만드는 함수
    /// </summary>
    public void SetCheckBlockGroup()
    {
        int checkBlockIndex = -1;

        foreach (var blockPos in BoardBlockDic.Keys)
        {
            BoardBlockObject boardBlock = BoardBlockDic[blockPos];

            for (int j = 0; j < boardBlock.colorType.Count; j++)
            {
                if (boardBlock.isCheckBlock && boardBlock.colorType[j] != ColorType.None)
                {
                    // 이 블록이 이미 그룹에 속해있는지 확인
                    if (boardBlock.checkGroupIdx.Count <= j)
                    {
                        if (boardBlock.isHorizon[j])
                        {
                            // 왼쪽 블록 확인
                            (int x, int y) leftPos = (boardBlock.x - 1, boardBlock.y);
                            if (BoardBlockDic.TryGetValue(leftPos, out BoardBlockObject leftBlock) &&
                                j < leftBlock.colorType.Count &&
                                leftBlock.colorType[j] == boardBlock.colorType[j] &&
                                leftBlock.checkGroupIdx.Count > j)
                            {
                                int grpIdx = leftBlock.checkGroupIdx[j];
                                CheckBlockGroupDic[grpIdx].Add(boardBlock);
                                boardBlock.checkGroupIdx.Add(grpIdx);
                            }
                            else
                            {
                                checkBlockIndex++;
                                CheckBlockGroupDic.Add(checkBlockIndex, new List<BoardBlockObject>());
                                CheckBlockGroupDic[checkBlockIndex].Add(boardBlock);
                                boardBlock.checkGroupIdx.Add(checkBlockIndex);
                            }
                        }
                        else
                        {
                            // 위쪽 블록 확인
                            (int x, int y) upPos = (boardBlock.x, boardBlock.y - 1);
                            if (BoardBlockDic.TryGetValue(upPos, out BoardBlockObject upBlock) &&
                                j < upBlock.colorType.Count &&
                                upBlock.colorType[j] == boardBlock.colorType[j] &&
                                upBlock.checkGroupIdx.Count > j)
                            {
                                int grpIdx = upBlock.checkGroupIdx[j];
                                CheckBlockGroupDic[grpIdx].Add(boardBlock);
                                boardBlock.checkGroupIdx.Add(grpIdx);
                            }
                            else
                            {
                                checkBlockIndex++;
                                CheckBlockGroupDic.Add(checkBlockIndex, new List<BoardBlockObject>());
                                CheckBlockGroupDic[checkBlockIndex].Add(boardBlock);
                                boardBlock.checkGroupIdx.Add(checkBlockIndex);
                            }
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// 보드의 크기를 결정하는 함수 (Width, Height)
    /// </summary>
    public void SetBoardSize()
    {
        BoardWidth = BoardBlockDic.Keys.Max(k => k.x);
        BoardHeight = BoardBlockDic.Keys.Max(k => k.y);
    }
    /// <summary>
    /// 보드 블록 생성후 관련 데이터를 처리하는 함수
    /// </summary>
    /// <param name="blockObj"> 생성된 보드 블록 </param>
    /// <param name="boardBlockData"> 보드 블록 데이터 </param>
    public void SetBoardBlock(BoardBlockObject blockObj, BoardBlockData boardBlockData)
    {
        blockObj.transform.localPosition = new Vector3(
            boardBlockData.x * BlockDistance,
            0,
            boardBlockData.y * BlockDistance
        );

        blockObj.x = boardBlockData.x;
        blockObj.y = boardBlockData.y;

        if (WallColorInfoDic.ContainsKey((blockObj.x, blockObj.y)))
        {
            for (int k = 0; k < WallColorInfoDic[(blockObj.x, blockObj.y)].Count; k++)
            {
                blockObj.colorType.Add(WallColorInfoDic[(blockObj.x, blockObj.y)].Keys.ElementAt(k).Item2);
                blockObj.len.Add(WallColorInfoDic[(blockObj.x, blockObj.y)].Values.ElementAt(k));

                DestroyWallDirection dir = WallColorInfoDic[(blockObj.x, blockObj.y)].Keys.ElementAt(k).Item1;
                bool horizon = dir == DestroyWallDirection.Up || dir == DestroyWallDirection.Down;
                blockObj.isHorizon.Add(horizon);

                StandardBlockDic.Add((++standardBlockIndex, horizon), blockObj);
            }

            blockObj.isCheckBlock = true;
        }
        else
        {
            blockObj.isCheckBlock = false;
        }

        BoardBlockDic.Add((blockObj.x, blockObj.y), blockObj);
    }
    /// <summary>
    /// 스탠다드 블록을 관련 처리하는 함수
    /// </summary>
    public void SetStandardBlock()
    {
        foreach (var kv in StandardBlockDic)
        {
            BoardBlockObject boardBlockObject = kv.Value;
            for (int i = 0; i < boardBlockObject.colorType.Count; i++)
            {
                if (kv.Key.Item2) // 가로 방향
                {
                    for (int j = boardBlockObject.x + 1; j < boardBlockObject.x + boardBlockObject.len[i]; j++)
                    {
                        if (BoardBlockDic.TryGetValue((j, boardBlockObject.y), out BoardBlockObject targetBlock))
                        {
                            targetBlock.colorType.Add(boardBlockObject.colorType[i]);
                            targetBlock.len.Add(boardBlockObject.len[i]);
                            targetBlock.isHorizon.Add(kv.Key.Item2);
                            targetBlock.isCheckBlock = true;
                        }
                    }
                }
                else // 세로 방향
                {
                    for (int k = boardBlockObject.y + 1; k < boardBlockObject.y + boardBlockObject.len[i]; k++)
                    {
                        if (BoardBlockDic.TryGetValue((boardBlockObject.x, k), out BoardBlockObject targetBlock))
                        {
                            targetBlock.colorType.Add(boardBlockObject.colorType[i]);
                            targetBlock.len.Add(boardBlockObject.len[i]);
                            targetBlock.isHorizon.Add(kv.Key.Item2);
                            targetBlock.isCheckBlock = true;
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// 생성된 플레이 블록의 데이터를 처리하는 함수
    /// </summary>
    /// <param name="blockGroupObject"> 생성된 플레이 블록 (블록 그룹) </param>
    /// <param name="pbData"> 플레이 블록 데이터 </param>
    public void SetPlayBlock(BlockDragHandler blockGroupObject, PlayingBlockData pbData)
    {
        if (blockGroupObject == null)
        {
            return;
        }
        
        blockGroupObject.transform.position = new Vector3(
            pbData.center.x * BlockDistance,
            0.33f,
            pbData.center.y * BlockDistance
        );
    
        blockGroupObject.blocks = new List<BlockObject>();
        blockGroupObject.uniqueIndex = pbData.uniqueIndex;
        
        foreach (var gimmick in pbData.gimmicks)
        {
            if (System.Enum.TryParse(gimmick.gimmickType, out ObjectPropertiesEnum.BlockGimmickType gimmickType))
            {
                blockGroupObject.gimmickType.Add(gimmickType);
            }
        }
    }
    /// <summary>
    /// 생성된 싱글 플레이 블록의 데이터를 처리하는 함수
    /// </summary>
    /// <param name="singleBlock"> 생성된 싱글 플레이 블록 </param>
    /// <param name="pbData"> 플레이어 블록 데이터 </param>
    /// <param name="shapeData"> Shape 데이터 </param>
    public void SetPlaySingleBlock(BlockObject singleBlock, PlayingBlockData pbData, ShapeData shapeData)
    {
        int maxX = 0;
        int minX = BoardWidth;
        int maxY = 0;
        int minY = BoardHeight;
        singleBlock.transform.localPosition = new Vector3(
            shapeData.offset.x * BlockDistance,
            0f,
            shapeData.offset.y * BlockDistance
        );
        singleBlock.dragHandler.blockOffsets.Add(new Vector2(shapeData.offset.x, shapeData.offset.y));

        singleBlock.colorType = pbData.colorType;
        singleBlock.x = pbData.center.x + shapeData.offset.x;
        singleBlock.y = pbData.center.y + shapeData.offset.y;
        singleBlock.offsetToCenter = new Vector2(shapeData.offset.x, shapeData.offset.y);

        if (singleBlock.dragHandler != null)
        {
            singleBlock.dragHandler.blocks.Add(singleBlock);
        }

        BoardBlockDic[((int)singleBlock.x, (int)singleBlock.y)].playingBlock = singleBlock;
        singleBlock.preBoardBlockObject = BoardBlockDic[((int)singleBlock.x, (int)singleBlock.y)];
        if (minX > singleBlock.x) minX = (int)singleBlock.x;
        if (minY > singleBlock.y) minY = (int)singleBlock.y;
        if (maxX < singleBlock.x) maxX = (int)singleBlock.x;
        if (maxY < singleBlock.y) maxY = (int)singleBlock.y;

        singleBlock.dragHandler.horizon = maxX - minX + 1;
        singleBlock.dragHandler.vertical = maxY - minY + 1;
    }
    /// <summary>
    /// 생성된 벽의 데이터를 처리하는 함수
    /// </summary>
    /// <param name="wallObj"> 생성된 벽 </param>
    /// <param name="wallData"> 벽의 데이터 </param>
    public void SetWall(GameObject wallObj, Project.Scripts.Data_Script.WallData wallData)
    {
        Quaternion rotation;

        // 기본 위치 계산
        var position = new Vector3(
            wallData.x * BlockDistance,
            0f,
            wallData.y * BlockDistance);

        DestroyWallDirection destroyDirection = DestroyWallDirection.None;
        bool shouldAddWallInfo = false;

        // 벽 방향과 유형에 따라 위치와 회전 조정
        switch (wallData.WallDirection)
        {
            case ObjectPropertiesEnum.WallDirection.Single_Up:
                position.z += 0.5f;
                rotation = Quaternion.Euler(0f, 180f, 0f);
                shouldAddWallInfo = true;
                destroyDirection = DestroyWallDirection.Up;
                break;

            case ObjectPropertiesEnum.WallDirection.Single_Down:
                position.z -= 0.5f;
                rotation = Quaternion.identity;
                shouldAddWallInfo = true;
                destroyDirection = DestroyWallDirection.Down;
                break;

            case ObjectPropertiesEnum.WallDirection.Single_Left:
                position.x -= 0.5f;
                rotation = Quaternion.Euler(0f, 90f, 0f);
                shouldAddWallInfo = true;
                destroyDirection = DestroyWallDirection.Left;
                break;

            case ObjectPropertiesEnum.WallDirection.Single_Right:
                position.x += 0.5f;
                rotation = Quaternion.Euler(0f, -90f, 0f);
                shouldAddWallInfo = true;
                destroyDirection = DestroyWallDirection.Right;
                break;

            case ObjectPropertiesEnum.WallDirection.Left_Up:
                // 왼쪽 위 모서리
                position.x -= 0.5f;
                position.z += 0.5f;
                rotation = Quaternion.Euler(0f, 180f, 0f);
                break;

            case ObjectPropertiesEnum.WallDirection.Left_Down:
                // 왼쪽 아래 모서리
                position.x -= 0.5f;
                position.z -= 0.5f;
                rotation = Quaternion.identity;
                break;

            case ObjectPropertiesEnum.WallDirection.Right_Up:
                // 오른쪽 위 모서리
                position.x += 0.5f;
                position.z += 0.5f;
                rotation = Quaternion.Euler(0f, 270f, 0f);
                break;

            case ObjectPropertiesEnum.WallDirection.Right_Down:
                // 오른쪽 아래 모서리
                position.x += 0.5f;
                position.z -= 0.5f;
                rotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            case ObjectPropertiesEnum.WallDirection.Open_Up:
                // 위쪽이 열린 벽
                position.z += 0.5f;
                rotation = Quaternion.Euler(0f, 180f, 0f);
                break;

            case ObjectPropertiesEnum.WallDirection.Open_Down:
                // 아래쪽이 열린 벽
                position.z -= 0.5f;
                rotation = Quaternion.identity;
                break;

            case ObjectPropertiesEnum.WallDirection.Open_Left:
                // 왼쪽이 열린 벽
                position.x -= 0.5f;
                rotation = Quaternion.Euler(0f, 90f, 0f);
                break;

            case ObjectPropertiesEnum.WallDirection.Open_Right:
                // 오른쪽이 열린 벽
                position.x += 0.5f;
                rotation = Quaternion.Euler(0f, -90f, 0f);
                break;

            default:
                Debug.LogError($"지원되지 않는 벽 방향: {wallData.WallDirection}");
                return;
        }

        if (shouldAddWallInfo && wallData.wallColor != ColorType.None)
        {
            var pos = (wallData.x, wallData.y);
            var wallInfo = (destroyDirection, wallData.wallColor);

            if (!WallColorInfoDic.ContainsKey(pos))
            {
                Dictionary<(DestroyWallDirection, ColorType), int> wallInfoDic =
                    new Dictionary<(DestroyWallDirection, ColorType), int> { { wallInfo, wallData.length } };
                WallColorInfoDic.Add(pos, wallInfoDic);
            }
            else
            {
                WallColorInfoDic[pos].Add(wallInfo, wallData.length);
            }
        }

        // 길이에 따른 위치 조정 (수평/수직 벽만 조정)
        if (wallData.length > 1)
        {
            // 수평 벽의 중앙 위치 조정 (Up, Down 방향)
            if (wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Up ||
                wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Down ||
                wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Up ||
                wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Down)
            {
                // x축으로 중앙으로 이동
                position.x += (wallData.length - 1) * BlockDistance * 0.5f;
            }
            // 수직 벽의 중앙 위치 조정 (Left, Right 방향)
            else if (wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Left ||
                     wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Right ||
                     wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Left ||
                     wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Right)
            {
                // z축으로 중앙으로 이동
                position.z += (wallData.length - 1) * BlockDistance * 0.5f;
            }
        }

        wallObj.transform.position = position;
        wallObj.transform.rotation = rotation;
        walls.Add(wallObj);
    }
    /// <summary>
    /// 마스킹 포지션들을 가져오는 함수
    /// </summary>
    /// <returns> 가져온 마스킹 포지션 리스트 </returns>
    public List<Vector2> GetMaskingPositions()
    {
        List<Vector2> positions = new();

        for (int i = -3; i <= BoardWidth + 3; i++)
        {
            for (int j = -3; j <= BoardHeight + 3; j++)
            {
                if (BoardBlockDic.ContainsKey((i, j))) continue;

                float xValue = i;
                float zValue = j;
                if (i == -1 && j <= BoardHeight) xValue -= 0.225f;
                if (i == BoardWidth + 1 && j <= BoardHeight + 1) xValue += 0.225f;
                if (j == -1 && i <= BoardWidth) zValue -= 0.225f;
                if (j == BoardHeight + 1 && i <= BoardWidth + 1) zValue += 0.225f;

                positions.Add(new Vector2(xValue, zValue));
            }
        }

        return positions;
    }
    /// <summary>
    /// 파괴 검사 함수
    /// </summary>
    /// <param name="boardBlock"> 대상 보드 블록 </param>
    /// <param name="block"> 대상 블록 </param>
    /// <returns> 파괴 가능한지 여부에 따라 true false 반환 </returns>
    private bool CheckCanDestroy(BoardBlockObject boardBlock, BlockObject block)
    {
        foreach (var checkGroupIdx in boardBlock.checkGroupIdx)
        {
            if (!boardBlock.isCheckBlock && !CheckBlockGroupDic.ContainsKey(checkGroupIdx)) return false;
        }

        //List<Vector2> checkCoordinates = new List<Vector2>();

        int pBlockminX = BoardWidth;
        int pBlockmaxX = -1;
        int pBlockminY = BoardHeight;
        int pBlockmaxY = -1;

        List<BlockObject> blocks = block.dragHandler.blocks;

        foreach (var playingBlock in blocks)
        {
            if (playingBlock.x <= pBlockminX) pBlockminX = (int)playingBlock.x;
            if (playingBlock.y <= pBlockminY) pBlockminY = (int)playingBlock.y;
            if (playingBlock.x >= pBlockmaxX) pBlockmaxX = (int)playingBlock.x;
            if (playingBlock.y >= pBlockmaxY) pBlockmaxY = (int)playingBlock.y;
        }

        List<BoardBlockObject> horizonBoardBlocks = new List<BoardBlockObject>();
        List<BoardBlockObject> verticalBoardBlocks = new List<BoardBlockObject>();

        foreach (var checkIndex in boardBlock.checkGroupIdx)
        {
            foreach (var boardBlockObj in CheckBlockGroupDic[checkIndex])
            {
                foreach (var horizon in boardBlockObj.isHorizon)
                {
                    if (horizon) horizonBoardBlocks.Add(boardBlockObj);
                    else verticalBoardBlocks.Add(boardBlockObj);
                }
                //checkCoordinates.Add(new Vector2(boardBlockObj.x, boardBlockObj.y));
            }
        }

        int matchingIndex = boardBlock.colorType.FindIndex(color => color == block.colorType);
        bool hor = boardBlock.isHorizon[matchingIndex];
        //Horizon
        if (hor)
        {
            int minX = BoardWidth;
            int maxX = -1;
            foreach (var coordinate in horizonBoardBlocks)
            {
                if (coordinate.x < minX) minX = (int)coordinate.x;

                if (coordinate.x > maxX) maxX = (int)coordinate.x;
            }

            // 개별 좌표가 나갔는지 여부를 판단.
            if (pBlockminX < minX - BlockDistance / 2 || pBlockmaxX > maxX + BlockDistance / 2)
            {
                return false;
            }

            (int, int)[] blockCheckCoors = new (int, int)[horizonBoardBlocks.Count];

            for (int i = 0; i < horizonBoardBlocks.Count; i++)
            {
                if (horizonBoardBlocks[i].y <= BoardHeight / 2)
                {
                    int maxY = -1;

                    for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                    {
                        var currentBlock = block.dragHandler.blocks[k];

                        if (currentBlock.y == horizonBoardBlocks[i].y)
                        {
                            if (currentBlock.y > maxY)
                            {
                                maxY = (int)currentBlock.y;
                            }
                        }
                    }

                    blockCheckCoors[i] = ((int)horizonBoardBlocks[i].x, maxY);

                    for (int l = blockCheckCoors[i].Item2; l <= horizonBoardBlocks[i].y; l++)
                    {
                        if (blockCheckCoors[i].Item1 < pBlockminX || blockCheckCoors[i].Item1 > pBlockmaxX)
                            continue;

                        (int, int) key = (blockCheckCoors[i].Item1, l);

                        if (BoardBlockDic.ContainsKey(key) &&
                            BoardBlockDic[key].playingBlock != null &&
                            BoardBlockDic[key].playingBlock.colorType != boardBlock.HorizonColorType)
                        {
                            return false;
                        }
                    }
                }
                // up to downside
                else
                {
                    int minY = 100;

                    for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                    {
                        var currentBlock = block.dragHandler.blocks[k];

                        if (currentBlock.y == horizonBoardBlocks[i].y)
                        {
                            if (currentBlock.y < minY)
                            {
                                minY = (int)currentBlock.y;
                            }
                        }
                    }

                    blockCheckCoors[i] = ((int)horizonBoardBlocks[i].x, minY);

                    for (int l = blockCheckCoors[i].Item2; l >= horizonBoardBlocks[i].y; l--)
                    {
                        if (blockCheckCoors[i].Item1 < pBlockminX || blockCheckCoors[i].Item1 > pBlockmaxX)
                            continue;
                        (int, int) key = (blockCheckCoors[i].Item1, l);

                        if (BoardBlockDic.ContainsKey(key) &&
                            BoardBlockDic[key].playingBlock != null &&
                            BoardBlockDic[key].playingBlock.colorType != boardBlock.HorizonColorType)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        // Vertical
        else
        {
            int minY = BoardHeight;
            int maxY = -1;

            foreach (var coordinate in verticalBoardBlocks)
            {
                if (coordinate.y < minY) minY = (int)coordinate.y;
                if (coordinate.y > maxY) maxY = (int)coordinate.y;
            }

            if (pBlockminY < minY - BlockDistance / 2 || pBlockmaxY > maxY + BlockDistance / 2)
            {
                return false;
            }

            (int, int)[] blockCheckCoors = new (int, int)[verticalBoardBlocks.Count];

            for (int i = 0; i < verticalBoardBlocks.Count; i++)
            {
                //x exist in left
                if (verticalBoardBlocks[i].x <= BoardWidth / 2)
                {
                    int maxX = int.MinValue;

                    for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                    {
                        var currentBlock = block.dragHandler.blocks[k];

                        if (currentBlock.y == verticalBoardBlocks[i].y)
                        {
                            if (currentBlock.x > maxX)
                            {
                                maxX = (int)currentBlock.x;
                            }
                        }
                    }

                    // 튜플에 y와 maxX를 저장
                    blockCheckCoors[i] = (maxX, (int)verticalBoardBlocks[i].y);

                    for (int l = blockCheckCoors[i].Item1; l >= verticalBoardBlocks[i].x; l--)
                    {
                        if (blockCheckCoors[i].Item2 < pBlockminY || blockCheckCoors[i].Item2 > pBlockmaxY)
                            continue;
                        (int, int) key = (l, blockCheckCoors[i].Item2);

                        if (BoardBlockDic.ContainsKey(key) &&
                            BoardBlockDic[key].playingBlock != null &&
                            BoardBlockDic[key].playingBlock.colorType != boardBlock.VerticalColorType)
                        {
                            return false;
                        }
                    }
                }
                //x exist in right
                else
                {
                    int minX = 100;

                    for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                    {
                        var currentBlock = block.dragHandler.blocks[k];

                        if (currentBlock.y == verticalBoardBlocks[i].y)
                        {
                            if (currentBlock.x < minX)
                            {
                                minX = (int)currentBlock.x;
                            }
                        }
                    }

                    // 튜플에 y와 minX를 저장
                    blockCheckCoors[i] = (minX, (int)verticalBoardBlocks[i].y);

                    for (int l = blockCheckCoors[i].Item1; l <= verticalBoardBlocks[i].x; l++)
                    {
                        if (blockCheckCoors[i].Item2 < pBlockminY || blockCheckCoors[i].Item2 > pBlockmaxY)
                            continue;
                        (int, int) key = (l, blockCheckCoors[i].Item2);

                        if (BoardBlockDic.ContainsKey(key) &&
                            BoardBlockDic[key].playingBlock != null &&
                            BoardBlockDic[key].playingBlock.colorType != boardBlock.VerticalColorType)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }
    /// <summary>
    /// 인접 블록 체크 함수
    /// </summary>
    /// <param name="boardBlock"> 대상 보드 블록 </param>
    /// <param name="block"> 대상 블록 </param>
    /// <param name="destroyStartPos"> 파괴 시작 지점 </param>
    /// <param name="destoryInfo"> 파괴 이펙트 정보 </param>
    public void CheckAdjacentBlock(BoardBlockObject boardBlock, BlockObject block, Vector3 destroyStartPos, out DestoryEffectData destoryInfo)
    {
        destoryInfo = null;
        if (!boardBlock.isCheckBlock) return;
        if (!block.dragHandler.enabled) return;
        for (int i = 0; i < boardBlock.colorType.Count; i++)
        {
            if (block.colorType == boardBlock.colorType[i])
            {
                int length = 0;
                if (boardBlock.isHorizon[i])
                {
                    if (block.dragHandler.horizon > boardBlock.len[i]) return;
                    if (!CheckCanDestroy(boardBlock, block)) return;
                    length = block.dragHandler.vertical;
                }
                else
                {
                    if (block.dragHandler.vertical > boardBlock.len[i]) return;
                    if (!CheckCanDestroy(boardBlock, block)) return;
                    length = block.dragHandler.horizon;
                }

                block.dragHandler.transform.position = destroyStartPos;
                // block.dragHandler.ReleaseInput();

                foreach (var blockObject in block.dragHandler.blocks)
                {
                    blockObject.ColliderOff();
                }

                block.dragHandler.enabled = false;

                bool isRight = boardBlock.isHorizon[i]
                    ? boardBlock.y < BoardHeight / 2
                    : boardBlock.x < BoardWidth / 2;
                if (!isRight) length *= -1;
                Vector3 endPos = boardBlock.isHorizon[i]
                    ? new Vector3(block.dragHandler.transform.position.x, block.dragHandler.transform.position.y,
                        block.dragHandler.transform.position.z - length * 0.79f)
                    : new Vector3(block.dragHandler.transform.position.x - length * 0.79f,
                        block.dragHandler.transform.position.y, block.dragHandler.transform.position.z);


                Vector3 startPos =
                    boardBlock.isHorizon[i]
                        ? block.dragHandler.GetCenterX()
                        : block.dragHandler.GetCenterZ(); //_ctrl.CenterOfBoardBlockGroup(len, isHorizon, this);
                LaunchDirection direction = GetLaunchDirection(boardBlock.x, boardBlock.y, boardBlock.isHorizon[i]);
                Quaternion rotation = Quaternion.identity;

                startPos.y = 0.55f;
                switch (direction)
                {
                    case LaunchDirection.Up:
                        startPos += Vector3.forward * 0.65f;
                        startPos.z = boardBlock.transform.position.z;
                        startPos.z += 0.55f;
                        rotation = Quaternion.Euler(0, 180, 0);
                        break;
                    case LaunchDirection.Down:
                        startPos += Vector3.back * 0.65f;
                        break;
                    case LaunchDirection.Left:
                        startPos += Vector3.left * 0.55f;
                        //offset.z = centerPos.transform.position.z;
                        rotation = Quaternion.Euler(0, 90, 0);
                        break;
                    case LaunchDirection.Right:
                        startPos += Vector3.right * 0.55f;
                        startPos.x = boardBlock.transform.position.x;
                        startPos.x += 0.65f;
                        rotation = Quaternion.Euler(0, -90, 0);
                        //offset.z = centerPos.transform.position.z;
                        break;
                }

                int blockLength = boardBlock.isHorizon[i] ? block.dragHandler.horizon : block.dragHandler.vertical;
                
                destoryInfo = new DestoryEffectData
                {
                    blockObject = block,
                    rotation = rotation,
                    blockLength = blockLength,
                    startPos = startPos,
                    endPos = endPos
                };
            }
        }
    }
    /// <summary>
    /// 파괴 방향을 가져오는 함수
    /// </summary>
    /// <param name="x"> 좌표 x값 </param>
    /// <param name="y"> 좌표 y값 </param>
    /// <param name="isHorizon"> Horizon 상태인지 </param>
    /// <returns> 파괴 방향 </returns>
    private LaunchDirection GetLaunchDirection(int x, int y, bool isHorizon)
    {
        // 모서리 케이스들
        if (x == 0 && y == 0)
            return isHorizon ? LaunchDirection.Down : LaunchDirection.Left;
        
        if (x == 0 && y == BoardHeight)
            return isHorizon ? LaunchDirection.Up : LaunchDirection.Left;
        
        if (x == BoardWidth && y == 0)
            return isHorizon ? LaunchDirection.Down : LaunchDirection.Right;
        
        if (x == BoardWidth && y == BoardHeight)
            return isHorizon ? LaunchDirection.Up : LaunchDirection.Right;
        
        // 기본 경계 케이스들
        if (x == 0)
            return isHorizon ? LaunchDirection.Down : LaunchDirection.Left;
        
        if (y == 0)
            return isHorizon ? LaunchDirection.Down : LaunchDirection.Left;
        
        if (x == BoardWidth)
            return isHorizon ? LaunchDirection.Down : LaunchDirection.Right;
        
        if (y == BoardHeight)
            return isHorizon ? LaunchDirection.Up : LaunchDirection.Right;
    
        // 기본값 (필요하다면)
        return LaunchDirection.Up;
    }

}
