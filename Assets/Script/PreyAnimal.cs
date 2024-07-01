using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;


[System.Serializable]
public class PreyStat
{
    public int Lv = 0;
    public string Name = "";
    public float MaxHP = 0;
    public float CurHP = 0;
    public long At = 0;
    public int Speed = 0;
    public int MaxExp = 0;
    public int CurExp = 0;
}
public enum State
{
    Move,
    Find,
    Follow
}
public class PreyAnimal : MonoBehaviour
{
    public TextMeshProUGUI AtText;

    Animator anim;
    Rigidbody rigid;

    [HideInInspector]
    public PreyStat stat;
    public string AniCode;

    public float wanderRadius = 50f; // 랜덤 이동 반경
    public float wanderTimer = 3; // 목적지 변경 주기

    private NavMeshAgent agent;
    private float timer;
    [HideInInspector] public Transform target;

    public State state;
    void Start()
    {
        target = GameManager.Instance.player.gameObject.transform;
        state = State.Move;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = stat.Speed * 0.8f;
        timer = wanderTimer;
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //print("먹잇감 레벨 : "+stat.Lv);
        //print(stat.Name);
        //print(stat.HP);
        //print(stat.At);
        //print(stat.MaxExp);
        //print(stat.CurExp);


        switch (state)
        {
            case State.Move:
                //print("이동 중");
                Movement();
                find();
                texta();
                break;

            case State.Find:
                //print("발견발견");
                //agent.SetDestination(target.position);
                find();
                texta();
                break;

            case State.Follow:
                AtText.text = "";
                follow();
                break;
        }
        
    }
    void texta()
    {
        long playerAt = target.gameObject.GetComponent<PlayerController>().playerstat.At;
        if (stat.At < playerAt)
        {
            AtText.color = Color.green;
        }
        else if (stat.At > playerAt)
        {
            AtText.color = Color.red;
        }
        else
        {
            AtText.color = Color.white;
        }
        AtText.text = GameManager.Instance.FormatNumber(stat.At);
    }
    private void follow()
    {
        //print("따라가기");
        agent.isStopped = true;
        // 타겟 방향 계산
        Vector3 direction = (target.position - transform.position).normalized;

        // 이동 벡터 계산
        Vector3 moveVector = direction * stat.Speed * Time.deltaTime;

        // 새로운 위치 계산
        Vector3 newPosition = transform.position + moveVector;

        // 오브젝트를 새로운 위치로 이동
        transform.position = newPosition;

        // 타겟 방향으로 회전
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8.5f);

        rigid.isKinematic = true;
    }
    private void find()
    {
        float Dis = Vector3.Distance(target.position ,transform.position);
        if (Dis < 1)
        {
            state = State.Find;
        }
        else
        {
            state = State.Move;
        }
    }

    private void Movement()
    {
        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawSphere(transform.position,1);
    }


}