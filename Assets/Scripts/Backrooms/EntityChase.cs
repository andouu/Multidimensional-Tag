using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Backrooms
{
    public class EntityChase : MonoBehaviour
    {
        public Transform player;
        public GameObject jumpscareUI;
        public float spawnDelay = 5f;
        public float moveSpeed = 1.75f;
        public float killDistance = 0.5f;
        public float postJumpscareDelay = 2.5f;

        [Header("Bobbing Settings")]
        public float bobAmplitude = 0.2f;
        public float bobFrequency = 2f;
        private float baseY;

        [Header("Heartbeat Settings")]
        public AudioSource heartbeatSource;
        public float heartbeatRange = 5f;
        public float minHeartbeatPitch = 0.5f;
        public float maxHeartbeatPitch = 3f;
        public float minHeartbeatVolume = 0.1f;
        public float maxHeartbeatVolume = 0.8f;

        [Header("Jumpscare Settings")]
        public AudioSource jumpscareAudioSource; // Assign in Inspector

        private bool hasSpawned = false;
        private bool gameEnded = false;
        private bool heartbeatPlaying = false;

        void Start()
        {
            gameObject.SetActive(false);
            if (jumpscareUI != null) jumpscareUI.SetActive(false);
            Invoke(nameof(SpawnEntity), spawnDelay);
        }

        void SpawnEntity()
        {
            gameObject.SetActive(true);
            hasSpawned = true;
            baseY = transform.position.y;

            if (player != null)
            {
                transform.LookAt(player);
            }
        }

        void Update()
        {
            if (!hasSpawned || gameEnded || Time.timeScale == 0) return;

            // Horizontal distance only
            float distanceXZ = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(player.position.x, 0, player.position.z)
            );

            // Always chase
            Vector3 flatTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
            Vector3 direction = (flatTarget - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            // Bobbing
            float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = new Vector3(transform.position.x, baseY + bobOffset, transform.position.z);

            // Look at player horizontally
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDirection);

            // Trigger jumpscare
            if (distanceXZ <= killDistance)
            {
                TriggerJumpscare();
            }

            // Heartbeat logic
            if (distanceXZ <= heartbeatRange)
            {
                float proximity = Mathf.InverseLerp(heartbeatRange, 0f, distanceXZ); // Closer = higher value

                if (!heartbeatPlaying)
                {
                    heartbeatSource.volume = minHeartbeatVolume;
                    heartbeatSource.Play();
                    heartbeatPlaying = true;
                }

                heartbeatSource.pitch = minHeartbeatPitch + (maxHeartbeatPitch - minHeartbeatPitch) * proximity;
                heartbeatSource.volume = minHeartbeatVolume + (maxHeartbeatVolume - minHeartbeatVolume) * proximity;

            }
            else if (heartbeatPlaying)
            {
                heartbeatSource.Stop();
                heartbeatPlaying = false;
            }
        }

        void TriggerJumpscare()
        {
            if (heartbeatSource.isPlaying)
            {
                heartbeatSource.Stop();
                heartbeatSource.enabled = false;
                heartbeatPlaying = false;
            }

            if (jumpscareAudioSource != null && !jumpscareAudioSource.isPlaying)
            {
                jumpscareAudioSource.Play();
            }

            if (jumpscareUI != null)
            {
                jumpscareUI.SetActive(true);
            }

            Time.timeScale = 0f;
            Debug.Log("Jumpscare triggered!");
            StartCoroutine(RestartAfterDelay(postJumpscareDelay));
        }

        IEnumerator RestartAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            Time.timeScale = 1f;
            gameEnded = true;
            if (jumpscareUI != null) jumpscareUI.SetActive(false);
            Debug.Log("Game Over!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
