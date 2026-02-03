using UnityEngine;
using System.Collections.Generic;


public class VisionManager : MonoBehaviour
{
    public static VisionManager Instance { get; private set; }

    [Header("Configuration des cônes")] [SerializeField]
    private VisionCone frontCone;

    [SerializeField] private VisionCone rearCone;

    [Header("Paramètres")] [SerializeField]
    private float coneAngle = 80f;

    private float coneDistance = 15f;

    private int rayCount = 30;

    [Header("Layers")] [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask detectableLayer;


    private bool isFrontViewActive = true;
    private bool isLoadObstructingView = false;

    private HashSet<VisionTarget> visibleTargets = new HashSet<VisionTarget>();

    private PlayerInputAction inputActions;
    public event System.Action<bool> OnViewSwitched;
    public event System.Action<VisionTarget> OnTargetEnterView;
    public event System.Action<VisionTarget> OnTargetExitView;
    public event System.Action<bool> OnViewObstructed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputActions = InputManager.InputActions;
        InitializeCones();
        SetActiveView(isFrontViewActive);
    }

    // Update is called once per frame
    void Update()
    {
        float switchInput = inputActions.Player.switchView.ReadValue<float>();
        HandleInput(switchInput);
        UpdateVision();
    }

    private void InitializeCones()
    {
        if (frontCone != null)
        {
            frontCone.Initialize(coneAngle, coneDistance, rayCount, obstacleLayer, detectableLayer);
        }

        if (rearCone != null)
        {
            rearCone.Initialize(coneAngle, coneDistance, rayCount, obstacleLayer, detectableLayer);
        }
    }

    private void HandleInput(float switchInput)
    {
        if (switchInput > 0)
        {
            SwitchView();
        }
    }

    public void SwitchView()
    {
        isFrontViewActive = !isFrontViewActive;
        SetActiveView(isFrontViewActive);
        OnViewSwitched?.Invoke(isFrontViewActive);

        Debug.Log($"Vue basculée : {(isFrontViewActive ? "AVANT" : "ARRIÈRE")}");
    }

    private void SetActiveView(bool frontActive)
    {
        if (frontCone != null)
        {
            frontCone.SetActive(frontActive && !isLoadObstructingView);
        }

        if (rearCone != null)
        {
            rearCone.SetActive(!frontActive);
        }
    }

    public void SetLoadObstruction(bool isObstructed)
    {
        if (isLoadObstructingView != isObstructed)
        {
            isLoadObstructingView = isObstructed;
            OnViewObstructed?.Invoke(isObstructed);

            // Si la vue avant est active et qu'une charge obstrue, désactiver le cône avant
            if (isFrontViewActive && isObstructed)
            {
                frontCone?.SetActive(false);
                Debug.LogWarning("ATTENTION : Charge obstruant la vue frontale ! Utilisez la marche arrière.");
            }
            else if (isFrontViewActive && !isObstructed)
            {
                frontCone?.SetActive(true);
            }
        }
    }

    private void UpdateVision()
    {
        VisionCone activeCone = GetActiveCone();
        if (activeCone == null) return;

        HashSet<VisionTarget> currentlyVisible = activeCone.GetVisibleTargets();

        // Détecter les nouvelles cibles visibles
        foreach (var target in currentlyVisible)
        {
            if (!visibleTargets.Contains(target))
            {
                target.OnBecomeVisible();
                OnTargetEnterView?.Invoke(target);
            }
        }

        // Détecter les cibles qui ne sont plus visibles
        foreach (var target in visibleTargets)
        {
            if (!currentlyVisible.Contains(target))
            {
                target.OnBecomeHidden();
                OnTargetExitView?.Invoke(target);
            }
        }

        visibleTargets = currentlyVisible;
    }

    private VisionCone GetActiveCone()
    {
        if (isFrontViewActive && !isLoadObstructingView)
        {
            return frontCone;
        }

        return isFrontViewActive ? null : rearCone;
    }

    public bool IsFrontViewActive => isFrontViewActive;
    public bool IsViewObstructed => isLoadObstructingView;
    public HashSet<VisionTarget> GetVisibleTargets() => new HashSet<VisionTarget>(visibleTargets);
}