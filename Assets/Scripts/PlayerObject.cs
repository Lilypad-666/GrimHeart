using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PlayerObject : MonoBehaviour {
    [SerializeField]
    [Tooltip("Multiplier for the scaling of the player.")]
    float scale = 1f;

    private Vector3 desiredPos;

    private float dampingFactor = 10f;

    private void Start() {
        //Define the desired position as this position
        desiredPos = this.transform.position;
    }

    public void setTransform(Vector3 pos) {
        desiredPos = pos;
    }

    void FixedUpdate() {
        if (desiredPos.x != 0 || desiredPos.y != 0 || desiredPos.z != 0)
            GetComponent<Rigidbody2D>().MovePosition(Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * dampingFactor));
    }

    public void setDampingFactor(float factor) {
        dampingFactor = factor;
    }
}