using UnityEngine;

public class Pipes : MonoBehaviour
{
    public Transform top;
    public Transform bottom;
    public float speed = 5f;
    public float gap = 3f;
    
    public bool moveVertically = false;
    public float verticalAmplitude = 0.5f;
    public float verticalSpeed = 1f;
    private float leftEdge;
    private float originalY;

    private void Start()
    {
        leftEdge = Camera.main.ScreenToWorldPoint(Vector3.zero).x - 1f;
        originalY = transform.position.y;

        top.position += Vector3.up * gap / 2;
        bottom.position += Vector3.down * gap / 2;
    }

    private void Update()
    {
        // move pipes horizontally
        transform.position += speed * Time.deltaTime * Vector3.left;

        // If moveVertically is true, gently move pipes up and down
        if (moveVertically)
        {
            float newY = originalY + Mathf.Sin(Time.time * verticalSpeed) * verticalAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        if (transform.position.x < leftEdge)
        {
            Destroy(gameObject);
        }
    }
}
