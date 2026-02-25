using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform gunTip;
    public LineRenderer lr;
    
    public LayerMask whatIsGrappleable;
    public PlayerMovement pm;

    [Header("Grapple Settings")]
    public float maxGrappleDistance = 30f;
    public float pullForce = 20f;
    public float swingForce = 6f;
    public float grapplingCd = 2f;
    public KeyCode grappleKey = KeyCode.Mouse1;

    [Header("Rope Launch")]
    public float ropeSpeed = 40f;
    public int ropeSegments = 16;

    [Header("Rope Physics")]
    public float ropeSag = 0.08f;
    public float ropeWaveFreq = 6f;
    public float ropeWaveAmp = 0.15f;

    [Header("Swing Physics")]
    public float swingPullSpeed = 5f;
    public float maxRopeLength = 25f;
    public float minRopeLength = 3f;

    [Header("Materials")]
    public Material materialFlying;
    public Material materialAttached;

    private float grapplingCdTimer;
    private Vector3 grapplePoint;
    private Vector3 grappleTargetPoint;
    private bool grappling;
    private bool attached;
    private bool missed;

    private float ropeProgress;
    private float ropeLength;
    private float currentRopeLength;
    private float waveTime;

    private bool forceStoped;

    private void Start()
    {
        lr.positionCount = ropeSegments;
        lr.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey))
        {
            forceStoped = false;
            StartGrapple();
        }
        if (Input.GetKeyUp(grappleKey))
        {
            forceStoped = false;
            StopGrapple();
        }

        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;

        // Если принудительно остановлено — ничего не делаем
        if (forceStoped) return;

        if (grappling && !attached && !missed)
        {
            ropeProgress += (ropeSpeed / ropeLength) * Time.deltaTime;
            waveTime += Time.deltaTime;

            if (ropeProgress >= 1f)
            {
                ropeProgress = 1f;
                OnRopeAttached();
            }
        }

        if (grappling && missed)
        {
            ropeProgress += (ropeSpeed / ropeLength) * Time.deltaTime;
            waveTime += Time.deltaTime;
            if (ropeProgress >= 1f)
            {
                grappling = false;
                lr.enabled = false;
            }
        }

        if (attached)
            HandleRopeLength();
    }

    private void LateUpdate()
    {
        if (!grappling || !lr.enabled) return;
        DrawRope();
    }

    private void StartGrapple()
    {
        lr.gameObject.SetActive(true);
        if (grapplingCdTimer > 0) return;
        if (forceStoped) return;

        RaycastHit hit;
        bool hasHit = Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable);

        if (hasHit)
        {
            grappleTargetPoint = hit.point;
            missed = false;
        }
        else
        {
            grappleTargetPoint = cam.position + cam.forward * maxGrappleDistance;
            missed = true;
        }

        ropeLength = Vector3.Distance(gunTip.position, grappleTargetPoint);
        ropeProgress = 0f;
        waveTime = 0f;
        attached = false;
        grappling = true;

        lr.positionCount = ropeSegments;
        lr.enabled = true;
        lr.material = materialFlying;
        lr.enabled = true;
    }

    private void OnRopeAttached()
    {
        attached = true;
        grapplePoint = grappleTargetPoint;
        lr.material = materialAttached;

        currentRopeLength = Vector3.Distance(transform.position, grapplePoint);
        currentRopeLength = Mathf.Clamp(currentRopeLength, minRopeLength, maxRopeLength);

        pm.StartSwing(grapplePoint, pullForce, swingForce);
    }

    private void HandleRopeLength()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            currentRopeLength -= swingPullSpeed * Time.deltaTime * 10f;
            currentRopeLength = Mathf.Max(currentRopeLength, minRopeLength);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            currentRopeLength += swingPullSpeed * Time.deltaTime * 10f;
            currentRopeLength = Mathf.Min(currentRopeLength, maxRopeLength);
        }
    }

    private void StopGrapple()
    {
        grappling = false;
        attached = false;
        missed = false;
        lr.enabled = false;
        lr.positionCount = 0;
        grapplingCdTimer = grapplingCd;
        lr.gameObject.SetActive(false);

        pm.StopSwing();
    }

    private void DrawRope()
    {
        Vector3 start = gunTip.position;
        Vector3 end = attached
            ? grapplePoint
            : Vector3.Lerp(start, grappleTargetPoint, ropeProgress);

        for (int i = 0; i < ropeSegments; i++)
        {
            float t = i / (float)(ropeSegments - 1);
            Vector3 point = Vector3.Lerp(start, end, t);

            float offsetY;
            if (attached)
            {
                float dynamicSag = ropeSag;
                offsetY = -dynamicSag * Mathf.Sin(t * Mathf.PI);

                Vector3 ropeDir = (end - start).normalized;
                Vector3 sideAxis = Vector3.Cross(ropeDir, Vector3.up).normalized;
                float sideWave = Mathf.Sin(waveTime * ropeWaveFreq * 0.5f + t * Mathf.PI)
                                   * ropeWaveAmp * 0.3f;
                point += sideAxis * sideWave;
                waveTime += Time.deltaTime * 0.01f;
            }
            else
            {
                offsetY = ropeWaveAmp
                          * Mathf.Sin(t * Mathf.PI * 2f + waveTime * ropeWaveFreq)
                          * ropeProgress;
            }

            point += Vector3.down * offsetY * ropeProgress;
            lr.SetPosition(i, point);
        }
    }

    public void StopGrappleExternal()
    {
        forceStoped = true;
        grappling = false;
        attached = false;
        missed = false;
        lr.enabled = false;
        lr.positionCount = 0;
        lr.gameObject.SetActive(false);
        // НЕ вызываем pm.StopSwing() — PlayerMovement сам вызвал StopSwing
    }
}