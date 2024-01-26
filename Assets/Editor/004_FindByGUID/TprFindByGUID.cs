using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;



public class TprFindByGUID : EditorWindow
{

    public string guid = "";


    [MenuItem("_tpr_/查找一个 guid (Window)", false, 701)]
    static void ShowWindow()
    {
        GetWindow<TprFindByGUID>("查找 guid 文件"); // 一个可缩放的窗口
    }
    
    void OnGUI()
    {
        float wStart = 10f;
        float hStart = 10f;
        //--

        GUI.Label( new Rect(wStart, hStart, 100, 30), "GUID:" );
        hStart += 30f + 10f;

        guid = GUI.TextField(new Rect(wStart, hStart, 300, 60), guid );
        hStart += 60f + 10f;

        if( GUI.Button(new Rect(wStart, hStart, 100, 60), "查找:") )
        {

            if( string.IsNullOrEmpty(guid) )
            {
                UnityEditor.EditorUtility.DisplayDialog( "异常", "请在 GUID 中输入正确的 内容", "OK" );
                return;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guid); // Get the asset path using the GUID
            string log = "guid 的 path: " + assetPath;
            UnityEditor.EditorUtility.DisplayDialog( "找到了", log, "OK" );
            Debug.Log(log);
        }
       
    }


}








