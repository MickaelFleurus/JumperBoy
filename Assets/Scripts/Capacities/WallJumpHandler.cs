using UnityEngine;
using System.IO;

[CreateAssetMenu(fileName = "WallJumpSettings", menuName = "Scriptable Objects/WallJumpSettings")]
public class WallJumpHandler : ScriptableObject
{

    [SerializeField] private Vector2 jumpPower = new Vector2(3.5f, 5.5f);

    [SerializeField] private float jumpExtensionDuration = 0.2f;
    [SerializeField] private float jumpMandatoryDuration = 0.5f;

    private float jumpTimeLeft = 0f;

    public bool canWallJump;
    private bool isWallJumping = false;
    private bool isExtending = false;

    // ===== DEBUG PANEL ACCESSORS (Added for visualization) =====
    public bool IsWallJumping => isWallJumping;
    public Vector2 JumpPower => jumpPower;
    public float JumpExtensionDuration => jumpExtensionDuration;
    public float JumpMandatoryDuration => jumpMandatoryDuration;

    public void SetJumpPower(Vector2 value) { jumpPower = value; Save(); }
    public void SetJumpExtensionDuration(float value) { jumpExtensionDuration = value; Save(); }
    public void SetJumpMandatoryDuration(float value) { jumpMandatoryDuration = value; Save(); }
    // ===== END DEBUG PANEL ACCESSORS ====

    void OnEnable()
    {
        Load();
    }

    public void Updated(float elapsed)
    {
        if (jumpTimeLeft <= 0f) return;

        jumpTimeLeft -= elapsed;
        if (jumpTimeLeft <= 0f)
        {
            if (isExtending)
            {
                isExtending = false;
                jumpTimeLeft = jumpExtensionDuration;
            }
            else
            {

                isWallJumping = false;
            }
        }
    }

    private void Load()
    {
        // Try Resources folder first
        TextAsset jsonFile = Resources.Load<TextAsset>("WallJumpSettings");
        if (jsonFile != null)
        {
            JsonUtility.FromJsonOverwrite(jsonFile.text, this);
            return;
        }

        // Fallback to persistent data path
        string loadPath = Path.Combine(Application.persistentDataPath, "WallJumpSettings.json");
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
        string savePath = Path.Combine(Application.persistentDataPath, "WallJumpSettings.json");
        File.WriteAllText(savePath, json);
#endif
    }

    public void StartWallJumping()
    {
        isWallJumping = true;
        isExtending = true;
        jumpTimeLeft = jumpMandatoryDuration;
    }

    public void OnJumpStop()
    {
        isWallJumping = false;
        isExtending = false;
        jumpTimeLeft = 0f;
    }

    public void OnJumpReleased()
    {
        // If isExtending is still true, we are still in the mandatory period, so it should continue
        isWallJumping = isExtending == true;
        isExtending = false;
    }

}


// [CreateAssetMenu(fileName = "JumpSettings", menuName = "ScriptableObjects/JumpSettings")]
// public class JumpHandler : ScriptableObject
// {
//     private bool isJumping = false;

//     [SerializeField] private float jumpPower = 5.5f;
//     [SerializeField] private float jumpingMore = 4f;
//     [SerializeField] private float maxJumpTime = 1f;
//     [SerializeField] private int jumpAmount = 3;
//     [SerializeField] private float jumpCooldown = 0.5f;


//     private int jumpLeft = 0;
//     private float cooldownLeft = 0f;

//     public bool IsJumping => isJumping;
//     public float JumpPower => jumpPower;
//     public float JumpingMore => jumpingMore;
//     public float MaxJumpTime => maxJumpTime;
//     private bool canJump = true;

//     public JumpHandler()
//     {
//     }

//     void OnEnable()
//     {
//         Load();
//         jumpLeft = jumpAmount;
//     }

//     private void Load()
//     {
//         // Try Resources folder first
//         TextAsset jsonFile = Resources.Load<TextAsset>("JumpSettings");
//         if (jsonFile != null)
//         {
//             JsonUtility.FromJsonOverwrite(jsonFile.text, this);
//             return;
//         }

//         // Fallback to persistent data path
//         string loadPath = Path.Combine(Application.persistentDataPath, "JumpSettings.json");
//         if (File.Exists(loadPath))
//         {
//             string json = File.ReadAllText(loadPath);
//             JsonUtility.FromJsonOverwrite(json, this);
//         }
//     }

//     public void Updated(float elapsed)
//     {
//         if (cooldownLeft > 0f)
//         {
//             cooldownLeft -= elapsed;
//             if (cooldownLeft <= 0f)
//             {
//                 canJump = true;
//             }
//         }
//     }

//     public bool TryJump()
//     {
//         if (canJump && jumpLeft - 1 >= 0)
//         {
//             jumpLeft--;
//             isJumping = true;
//             return true;
//         }
//         return false;
//     }

//     public void OnJumpingStop()
//     {
//         isJumping = false;
//         cooldownLeft = jumpCooldown;
//     }

//     public void OnJumpReset()
//     {
//         isJumping = false;
//         canJump = true;
//         jumpLeft = jumpAmount;
//     }

// }
