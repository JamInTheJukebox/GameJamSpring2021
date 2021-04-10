using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerSettings
{
    public static bool Mouse_Y_Invert = true;       // 1 for no invert, -1 for invert;
    public static bool Mouse_X_Invert = false;      // 1 for no invert, -1 for invert;
    public static bool ToolTip = false;
    public static void On_Y_Invert_Changed(bool newVal)
    {
        Mouse_Y_Invert = newVal;
    }

    public static void On_ToolTip_Changed(bool newVal)
    {
        ToolTip = newVal;
    }
}
