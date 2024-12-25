using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Pipes prefab;
    public float spawnRate = 1f;
    public float minHeight = -1f;
    public float maxHeight = 2f;
    public float verticalGap = 3f;

    private void OnEnable()
    {
        InvokeRepeating(nameof(Spawn), spawnRate, spawnRate);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Spawn));
    }

    private void Spawn()
    {
        Pipes pipes = Instantiate(prefab, transform.position, Quaternion.identity);
        pipes.transform.position += Vector3.up * Random.Range(minHeight, maxHeight);
        pipes.gap = verticalGap;

        // determine net behavior based on the player score
        UpdateNetMovement(pipes);
    }

    private void UpdateNetMovement(Pipes pipes)
    {
        // get the current score
        int score = GameManager.Instance.score;

        if (score > 50)
        {
            pipes.moveVertically = true;
            pipes.verticalSpeed = 3f;
        }
        else if (score > 30)
        {
            pipes.moveVertically = Random.value > 0.5f;
            pipes.verticalSpeed = 2.5f;
        }
        else if (score > 20)
        {
            pipes.moveVertically = Random.value > 0.7f;
            pipes.verticalSpeed = 2f;
        }
        else if (score > 5)
        {
            pipes.moveVertically = Random.value > 0.8f;
            pipes.verticalSpeed = 1.5f;
        }
        else
        {
            pipes.moveVertically = false; 
            pipes.verticalSpeed = 1f;
        }
    }
}
