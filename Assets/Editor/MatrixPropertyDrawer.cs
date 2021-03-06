﻿using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Matrix4x4))]
public class MatrixPropertyDrawer : PropertyDrawer
{
    const float CELL_HEIGHT = 16;

    Rect position;
    SerializedProperty property;
    GUIContent label;

    // Draw the property inside the given rect
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent lab)
    {
        position = pos;
        property = prop;
        label = lab;

        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        Matrix4x4 matrix = Matrix4x4.identity;

        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 4; c++)
            {
                DrawCell(c, r);
                matrix[r, c] = property.FindPropertyRelative("e" + r + c).floatValue;
            }
        }

        pos.y += 5 * CELL_HEIGHT;

        Vector3 translation = matrix.GetColumn(3);

        Quaternion rotation = Quaternion.LookRotation(
            matrix.GetColumn(2),
            matrix.GetColumn(1)
        );

        Vector3 scale = new Vector3(
            matrix.GetColumn(0).magnitude,
            matrix.GetColumn(1).magnitude,
            matrix.GetColumn(2).magnitude
        );

        bool wasEnabled = GUI.enabled;
        GUI.enabled = false;
        position = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Translation"));
        translation = EditorGUI.Vector3Field(position, "", translation);
        pos.y += CELL_HEIGHT;

        position = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Rotation (Euler)"));
        rotation.eulerAngles = EditorGUI.Vector3Field(position, "", rotation.eulerAngles);
        pos.y += CELL_HEIGHT;

        position = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Rotation (Quaternion)"));
        rotation = Vector4ToQuaternion(EditorGUI.Vector4Field(position, "", QuaternionToVector4(rotation)));
        pos.y += CELL_HEIGHT;

        position = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Scale"));
        scale = EditorGUI.Vector3Field(position, "", scale);
        pos.y += CELL_HEIGHT;
        GUI.enabled = wasEnabled;


        if (GUI.Button(pos, "From Camera"))
        {
            matrix = Camera.main.projectionMatrix;
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    property.FindPropertyRelative("e" + r + c).floatValue = matrix[r, c];
                }
            }

        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    void DrawCell(int column, int row)
    {
        Vector2 cellPos = position.position;
        cellPos.x += position.width * column / 4;
        cellPos.y += CELL_HEIGHT * row;
        EditorGUI.PropertyField(
            new Rect(cellPos, new Vector2(position.width / 4, CELL_HEIGHT)),
            property.FindPropertyRelative("e" + row + column),
            GUIContent.none
        );
    }

    static Vector4 QuaternionToVector4(Quaternion rot)
    {
        return new Vector4(rot.x, rot.y, rot.z, rot.w);
    }

    static Quaternion Vector4ToQuaternion(Vector4 v)
    {
        return new Quaternion(v.x, v.y, v.z, v.w);
    }

}