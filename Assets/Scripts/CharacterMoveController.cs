using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMoveController : MonoBehaviour
{
    
    [Header("Movement")]
    public float moveAccel;
    public float maxSpeed;

    [Header("Jump")]
    public float jumpAccel;

    private bool isJumping;
    private bool isOnGround;

    
    [Header("Ground Raycast")]
    public float groundRaycastDistance;
    public LayerMask groundLayerMask;  

    [Header("Scoring")]
    public ScoreController score;
    public float scoringRatio;
    private float lastPositionX;

    [Header("GameOver")]
    public GameObject gameOverScreen;
    public float fallPositionY;

    [Header("Camera")]
    public CameraMoveController gameCamera;    

    private Rigidbody2D rig;

    public RigidbodyConstraints2D PlayerCon;

    private Animator anim;

    private CharacterSoundController sound;

    public AudioClip fall;
    public AudioClip diamondSound;
    AudioSource audioSource;

    public GameObject Player;

    public ScoreController currentScore;
    public int diamond = 0;
    public Text diamondAmount;

    private void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sound = GetComponent<CharacterSoundController>();
        audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        // raycast ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayerMask);
        if (hit)
        {
            if (!isOnGround && rig.velocity.y <= 0)
            {
                isOnGround = true;
            }
        }
        else
        {
            isOnGround = false;
        }

        // calculate velocity vector
        Vector2 velocityVector = rig.velocity;

        if (isJumping)
        {
            velocityVector.y += jumpAccel;
            isJumping = false;
        }

        velocityVector.x = Mathf.Clamp(velocityVector.x + moveAccel * Time.deltaTime, 0.0f, maxSpeed);

        rig.velocity = velocityVector;
    }
    // Tambahkan OnDrawGizmos
    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + (Vector3.down * groundRaycastDistance), Color.white);
    }

    private void Update()
    {
        diamondAmount.text = diamond.ToString();

        // baca input
        if (Input.GetMouseButtonDown(0))
        {
            if (isOnGround)
            {
                isJumping = true;

                sound.PlayJump();
            }
        }


        // rubah animasi
        anim.SetBool("isOnGround", isOnGround);
        // hitung skor
        int distancePassed = Mathf.FloorToInt(transform.position.x - lastPositionX);
        int scoreIncrement = Mathf.FloorToInt(distancePassed / scoringRatio);

        if (scoreIncrement > 0)
        {
            score.IncreaseCurrentScore(scoreIncrement);
            lastPositionX += distancePassed;
        }
        // Jika game over
        if (transform.position.y < fallPositionY)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        // Play musik saat jatuh
        audioSource.PlayOneShot(fall, 0.7f);

        // set high score
        score.FinishScoring();

        // stop camera movement
        gameCamera.enabled = false;

        // show gameover
        gameOverScreen.SetActive(true);

        // disable this too
        this.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D target)
    {
        if (target.tag == "spike")
        {
            anim.enabled = false;
            rig.constraints = RigidbodyConstraints2D.FreezeAll;
            Vector2 velocityVector = rig.velocity;
            velocityVector.y = 0;
            velocityVector.x = 0;
            rig.velocity = velocityVector;
            isOnGround = false;

            audioSource.PlayOneShot(fall, 0.7f);
            GameOver();
        }

        if (target.tag == "dia1")
        {
            Destroy(target.gameObject);
            diamond += 1;
            audioSource.PlayOneShot(diamondSound, 0.7f);
        }
    }
}
