using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StageEditorWindow : EditorWindow
{
    private enum EditMode { Block, Wall }
    private EditMode currentMode = EditMode.Block;

    private Dictionary<Vector2Int, BlockData> blocks = new();
    private Dictionary<Vector2Int, WallData> walls = new();

    private Vector2 scrollPosition;
    private int gridSize = 50;
    private int selectedColor = 1;
    private string gimmick = "None";
    private ObjectPropertiesEnum.WallDirection wallDir = ObjectPropertiesEnum.WallDirection.Single_Up;
    private int wallLength = 1;

    [MenuItem("Tools/Stage Editor")]
    public static void OpenWindow()
    {
        GetWindow<StageEditorWindow>("Stage Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Edit Mode", EditorStyles.boldLabel);
        currentMode = (EditMode)EditorGUILayout.EnumPopup("Mode", currentMode);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(600));
        DrawGrid();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (currentMode == EditMode.Block)
        {
            selectedColor = EditorGUILayout.IntPopup("Color Type", selectedColor, 
                System.Enum.GetNames(typeof(ColorType)), 
                System.Enum.GetValues(typeof(ColorType)).Cast<int>().ToArray());

            gimmick = EditorGUILayout.TextField("Gimmick Type", gimmick);
        }
        else
        {
            wallDir = (ObjectPropertiesEnum.WallDirection)EditorGUILayout.EnumPopup("Wall Direction", wallDir);
            wallLength = EditorGUILayout.IntField("Wall Length", wallLength);
        }

        if (GUILayout.Button("Clear All"))
        {
            if (EditorUtility.DisplayDialog("확인", "모든 데이터를 초기화하시겠습니까?", "예", "아니오"))
            {
                blocks.Clear();
                walls.Clear();
            }
        }
    }

    private void DrawGrid()
    {
        Event e = Event.current;
        Handles.BeginGUI();
        int viewSize = 10;

        for (int x = -viewSize; x < viewSize; x++)
        {
            for (int y = -viewSize; y < viewSize; y++)
            {
                Rect rect = new Rect(300 + x * gridSize, 300 - y * gridSize, gridSize, gridSize);
                Handles.DrawSolidRectangleWithOutline(rect, new Color(1,1,1,0.03f), new Color(0.5f, 0.5f, 0.5f, 0.2f));

                Vector2Int pos = new Vector2Int(x, y);
                if (blocks.ContainsKey(pos))
                {
                    EditorGUI.DrawRect(rect, GetColorFromType(blocks[pos].colorType));
                    GUI.Label(rect, blocks[pos].gimmickType, EditorStyles.whiteLabel);
                }
                else if (walls.ContainsKey(pos))
                {
                    DrawWall(rect, walls[pos]);
                }

                if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
                {
                    HandleClick(pos);
                    e.Use();
                }
            }
        }

        Handles.EndGUI();
    }

    private void HandleClick(Vector2Int pos)
    {
        if (currentMode == EditMode.Block)
        {
            if (walls.ContainsKey(pos)) return;

            if (blocks.ContainsKey(pos))
                blocks.Remove(pos);
            else
                blocks[pos] = new BlockData
                {
                    position = pos,
                    colorType = (ColorType)selectedColor,
                    gimmickType = gimmick
                };
        }
        else
        {
            if (blocks.ContainsKey(pos)) return;

            if (walls.ContainsKey(pos))
                walls.Remove(pos);
            else
                walls[pos] = new WallData
                {
                    position = pos,
                    direction = wallDir,
                    length = wallLength
                };
        }
    }

    private void DrawWall(Rect rect, WallData wall)
    {
        Color col = new Color(0.2f, 0.6f, 1f, 0.4f);
        EditorGUI.DrawRect(rect, col);

        Vector2Int dir = GetWallOffset(wall.direction);
        for (int i = 1; i < wall.length; i++)
        {
            Rect extra = new Rect(
                rect.x + dir.x * gridSize * i,
                rect.y - dir.y * gridSize * i,
                gridSize,
                gridSize
            );
            EditorGUI.DrawRect(extra, col);
        }

        GUI.Label(rect, wall.direction.ToString(), EditorStyles.whiteMiniLabel);
    }

    private Vector2Int GetWallOffset(ObjectPropertiesEnum.WallDirection dir)
    {
        return dir switch
        {
            ObjectPropertiesEnum.WallDirection.Single_Up => new Vector2Int(0, 1),
            ObjectPropertiesEnum.WallDirection.Single_Down => new Vector2Int(0, -1),
            ObjectPropertiesEnum.WallDirection.Single_Left => new Vector2Int(-1, 0),
            ObjectPropertiesEnum.WallDirection.Single_Right => new Vector2Int(1, 0),
            ObjectPropertiesEnum.WallDirection.Left_Up => new Vector2Int(-1, 1),
            ObjectPropertiesEnum.WallDirection.Left_Down => new Vector2Int(-1, -1),
            ObjectPropertiesEnum.WallDirection.Right_Up => new Vector2Int(1, 1),
            ObjectPropertiesEnum.WallDirection.Right_Down => new Vector2Int(1, -1),
            ObjectPropertiesEnum.WallDirection.Open_Up => new Vector2Int(0, 1),
            ObjectPropertiesEnum.WallDirection.Open_Down => new Vector2Int(0, -1),
            ObjectPropertiesEnum.WallDirection.Open_Left => new Vector2Int(-1, 0),
            ObjectPropertiesEnum.WallDirection.Open_Right => new Vector2Int(1, 0),
            _ => Vector2Int.zero
        };
    }

    private Color GetColorFromType(ColorType type)
    {
        return type switch
        {
            ColorType.Red => Color.red,
            ColorType.Orange => new Color(1f, 0.5f, 0f),
            ColorType.Yellow => Color.yellow,
            ColorType.Gray => Color.gray,
            ColorType.Purple => new Color(0.5f, 0f, 0.5f),
            ColorType.Beige => new Color(1f, 0.9f, 0.7f),
            ColorType.Blue => Color.blue,
            ColorType.Green => Color.green,
            _ => Color.white
        };
    }

    // === Data Structures ===

    private class BlockData
    {
        public Vector2Int position;
        public ColorType colorType;
        public string gimmickType;
    }

    private class WallData
    {
        public Vector2Int position;
        public ObjectPropertiesEnum.WallDirection direction;
        public int length;
    }
}
