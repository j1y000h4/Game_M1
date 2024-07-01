using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ObjectManager 
{
    // spawn despawn
    // 온라인 게임이라면 히어로나 몬스터나 모든 애들이 아이디를 들고 있을 것이기 때문에 키랑 값을 들고 있는게 일반적. 보통 리스트 사용
    // 싱글 게임이니 일단은 해시셋으로
    public HashSet<Hero> Heroes { get; } = new HashSet<Hero>();
    public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
    public HashSet<Env> Envs { get; } = new HashSet<Env>();
    public HeroCamp Camp { get; private set; }

    #region Roots
    // root가 없으면 만들어주고 root를 리턴
    public Transform GetRootTransform(string name)
    {
        GameObject root = GameObject.Find(name);
        if (root == null)
        {
            root = new GameObject { name = name };
        }

        return root.transform;
    }

    public Transform HeroRoot { get { return GetRootTransform("@Heroes"); } }
    public Transform MonsterRoot { get { return GetRootTransform("@Monsters"); } }
    public Transform EnvRoot { get { return GetRootTransform("@Envs"); } }
    #endregion

    // BaseObject를 상속받는 컴포넌트를 기입해서 Spawn해달라
    // templateID를 추가해서 종류에 따라 다르게 셋팅하기 위해
    public T Spawn<T>(Vector3 position, int templateID) where T : BaseObject
    {
        string prefabName = typeof(T).Name;

        GameObject go = Managers.resourceManager.Instantiate(prefabName);
        go.name = prefabName;
        go.transform.position = position;

        BaseObject obj = go.GetComponent<BaseObject>();

        if (obj.ObjectType == EObjectType.Creature)
        {
            // Data Check 예외처리
            // templateID가 맞지 않을 수도 있으니 한번 더 체크해준다.
            if (templateID != 0 && Managers.dataManager.CreatureDic.TryGetValue(templateID, out Data.CreatureData data) == false)
            {
                Debug.LogError($"ObjectManager Spawn Creature Failed !! TryGetValue TemplateID : {templateID}");
                return null;
            }

            Creature creature = go.GetComponent<Creature>();
            switch (creature.CreatureType)
            {
                case ECreatureType.Hero:
                    obj.transform.parent = HeroRoot;
                    Hero hero = creature as Hero;
                    Heroes.Add(hero);
                    break;

                case ECreatureType.Monster:
                    obj.transform.parent = MonsterRoot;
                    Monster monster = creature as Monster;
                    Monsters.Add(monster);
                    break;
            }

            // 원하는 데이터 시트 아이디에 따라 SetInfo 호출
            creature.SetInfo(templateID);
        }
        else if (obj.ObjectType == EObjectType.Projectile)
        {
            // TODO
        }
        else if (obj.ObjectType == EObjectType.Env)
        {
            // Data Check 예외처리
            if (templateID != 0 && Managers.dataManager.EnvDic.TryGetValue(templateID, out Data.EnvData data) == false)
            {
                Debug.LogError($"ObjectManager Spawn Creature Failed !! TryGetValue TemplateID : {templateID}");
                return null;
            }
            obj.transform.parent = EnvRoot;

            Env env = go.GetComponent<Env>();
            Envs.Add(env);

            env.SetInfo(templateID);
        }

        else if(obj.ObjectType == EObjectType.HeroCamp)
        {
            Camp = go.GetComponent<HeroCamp>();
        }

        // obj는 BaseObject를 가지고 온것이고, T는 BaseObject상속받은 그 무언가
        return obj as T;
    }

    public void Despawn<T>(T obj) where T : BaseObject
    {
        EObjectType objectType = obj.ObjectType;

        if (obj.ObjectType == EObjectType.Creature)
        {
            Creature creature = obj.GetComponent<Creature>();
            switch (creature.CreatureType)
            {
                case ECreatureType.Hero:
                    Hero hero = creature as Hero;
                    Heroes.Remove(hero);
                    break;
                case ECreatureType.Monster:
                    Monster monster = creature as Monster;
                    Monsters.Remove(monster);
                    break;
            }
        }
        else if (obj.ObjectType == EObjectType.Projectile)
        {
            // TODO
        }
        else if (obj.ObjectType == EObjectType.Env)
        {
            // TODO

            Env env = obj as Env;
            Envs.Remove(env);
        }
        else if (obj.ObjectType == EObjectType.HeroCamp)
        {
            Camp = null;
        }

        Managers.resourceManager.Destory(obj.gameObject);
    }
}
