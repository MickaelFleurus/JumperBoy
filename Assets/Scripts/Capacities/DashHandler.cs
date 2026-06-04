
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "DashHandler", menuName = "Scriptable Objects/DashHandler")]
public class DashHandler : ScriptableObject
{
    [SerializeField] private float dashPower = 15.5f;
    [SerializeField] private int dashAmount = 3;
    [SerializeField] private float dashDuration = 0.40f;
    [SerializeField] private float dashCooldown = 1.5f;
    private int dashLeft = 0;
    private float cooldownLeft = 0f;
    private float dashDurationLeft = 0f;
    private bool isDashing = false;


    // ===== DEBUG PANEL ACCESSORS (Added for visualization) =====
    public bool IsDashing => isDashing;
    public float DashPower => dashPower;
    public int DashAmount => dashAmount;
    public float DashDuration => dashDuration;
    public float DashCooldown => dashCooldown;

    public void SetDashPower(float value) => dashPower = value;
    public void SetDashAmount(int value) => dashAmount = value;
    public void SetDashDuration(float value) => dashDuration = value;
    public void SetDashCooldown(float value) => dashCooldown = value;
    // ===== END DEBUG PANEL ACCESSORS =====

    private bool canDash = true;

    public DashHandler()
    {
    }

    void OnEnable()
    {
        Load();
        dashLeft = dashAmount;
    }

    private void Load()
    {
        // Try Resources folder first
        TextAsset jsonFile = Resources.Load<TextAsset>("DashHandler");
        if (jsonFile != null)
        {
            JsonUtility.FromJsonOverwrite(jsonFile.text, this);
            return;
        }

        // Fallback to persistent data path
        string loadPath = Path.Combine(Application.persistentDataPath, "JumpSettings.json");
        if (File.Exists(loadPath))
        {
            string json = File.ReadAllText(loadPath);
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }

    public void Updated(float elapsed)
    {
        if (isDashing)
        {
            dashDurationLeft -= elapsed;
            if (dashDurationLeft <= 0f)
            {
                isDashing = false;
                cooldownLeft = dashCooldown;
            }
        }
        else if (cooldownLeft > 0f)
        {
            cooldownLeft -= elapsed;
            if (cooldownLeft <= 0f)
            {
                canDash = true;
            }
        }
    }

    public bool TryDash()
    {
        if (canDash && dashLeft - 1 >= 0)
        {
            dashLeft--;
            dashDurationLeft = dashDuration;
            isDashing = true;
            return true;
        }
        return false;
    }

    public void OnDashReset()
    {
        isDashing = false;
        canDash = true;
        dashLeft = dashAmount;
    }
}
