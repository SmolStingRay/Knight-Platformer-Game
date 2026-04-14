using UnityEngine;

[DisallowMultipleComponent]
public class CameraFollow2D : MonoBehaviour
{
    [System.Serializable]
    private struct CameraProfile
    {
        public Vector3 offset;
        public float orthographicSize;
        public float smoothTime;
    }

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";

    [Header("Follow")]
    [SerializeField] private Vector3 offset = new(0f, 1.5f, -10f);
    [SerializeField] private float smoothTime = 0.18f;
    [SerializeField] private bool followOnStart = true;

    [Header("Profiles")]
    [SerializeField] private string townSceneName = "HealthSystemTest";
    [SerializeField] private CameraProfile townProfile = new CameraProfile
    {
        offset = new Vector3(0f, 0f, -10f),
        orthographicSize = 10f,
        smoothTime = 0.12f
    };
    [SerializeField] private CameraProfile combatProfile = new CameraProfile
    {
        offset = new Vector3(0f, 1.75f, -10f),
        orthographicSize = 7f,
        smoothTime = 0.18f
    };

    private Vector3 velocity;
    private bool isFollowing;
    private Camera cachedCamera;

    private void Awake()
    {
        cachedCamera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        isFollowing = followOnStart;
        velocity = Vector3.zero;
        ApplyProfileForCurrentScene(true);
    }

    private void LateUpdate()
    {
        if (!isFollowing)
        {
            return;
        }

        EnsureTarget();
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, Mathf.Max(0.01f, smoothTime));
    }

    public void SetTarget(Transform newTarget, bool snapImmediately = false)
    {
        target = newTarget;

        if (snapImmediately && target != null)
        {
            velocity = Vector3.zero;
            transform.position = target.position + offset;
        }
    }

    public void SetFollowing(bool shouldFollow)
    {
        isFollowing = shouldFollow;

        if (!isFollowing)
        {
            velocity = Vector3.zero;
        }
    }

    private void EnsureTarget()
    {
        if (target != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(targetTag);
        if (player != null)
        {
            target = player.transform;
            return;
        }

        PlayerMovementModeSwitcher modeSwitcher = FindFirstObjectByType<PlayerMovementModeSwitcher>();
        if (modeSwitcher != null)
        {
            target = modeSwitcher.transform;
            return;
        }

        TopDownMovement topDownMovement = FindFirstObjectByType<TopDownMovement>();
        if (topDownMovement != null)
        {
            target = topDownMovement.transform;
            return;
        }

        PlatformerMovement2D platformerMovement = FindFirstObjectByType<PlatformerMovement2D>();
        if (platformerMovement != null)
        {
            target = platformerMovement.transform;
        }
    }

    public void ApplyProfileForCurrentScene(bool snapToTarget = false)
    {
        CameraProfile profile = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == townSceneName
            ? townProfile
            : combatProfile;

        offset = profile.offset;
        smoothTime = Mathf.Max(0.01f, profile.smoothTime);

        if (cachedCamera != null && cachedCamera.orthographic)
        {
            cachedCamera.orthographicSize = Mathf.Max(0.1f, profile.orthographicSize);
        }

        if (snapToTarget && target != null)
        {
            velocity = Vector3.zero;
            transform.position = target.position + offset;
        }
    }
}
