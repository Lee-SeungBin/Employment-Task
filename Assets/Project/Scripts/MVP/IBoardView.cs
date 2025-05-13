using System.Collections.Generic;
using UnityEngine;

public interface IBoardView
{
    public void CreateBoardBlock(BoardBlockData data);

    public void CreatePlayingBlock(PlayingBlockData data);

    public void CreateWall(Project.Scripts.Data_Script.WallData data);

    public void CreateDestroyEffect(BlockObject block, int length, Quaternion rotation, Vector3 centerPos, Vector3 endPos);
    
    public void ShowMasking(List<Vector2> positions, float blockDistance);
    
    public event CreateBoardBlockDelegate OnCreateBlock;
    
    public event CreateWallDelegate OnCreateWall;
    
    public event CreatePlayingBlockDelegate OnCreatePlayingBlock;
    
    public event CreateSingleBlockDelegate OnCreateSingleBlock;
    
    public delegate void CreateBoardBlockDelegate(BoardBlockObject boardBlockObject, BoardBlockData boardBlockData);
    
    public delegate void CreateWallDelegate(GameObject wall, Project.Scripts.Data_Script.WallData wallData);

    public delegate void CreatePlayingBlockDelegate(BlockDragHandler blockGroupObject, PlayingBlockData pbData);

    public delegate void CreateSingleBlockDelegate(BlockObject singleBlock, PlayingBlockData pbData, ShapeData shapeData);
}