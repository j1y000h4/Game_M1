using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[Serializable]
public class CreatureStat
{
    public float BaseValue { get; private set; }
    private bool _isDirty = true;

    [SerializeField]
    private float _value;
    public virtual float Value
    {
        // get을 하는 순간에 최종 계산을 다시 한 뒤 리턴
        get
        {
            if (_isDirty)
            {
                _value = CalculateFinalValue();
                _isDirty = false;
            }

            return _value;
        }

        private set { _value = value; }
    }

    public List<StatModifier> StatModifiers = new List<StatModifier>();

    public CreatureStat()
    {

    }

    public CreatureStat(float baseValue) : this()
    {
        BaseValue = baseValue;
    }

    public virtual void AddModifier(StatModifier modifier)
    {
        _isDirty = true;
        StatModifiers.Add(modifier);
    }

    public virtual bool RemoveModifier(StatModifier modifier)
    {
        if (StatModifiers.Remove(modifier))
        {
            _isDirty = true;
            return true;
        }

        return false;
    }

    public virtual bool ClearModifiersFromSource(object source)
    {
        int numRemovals = StatModifiers.RemoveAll(mod => mod.Source == source);

        if (numRemovals > 0)
        {
            _isDirty = true;
            return true;
        }
        return false;
    }

    private int CompareOrder(StatModifier a, StatModifier b)
    {
        if (a.Order == b.Order)
            return 0;

        return (a.Order < b.Order) ? -1 : 1;
    }


    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        // CompareOrder를 통해 중요도 순으로 연산을 진행
        StatModifiers.Sort(CompareOrder);

        for (int i = 0; i < StatModifiers.Count; i++)
        {
            StatModifier modifier = StatModifiers[i];

            switch (modifier.Type)
            {
                case EStatModType.Add:
                    finalValue += modifier.Value;
                    break;
                case EStatModType.PercentAdd:
                    sumPercentAdd += modifier.Value;
                    if (i == StatModifiers.Count - 1 || StatModifiers[i + 1].Type != EStatModType.PercentAdd)
                    {
                        finalValue *= 1 + sumPercentAdd;
                        sumPercentAdd = 0;
                    }
                    break;
                case EStatModType.PercentMult:
                    finalValue *= 1 + modifier.Value;
                    break;
            }
        }

        return (float)Math.Round(finalValue, 4);
    }
}
