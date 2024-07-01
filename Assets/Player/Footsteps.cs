using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    [SerializeField] RigidBodyCharacterController cc;
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip[] RunningClips;
    [SerializeField] AudioClip[] WalkingClips;
    [SerializeField] AudioClip[] LandingClips;
    [SerializeField] Vector2 RandomPauseRangeRunning = new Vector2(0.8f, 0);
    [SerializeField] Vector2 RandomPauseRangeWalking = new Vector2(0.7f,0.1f);
    [SerializeField] float pauseLength = .35f;

    [SerializeField] float runThreshold = 6f;

    float m_currentTime = 0;
    float m_pauseLength = 0;
    bool m_groundedLastFrame = true;

    // Start is called before the first frame update
    void Start()
    {
        source.playOnAwake = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_currentTime += Time.fixedDeltaTime;

        float playerSpeed = new Vector2(cc.m_PlayerVelocity.x, cc.m_PlayerVelocity.z).magnitude;

        //If No Step Is Playing
        if (!source.isPlaying && m_currentTime >= m_pauseLength && playerSpeed > 1f && cc.m_Character.isGrounded)
        {
            m_currentTime = 0;
            if (playerSpeed > runThreshold)
            {
                //Is Running
                int randomClip = Random.Range(0, RunningClips.Length);
                m_pauseLength = pauseLength + Mathf.Lerp(RandomPauseRangeRunning.x, RandomPauseRangeRunning.y, playerSpeed / 8.0f);
                source.clip = RunningClips[randomClip];
                m_currentTime = 0;
                source.Play();
            }
            else
            {
                //Is Walking
                int randomClip = Random.Range(0, WalkingClips.Length);
                m_pauseLength = pauseLength + Mathf.Lerp(RandomPauseRangeWalking.x, RandomPauseRangeWalking.y, playerSpeed / 8.0f);
                source.clip = WalkingClips[randomClip];
                m_currentTime = 0;
                source.Play();
            }

        }//else
        //Play Landing Sound
        //if (m_groundedLastFrame != cc.m_Character.isGrounded && m_currentTime >= m_pauseLength && cc.m_Character.isGrounded)
        //{
        //    //Just Landed
        //    int randomClip = Random.Range(0, LandingClips.Length);
        //    m_pauseLength = pauseLength + Mathf.Lerp(RandomPauseRangeWalking.x, RandomPauseRangeWalking.y, playerSpeed / 8.0f);
        //    source.clip = LandingClips[randomClip];
        //    m_currentTime = 0;
        //    source.Play();
        //}

        //m_groundedLastFrame = cc.m_Character.isGrounded;
    }
}
