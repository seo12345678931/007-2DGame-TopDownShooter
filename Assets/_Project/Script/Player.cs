using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody rb;
    public float moveSpeed = 10.0f;
    public GameObject mousePointer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // 이동조작 및 움직임
    private void FixedUpdate()
    {
        float H = Input.GetAxis("Horizontal");
        float V = Input.GetAxis("Vertical");
        Vector3 moveVec = new Vector3(H * moveSpeed, 0.0f, V * moveSpeed);
        rb.linearVelocity = moveVec;

        UpdateAim();
    }

    // 마우스 포인터
    public void UpdateAim()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.y = transform.position.y;
        mousePointer.transform.position = mousePos;
        float deltaY = mousePos.z - transform.position.z;
        float deltaX = mousePos.x - transform.position.x;
        float angleInDegrees = Mathf.Atan2(deltaY, deltaX) * 180 / Mathf.PI;
        transform.eulerAngles = new Vector3(0, -angleInDegrees, 0);
    }
}
