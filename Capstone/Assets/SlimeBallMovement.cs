using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBallMovement : MonoBehaviour
{
    public float speed = 2f;
    public Vector3[] movePoints;
    private int currentPointIndex = 0;

    void Update()
    {
        if (movePoints.Length == 0) return;

        MoveTowardsPoint(movePoints[currentPointIndex]);

        if (Vector3.Distance(transform.position, movePoints[currentPointIndex]) < 0.1f)
        {
            currentPointIndex++;
            if (currentPointIndex >= movePoints.Length)
            {
                currentPointIndex = 0;
            }
        }
    }

    private void MoveTowardsPoint(Vector3 point)
    {
        transform.position = Vector3.MoveTowards(transform.position, point, speed * Time.deltaTime);
    }
	
	private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die();
            }
        }
    }
}

