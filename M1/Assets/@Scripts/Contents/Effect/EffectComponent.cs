using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class EffectComponent : MonoBehaviour
{
    // 컨테이너를 통해 Effect를 들고 있도록
    public List<EffectBase> ActiveEffects = new List<EffectBase>();
    private Creature _owner;

    public void SetInfo(Creature Owner)
    {
        _owner = Owner;
    }


    // 이펙트 생성 후 등록, 실행
    public List<EffectBase> GenerateEffects(IEnumerable<int> effectIds, EEffectSpawnType spawnType)
    {
        List<EffectBase> generatedEffects = new List<EffectBase>();

        foreach (int id in effectIds)
        {
            string className = Managers.dataManager.EffectDic[id].ClassName;
            Type effectType = Type.GetType(className);

            if (effectType == null) 
            {
                Debug.LogError($"Effect Type not found: {className}");
                return null;
            }

            // EffectBase를 상속받아서 구성하기 때문에 구조가 약간 꼬이는 부분이 있다. Manager 통한~
            GameObject go = Managers.objectManager.SpawnGameObject(_owner.CenterPosition, "EffectBase");
            go.name = Managers.dataManager.EffectDic[id].ClassName;
            EffectBase effect = go.AddComponent(effectType) as EffectBase;
            effect.transform.parent = _owner.Effects.transform;
            effect.transform.localPosition = Vector2.zero;
            Managers.objectManager.Effects.Add(effect);

            ActiveEffects.Add(effect);
            generatedEffects.Add(effect);

            effect.SetInfo(id, _owner, spawnType);
            effect.ApplyEffect();
        }

        return generatedEffects;
    }

    public void RemoveEffects(EffectBase effects)
    {

    }

    public void ClearDebuffsBySkill()
    {
        foreach (var buff in ActiveEffects.ToArray())
        {
            if (buff.EffectType != EEffectType.Buff)
            {
                buff.ClearEffect(EEffectClearType.ClearSkill);
            }
        }
    }
}
