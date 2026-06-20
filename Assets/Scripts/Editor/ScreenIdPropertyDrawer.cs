using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ScreenIdAttribute))]
public class ScreenIdPropertyDrawer : PropertyDrawer
{
    private static string[] _labels;
    private static int[] _values;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        EnsureOptions();

        EditorGUI.BeginProperty(position, label, property);
        property.intValue = EditorGUI.IntPopup(
            position,
            label.text,
            property.intValue,
            _labels,
            _values
        );
        EditorGUI.EndProperty();
    }

    private static void EnsureOptions()
    {
        if (_labels != null)
        {
            return;
        }

        var fields = typeof(ScreenIds).GetFields(BindingFlags.Public | BindingFlags.Static);
        var count = 0;

        for (var i = 0; i < fields.Length; i++)
        {
            if (fields[i].FieldType == typeof(int) && fields[i].IsLiteral)
            {
                count++;
            }
        }

        _labels = new string[count];
        _values = new int[count];
        var index = 0;

        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            if (field.FieldType != typeof(int) || !field.IsLiteral)
            {
                continue;
            }

            _labels[index] = field.Name;
            _values[index] = (int)field.GetRawConstantValue();
            index++;
        }

        Array.Sort(_values, _labels);
    }
}
