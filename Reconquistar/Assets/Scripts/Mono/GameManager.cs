using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

public class CardInfo
{
    public int KingdomType;
    public int CardType;
    // public List<Buff> BuffList;
    public Color CardColor;
    private bool deleteCandidate;
    public bool DeleteCandidate
    {
        get { return deleteCandidate; }
        set { deleteCandidate = value; }
    }

    private bool showLine;

    public bool ShowLine
    {
        get { return showLine; }
        set { showLine = value; }
    }

    public CardInfo(int kingdomType, int cardType)
    {
        KingdomType = kingdomType;
        CardType = cardType;
        CardColor = Color.white;
    }
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject SelectionPanel;
    [SerializeField] private TextMeshProUGUI EventTimer;
    private Button[] SelectionPanelButtons;

    private int playerNum = 4;
    public static int currentPlayerNum;
    private int currentTurn;
    public static bool isRolled; // 주사위 굴렸는지
    public static int isMoving; // 0: 이동 전 / 1: 이동 중 / 2: 이동 후
    private bool isComplete; // 턴 종료 가능한지
    public static bool isSelected; // 모집할 때, 버릴 카드 선택했는지

    public static Player[] players;
    public static Player currentPlayer => players[currentPlayerNum];
    private Dictionary<int, int> tilePerPlayer; // player 소유한 타일 수
    private List<int> playerTurnOrder = new List<int>();

    // 1번풀 : 6~9 (공용)
    // 2번풀 : 전문군인 3~5 (공용)
    // 3번풀 : 귀족 카드 J, Q, K (고유)
    public static List<List<CardInfo>> cardPool1 = new List<List<CardInfo>>();
    public static List<List<CardInfo>> cardPool2 = new List<List<CardInfo>>();
    public static List<List<CardInfo>> cardPool3 = new List<List<CardInfo>>();

    public static int eventType = 0;
    public static int roundUntilEventOccur = 0;

