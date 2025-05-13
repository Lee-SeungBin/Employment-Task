// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using UnityEngine;
//
// public partial class BoardController : MonoBehaviour
// {
//     public static BoardController Instance;
//     public BoardModel boardModel { get; private set; }
//     [SerializeField] private BoardView boardView ;
//
//     [SerializeField] private StageData[] stageDatas;
//
//     [SerializeField] private GameObject boardBlockPrefab;
//     [SerializeField] private GameObject blockGroupPrefab; 
//     [SerializeField] private GameObject blockPrefab;
//     [SerializeField] private Material[] blockMaterials;
//     [SerializeField] private Material[] testBlockMaterials;
//     [SerializeField] private GameObject[] wallPrefabs;
//     [SerializeField] private Material[] wallMaterials;
//     [SerializeField] private Transform spawnerTr;
//     [SerializeField] private Transform quadTr;
//     [SerializeField] ParticleSystem destroyParticle;
//
//     public ParticleSystem destroyParticlePrefab => destroyParticle;
//     public List<SequentialCubeParticleSpawner> particleSpawners;
//     private GameObject boardParent;
//     private GameObject playingBlockParent;
//
//     private void Awake()
//     {
//         Instance = this;
//         Application.targetFrameRate = 60;
//     }
//
//     private void Start()
//     {
//         boardModel = new BoardModel();
//         Init();
//     }
//
//     private async void Init(int stageIdx = 0)
//     {
//         if (stageDatas == null)
//         {
//             Debug.LogError("StageData가 할당되지 않았습니다!");
//             return;
//         }
//
//         boardParent = new GameObject("BoardParent");
//         boardParent.transform.SetParent(transform);
//         boardModel.Initialize(stageIdx);
//         
//         await CreateCustomWalls(stageIdx);
//         
//         await CreateBoardAsync(stageIdx);
//         
//         await CreatePlayingBlocksAsync(stageIdx);
//
//         // CreateMaskingTemp();
//     }
//
//     private async Task CreateBoardAsync(int stageIdx = 0)
//     {
//         // 보드 블록 생성
//         for (int i = 0; i < stageDatas[stageIdx].boardBlocks.Count; i++)
//         {
//             BoardBlockData data = stageDatas[stageIdx].boardBlocks[i];
//
//             GameObject blockObj = Instantiate(boardBlockPrefab, boardParent.transform);
//
//             if (blockObj.TryGetComponent(out BoardBlockObject boardBlock))
//             {
//                 // boardBlock._ctrl = this;
//                 boardModel.SetBoardBlock(boardBlock, data);
//             }
//             else
//             {
//                 Debug.LogWarning("boardBlockPrefab에 BoardBlockObject 컴포넌트가 필요합니다!");
//             }
//         }
//         boardModel.SetStandardBlock();
//         boardModel.SetCheckBlockGroup();
//         
//         await Task.Yield();
//         
//         boardModel.SetBoardSize();
//     }
//     
//      private async Task CreatePlayingBlocksAsync(int stageIdx = 0)
//      {
//          playingBlockParent = new GameObject("PlayingBlockParent");
//          
//          for (int i = 0; i < stageDatas[stageIdx].playingBlocks.Count; i++)
//          {
//              var pbData = stageDatas[stageIdx].playingBlocks[i];
//             
//              GameObject blockGroupObject = Instantiate(blockGroupPrefab, playingBlockParent.transform);
//
//              blockGroupObject.transform.position = new Vector3(
//                  pbData.center.x * boardModel.BlockDistance, 
//                  0.33f, 
//                  pbData.center.y * boardModel.BlockDistance
//              );
//
//              BlockDragHandler dragHandler = blockGroupObject.GetComponent<BlockDragHandler>();
//              if (dragHandler != null) dragHandler.blocks = new List<BlockObject>();
//
//              dragHandler.uniqueIndex = pbData.uniqueIndex;
//              foreach (var gimmick in pbData.gimmicks)
//              {
//                  if (Enum.TryParse(gimmick.gimmickType, out ObjectPropertiesEnum.BlockGimmickType gimmickType))
//                  {
//                      dragHandler.gimmickType.Add(gimmickType);
//                  }
//              }
//              
//              int maxX = 0;
//              int minX = boardModel.BoardWidth;
//              int maxY = 0;
//              int minY = boardModel.BoardHeight;
//              foreach (var shape in pbData.shapes)
//              {
//                  GameObject singleBlock = Instantiate(blockPrefab, blockGroupObject.transform);
//                  singleBlock.transform.localPosition = new Vector3(
//                      shape.offset.x * boardModel.BlockDistance,
//                      0f,
//                      shape.offset.y * boardModel.BlockDistance
//                  );
//                  dragHandler.blockOffsets.Add(new Vector2(shape.offset.x, shape.offset.y));
//
//                  /*if (shape.colliderDirectionX > 0 && shape.colliderDirectionY > 0)
//                  {
//                      BoxCollider collider = dragHandler.AddComponent<BoxCollider>();
//                      dragHandler.col = collider;
//
//                      Vector3 localColCenter = singleBlock.transform.localPosition;
//                      int x = shape.colliderDirectionX;
//                      int y = shape.colliderDirectionY;
//                      
//                      collider.center = new Vector3
//                          (x > 1 ? localColCenter.x + blockDistance * (x - 1)/ 2 : 0
//                           ,0.2f, 
//                           y > 1 ? localColCenter.z + blockDistance * (y - 1)/ 2 : 0);
//                      collider.size = new Vector3(x * (blockDistance - 0.04f), 0.4f, y * (blockDistance - 0.04f));
//                  }*/
//                  var renderer = singleBlock.GetComponentInChildren<SkinnedMeshRenderer>();
//                  if (renderer != null && pbData.colorType >= 0)
//                  {
//                      renderer.material = testBlockMaterials[(int)pbData.colorType];
//                  }
//
//                  if (singleBlock.TryGetComponent(out BlockObject blockObj))
//                  {
//                      blockObj.colorType = pbData.colorType;
//                      blockObj.x = pbData.center.x + shape.offset.x;
//                      blockObj.y = pbData.center.y + shape.offset.y;
//                      blockObj.offsetToCenter = new Vector2(shape.offset.x, shape.offset.y);
//                      
//                      if (dragHandler != null)
//                          dragHandler.blocks.Add(blockObj);
//                      boardModel.BoardBlockDic[((int)blockObj.x, (int)blockObj.y)].playingBlock = blockObj;
//                      blockObj.preBoardBlockObject = boardModel.BoardBlockDic[((int)blockObj.x, (int)blockObj.y)];
//                      if(minX > blockObj.x) minX = (int)blockObj.x;
//                      if(minY > blockObj.y) minY = (int)blockObj.y;
//                      if(maxX < blockObj.x) maxX = (int)blockObj.x;
//                      if(maxY < blockObj.y) maxY = (int)blockObj.y;
//                  }
//              }
//
//              dragHandler.horizon = maxX - minX + 1;
//              dragHandler.vertical = maxY - minY + 1;
//          }
//
//          await Task.Yield();
//      }
//
//     public void GoToPreviousLevel()
//     {
//         // if (nowStageIndex == 0) return;
//         //
//         // Destroy(boardParent);
//         // Destroy(playingBlockParent.gameObject);
//         // Init(--nowStageIndex);
//         //
//         // StartCoroutine(Wait());
//     }
//
//     public void GotoNextLevel()
//     {
//         // if (nowStageIndex == stageDatas.Length - 1) return;
//         //
//         // Destroy(boardParent);
//         // Destroy(playingBlockParent.gameObject);
//         // Init(++nowStageIndex);
//         //
//         // StartCoroutine(Wait());
//     }
//
//     IEnumerator Wait()
//     {
//         yield return null;
//         
//         Vector3 camTr = Camera.main.transform.position;
//         Camera.main.transform.position = new Vector3(1.5f + 0.5f * (boardModel.BoardWidth - 4),camTr.y,camTr.z);
//     } 
//     private async Task CreateCustomWalls(int stageIdx)
//     {
//         if (stageIdx < 0 || stageIdx >= stageDatas.Length || stageDatas[stageIdx].Walls == null)
//         {
//             Debug.LogError($"유효하지 않은 스테이지 인덱스이거나 벽 데이터가 없습니다: {stageIdx}");
//             return;
//         }
//
//         GameObject wallsParent = new GameObject("CustomWallsParent");
//         
//         wallsParent.transform.SetParent(boardParent.transform);
//         
//         foreach (var wallData in stageDatas[stageIdx].Walls)
//         {
//             var canWallCreate = wallData.length - 1 >= 0 && wallData.length - 1 < wallPrefabs.Length;
//
//             if (!canWallCreate)
//             {
//                 return;
//             }
//             GameObject wallObj = Instantiate(wallPrefabs[wallData.length - 1], wallsParent.transform);
//             WallObject wall = wallObj.GetComponent<WallObject>();
//             wall.SetWall(wallMaterials[(int)wallData.wallColor], wallData.wallColor != ColorType.None);
//             boardModel.SetWall(wallObj, wallData);
//         }
//         
//         await Task.Yield();
//     }
// }