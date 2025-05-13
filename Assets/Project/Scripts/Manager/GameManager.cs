using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardView boardView;
    [SerializeField] private StageData[] stageDatas;
    
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        var model = new BoardModel();
        var presenter = new BoardPresenter(boardView, model, stageDatas);
        presenter.Init();
    }
}
