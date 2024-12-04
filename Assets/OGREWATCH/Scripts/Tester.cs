using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Tester : MonoBehaviour
{
    public MyPlayerStatus playerStatus;
}

[CustomEditor(typeof(Tester))]
public class TesterSetup : Editor
{
    MyPlayerStatus ps;
    float stunDuration = 2f;
    float rootDuration = 3f;
    float slowDuration = 4f;
    float slowMultiplier = 0.5f;
    
    public override void OnInspectorGUI()
    {
        // Tester tester = (Tester)target;
        // ps = tester.playerStatus;
        ps = EditorGUILayout.ObjectField("Player Status", ps, typeof(MyPlayerStatus), true) as MyPlayerStatus;
        stunDuration = EditorGUILayout.FloatField("Stun Duration", stunDuration);
        rootDuration = EditorGUILayout.FloatField("Root Duration", rootDuration);
        slowDuration = EditorGUILayout.FloatField("Slow Duration", slowDuration);
        slowMultiplier = EditorGUILayout.FloatField("Slow Multiplier", slowMultiplier);

        if (GUILayout.Button("Apply Stun"))
        {
            ps.ApplyStun(stunDuration);
        }
        if (GUILayout.Button("Apply Root"))
        {
            ps.ApplyRoot(rootDuration);
        }
        if (GUILayout.Button("Apply Slow"))
        {
            ps.ApplySlow(slowDuration, slowMultiplier);
        }
    } 
}
