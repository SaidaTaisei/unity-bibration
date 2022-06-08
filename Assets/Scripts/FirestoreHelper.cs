using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FireStoreHelper
{
    public Task previousTask;
    public bool operationInProgress = false;
    public bool isErr = false;
    public int dayStay;
    public bool isWrited;
    public List<KeyValuePair<string, object>> resultItems;

    class WaitForTaskCompletion : CustomYieldInstruction
    {
        Task task;
        FireStoreHelper uiHandler;
        private float timeoutTime = 10.0f;
        private float startTime;

        // Create an enumerator that waits for the specified task to complete.
        public WaitForTaskCompletion(FireStoreHelper uiHandler, Task task)
        {
            uiHandler.previousTask = task;
            uiHandler.operationInProgress = true;
            uiHandler.isErr = false;
            uiHandler.isWrited = false;
            this.uiHandler = uiHandler;
            this.task = task;
            this.startTime = Time.realtimeSinceStartup;
        }

        // Wait for the task to complete.
        public override bool keepWaiting
        {
            get
            {
                if (task.IsCompleted)
                {
                    uiHandler.operationInProgress = false;
                    if (task.IsFaulted)
                    {
                        uiHandler.isErr = true;
                        Debug.Log(task.Exception);
                    }
                    return false;
                }
                // 時間が経っていたら、処理をキャンセル
                if (Time.realtimeSinceStartup - startTime > timeoutTime)
                {
                    uiHandler.isErr = true;
                    return false;
                }
                return true;
            }
        }
    }

    public IEnumerator getData()
    {
        Debug.Log("downloading...");
        var collectionRef = FirebaseFirestore.DefaultInstance.Collection("bibration_informations");
        Query documentQuerys = collectionRef.OrderByDescending("time").Limit(3);
        Task<QuerySnapshot> task = documentQuerys.GetSnapshotAsync();
        yield return new WaitForTaskCompletion(this, task);
        this.isWrited = false;
        if (!this.isErr)
        {
            Debug.Log(task.Result.Documents.ToString());
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>> { };
            foreach (var doc in task.Result.Documents)
                results.Add(doc.ToDictionary());
            List<KeyValuePair<string, object>> items = new List<KeyValuePair<string, object>> { };
            foreach (KeyValuePair<string, object> item in results[0])
            {
                //Debug.Log(item.Key);
                //Debug.Log(item.Value);
                items.Add(item);
            }
            resultItems = items;
        }
    }
}
