using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SfxManager))]
public class SfxManagerEditor : UnityEditor.Editor
{
    int selGridInt = 0;
    string[] selStrings = {"seedPickup", "collideWithTree", "collideWithTreeSeedDrop", "splash", "success"};
    
    public override void OnInspectorGUI()
    {
        string[] strings = SfxManager.Instance ? SfxManager.Instance.Sfx.Keys.ToArray() : Array.Empty<string>();
        
        DrawDefaultInspector();
        
        EditorGUILayout.Space(20);

        GUILayout.BeginVertical("Box");
        selGridInt = GUILayout.SelectionGrid(selGridInt, strings, 1);
        EditorGUILayout.Space(10);
        if (GUILayout.Button("PlaySfx"))
        {
            if (strings.Length <= 0)
            {
                Debug.Log("This button only works in Play mode");
                return;
            }
            SfxManager.Instance.PlaySfx(strings[selGridInt], 1f);
        }
        if (GUILayout.Button("PlaySfx random pitch"))
        {
            if (strings.Length <= 0)
            {
                Debug.Log("This button only works in Play mode");
                return;
            }
            SfxManager.Instance.PlaySfx(strings[selGridInt], 1f, true);
        }
        GUILayout.EndVertical();
    }
}
