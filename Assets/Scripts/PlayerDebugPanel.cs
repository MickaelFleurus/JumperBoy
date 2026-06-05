using UnityEngine;

/// <summary>
/// Debug panel for visualizing and modifying player velocity and handler parameters.
/// This script is only instantiated in non-release builds.
/// </summary>
public class PlayerDebugPanel : MonoBehaviour
{
    private Player player;
    private bool showDebugPanel = true;
    private bool showVelocitySection = true;
    private bool showPlayerStateSection = true;
    private bool showJumpHandlerSection = true;
    private bool showDashHandlerSection = true;
    private bool showWallJumpHandlerSection = true;
    private Vector2 scrollPosition = Vector2.zero;

    // ===== CUSTOM STYLES =====
    private GUIStyle panelBoxStyle;
    private GUIStyle sectionHeaderStyle;
    private GUIStyle labelStyle;
    private bool stylesInitialized = false;

    private void InitializeStyles()
    {
        if (stylesInitialized)
            return;

        // Panel Box Style - use default box style as base
        panelBoxStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(5, 5, 5, 5),
            margin = new RectOffset(2, 2, 2, 2)
        };

        // Section Header Style - use button as base with custom colors
        sectionHeaderStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(5, 5, 3, 3),
            alignment = TextAnchor.MiddleLeft
        };
        // Color the text white
        sectionHeaderStyle.normal.textColor = Color.white;
        sectionHeaderStyle.hover.textColor = Color.white;
        sectionHeaderStyle.active.textColor = Color.white;

        // Label Style
        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            padding = new RectOffset(3, 3, 2, 2)
        };
        labelStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        stylesInitialized = true;
    }

    private Texture2D CreateTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    public void Initialize(Player playerReference)
    {
        player = playerReference;
        // Styles will be initialized in OnGUI() on first call
    }

    private bool DrawFoldableSection(ref bool isOpen, string title)
    {
        string arrow = isOpen ? "▼ " : "► ";
        bool newState = GUILayout.Toggle(isOpen, arrow + title, sectionHeaderStyle, GUILayout.Height(22));
        return newState;
    }

    void OnGUI()
    {
        if (player == null)
            return;

        // Ensure styles are initialized before using them
        if (!stylesInitialized)
            InitializeStyles();

        if (panelBoxStyle == null || sectionHeaderStyle == null || labelStyle == null)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 500), panelBoxStyle);
        GUILayout.BeginVertical();

        showDebugPanel = GUILayout.Toggle(showDebugPanel, "▼ Player Debug Panel", sectionHeaderStyle, GUILayout.Height(25));

        if (showDebugPanel)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            GUILayout.Space(5);

            // Velocity Section
            showVelocitySection = DrawFoldableSection(ref showVelocitySection, "Velocity");
            if (showVelocitySection)
            {
                GUILayout.Label($"Velocity X: {player.GetVelocity().x:F2}", labelStyle, GUILayout.Height(20));
                GUILayout.Label($"Velocity Y: {player.GetVelocity().y:F2}", labelStyle, GUILayout.Height(20));
                GUILayout.Label($"Magnitude: {player.GetVelocity().magnitude:F2}", labelStyle, GUILayout.Height(20));
                GUILayout.Label($"Direction: {player.GetVelocity().normalized}", labelStyle, GUILayout.Height(20));
                GUILayout.Label($"Current Direction: {player.GetCurrentDirection()}", labelStyle, GUILayout.Height(20));
                GUILayout.Label($"Grounded: {player.IsGrounded()}", labelStyle, GUILayout.Height(20));
                GUILayout.Label($"Gravity Scale: {player.GetGravityScale():F2}", labelStyle, GUILayout.Height(20));
            }

            GUILayout.Space(10);

            // Player State Section
            showPlayerStateSection = DrawFoldableSection(ref showPlayerStateSection, "Player State");
            if (showPlayerStateSection)
            {
                GUILayout.Label($"Can Wall Jump: {player.CanWallJump()}", labelStyle, GUILayout.Height(20));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Walk Speed:", labelStyle, GUILayout.Width(120));
                GUILayout.Label($"{player.GetWalkSpeed():F2}", labelStyle, GUILayout.Width(50));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Run Speed:", labelStyle, GUILayout.Width(120));
                GUILayout.Label($"{player.GetRunSpeed():F2}", labelStyle, GUILayout.Width(50));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Acceleration:", labelStyle, GUILayout.Width(120));
                GUILayout.Label($"{player.GetAccelerationRate():F2}", labelStyle, GUILayout.Width(50));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Deceleration:", labelStyle, GUILayout.Width(120));
                GUILayout.Label($"{player.GetDecelerationRate():F2}", labelStyle, GUILayout.Width(50));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            // Jump Handler Section
            showJumpHandlerSection = DrawFoldableSection(ref showJumpHandlerSection, "Jump Handler");
            if (showJumpHandlerSection)
            {
                var jumpHandler = player.GetJumpHandler();
                GUILayout.Label($"Is Jumping: {jumpHandler.IsJumping}", labelStyle, GUILayout.Height(20));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Jump Power:", labelStyle, GUILayout.Width(120));
                float jumpPower = GUILayout.HorizontalSlider(jumpHandler.JumpPower, 1f, 15f);
                GUILayout.Label($"{jumpPower:F2}", labelStyle, GUILayout.Width(50));
                if (jumpPower != jumpHandler.JumpPower)
                    jumpHandler.SetJumpPower(jumpPower);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Max Jump Time:", labelStyle, GUILayout.Width(120));
                float maxJumpTime = GUILayout.HorizontalSlider(jumpHandler.MaxJumpTime, 0.1f, 2f);
                GUILayout.Label($"{maxJumpTime:F2}", labelStyle, GUILayout.Width(50));
                if (maxJumpTime != jumpHandler.MaxJumpTime)
                    jumpHandler.SetMaxJumpTime(maxJumpTime);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Jump Amount:", labelStyle, GUILayout.Width(120));
                int jumpAmount = (int)GUILayout.HorizontalSlider(jumpHandler.JumpAmount, 1f, 10f);
                GUILayout.Label($"{jumpAmount}", labelStyle, GUILayout.Width(50));
                if (jumpAmount != jumpHandler.JumpAmount)
                    jumpHandler.SetJumpAmount(jumpAmount);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Jump Cooldown:", labelStyle, GUILayout.Width(120));
                float jumpCooldown = GUILayout.HorizontalSlider(jumpHandler.JumpCooldown, 0.1f, 3f);
                GUILayout.Label($"{jumpCooldown:F2}", labelStyle, GUILayout.Width(50));
                if (jumpCooldown != jumpHandler.JumpCooldown)
                    jumpHandler.SetJumpCooldown(jumpCooldown);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            // Dash Handler Section
            showDashHandlerSection = DrawFoldableSection(ref showDashHandlerSection, "Dash Handler");
            if (showDashHandlerSection)
            {
                var dashHandler = player.GetDashHandler();
                GUILayout.Label($"Is Dashing: {dashHandler.IsDashing}", labelStyle, GUILayout.Height(20));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Dash Power:", labelStyle, GUILayout.Width(120));
                float dashPower = GUILayout.HorizontalSlider(dashHandler.DashPower, 5f, 30f);
                GUILayout.Label($"{dashPower:F2}", labelStyle, GUILayout.Width(50));
                if (dashPower != dashHandler.DashPower)
                    dashHandler.SetDashPower(dashPower);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Dash Amount:", labelStyle, GUILayout.Width(120));
                int dashAmount = (int)GUILayout.HorizontalSlider(dashHandler.DashAmount, 1f, 10f);
                GUILayout.Label($"{dashAmount}", labelStyle, GUILayout.Width(50));
                if (dashAmount != dashHandler.DashAmount)
                    dashHandler.SetDashAmount(dashAmount);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Dash Duration:", labelStyle, GUILayout.Width(120));
                float dashDuration = GUILayout.HorizontalSlider(dashHandler.DashDuration, 0.1f, 1f);
                GUILayout.Label($"{dashDuration:F2}", labelStyle, GUILayout.Width(50));
                if (dashDuration != dashHandler.DashDuration)
                    dashHandler.SetDashDuration(dashDuration);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Dash Cooldown:", labelStyle, GUILayout.Width(120));
                float dashCooldown = GUILayout.HorizontalSlider(dashHandler.DashCooldown, 0.1f, 3f);
                GUILayout.Label($"{dashCooldown:F2}", labelStyle, GUILayout.Width(50));
                if (dashCooldown != dashHandler.DashCooldown)
                    dashHandler.SetDashCooldown(dashCooldown);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            // Wall Jump Handler Section
            showWallJumpHandlerSection = DrawFoldableSection(ref showWallJumpHandlerSection, "Wall Jump Handler");
            if (showWallJumpHandlerSection)
            {
                var wallJumpHandler = player.GetWallJumpHandler();
                GUILayout.Label($"Is Wall Jumping: {wallJumpHandler.IsWallJumping}", labelStyle, GUILayout.Height(20));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Jump Power X:", labelStyle, GUILayout.Width(120));
                Vector2 jumpPowerWall = wallJumpHandler.JumpPower;
                float jumpPowerX = GUILayout.HorizontalSlider(jumpPowerWall.x, 0f, 10f);
                GUILayout.Label($"{jumpPowerX:F2}", labelStyle, GUILayout.Width(50));
                if (jumpPowerX != jumpPowerWall.x)
                    wallJumpHandler.SetJumpPower(new Vector2(jumpPowerX, jumpPowerWall.y));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Jump Power Y:", labelStyle, GUILayout.Width(120));
                float jumpPowerY = GUILayout.HorizontalSlider(jumpPowerWall.y, 0f, 15f);
                GUILayout.Label($"{jumpPowerY:F2}", labelStyle, GUILayout.Width(50));
                if (jumpPowerY != jumpPowerWall.y)
                    wallJumpHandler.SetJumpPower(new Vector2(jumpPowerWall.x, jumpPowerY));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Extension Duration:", labelStyle, GUILayout.Width(120));
                float extDuration = GUILayout.HorizontalSlider(wallJumpHandler.JumpExtensionDuration, 0.1f, 1f);
                GUILayout.Label($"{extDuration:F2}", labelStyle, GUILayout.Width(50));
                if (extDuration != wallJumpHandler.JumpExtensionDuration)
                    wallJumpHandler.SetJumpExtensionDuration(extDuration);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Mandatory Duration:", labelStyle, GUILayout.Width(120));
                float mandDuration = GUILayout.HorizontalSlider(wallJumpHandler.JumpMandatoryDuration, 0.1f, 2f);
                GUILayout.Label($"{mandDuration:F2}", labelStyle, GUILayout.Width(50));
                if (mandDuration != wallJumpHandler.JumpMandatoryDuration)
                    wallJumpHandler.SetJumpMandatoryDuration(mandDuration);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
