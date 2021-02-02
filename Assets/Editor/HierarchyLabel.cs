using UnityEditor;

namespace Project2020
{
    using UnityEngine;
    [InitializeOnLoad]
    public class HierarchyLabel : MonoBehaviour
    {
        static HierarchyLabel()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        static void HierarchyWindowItemOnGUI(int instanceID, Rect SelectionRect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if(obj != null && obj.name.StartsWith("---", System.StringComparison.Ordinal))
            {
                EditorGUI.DrawRect(SelectionRect, Color.grey);
                EditorGUI.DropShadowLabel(SelectionRect, obj.name.Replace("-", "").ToString());
            }
            else if(obj != null && obj.name.StartsWith(">>>",System.StringComparison.Ordinal))
            {
                EditorGUI.DrawRect(SelectionRect, Color.HSVToRGB(0,0,0));
                EditorGUI.DropShadowLabel(SelectionRect, obj.name.Replace(">", "").ToString());
            }
        }
    }
}

