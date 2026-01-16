using UnityEngine;

public class Pallet : MonoBehaviour
{
    [Header("Zones de fourches")] [SerializeField]
    private ForkZone forkZoneLeft;

    [SerializeField] private ForkZone forkZoneRight;

    [Header("Configuration")] [SerializeField]
    private float weight = 100f;

    private Rigidbody2D rb;
    private Transform originalParent;
    private bool isAttached = false;

    private ForkController registeredForkController;
    private bool wasReady = false;

    public bool IsAttached => isAttached;
    public float Weight => weight;

    // Vérifie si les deux fourches sont bien insérées
    public bool AreBothForksInserted => forkZoneLeft != null && forkZoneRight != null
                                                             && forkZoneLeft.HasFork && forkZoneRight.HasFork;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalParent = transform.parent;

        // Auto-détection des zones si pas assignées
        if (forkZoneLeft == null || forkZoneRight == null)
        {
            ForkZone[] zones = GetComponentsInChildren<ForkZone>();
            foreach (var zone in zones)
            {
                if (zone.name.Contains("Gauche") || zone.name.Contains("Left") || zone.name.Contains("G"))
                    forkZoneLeft = zone;
                else if (zone.name.Contains("Droit") || zone.name.Contains("Right") || zone.name.Contains("D"))
                    forkZoneRight = zone;
            }
        }
    }

    private void Start()
    {
        // Lie les zones à cette palette
        if (forkZoneLeft != null) forkZoneLeft.Initialize(this);
        if (forkZoneRight != null) forkZoneRight.Initialize(this);
    }

    private void Update()
    {
        if (isAttached) return;

        bool isReady = AreBothForksInserted;

        // État a changé ?
        if (isReady != wasReady)
        {
            if (isReady)
            {
                // Les deux fourches sont maintenant insérées
                ForkController forkController = FindForkController();
                if (forkController != null)
                {
                    registeredForkController = forkController;
                    forkController.RegisterPalletInRange(this);
                }
            }
            else
            {
                // Une fourche a été retirée
                if (registeredForkController != null)
                {
                    registeredForkController.UnregisterPalletInRange(this);
                    registeredForkController = null;
                }
            }

            wasReady = isReady;
        }
    }

    private ForkController FindForkController()
    {
        // Cherche le ForkController via les fourches détectées
        if (forkZoneLeft.CurrentFork != null)
        {
            ForkController fc = forkZoneLeft.CurrentFork.GetComponentInParent<ForkController>();
            if (fc != null) return fc;
        }

        if (forkZoneRight.CurrentFork != null)
        {
            ForkController fc = forkZoneRight.CurrentFork.GetComponentInParent<ForkController>();
            if (fc != null) return fc;
        }

        return null;
    }

    public void AttachToForks(Transform attachPoint)
    {
        if (isAttached) return;

        isAttached = true;

        // Désactive COMPLÈTEMENT la physique
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false; // AJOUTE CETTE LIGNE - désactive totalement le Rigidbody
        }

        // Désactive les colliders pour éviter les conflits
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false; // AJOUTE CETTE BOUCLE
        }

        transform.SetParent(attachPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Debug.Log("[Pallet] Attachée aux fourches");
    }

    public void DetachFromForks()
    {
        if (!isAttached) return;

        isAttached = false;
        transform.SetParent(originalParent);

        // Réactive les colliders
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }

        // Réactive la physique
        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        registeredForkController = null;
        wasReady = false;

        Debug.Log("[Pallet] Déposée");
    }
}