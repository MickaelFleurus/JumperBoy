using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "JumpSettings", menuName = "ScriptableObjects/JumpSettings")]
public class JumpHandler : ScriptableObject
{
    private bool isJumping = false;

    [SerializeField] private float jumpPower = 5.5f;
    [SerializeField] private float maxJumpTime = 1f;
    [SerializeField] private int jumpAmount = 3;
    [SerializeField] private float jumpCooldown = 1.5f;


    private float jumpTimeLeft = 0f;
    private int jumpLeft = 0;
    private float cooldownLeft = 0f;

    // ===== DEBUG PANEL ACCESSORS (Added for visualization) =====
    public bool IsJumping => isJumping;
    public float JumpPower => jumpPower;
    public float MaxJumpTime => maxJumpTime;
    public int JumpAmount => jumpAmount;
    public float JumpCooldown => jumpCooldown;
    public int JumpLeft => jumpLeft;

    public void SetJumpPower(float value) { jumpPower = value; Save(); }
    public void SetMaxJumpTime(float value) { maxJumpTime = value; Save(); }
    public void SetJumpAmount(int value) { jumpAmount = value; Save(); }
    public void SetJumpCooldown(float value) { jumpCooldown = value; Save(); }
    // ===== END DEBUG PANEL ACCESSORS =====

    private bool canJump = true;

    public JumpHandler()
    {
    }

    void OnEnable()
    {
        Load();
        jumpLeft = jumpAmount;
    }

    private void Load()
    {
        // Try Resources folder first
        TextAsset jsonFile = Resources.Load<TextAsset>("JumpSettings");
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

    public void Save()
    {
        string json = JsonUtility.ToJson(this, true);
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: use PlayerPrefs (LocalStorage) - File I/O can cause "Permissions check failed" in iframes
        PlayerPrefs.SetString(WebGLPrefsKey, json);
        PlayerPrefs.Save();
#else
        // Standalone/Editor: save to JSON file
        string savePath = Path.Combine(Application.persistentDataPath, "JumpSettings.json");
        File.WriteAllText(savePath, json);
#endif
    }

    public void Updated(float elapsed)
    {

        if (jumpTimeLeft > 0f)
        {
            jumpTimeLeft -= elapsed;
            if (jumpTimeLeft <= 0f)
            {
                isJumping = false;
                cooldownLeft = jumpCooldown;
            }
        }
        else if (cooldownLeft > 0f)
        {
            cooldownLeft -= elapsed;
            if (cooldownLeft <= 0f)
            {
                canJump = true;
            }
        }
    }

    public bool TryJump()
    {
        if (canJump && jumpLeft - 1 >= 0)
        {
            jumpLeft--;
            isJumping = true;
            jumpTimeLeft = maxJumpTime;
            return true;
        }
        return false;
    }
    public void OnJumpStop()
    {
        OnJumpReleased();
    }

    public void OnJumpReleased()
    {
        isJumping = false;
        cooldownLeft = jumpCooldown;
    }

    public void OnJumpReset()
    {
        isJumping = false;
        canJump = true;
        jumpLeft = jumpAmount;
    }

}
