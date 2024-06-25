using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoader<Key,Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Data.TestData> TestDic { get; private set; } = new Dictionary<int, Data.TestData>();

    public void Init()
    {
        // 데이터를 긁어오는 부분
        TestDic = LoadJson<Data.TestDataLoader, int, Data.TestData>("TestData").MakeDict();
    }

    // Json 불러와서 Deserialize
    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.resourceManager.Load<TextAsset>(path);
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}
