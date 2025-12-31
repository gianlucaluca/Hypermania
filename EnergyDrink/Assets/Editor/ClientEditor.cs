#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Client))]
public class ClientEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var c = (Client)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Create Room")) c.CreateRoom();
        if (GUILayout.Button("Join Room")) c.JoinRoom();
        if (GUILayout.Button("Leave Room")) c.LeaveRoom();
        if (GUILayout.Button("Send Bind")) c.Bind();
        if (GUILayout.Button("Send Test")) c.SendTest();
    }
}
#endif
