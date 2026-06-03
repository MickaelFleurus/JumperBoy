using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "JumpSettings", menuName = "ScriptableObjects/JumpSettings")]
public class JumpHandler : ScriptableObject
{
    private bool isJumping = false;

    [SerializeField] private float jumpPower = 5.5f;
    [SerializeField] private float jumpingMore = 4f;
    [SerializeField] private float maxJumpTime = 1f;
    [SerializeField] private int jumpAmount = 3;
    [SerializeField] private float jumpCooldown = 0.5f;


    private int jumpLeft = 0;
    private float cooldownLeft = 0f;

    public bool IsJumping => isJumping;
    public float JumpPower => jumpPower;
    public float JumpingMore => jumpingMore;
    public float MaxJumpTime => maxJumpTime;
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

    public void Updated(float elapsed)
    {
        if (cooldownLeft > 0f)
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
            return true;
        }
        return false;
    }

    public void OnJumpingStop()
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
