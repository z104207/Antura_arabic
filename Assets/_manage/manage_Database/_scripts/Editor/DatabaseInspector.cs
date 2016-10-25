﻿using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EA4S.Db.Management.Editor
{
    [CustomEditor(typeof(Database))]
    public class DatabaseLoaderInspector : UnityEditor.Editor
    {
        DatabaseLoader src;
        SerializedObject sobj;

        void OnEnable()
        {
            src = target as DatabaseLoader;
            sobj = new SerializedObject(target);
        }

        public override void OnInspectorGUI()
        {
            sobj.Update();

            var iterator = sobj.GetIterator();
            iterator.Next(true);
            do
            {
                var innerList = iterator.FindPropertyRelative("innerList");
                if (innerList != null)
                { 
                    EditorGUILayout.PropertyField(innerList, new GUIContent(iterator.displayName), true);
                }
            } while (iterator.Next(false));

            sobj.ApplyModifiedProperties();
        }

    }
}