using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ColorPreviewAttribute))]
public class ColorPreviewDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var att = attribute as ColorPreviewAttribute;
        var button = new Rect(position.x, position.y, 60, position.height);
        var colorPos = new Rect(position.x + 60, position.y, 100, position.height);
        var colorValuePos = new Rect(position.x + 160, position.y, position.width-150, position.height);

        string colorValueTxt = string.Format("R = {0} , G = {1}", property.colorValue.r, property.colorValue.g);
        if (EditorGUI.ToggleLeft(button, att.text, false))
        {
            Hukiry.HukiryToolEditor.LocationObject<Material>(att.materialName);
        }
        EditorGUI.ColorField(colorPos, property.colorValue);
        EditorGUI.LabelField(colorValuePos, colorValueTxt);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}


