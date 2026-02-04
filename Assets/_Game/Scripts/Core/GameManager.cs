using UnityEngine;

// Sau này gộp vô Game controller sau để tôi test
public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public Transform PlayerTransform;

    private void Awake() { Instance = this; }
}