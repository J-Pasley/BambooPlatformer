using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
	public float dashDistance = 2f; // The distance the player will dash
	public float dashCooldown = 1f; // Cooldown time for dashing
	private bool canDash = true; // Whether the player can currently dash
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;
	private int flowerCount = 0;
	public TextMeshProUGUI flowerCounterText;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool isFacingRight = true; // Track which way the player is facing

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        groundLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        // Flip the player's sprite if moving left or right
        if (horizontalInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && isFacingRight)
        {
            Flip();
        }

        animator.SetBool("IsRunning", horizontalInput != 0);

        if (isGrounded && Input.GetKeyDown(KeyCode.UpArrow))
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            animator.SetBool("IsJumping", true);
            isGrounded = false;
        }
        else if (!isGrounded)
        {
            animator.SetBool("IsJumping", false);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Dash();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
		
		// Set the IsIdling parameter based on the horizontal input and grounded status
        animator.SetBool("IsIdling", horizontalInput == 0 && isGrounded);

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            GrowNearestBamboo();
        }
    }

    void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.BoxCast(groundCheck.position, groundCheckSize, 0f, Vector2.down, 0f, groundLayer);
        isGrounded = hit.collider != null;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

	void Dash()
	{
		if (canDash)
		{
			StartCoroutine(DelayedDash());
		}
	}

	IEnumerator DelayedDash()
	{
		// Trigger the dash animation
		animator.SetTrigger("DashTrigger");

		// Wait for 1 second
		yield return new WaitForSeconds(1f);

		// Perform the dash teleportation
		float dashAmount = isFacingRight ? dashDistance : -dashDistance;
		rb.MovePosition(new Vector2(transform.position.x + dashAmount, transform.position.y));

		// Start the dash cooldown
		StartCoroutine(DashCooldown());
	}

	IEnumerator DashCooldown()
	{
		canDash = false;
		yield return new WaitForSeconds(dashCooldown);
		canDash = true;
	}

    void RestartGame()
    {
        // Reloads the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GrowNearestBamboo()
	{
		GameObject standingOnBamboo = GetBambooPlayerIsStandingOn();

		// Find all bamboo objects
		GameObject[] bamboos = GameObject.FindGameObjectsWithTag("Bamboo");

		// Find the closest bamboo that is not the one the player is standing on
		GameObject closestBamboo = null;
		float closestDistance = float.MaxValue;

		foreach (var bamboo in bamboos)
		{
			if (bamboo != standingOnBamboo)
			{
				float distance = (bamboo.transform.position - transform.position).sqrMagnitude;
				if (distance < closestDistance)
				{
					closestBamboo = bamboo;
					closestDistance = distance;
				}
			}
		}

		// Grow the closest bamboo
		if (closestBamboo != null)
		{
			bool didGrow = closestBamboo.GetComponent<Bamboo>().Grow();
		}
	}


    private GameObject GetBambooPlayerIsStandingOn()
    {
        RaycastHit2D hit = Physics2D.BoxCast(groundCheck.position, groundCheckSize, 0f, Vector2.down, groundCheckSize.y, groundLayer);
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Bamboo"))
        {
            return hit.collider.gameObject;
        }
        return null;
    }
	
	public void Die()
    {
        // Play death animation
        animator.SetTrigger("Die");

        // Disable player controls
        this.enabled = false;

        rb.isKinematic = true;
        rb.velocity = Vector2.zero;

        // Wait for the death animation to finish before loading the game over scene
        StartCoroutine(LoadGameOverAfterDelay());
    }

    private IEnumerator LoadGameOverAfterDelay()
    {
        yield return new WaitForSeconds(2f); 

        // Load the game over scene
        SceneManager.LoadScene("GameOver"); 
    }
	
	private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("DeadlyPlatform"))
        {
            Die();
        }
    }
	
	private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PinkFlower"))
        {
            CollectFlower(other.gameObject);
        }
		
		if (other.gameObject.CompareTag("GoldenBamboo"))
        {
            WinGame();
        }
    }
	
	private void CollectFlower(GameObject flower)
    {
        flowerCount++;
        UpdateFlowerCounterUI();
        Destroy(flower); // Removes the flower from the scene
    }
	
	private void UpdateFlowerCounterUI()
    {
        if (flowerCounterText != null)
        {
            flowerCounterText.text = "Flowers: " + flowerCount;
        }
    }

    private void WinGame()
    {
        // Load the Success scene
        SceneManager.LoadScene("Success"); 
    }
}
