using System.Collections;
using Gameplay.NPC;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Player
{
    /// <summary>
    /// HornController : Gère le klaxon du chariot joueur.
    /// - Touche H (clavier) ou bouton mobile
    /// - Portée configurable : arrête les NPC dans le rayon pendant npcStopDuration secondes
    /// - Anti-spam : max maxUsesBeforeCooldown klaxons sur usageWindow secondes,
    ///   puis cooldownDuration secondes de blocage
    /// - Effet visuel : anneau expansif via LineRenderer (aucun asset requis)
    /// </summary>
    public class HornController : MonoBehaviour
    {
        #region Configuration

        [Header("Portée")]
        [SerializeField] private float hornRange = 8f;
        [SerializeField] private float npcStopDuration = 3f;

        [Header("Anti-spam")]
        [SerializeField] private int maxUsesBeforeCooldown = 3;
        [SerializeField] private float usageWindow = 10f;
        [SerializeField] private float cooldownDuration = 10f;

        [Header("Effet visuel")]
        [SerializeField] private Material ringMaterial;
        [SerializeField] private Color ringColor = new Color(1f, 0.8f, 0f, 0.8f);
        [SerializeField] private float ringExpandDuration = 0.6f;
        [SerializeField] private int ringSegments = 48;

        #endregion

        #region State

        private float[] usageTimestamps;
        private int usageCount = 0;
        private float cooldownEndTime = -1f;

        // Impulsion mobile — consommée à chaque Update
        private bool mobileHornTriggered = false;

        public bool IsOnCooldown => Time.time < cooldownEndTime;
        public float CooldownRemaining => Mathf.Max(0f, cooldownEndTime - Time.time);

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            usageTimestamps = new float[maxUsesBeforeCooldown];
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                return;

            bool hornPressed = mobileHornTriggered;
            mobileHornTriggered = false;

            // Clavier : touche H
            if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
                hornPressed = true;

            if (hornPressed)
                TryHonk();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Appelé par MobileInputController lors d'un appui sur le bouton klaxon.
        /// </summary>
        public void TriggerHorn()
        {
            mobileHornTriggered = true;
        }

        #endregion

        #region Horn Logic

        private void TryHonk()
        {
            if (IsOnCooldown)
            {
                AudioManager.Instance?.PlaySFX("horn_blocked");
                Debug.Log($"[HornController] Klaxon bloqué — cooldown restant : {CooldownRemaining:F1}s");
                return;
            }

            PruneOldUsages();

            if (usageCount >= maxUsesBeforeCooldown)
            {
                cooldownEndTime = Time.time + cooldownDuration;
                AudioManager.Instance?.PlaySFX("horn_blocked");
                Debug.Log($"[HornController] Spam détecté — cooldown de {cooldownDuration}s activé");
                return;
            }

            RecordUsage();
            ExecuteHorn();
        }

        private void ExecuteHorn()
        {
            AudioManager.Instance?.PlaySFX("horn");
            SpawnRingEffect();
            StopNearbyNPCs();

            Debug.Log($"[HornController] Klaxon ! ({usageCount}/{maxUsesBeforeCooldown} utilisations dans la fenêtre)");
        }

        private void StopNearbyNPCs()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hornRange);
            foreach (var hit in hits)
            {
                if (hit == null) continue;
                NPCForkliftController npc = hit.GetComponentInParent<NPCForkliftController>();
                if (npc != null)
                    npc.HonkReaction(npcStopDuration);
            }
        }

        #endregion

        #region Anti-Spam

        private void PruneOldUsages()
        {
            float cutoff = Time.time - usageWindow;
            int valid = 0;
            for (int i = 0; i < usageCount; i++)
            {
                if (usageTimestamps[i] > cutoff)
                    usageTimestamps[valid++] = usageTimestamps[i];
            }

            usageCount = valid;
        }

        private void RecordUsage()
        {
            if (usageCount < usageTimestamps.Length)
                usageTimestamps[usageCount++] = Time.time;
        }

        #endregion

        #region Visual Effect

        private void SpawnRingEffect()
        {
            GameObject ring = new GameObject("HornRing");
            ring.transform.position = transform.position;

            LineRenderer lr = ring.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = ringSegments;
            lr.startWidth = 0.12f;
            lr.endWidth = 0.12f;
            lr.sortingOrder = 100;
            lr.material = ringMaterial != null
                ? ringMaterial
                : new Material(Shader.Find("Sprites/Default"));
            lr.startColor = ringColor;
            lr.endColor = ringColor;

            // Cercle unitaire — mis à l'échelle par la coroutine
            for (int i = 0; i < ringSegments; i++)
            {
                float a = (float)i / ringSegments * 2f * Mathf.PI;
                lr.SetPosition(i, new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f));
            }

            StartCoroutine(AnimateRing(ring, lr));
        }

        private IEnumerator AnimateRing(GameObject ring, LineRenderer lr)
        {
            float elapsed = 0f;
            while (elapsed < ringExpandDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / ringExpandDuration;

                ring.transform.localScale = Vector3.one * Mathf.Lerp(0.1f, hornRange, t);

                Color c = ringColor;
                c.a = Mathf.Lerp(ringColor.a, 0f, t);
                lr.startColor = c;
                lr.endColor = c;

                yield return null;
            }

            Destroy(ring);
        }

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.8f, 0f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, hornRange);
        }

        #endregion
    }
}
