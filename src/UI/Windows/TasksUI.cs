using System.Linq;
using UnityEngine;

namespace KarakMenu;

public class TasksUI : MonoBehaviour
{
    public static int windowHeight = 360;
    public static int windowWidth = 580;
    private Rect _windowRect;

    private Vector2 _scrollPosition = Vector2.zero;
    private GUIStyle _playerHeaderStyle;
    private int _playerHeaderVersion = -1;
    private Il2CppSystem.Text.StringBuilder _tasksString = new();
    private readonly System.Collections.Generic.Dictionary<string, bool> _expandedPlayers = new();

    private void Start()
    {
        // Instantiate 2D area of TasksUI
        _windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    private void OnGUI()
    {
        if (!CheatToggles.showTasksMenu || !(MenuUI.isGUIActive || KarakMenu.menuKeepSubwindowsOpen.Value) || KarakMenu.isPanicked) return;

        if (_playerHeaderStyle == null || _playerHeaderVersion != KarakTheme.TextureVersion)
        {
            _playerHeaderVersion = KarakTheme.TextureVersion;
            _playerHeaderStyle = new GUIStyle(GUIStylePreset.Button)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft
            };
        }

        GUIStylePreset.EnsureSkin();
		UIHelpers.ApplyAccent();

        GUI.color = Color.white; GUI.backgroundColor = Color.white; GUI.contentColor = Color.white;

        if (KarakTheme.RgbMode) KarakUI.DrawRgbGlow(_windowRect);

        _windowRect = GUI.Window((int)WindowId.TasksUI, _windowRect, (GUI.WindowFunction)TasksWindow, "", KarakUI.SubwindowStyle);
    }

    private void TasksWindow(int windowID)
    {
        GUI.Label(new Rect(16f, 12f, windowWidth - 32f, 22f), "Tasks", KarakUI.WindowTitle);

        GUILayout.BeginVertical();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.Data || !player.Data.Role || string.IsNullOrEmpty(player.Data.PlayerName)) continue;

            GUILayout.BeginVertical();

            var nameKey = player.Data.PlayerName;
            _expandedPlayers.TryGetValue(nameKey, out var expanded);
            var arrow = expanded ? "\u25BC" : "\u25B6"; // ▼ or ▶

            var taskCount = player.myTasks.Count;
            var completeCount = player.myTasks.ToArray().Count(t => t.IsComplete);

            if (player == PlayerControl.LocalPlayer && player.Data.IsDead)
            {
                taskCount -= 1;
            }
            if (player == PlayerControl.LocalPlayer && Utils.isAnySabotageActive)
            {
                taskCount -= 1;
            }
            if (player == PlayerControl.LocalPlayer && player.Data.Role.IsImpostor)
            {
                taskCount -= 1;
            }

            if (GUILayout.Button($"{arrow} [{completeCount}/{taskCount}] <color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{nameKey}</color>", _playerHeaderStyle))
            {
                _expandedPlayers[nameKey] = !expanded;
                expanded = !expanded;
            }

            if (expanded)
            {
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();

                foreach (var task in player.myTasks)
                {
                    // Do some checks to not show texts: sabotage active, dead hint, impostor hint
                    if (task.TaskType is TaskTypes.ResetReactor or TaskTypes.RestoreOxy or TaskTypes.FixLights or TaskTypes.FixComms or TaskTypes.ResetSeismic or TaskTypes.StopCharles or TaskTypes.MushroomMixupSabotage) continue;

                    _tasksString.Clear();
                    task.AppendTaskText(_tasksString);
                    //_tasksString.Append($"Task Type: {task.TaskType.ToString()}");
                    var taskText = _tasksString.ToString();

                    if (taskText.Contains("You're dead") || taskText.Contains("Sabotage and kill")) continue;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(taskText.Replace("\n", "").Replace("</color>", "").Replace("<color=#00DD00FF>", "").Replace("<color=#FFFF00FF>", ""), GUIStylePreset.Label);
                    GUILayout.FlexibleSpace();

                    if (task.IsComplete)
                    {
                        GUILayout.Label("<color=#00ff00>✔ Complete</color>", GUIStylePreset.Label);
                    }
                    else
                    {
                        if (player == PlayerControl.LocalPlayer)
                        {
                            if (GUILayout.Button("Complete", GUIStylePreset.NormalButton))
                            {
                                Utils.CompleteTask(task);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        GUILayout.EndScrollView();

        if (KarakUI.Button("Complete My Tasks", GUILayout.Height(24f)))
        {
            CheatToggles.completeMyTasks = true;
        }

        GUILayout.EndVertical();

        GUI.DragWindow();
    }
}
