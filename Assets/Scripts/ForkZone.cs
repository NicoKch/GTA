using UnityEngine;

public class ForkZone : MonoBehaviour
{
    private Pallet parentPallet;
    private bool hasFork = false;
    private Collider2D forkInZone;

    public bool HasFork => hasFork;
    public Collider2D CurrentFork => forkInZone;

    public void Initialize(Pallet pallet)
    {
        parentPallet = pallet;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[ForkZone] {gameObject.name} - Trigger ENTER avec '{other.name}', tag='{other.tag}'");

        if (other.CompareTag("Fork"))
        {
            hasFork = true;
            forkInZone = other;
            Debug.Log($"[ForkZone] Fourche insérée dans {gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[ForkZone] {gameObject.name} - Trigger EXIT avec '{other.name}'");

        if (other == forkInZone)
        {
            hasFork = false;
            forkInZone = null;
            Debug.Log($"[ForkZone] Fourche retirée de {gameObject.name}");
        }
    }
}