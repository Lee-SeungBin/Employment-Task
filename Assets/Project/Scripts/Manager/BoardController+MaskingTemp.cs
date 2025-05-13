// using System.Collections.Generic;
// using UnityEngine;
//
// public partial class BoardController
// {
//     [SerializeField] private GameObject quadPrefab;
//     private float yoffset = 0.625f;
//     private float wallOffset = 0.225f;
//     private List<GameObject> quads = new List<GameObject>();
//
//     private void CreateMaskingTemp()
//     {
//         foreach (var quad in quads)
//         {
//             Destroy(quad);
//         }
//         quads.Clear();
//         
//         for (int i = -3; i <= boardModel.BoardWidth + 3; i++)
//         {
//             for (int j = -3; j <= boardModel.BoardHeight + 3; j++)
//             {
//                 if (boardModel.BoardBlockDic.ContainsKey((i, j))) continue;
//
//                 float xValue = i;
//                 float zValue = j;
//                 if (i == -1 && j <= boardModel.BoardHeight) xValue -= wallOffset;
//                 if (i == boardModel.BoardWidth + 1 && j <= boardModel.BoardHeight + 1) xValue += wallOffset;
//                 
//                 if (j == -1 && i <= boardModel.BoardWidth) zValue -= wallOffset;
//                 if (j == boardModel.BoardHeight + 1 && i <= boardModel.BoardWidth + 1) zValue += wallOffset;
//                 
//                 GameObject quad = GameObject.Instantiate(quadPrefab, quadTr);
//                 quads.Add(quad);
//                 
//                 quad.transform.position = boardModel.BlockDistance * new Vector3(xValue, yoffset, zValue);
//             }
//         }
//     }
// }