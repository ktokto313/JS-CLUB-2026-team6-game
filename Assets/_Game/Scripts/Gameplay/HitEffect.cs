using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField] private float secondsToDisappear = 0.2f;

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(HideAfterTime(secondsToDisappear, gameObject));
    }

    private IEnumerator HideAfterTime(float delay, GameObject objectToBeDeleted)
    {
        yield return new WaitForSecondsRealtime(delay);
        GlobalPoolManager.Instance.Return(objectToBeDeleted);
    }
}
