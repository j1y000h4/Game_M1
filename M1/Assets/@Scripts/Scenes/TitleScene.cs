using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Managers.resourceManager.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count}/{totalCount}");

            if (count == totalCount)
            {

            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
