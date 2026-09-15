using ChronoBlade.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ChronoBlade.Gameplay
{
    // Bare-bones OnGUI HUD so the prototype is playable right now — spec 06 replaces this with
    // a proper debug/production HUD reading TimeEconomyController's ledger.
    public class GameHUD : MonoBehaviour
    {
        const float FlashDuration = 0.25f;
        const float AutoRestartDelay = 8f; // unattended booth demo: reset without staff intervention

        TimeEconomyController _economy;
        float _flashTimer;
        float _completeTimer;

        void Awake()
        {
            _economy = FindFirstObjectByType<TimeEconomyController>();
        }

        void OnEnable()
        {
            if (_economy == null) _economy = FindFirstObjectByType<TimeEconomyController>();
            if (_economy != null) _economy.OnTimeSpent += HandleTimeSpent;
        }

        void OnDisable()
        {
            if (_economy != null) _economy.OnTimeSpent -= HandleTimeSpent;
        }

        void HandleTimeSpent(TimeLedgerEntry entry)
        {
            if (entry.Delta < 0f && entry.Reason != null && entry.Reason.StartsWith("Hit by"))
            {
                _flashTimer = FlashDuration; // player got hit — red screen flash
            }
        }

        void Update()
        {
            if (_flashTimer > 0f) _flashTimer -= Time.deltaTime;

            var run = RunManager.Instance;
            if (run == null) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (run.Phase == RunPhase.Altar)
            {
                if (!run.HasPickedUpgradeThisAltar)
                {
                    if (keyboard.digit1Key.wasPressedThisFrame) run.ChooseUpgrade(0);
                    else if (keyboard.digit2Key.wasPressedThisFrame) run.ChooseUpgrade(1);
                    else if (keyboard.digit3Key.wasPressedThisFrame) run.ChooseUpgrade(2);
                }
                else
                {
                    if (keyboard.cKey.wasPressedThisFrame) run.ContinueToNextWave();
                    else if (keyboard.bKey.wasPressedThisFrame) run.BankAndEnd();
                }
            }
            else if (run.Phase == RunPhase.Complete)
            {
                _completeTimer += Time.deltaTime;
                if (keyboard.rKey.wasPressedThisFrame || _completeTimer >= AutoRestartDelay)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
            else
            {
                _completeTimer = 0f;
            }
        }

        void OnGUI()
        {
            if (_economy == null) return;
            var run = RunManager.Instance;

            if (_flashTimer > 0f)
            {
                var prevColor = GUI.color;
                GUI.color = new Color(1f, 0f, 0f, 0.25f * (_flashTimer / FlashDuration));
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = prevColor;
            }

            DrawTopBar(run);

            if (run == null) return;

            if (run.Phase == RunPhase.Altar) DrawAltar(run);
            else if (run.Phase == RunPhase.Complete) DrawComplete(run);
            else DrawHelp();
        }

        void DrawTopBar(RunManager run)
        {
            var timeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(20, 15, 400, 45), $"TIME: {_economy.CurrentTime:F1}s", timeStyle);

            var subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                normal = { textColor = new Color(1f, 0.85f, 0.3f) }
            };
            string subLine = run != null ? $"Wave {run.WaveNumber}    Kills: {run.KillCount}" : "";
            GUI.Label(new Rect(20, 55, 400, 30), subLine, subStyle);
        }

        void DrawHelp()
        {
            var helpStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                normal = { textColor = new Color(1f, 1f, 1f, 0.75f) }
            };
            GUI.Label(new Rect(20, Screen.height - 35, 700, 30),
                "MOVE: WASD / Arrows    ATTACK: SPACE (kills nearby enemies)", helpStyle);
        }

        void DrawAltar(RunManager run)
        {
            var boxStyle = new GUIStyle(GUI.skin.box) { fontSize = 14 };
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            var promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.85f, 0.3f) }
            };

            GUI.Label(new Rect(0, 90, Screen.width, 40), "ALTAR — WAVE CLEARED", titleStyle);

            if (!run.HasPickedUpgradeThisAltar)
            {
                float boxWidth = 220f, spacing = 20f;
                float totalWidth = boxWidth * 3 + spacing * 2;
                float startX = (Screen.width - totalWidth) / 2f;
                float y = 150f;

                for (int i = 0; i < run.CurrentAltarOptions.Length; i++)
                {
                    var node = run.CurrentAltarOptions[i];
                    var rect = new Rect(startX + i * (boxWidth + spacing), y, boxWidth, 140);
                    GUI.Box(rect, "", boxStyle);

                    GUILayout.BeginArea(rect);
                    GUILayout.Space(10);
                    GUILayout.Label($"[{i + 1}] {node.displayName}", new GUIStyle(GUI.skin.label)
                        { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } });
                    foreach (var mod in node.statModifiers)
                    {
                        string sign = mod.type == ModifierType.Percent ? $"{mod.value:+0%;-0%}" : $"{mod.value:+0.#;-0.#}";
                        GUILayout.Label($"{mod.stat} {sign}", new GUIStyle(GUI.skin.label)
                            { fontSize = 14, normal = { textColor = new Color(0.7f, 1f, 0.7f) } });
                    }
                    GUILayout.Label($"Cost: {node.timeCost:F1}s", new GUIStyle(GUI.skin.label)
                        { fontSize = 14, normal = { textColor = new Color(1f, 0.6f, 0.6f) } });
                    GUILayout.EndArea();
                }

                GUI.Label(new Rect(0, y + 160, Screen.width, 30), "Press 1 / 2 / 3 to choose", promptStyle);
            }
            else
            {
                GUI.Label(new Rect(0, 180, Screen.width, 30),
                    "Upgrade applied.", promptStyle);
                GUI.Label(new Rect(0, 220, Screen.width, 30),
                    "Press C to push on (harder wave)   or   B to bank this run and stop here",
                    promptStyle);
            }
        }

        void DrawComplete(RunManager run)
        {
            var bigStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 48,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = run.Banked ? new Color(0.4f, 1f, 0.5f) : Color.red }
            };
            string title = run.Banked ? "RUN BANKED" : "TIME'S UP";
            GUI.Label(new Rect(0, Screen.height / 2f - 60, Screen.width, 80), title, bigStyle);

            var restartStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(0, Screen.height / 2f + 10, Screen.width, 40),
                $"Reached wave {run.WaveNumber} — {run.KillCount} kills — press R to restart", restartStyle);

            var timerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 1f, 1f, 0.5f) }
            };
            GUI.Label(new Rect(0, Screen.height / 2f + 55, Screen.width, 30),
                $"(auto-restart in {Mathf.Max(0f, AutoRestartDelay - _completeTimer):F0}s)", timerStyle);
        }
    }
}
