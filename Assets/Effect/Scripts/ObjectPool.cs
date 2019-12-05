using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private List<GameObject> pool_obj_list;
    private GameObject pool_list;

    // オブジェクトプールを作成
    public void CreatePool(GameObject obj, int max_count)
    {
        pool_list = obj;
        pool_obj_list = new List<GameObject>();
        for (int i = 0; i < max_count; i++)
        {
            var new_obj = CreateNewObject();
            new_obj.SetActive(false);
            pool_obj_list.Add(new_obj);
        }
    }

    public GameObject GetObject()
    {
        // 使用中でないものを探して返す
        foreach (var obj in pool_obj_list)
        {
            if (obj.activeSelf == false)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // 全て使用中だったら新しく作って返す
        var new_obj = CreateNewObject();
        new_obj.SetActive(true);
        pool_obj_list.Add(new_obj);
        
        return new_obj;
    }

    private GameObject CreateNewObject()
    {
        var new_obj = Instantiate(pool_list);
        new_obj.name = pool_list.name + (pool_obj_list.Count + 1);

        return new_obj;
    }

}