    public static GameManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        InitializeCardPool();
        SelectionPanelButtons = SelectionPanel.GetComponentsInChildren<Button>();
    }

    private void Start()
    {
        InitializePlayers();
        isRolled = false;
        isMoving = 0;
        isComplete = false;
        isSelected = false;

        SortPlayerTurnOrder();
    }

    // player 생성, 소유 타일 수 지정
    private void InitializePlayers()
    {
        players = new Player[playerNum];
        tilePerPlayer = new Dictionary<int, int>();

        for (int i = 0; i < playerNum; i++)
        {
            GameObject playerObject = Instantiate(PlayerPrefab);
            Player player = playerObject.GetComponent<Player>();
            player.Initialize(kingdomType: i, cellIndex: i * GameBoard.tilePerLine);
            players[i] = player;
            tilePerPlayer.Add(i, GameBoard.tilePerLine);
        }
    }

    private void InitializeCardPool()
    {
        for (int i = 0; i < 4; i++)
        {
            List<CardInfo> c = new List<CardInfo>();
            for (int j = 6; j < 10; j++)
            {
                c.Add(new CardInfo(i, j));
            }
            cardPool1.Add(c);
        }

        for (int i = 0; i < 4; i++)
        {
            List<CardInfo> c = new List<CardInfo>();
            for (int j = 3; j < 6; j++)
            {
                c.Add(new CardInfo(i, j));
            }
            cardPool2.Add(c);
        }

        for (int i = 0; i < 4; i++)
        {
            List<CardInfo> c = new List<CardInfo>();
            for (int j = 0; j < 3; j++)
            {
                c.Add(new CardInfo(i, j));
            }
            cardPool3.Add(c);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isRolled)
        {
            StartCoroutine(Dice.Instance.RollDice());
        }
    }

    // 라운드 시작 전 플레이어 순서 정렬 + 이벤트 남은 라운드 조정
    private void SortPlayerTurnOrder()
    {
        tilePerPlayer = tilePerPlayer.OrderBy(item => item.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

        playerTurnOrder.Clear();
        foreach (KeyValuePair<int, int> player in tilePerPlayer)
        {
            playerTurnOrder.Add(player.Key);
        }
        Debug.Log("라운드 순서: " + string.Join(", ", playerTurnOrder));

        currentPlayerNum = playerTurnOrder[0];
        players[currentPlayerNum].SetArrow(true);
        currentTurn = 0;

        if (EventTimer.text == "이벤트 발동")
        {
            NewEvent();
        }
        else
        {
            roundUntilEventOccur--;
            if (roundUntilEventOccur <= 0) EventTimer.text = "이벤트 발동";
            else EventTimer.text = "이벤트 " + eventType + " 발동까지 " + roundUntilEventOccur + " 라운드 남음";
        }
    }

    // 이동 버튼 클릭
    public void ArrowBtnClick(bool clockwise)
    {
        if (!isRolled || isMoving >= 1)
        {
            Debug.Log("이미 이동했거나 주사위를 굴리지 않았습니다.");
            return;
        }

        isMoving = 1;
        StartCoroutine(PlayerTurn(clockwise));
    }

    // 이동 -> 다음 플레이어 턴 / selection panel 선택지 조건 확인
    private IEnumerator PlayerTurn(bool clockwise)
    {
        yield return StartCoroutine(players[currentPlayerNum].PlayerMove(Dice.finalDiceValue, clockwise));

        isMoving = 2;

        int kingdomType = players[currentPlayerNum].GetCurrentTileOwner();
        if (cardPool1[kingdomType].Count == 0) SelectionPanelButtons[0].interactable = false;
        else SelectionPanelButtons[0].interactable = true;

        if (players[currentPlayerNum].CheckMojip()) SelectionPanelButtons[1].interactable = true;
        else SelectionPanelButtons[1].interactable = false;

        if (roundUntilEventOccur > 0) SelectionPanelButtons[2].interactable = true;
        else SelectionPanelButtons[2].interactable = false;

        if (players[currentPlayerNum].CheckJeomryeong()) SelectionPanelButtons[3].interactable = false;
        else SelectionPanelButtons[3].interactable = true;

        SelectionPanel.SetActive(true);
    }

    public void NextTurn()
    {
        if (!isComplete)
        {
            Debug.Log("아직 턴이 종료되지 않았습니다.");
            return;
        }
        players[currentPlayerNum].SetArrow(false);
        currentTurn++;

        // 모두 턴 가졌으면 다음 라운드
        if (currentTurn == playerNum) SortPlayerTurnOrder();
        else
        {
            currentPlayerNum = playerTurnOrder[currentTurn];
            players[currentPlayerNum].SetArrow(true);
        }

        layoutgroupcontroller.Instance.RefreshLayoutGroup(players[currentPlayerNum].cardList);
        isComplete = false;
        isRolled = false;
        isMoving = 0;
    }

    // 점령 버튼 클릭 시 작동
    public void OccupyRegion()
    {
        // 싸우는 스크립트 넣는 곳

        bool win = true;
        int currentCellIndex = players[currentPlayerNum].GetCellIndex();
        MapTile currentMapTile = GameBoard.tileInfos[currentCellIndex].GetMapTile();
        if (win)
        {
            int currentOwner = currentMapTile.Owner;
            GameBoard.tileInfos[currentCellIndex].tileColor =
                currentMapTile.ChangeOwner(currentPlayerNum);

            tilePerPlayer[currentOwner]--;
            tilePerPlayer[currentPlayerNum]++;
        }

        SelectionPanel.SetActive(false);
        isComplete = true;
    }

    // 징집 버튼 클릭 시 작동
    public void Jingjip()
    {
        players[currentPlayerNum].AddCard(type: 1);
        SelectionPanel.SetActive(false);
        isComplete = true;
    }

    // 모집 버튼 클릭 시 작동
    public void Mojip()
    {
        SelectionPanel.SetActive(false);
        StartCoroutine(currentPlayer.SelectDeleteCard());
        isComplete = true;
    }

    // 랜덤 이벤트 생성
    private void NewEvent()
    {
        eventType = Random.Range(1, 5);
        roundUntilEventOccur = Random.Range(10, 20);
        EventTimer.text = "이벤트 " + eventType + " 발동까지 " + roundUntilEventOccur + " 라운드 남음";
    }

    // 기도 버튼 클릭 시 작동
    public void Pray()
    {
        SelectionPanel.SetActive(false);
        isComplete = true;
        roundUntilEventOccur--;
        if (roundUntilEventOccur <= 0) EventTimer.text = "이벤트 발동";
        else EventTimer.text = "이벤트 " + eventType + " 발동까지 " + roundUntilEventOccur + " 라운드 남음";
    }
}
