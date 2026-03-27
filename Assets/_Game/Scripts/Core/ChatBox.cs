using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WorldChatUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject chatCanvas;
    [SerializeField] private Image bubbleBackground; 
    [SerializeField] private TextMeshProUGUI chatText;

    [Header("Sprites")]
    [SerializeField] private Sprite[] bubbleSprites;

    [Header("Settings")]
    [SerializeField] private float defaultDuration = 1.5f;

    private Coroutine hideCoroutine;

    void LateUpdate()
    {
        if (chatCanvas != null && chatCanvas.activeSelf && transform.parent != null)
        {
            float direction = transform.parent.localScale.x > 0 ? 1 : -1;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
            transform.eulerAngles = Vector3.zero;
        }
    }
    
    public void ShowChat(string[] textPool, float duration = -1f)
    {
        if (textPool == null || textPool.Length == 0) return;
        float finalDuration = (duration > 0) ? duration : defaultDuration;
        if (bubbleSprites != null && bubbleSprites.Length > 0 && bubbleBackground != null)
        {
            bubbleBackground.sprite = bubbleSprites[Random.Range(0, bubbleSprites.Length)];
        }
        if (chatText != null)
        {
            chatText.text = textPool[Random.Range(0, textPool.Length)];
        }
        if (bubbleBackground != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleBackground.rectTransform);
        }
        if (chatCanvas != null) chatCanvas.SetActive(true);
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        if (gameObject.activeInHierarchy)
        {
            hideCoroutine = StartCoroutine(HideAfterDelay(finalDuration));
        }
    }
    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (chatCanvas != null)
        {
            chatCanvas.SetActive(false);
        }
    }
}