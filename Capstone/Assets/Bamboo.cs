using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bamboo : MonoBehaviour
{
    private int growthsRemaining = 3; // Maximum growths

    public bool Grow()
    {
        if (growthsRemaining > 0)
        {
            transform.position += new Vector3(0, 1, 0); // Move up by 1 on the Y axis
            growthsRemaining--;
            return true; // Growth was successful
        }
        return false; // No more growths remaining
    }
}

