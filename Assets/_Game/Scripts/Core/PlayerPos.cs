using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("Player Reference")]
    public Transform PlayerTransform;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        if (PlayerTransform == null) {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) {
                PlayerTransform = playerObj.transform;
            } else {
                Debug.LogError("GameManager: Không tìm thấy đối tượng nào có Tag là 'Player'!");
            }
        }
    }
}