using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    [SerializeField] Text currentTimeText;
    [SerializeField] Text previousTimeText;
    private MeshFilter meshFilter;
    private Mesh myMesh;
    int beamNodeNum = 30;
    private Vector3[] Vertices = new Vector3[20];
    private int[] Triangles = new int[6 * 9];
    private float width = 2;
    private float hight = 2;
    int t = 0;

    // Start is called before the first frame update
    void Start()
    {
        Vertices = new Vector3[beamNodeNum * 2];
        Triangles = new int[6 * (beamNodeNum - 1)];
        StartCoroutine(updateState());
    }

    // Update is called once per frame
    void Update()
    {
        if (t % 100 == 0)
        {
            StartCoroutine(updateState());
        }
        t = t + 1;
    }

    IEnumerator updateState()
    {
        FireStoreHelper fireStoreHelper = new FireStoreHelper();
        yield return StartCoroutine(fireStoreHelper.getData());
        if (!fireStoreHelper.isErr)
        {
            List<KeyValuePair<string, object>> downloads = fireStoreHelper.resultItems;
            List<float> disps_float = new List<float> { };
            DateTime dateTime = new DateTime();
            foreach (var item in downloads)
            {
                if (item.Key == "displacement")
                {
                    List<object> disps = (List<object>)(item.Value);
                    foreach (var di in disps)
                    {
                        disps_float.Add((float)Convert.ToDouble(di));
                        Debug.Log(di);
                    }
                }
                if (item.Key == "relative_time")
                {
                    Debug.Log((item.Value.ToString()));
                }
                if (item.Key == "time")
                {
                    Debug.Log((item.Value.ToString()));
                    Debug.Log(item.Value.ToString().Replace("Timestamp: ", "").Trim());
                    dateTime = DateTime.ParseExact(item.Value.ToString().Replace("Timestamp: ", "").Trim(), "yyyy-MM-ddTHH:mm:ss.ffffffZ", null);
                    dateTime=dateTime.AddHours(-9);
                }
            }
            List<float> zsList = disps_float;
            Debug.Log(zsList);
            // node座標の設定
            for (int i = 0; i < beamNodeNum; i++)
            {
                Vertices[i] = new Vector3(i * 0.10f, zsList[i]*100f, 0.0f);
                Vertices[i + beamNodeNum] = new Vector3(i * 0.10f, zsList[i]*100f, 0.1f);
            }
            // meshの構成
            for (int i = 0; i < beamNodeNum - 1; i++)
            {
                Triangles[6 * i] = i;
                Triangles[6 * i + 1] = i + beamNodeNum;
                Triangles[6 * i + 2] = i + beamNodeNum + 1;
                Triangles[6 * i + 3] = i;
                Triangles[6 * i + 4] = i + beamNodeNum + 1;
                Triangles[6 * i + 5] = i + 1;
            }
            meshFilter = gameObject.GetComponent<MeshFilter>();
            myMesh = new Mesh();
            myMesh.SetVertices(Vertices);
            myMesh.SetTriangles(Triangles, 0);

            //MeshFilterへの割り当て
            meshFilter.mesh = myMesh;

            var dt = DateTime.Now;
            currentTimeText.text = "現在時刻：" + dt.Month.ToString() + "/" + dt.Day.ToString() + " " + dt.Hour.ToString() + ":" + dt.Minute.ToString() + ":" + dt.Second.ToString();
            previousTimeText.text = "解析時刻：" + dateTime.Month.ToString() + "/" + dateTime.Day.ToString() + " " + dateTime.Hour.ToString() + ":" + dateTime.Minute.ToString() + ":" + dateTime.Second.ToString();
        }
    }
}
