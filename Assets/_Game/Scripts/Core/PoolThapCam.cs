using System.Collections.Generic;
using UnityEngine;

public class GlobalPoolManager : MonoBehaviour {
    public static GlobalPoolManager Instance;
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

    private void Awake() { Instance = this; }

public GameObject Get(GameObject prefab, Vector3 pos) {
    string key = prefab.name;
    if (!pools.ContainsKey(key)) pools[key] = new Queue<GameObject>();

    GameObject obj;
    if (pools[key].Count > 0) {
        obj = pools[key].Dequeue();
        
        // 1. Tách cha (nếu đang bị dính vào Player) và đặt vị trí Spawn
        obj.transform.SetParent(null);
        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = prefab.transform.localScale; // Đảm bảo scale không bị loạn

        // 2. QUAN TRỌNG: Reset toàn bộ con (Cái thước kẻ, Sprite, vũ khí...)
        // Chúng ta duyệt qua tất cả các transform con bên trong
        Transform[] allChildren = obj.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren) {
            // Không reset chính thằng cha (vì ta đã set ở bước 1)
            if (child == obj.transform) continue;

            // Lấy dữ liệu gốc từ Prefab để đưa các con về đúng vị trí/góc quay ban đầu
            // Nếu bạn muốn đơn giản là đưa về 0, dùng dòng dưới:
            child.localPosition = Vector3.zero; 
            child.localRotation = Quaternion.identity;
            
            // Nếu cái thước có vị trí mặc định không phải 0,0,0 (ví dụ cầm ở tay),
            // thì tốt nhất là Animator.Play sẽ tự đưa nó về (nếu có Keyframe).
        }

        // 3. Reset Vật lý
        Rigidbody2D[] rbs = obj.GetComponentsInChildren<Rigidbody2D>();
        foreach (var rb in rbs) {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.isKinematic = false; 
        }

        // 4. Reset Animator (Cực kỳ quan trọng để reset cái thước)
        Animator[] anims = obj.GetComponentsInChildren<Animator>();
        foreach (var anim in anims) {
            foreach (var param in anim.parameters) {
                if (param.type == AnimatorControllerParameterType.Trigger)
                    anim.ResetTrigger(param.name);
            }
            // Ép Animator quay về tư thế mặc định (Tư thế cầm thước thẳng)
            // Số 0 là layer, 0f là bắt đầu từ frame đầu tiên
            anim.Play(0, -1, 0f); 
            anim.Update(0); // Ép cập nhật ngay lập tức để hình ảnh nhảy về đúng chỗ
        }

        // 5. Reset Collider
        Collider2D[] cols = obj.GetComponentsInChildren<Collider2D>();
        foreach (var col in cols) col.enabled = true;

        obj.SetActive(true);
    } else {
        obj = Instantiate(prefab, pos, Quaternion.identity);
        obj.name = key;
    }
    return obj;
}

    public void Return(GameObject obj) {
        // Trước khi ẩn, đưa về gốc để không làm loạn Hierarchy
        obj.transform.SetParent(null); 
        obj.SetActive(false);
        
        if (pools.ContainsKey(obj.name)) pools[obj.name].Enqueue(obj);
    }
}