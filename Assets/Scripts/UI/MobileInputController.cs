using Gameplay.Player;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;

public class MobileInputController : MonoBehaviour
{
    [Header("Boutons de déplacement")] [SerializeField]
    private GameObject btnForward;

    [SerializeField] private GameObject btnBackward;
    [SerializeField] private GameObject btnLeft;
    [SerializeField] private GameObject btnRight;

    [Header("Boutons fourches")] [SerializeField]
    private GameObject btnLiftUp;

    [SerializeField] private GameObject btnLiftDown;

    [Header("Bouton vue")] [SerializeField]
    private GameObject btnSwitchView;

    [Header("Bouton klaxon")] [SerializeField]
    private GameObject btnHorn;

    private bool fwdHeld, bwdHeld, leftHeld, rightHeld;
    private bool liftUpHeld, liftDownHeld;

    private HornController hornController;

    private void Start()
    {
        // Désactive sur PC standalone (mais actif dans l'éditeur pour tester)
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR
        gameObject.SetActive(false);
        return;
#endif

        // Déplacement
        SetupButton(btnForward, () => fwdHeld = true, () => fwdHeld = false);
        SetupButton(btnBackward, () => bwdHeld = true, () => bwdHeld = false);
        SetupButton(btnLeft, () => leftHeld = true, () => leftHeld = false);
        SetupButton(btnRight, () => rightHeld = true, () => rightHeld = false);

        // Fourches
        SetupButton(btnLiftUp, () => liftUpHeld = true, () => liftUpHeld = false);
        SetupButton(btnLiftDown, () => liftDownHeld = true, () => liftDownHeld = false);

        // Vue : appel direct au VisionManager sur press
        SetupButton(btnSwitchView,
            onPress: () => VisionManager.Instance?.SwitchView(),
            onRelease: () => { });

        // Klaxon : impulsion sur press
        hornController = FindFirstObjectByType<HornController>();
        SetupButton(btnHorn,
            onPress: () => hornController?.TriggerHorn(),
            onRelease: () => { });
    }

    private void Update()
    {
        if (InputManager.Instance == null) return;

        // Move : avant = +1, arrière = -1
        float moveVal = 0f;
        if (fwdHeld) moveVal += 1f;
        if (bwdHeld) moveVal -= 1f;
        // Si aucun bouton pressé, retire l'override → le clavier reprend la main
        InputManager.Instance.SetMoveOverride(moveVal != 0f ? moveVal : null);

        // Rotate : droite = +1, gauche = -1
        float rotVal = 0f;
        if (rightHeld) rotVal += 1f;
        if (leftHeld) rotVal -= 1f;
        InputManager.Instance.SetRotateOverride(rotVal != 0f ? rotVal : null);

        // Lift : monter = +1, descendre = -1
        float liftVal = 0f;
        if (liftUpHeld) liftVal += 1f;
        if (liftDownHeld) liftVal -= 1f;
        InputManager.Instance.SetLiftOverride(liftVal != 0f ? liftVal : null);
    }

    private void OnDisable()
    {
        // Réinitialise les états quand le HUD se cache (pause, game over...)
        fwdHeld = bwdHeld = leftHeld = rightHeld = false;
        liftUpHeld = liftDownHeld = false;

        if (InputManager.Instance == null) return;
        InputManager.Instance.SetMoveOverride(null);
        InputManager.Instance.SetRotateOverride(null);
        InputManager.Instance.SetLiftOverride(null);
    }

    private void OnDestroy()
    {
        // Nettoyage si l'objet est détruit pendant le jeu
        if (InputManager.Instance == null) return;
        InputManager.Instance.SetMoveOverride(null);
        InputManager.Instance.SetRotateOverride(null);
        InputManager.Instance.SetLiftOverride(null);
    }

    /// <summary>
    /// Attache un EventTrigger PointerDown/PointerUp sur un GameObject UI.
    /// Crée automatiquement un EventTrigger si le GameObject n'en a pas.
    /// </summary>
    private void SetupButton(GameObject btn, System.Action onPress, System.Action onRelease)
    {
        if (btn == null)
        {
            Debug.LogWarning("[MobileInputController] Un bouton n'est pas assigné dans l'Inspector.");
            return;
        }

        EventTrigger trigger = btn.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = btn.AddComponent<EventTrigger>();

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener(_ => onPress?.Invoke());
        trigger.triggers.Add(down);

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener(_ => onRelease?.Invoke());
        trigger.triggers.Add(up);
    }
}