using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;



[Serializable]
public class NpcData
{
    public GameObject npcModel;
    public List<string> demands;
    public bool accepting;
    public CancellationTokenSource Cts;
}

public class NpcManager : MonoBehaviour
{
    
    [SerializeField] private GameObject npcStart;
    [SerializeField] private GameObject npcStop;
    private Transform dump;
    private System.Random _random;

    private List<GameObject> _npcs;

    private void Awake()
    {
        _random = new System.Random();
        dump = transform.parent.Find("DUMP");
        _npcs =  new List<GameObject>();
        ReadChildrenData();
        Debug.Log("TOTAL NPCS: " + _npcs.Count);
    }
    
    private void ReadChildrenData()
    {
        foreach (Transform npc in transform)
        {
            _npcs.Add(npc.gameObject);
            Debug.Log("NPC REGISTERED: " + npc.name);
        }
    }
    
    public NpcData InviteNpc(List<string> demands)
    {
        return new NpcData
        {
            npcModel = Instantiate(_npcs[_random.Next(0, _npcs.Count)], dump),
            demands = demands,
            accepting = false,
            Cts = new CancellationTokenSource(),
        };
    }

    public bool GiveItem(string productName, NpcData npc)
    { 
        if (npc.accepting && npc.demands.Contains(productName))
        {
            npc.demands.Remove(productName);
            return true;
        }
        return false;
    }
    
    public async Task WalkToTable(NpcData npc)
    {
        var rb = npc.npcModel.GetComponent<Rigidbody>();
        var startPos = npcStart.transform.position;
        var targetPos = npcStop.transform.position;
        var speed = 40f;
        
        var animator = npc.npcModel.GetComponent<Animator>();
        if (animator != null) animator.SetBool("isWalking", true);
        
        rb.MovePosition(startPos);
        rb.MoveRotation(Quaternion.LookRotation(targetPos - startPos));
        while (Vector3.Distance(rb.position, targetPos) > 0.1f)
        {
            var direction = (targetPos - rb.position).normalized;
            var newPos = rb.position + direction * speed * Time.deltaTime;
            rb.MovePosition(newPos);
            await Task.Yield();
        }
        
        rb.MovePosition(targetPos);
        if (animator != null) animator.SetBool("isWalking", false);
        npc.accepting = true;
    }
    
    public async Task WalkAwayFromTable(NpcData npc)
    {
        npc.accepting = false;
        var rb = npc.npcModel.GetComponent<Rigidbody>();
        var startPos = npcStop.transform.position;
        var targetPos = npcStart.transform.position;
        var speed = 40f;
        
        var animator = npc.npcModel.GetComponent<Animator>();
        if (animator != null) animator.SetBool("isWalking", true);
        
        rb.MovePosition(startPos);
        rb.MoveRotation(Quaternion.LookRotation(targetPos - startPos));
        while (Vector3.Distance(rb.position, targetPos) > 0.1f)
        {
            var direction = (targetPos - rb.position).normalized;
            var newPos = rb.position + direction * speed * Time.deltaTime;
            rb.MovePosition(newPos);
            await Task.Yield();
        }
        
        rb.MovePosition(targetPos);
        
        if (animator != null) animator.SetBool("isWalking", false);
        
        Destroy(npc.npcModel);
    }
}
