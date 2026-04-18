using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityAI;

public class EnemyVisualizer : MonoBehaviour
{
    [Header("Display Settings")]
    public bool showVisualizer = true;
    public KeyCode toggleKey = KeyCode.F1;

    // Cached style built once in OnGUI
    GUIStyle headerStyle;
    GUIStyle rowStyle;
    GUIStyle connectedStyle;
    GUIStyle disconnectedStyle;
    GUIStyle squadHeaderStyle;

    // Scroll position for the panel
    Vector2 scrollPos;

    const float PanelX      = 10f;
    const float PanelY      = 10f;
    const float PanelWidth  = 340f;
    const float RowHeight   = 22f;
    const float MaxHeight   = 400f;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            showVisualizer = !showVisualizer;
    }

    void OnGUI()
    {
        if (!showVisualizer)
            return;

        BuildStyles();

        EnemyLogic[] enemies = FindObjectsOfType<EnemyLogic>();

        // Group by squad, enemies without a brain go last
        var groups = enemies
            .GroupBy(e => e.thisBrain != null ? (int)e.thisBrain.squad : int.MaxValue)
            .OrderBy(g => g.Key)
            .ToList();

        int totalRows = enemies.Length + groups.Count; // rows = enemy rows + one header per group
        float contentHeight = RowHeight * totalRows + 8f;
        float panelHeight   = Mathf.Min(contentHeight + RowHeight + 20f, MaxHeight);

        // Outer box
        GUI.Box(new Rect(PanelX, PanelY, PanelWidth, panelHeight), "");

        // Header
        GUI.Label(new Rect(PanelX + 4f, PanelY + 4f, PanelWidth - 8f, RowHeight),
                  $"Enemy Status  [{enemies.Length}]   ({toggleKey} to toggle)", headerStyle);

        // Scrollable rows
        Rect viewRect    = new Rect(PanelX + 4f, PanelY + RowHeight + 6f, PanelWidth - 8f, panelHeight - RowHeight - 14f);
        Rect contentRect = new Rect(0f, 0f, PanelWidth - 24f, RowHeight * enemies.Length);

        scrollPos = GUI.BeginScrollView(viewRect, scrollPos, contentRect);

        int row = 0;
        foreach (var group in groups)
        {
            // Squad header
            string squadName = group.First().thisBrain != null
                ? group.First().thisBrain.squad.ToString().Replace("s_", "")
                : "No Brain";

            GUI.Label(new Rect(0f, row * RowHeight, PanelWidth - 24f, RowHeight),
                      $"── {squadName} ──", squadHeaderStyle);
            row++;

            foreach (EnemyLogic enemy in group)
            {
                if (enemy == null) continue;

                Brain brain = enemy.thisBrain;

                bool connected    = brain != null && brain.isConnectedToHive;
                string actionName = brain != null ? brain.CurrentActionName : "—";
                string signal     = connected ? "✔ Signal" : "✘ No Signal";
                GUIStyle sigStyle = connected ? connectedStyle : disconnectedStyle;

                float col2 = (PanelWidth - 24f) * 0.42f;
                float col3 = (PanelWidth - 24f) * 0.68f;

                GUI.Label(new Rect(0f,   row * RowHeight, col2,                        RowHeight), enemy.gameObject.name, rowStyle);
                GUI.Label(new Rect(col2, row * RowHeight, col3 - col2,                 RowHeight), signal,                sigStyle);
                GUI.Label(new Rect(col3, row * RowHeight, (PanelWidth - 24f) - col3,  RowHeight), actionName,            rowStyle);

                row++;
            }
        }

        GUI.EndScrollView();
    }

    void BuildStyles()
    {
        if (headerStyle != null)
            return;

        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize  = 12
        };

        rowStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11
        };

        connectedStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 11,
            fontStyle = FontStyle.Normal
        };
        connectedStyle.normal.textColor = new Color(0.3f, 1f, 0.3f);

        disconnectedStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 11,
            fontStyle = FontStyle.Normal
        };
        disconnectedStyle.normal.textColor = new Color(1f, 0.35f, 0.35f);

        squadHeaderStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 11,
            fontStyle = FontStyle.Bold
        };
        squadHeaderStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);
    }
}
