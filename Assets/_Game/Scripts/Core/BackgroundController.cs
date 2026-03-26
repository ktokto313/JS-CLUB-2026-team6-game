using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    public static BackgroundController Instance { private set; get; }
    
    [SerializeField] private List<GameObject> backgroundList;
    [SerializeField] private float transitionTime = 2f;
    [SerializeField] private int waveCount = 5;
    private int currentWave = 0;
    private int currentBackgroundIndex = 0;

    // Allows repeated calls while a transition is already running.
    private int queuedTransitions = 0;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    public void OnNextWave()
    {
        currentWave++;
        if ((waveCount / backgroundList.Count) * (currentBackgroundIndex + 1) >= currentWave) return;
        if (currentBackgroundIndex >= backgroundList.Count - 1) return; // Out of backgrounds.

        queuedTransitions++;

        if (transitionCoroutine == null)
        {
            transitionCoroutine = StartCoroutine(MoveBackgroundRoutine());
        }
    }

    private IEnumerator MoveBackgroundRoutine()
    {
        while (queuedTransitions > 0)
        {
            if (currentBackgroundIndex >= backgroundList.Count - 1)
            {
                queuedTransitions = 0; // Do nothing when run out of background.
                break;
            }

            int nextBackgroundIndex = currentBackgroundIndex + 1;
            // var currentObj = backgroundList[currentBackgroundIndex];
            // var nextObj = backgroundList[nextBackgroundIndex];
            
            // if (currentObj == null || nextObj == null)
            // {
            //     currentBackgroundIndex = nextBackgroundIndex; // Skip invalid entries.
            //     queuedTransitions--;
            //     continue;
            // }

            // Tranform cause position is always copy by value
            Transform currentTr = backgroundList[currentBackgroundIndex].transform;
            Transform nextTr = backgroundList[nextBackgroundIndex].transform;

            Vector3 currentStart = currentTr.position;
            Vector3 nextStart = nextTr.position;
            Vector3 delta = new Vector3(30f, 0f, 0f);

            float duration = transitionTime;
            float elapsed = 0f;

            queuedTransitions--; // Consume 1 transition.

            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration); // Normalized t in [0..1]
                currentTr.position = Vector3.Lerp(currentStart, currentStart + delta, t);
                nextTr.position = Vector3.Lerp(nextStart, nextStart + delta, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Snap to exact end positions.
            currentTr.position = currentStart + delta;
            nextTr.position = nextStart + delta;

            currentBackgroundIndex = nextBackgroundIndex;
        }

        transitionCoroutine = null;
    }
}

