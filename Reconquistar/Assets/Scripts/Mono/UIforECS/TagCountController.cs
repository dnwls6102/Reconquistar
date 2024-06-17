using System.Collections;
using System.Collections.Generic;
using _1.Scripts.DOTS.Components___Tags;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class TagCountController : MonoBehaviour
{
    public TMP_Text PmoveDoneTag;

    public TMP_Text PmovingTag;

    public TMP_Text movingTag;

    public TMP_Text normalactionDoneTag;

    public TMP_Text attackDoneTag;

    public TMP_Text attackingTag;
    private EntityManager _entityManager;

    private EntityQuery PmoveDone;

    private EntityQuery Pmoving;

    private EntityQuery moving;

    private EntityQuery NormalActionDone;

    private EntityQuery AttackDone;

    private EntityQuery Attacking;
    // Start is called before the first frame update
    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        PmoveDone = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<PriorityMoveDoneTag>());
        Pmoving = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<PriorityMovingTag>());
        moving = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<MovingTag>());
        NormalActionDone = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<NormalActionDoneTag>());
        AttackDone = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<AttackDoneTag>());
        Attacking =_entityManager.CreateEntityQuery(ComponentType.ReadOnly<AttackTag>());
    }

    // Update is called once per frame
    void Update()
    {
        PmoveDoneTag.SetText("{0}",PmoveDone.CalculateEntityCount());
        PmovingTag.SetText("{0}",Pmoving.CalculateEntityCount());
        movingTag.SetText("{0}",moving.CalculateEntityCount());
        normalactionDoneTag.SetText("{0}",NormalActionDone.CalculateEntityCount());
        attackDoneTag.SetText("{0}",AttackDone.CalculateEntityCount());
        attackingTag.SetText("{0}",Attacking.CalculateEntityCount());
    }
}
