using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;


public class GameManager : MonoBehaviour
{

    [HideInInspector] public string StatData;
    const string URL = "https://docs.google.com/spreadsheets/d/1qSbuMSkixGuWZJ--hDFaKnOlVopao0pXk9RJN53N90Q/export?format=tsv&range=A2:J17";

    public FloatingJoystick Joystick;
    public GameObject StartAni;

    [HideInInspector] public UIManager UImanager;
    [HideInInspector] public SpawnManager Spawnmanager;
    [HideInInspector] public PlayerController player;
    [HideInInspector] public int PlayerLv = 1;
    Vector3 spawnPos = new Vector3(0, 0.3f, 0);
    public float[] AnimalSize;

    public float MaxTime = 90; // 최대 제한 시간
    public float CurTime = 90; // 현재 제한 시간
    public bool IsStart = false; // 게임 시작 여부
    public bool GETSheet = false; // 시트 받아오기 여부
    public long[] goalScore; // 레벨에 따른 목표 점수
    public int Score = 0; // 현재 점수


    // 아이템 playerprefs

    [HideInInspector] public int Coin;
    [HideInInspector] public int AttackItem1Lv;
    [HideInInspector] public int TimeItem2Lv;
    [HideInInspector] public int SizeItem3Lv;

    private string coinKey = "PlayersCoin";
    private string AttackItem1LvKey = "AttackItem1Lv";
    private string TimeItem2LvKey = "TimeItem2Lv";
    private string SizeItem3LvKey = "SizeItem3Lv";

    // 싱글톤
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }



    private void Start()
    {
        LoadCoin(); // 저장된 playerprefs 불러오기
    }

    // PlayerPrefs 불러오기
    void LoadCoin()
    {
        if (PlayerPrefs.HasKey(coinKey))
        {
            Coin = PlayerPrefs.GetInt(coinKey);
            AttackItem1Lv = PlayerPrefs.GetInt(AttackItem1LvKey);
            TimeItem2Lv = PlayerPrefs.GetInt(TimeItem2LvKey);
            SizeItem3Lv = PlayerPrefs.GetInt(SizeItem3LvKey);
        }
        else // 저장된 값이 없을 경우 기본값 0으로 설정
        {            
            Coin = 0;
            AttackItem1Lv = 1;
            TimeItem2Lv = 1;
            SizeItem3Lv = 1;
        }

        // 아이템 적용
        CurTime += TimeItem2Lv * 1;
        MaxTime += TimeItem2Lv * 1;
    }

    // Playerprefs 저장
    public void SaveCoin()
    {
        PlayerPrefs.SetInt(coinKey, Coin);
        PlayerPrefs.SetInt(AttackItem1LvKey, AttackItem1Lv);
        PlayerPrefs.SetInt(TimeItem2LvKey, TimeItem2Lv);
        PlayerPrefs.SetInt(SizeItem3LvKey, SizeItem3Lv);
        PlayerPrefs.Save();
    }

    //앱이 일시 중지될 때 마지막으로 저장
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveCoin();
        }
    }

    // 코인 감소
    public void UseCoin(int amount)
    {
        Coin -= amount;
        if (Coin < 0) Coin = 0;
        SaveCoin(); // 저장
    }





    private void Update()
    {
        if (player != null) PlayerLv = player.playerstat.Lv;
    }

    // 게임 오버 리셋
    public void GameOver(bool over)
    {
        Coin += Score;
        SaveCoin();
        IsStart = false;
        CurTime = MaxTime;

        UImanager.gameUI.gameUI.SetActive(false);
        if (over)
        {
            UImanager.endUI.gameOverUI.SetActive(true);
            UImanager.OverUIUpdate(Score, Coin);
        }
        else
        {
            UImanager.endUI.gameClearUI.SetActive(true);
            UImanager.ClearUIUpdate(Score, Coin);
        }

        Score = 0;
        Destroy(player.gameObject);
    }

    // 플레이어 프리팹 생성
    public void PlayerIns()
    {
        // 플레이어 생성
        player = Instantiate(StartAni, spawnPos, Quaternion.identity).GetComponent<PlayerController>();

        // (크기)아이템 적용
        Vector3 playerScale = player.transform.localScale;
        player.transform.localScale = new Vector3(playerScale.x + 0.02f * SizeItem3Lv, playerScale.y + 0.02f * SizeItem3Lv, playerScale.z + 0.02f * SizeItem3Lv);

        StartCoroutine(GetSheet()); // 구글 스프레드 시트 받아오기 시작
    }

    // 구글스프레드시트 데이터 받아오기
    IEnumerator GetSheet()
    {
        UnityWebRequest www = UnityWebRequest.Get(URL);

        yield return www.SendWebRequest();

        StatData = www.downloadHandler.text;
        PlayerInitStat(StatData, "1");
        GETSheet = true;
    }

    // 플레이어 스탯 초기화
    public void PlayerInitStat(string tsv, string AniCode)
    {
        string[] row = tsv.Split("\n");
        int rowSize = row.Length;
        int columnSize = row[0].Split("\t").Length;

        for (int i = 0; i < rowSize; i++)
        {
            string[] column = row[i].Split("\t");
            for (int j = 0; j < columnSize; j++)
            {
                if (column[0] == AniCode)
                {
                    player.playerstat.Lv = int.Parse(column[0]);
                    player.playerstat.Name = column[1];
                    player.playerstat.Speed = int.Parse(column[3]);
                }

                // 플레이어 정보가 아닌 그 외 정보 (동물이름, 목표점수, 동물크기...)
                UImanager.gameUI.AnimalName[i] = column[1];
                goalScore[i] = long.Parse(column[4]);
                AnimalSize[i] = float.Parse(column[5]);
                Spawnmanager.PoolObjText[i] = column[6];
                Spawnmanager.spawnTime[i] = float.Parse(column[7]);
            }
        }
        player.playerstat.At += 3 * AttackItem1Lv; // 공격력 증가 아이템1번 구현
        Score = 0; // 플레이어 업그레이드 후 목표점수 초기화
    }

    // 동물 오브젝트 스탯 초기화
    public void PreyInitStat(string tsv, string AniCode, PreyAnimal prey)
    {
        string[] row = tsv.Split("\n");
        int rowSize = row.Length;
        int columnSize = row[0].Split("\t").Length;

        for (int i = 0; i < rowSize; i++)
        {
            string[] column = row[i].Split("\t");
            for (int j = 0; j < columnSize; j++)
            {
                if (column[0] == AniCode)
                {
                    // AniCode에 해당하는 스탯 부여
                    prey.stat.Lv = int.Parse(column[0]);
                    prey.stat.Name = column[1];
                    prey.stat.At = double.Parse(column[2]);
                    prey.stat.Speed = int.Parse(column[3]);
                }
            }
        }
    }

    // 자릿수 단위 메서드
    public string FormatNumber(double number)
    {
        if (number >= 1_000_000_000_000)
        {
            return (number / 1_000_000_000_000D).ToString(number >= 100 ? "0" : "0.00") + "T"; // 1조
        }
        else if (number >= 1_000_000_000)
        {
            return (number / 1_000_000_000D).ToString(number >= 100 ? "0" : "0.00") + "B"; // 10억
        }
        else if (number >= 1_000_000)
        {
            return (number / 1_000_000D).ToString(number >= 100 ? "0" : "0.00") + "M"; // 10만
        }
        else if (number >= 100)
        {
            return number.ToString("N0"); // 소숫점 없음
        }
        else
        {
            return number.ToString("N2"); // 소숫점 2자리까지
        }
    }

}