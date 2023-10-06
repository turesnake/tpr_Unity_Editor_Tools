using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Linq;


using Cinemachine;
using Unity.VisualScripting;

// 


public class VCamSerialize : EditorWindow
{


    public class ReorderableCache : ScriptableObject
    {
        [Serializable]
        public class Node
        {
            public CinemachineVirtualCamera vcam;
            public Node()
            {
                vcam = null;
            }
        }

        [SerializeField]
        List<Node> m_nodes = new List<Node>();

        public List<Node> Nodes
        {
            get { return m_nodes; }
        }
    }


    public class WindowParams
    {
        public string folderPath = "Assets/_Tmp_/VCams/";
        public string fileName = "vcam_001";
        public Vector3 centerGroundPos = Vector3.zero;

    }


    // ---------------------------------------
    ReorderableCache m_reorderableCache; // m_objs 的本地缓存, 方便 EditorWindow 再次打开时, 显示上一次配置的信息;
    ReorderableList m_objs;
    Vector2 m_objs_scrollPos; // 在 m_objs 右侧维护一个 滑动条

    WindowParams windowParams = new WindowParams();
    static string assetCachePath = TprIO.NormalizePathSeparator( "Assets/_Tmp_/VCamSerialize.asset" );// m_reorderableCache 本地缓存地址



    [MenuItem("_Tools_/003_VCams 序列化 (Window)", false, 701)]
    static void ShowWindow()
    {
        GetWindow<VCamSerialize>(); // 一个可缩放的窗口
    }


    void OnGUI()
    {
        Init();

        // 绘制 上部 oths...
        // ...
        windowParams.folderPath = EditorGUILayout.TextField( "存储目录 (谨慎修改)", windowParams.folderPath );
        windowParams.fileName   = EditorGUILayout.TextField( "存储文件名 (别写后缀)", windowParams.fileName );
        windowParams.centerGroundPos = EditorGUILayout.Vector3Field( "舞台中央地面坐标", windowParams.centerGroundPos );


        // 绘制 m_objs:
        if (m_objs != null)
        {
            EditorGUI.BeginChangeCheck();
            m_objs_scrollPos = GUILayout.BeginScrollView(m_objs_scrollPos);
            m_objs.DoLayoutList();
            GUILayout.EndScrollView();
            bool isDirty_objs = EditorGUI.EndChangeCheck();
            if (isDirty_objs)
            {
                EditorUtility.SetDirty(m_reorderableCache);
                AssetDatabase.SaveAssets();
                Repaint();
            } 
        }

        // 绘制 下部 oths...
        // ...

        GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(1)); // draw a line
        if(GUILayout.Button("基于相机 生成配表数据", GUILayout.Width(150), GUILayout.Height(40) ))
        {
            VCams2Data();
        }

        GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(1)); // draw a line
        if(GUILayout.Button("基于配表数据 复原相机", GUILayout.Width(150), GUILayout.Height(40) ))
        {
            Data2VCams();
        }

        GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(1)); // draw a line
        if(GUILayout.Button("测试", GUILayout.Width(150), GUILayout.Height(40) ))
        {
            Test();
        }



    }

    void Init()
    {
        if (m_reorderableCache == null)
        {
            TprIO.CheckAndCreateDirectory( TprIO.GetDirectoryName( assetCachePath ));
            m_reorderableCache = AssetDatabase.LoadAssetAtPath<ReorderableCache>( assetCachePath );
            if (m_reorderableCache == null)
            {
                m_reorderableCache = (ReorderableCache)ScriptableObject.CreateInstance(typeof(ReorderableCache));
                m_reorderableCache.Nodes.Add(new ReorderableCache.Node());
                AssetDatabase.CreateAsset(m_reorderableCache, assetCachePath );
            }
        }

        if (m_objs == null)
        {
            m_objs = new ReorderableList(m_reorderableCache.Nodes, null, true, false, true, true);
            m_objs.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "KOKO"); }; // draw list header 好像没啥用, 没找到画在哪了
            m_objs.elementHeightCallback = (int index) => { return EditorGUIUtility.singleLineHeight + 1; }; // 每一行元素的高度
            m_objs.drawElementCallback = OnDrawElementCallback;
        }
    }


    // 最简单: 一行只有一个 obj slot;
    void OnDrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
    {
        if (m_reorderableCache == null || m_reorderableCache.Nodes.Count < index)
        {
            return;
        }
        var info = m_reorderableCache.Nodes[index];
        rect.height = EditorGUIUtility.singleLineHeight;
        info.vcam = (CinemachineVirtualCamera)EditorGUI.ObjectField(rect, info.vcam, typeof(CinemachineVirtualCamera), true );
    }





    void VCams2Data() 
    {
        Debug.Log("基于相机 生成配表数据");
        // ----------- 取出 vcams -----------:
        List<CinemachineVirtualCamera> vcams = new List<CinemachineVirtualCamera>();
        foreach( var node in m_reorderableCache.Nodes )
        {
            if( node.vcam != null )
            {
                vcams.Add(node.vcam);
            }
        }
        if(vcams.Count == 0)
        {
            UnityEditor.EditorUtility.DisplayDialog( "Error", "没绑定任何的 vcams", "OK" );
            return;
        }

        // 生成数据, 转换为 json 格式:
        VCamPesistList vcamPesistList = new VCamPesistList();
        foreach( var vcam in vcams )
        {
            vcamPesistList.vcamPesists.Add( VCamPesist.VCam2Data(vcam, windowParams.centerGroundPos) );
        }
        string jsonData = JsonUtility.ToJson(vcamPesistList);


        // -- 将数据 写入 json 文件:
        string jsonFullPath = CheckAndGetJsonFullPath();
        TprIO.WriteToFile( jsonData, jsonFullPath, FileMode.Create );

        // =====
        UnityEditor.EditorUtility.DisplayDialog( "成功", "您已成功为 " + vcams.Count + " 个 vcam 信息写入文件 " + jsonFullPath, "OK" );
    }


    


    void Data2VCams() 
    {
        Debug.Log("基于配表数据 复原相机");

        string jsonFullPath = CheckAndGetJsonFullPath();

        bool readRet = TprIO.ReadFile( jsonFullPath, out string fileData );
        if(!readRet)
        {
            UnityEditor.EditorUtility.DisplayDialog( "Error", "目标 json 文件并不存在, 您可能尚未存储", "OK" );
            return;
        }

        if(fileData == null || fileData == "")
        {
            UnityEditor.EditorUtility.DisplayDialog( "Error", "目标 json 文件存在, 但没有数据, 请检查异常", "OK" );
            return;
        }


        VCamPesistList vcamPesistList = JsonUtility.FromJson<VCamPesistList>(fileData);
        Debug.Log( "从 json 文件中找到 vcam 的个数: " + vcamPesistList.vcamPesists.Count );

        m_reorderableCache.Nodes.Clear();
        
        var allVCamsInScene = UnityEngine.Object.FindObjectsOfType<CinemachineVirtualCamera>(true); // 场景中所有 vcams, 包含 inactive 的;
        // --- 基于 json 数据, 还原场景中的 虚拟相机;
        for( int i=0; i<vcamPesistList.vcamPesists.Count; i++ )
        {
            var vcamData = vcamPesistList.vcamPesists[i];
            CinemachineVirtualCamera vcam = CreateVCam2( allVCamsInScene, vcamData );
            SetDate2VCam( vcamData, vcam );

            // 将这些 vcams in scene, 绑定回 m_reorderableCache:
            m_reorderableCache.Nodes.Add(new ReorderableCache.Node(){ vcam = vcam });
        }

        EditorUtility.SetDirty(m_reorderableCache);
        AssetDatabase.SaveAssets();
        Repaint();
    }


    string CheckAndGetJsonFullPath() 
    {
        string jsonFullPath = TprIO.NormalizePathSeparator( Path.Combine( windowParams.folderPath, windowParams.fileName ) );
        jsonFullPath = System.IO.Path.ChangeExtension(jsonFullPath, ".json");
        TprIO.CheckAndCreateDirectory( TprIO.GetDirectoryName( jsonFullPath )); // 检查或生成目录
        return jsonFullPath;
    }



    void Test() 
    {
        Debug.Log("test");


        //TprIO.RebuildGameObjectByFullPath( "ccc" );

    }


    

    Transform vcamToolTF = null; // 辅助用的 transform
    static string vcamToolName = "_tmpTool_"; 

    CinemachineVirtualCamera CreateVCam2( CinemachineVirtualCamera[] allVCams_, VCamPesist data_ )
    {
        if( vcamToolTF == null ) 
        {
            GameObject toolGO = GameObject.Find( "/" + vcamToolName);
            if( toolGO == null ) 
            {
                toolGO = new GameObject(vcamToolName);
            }
            vcamToolTF = toolGO.transform;
        }

        CinemachineVirtualCamera vcamComp = Array.Find<CinemachineVirtualCamera>( allVCams_, e=>TprIO.GetGameObjectPathInScene(e.transform) == data_.pathInScene );
        if( vcamComp == null ) 
        {
            Transform vcamTF = TprIO.RebuildGameObjectByFullPath( data_.pathInScene );
            vcamComp = vcamTF.AddComponent<CinemachineVirtualCamera>();
        }
        return vcamComp;
    }




    public void SetDate2VCam( VCamPesist data_, CinemachineVirtualCamera vcam_)
    {
        Transform vcamTF = vcam_.transform;
        //---
        vcamTF.position = windowParams.centerGroundPos + new Vector3(0f,data_.cameraH,-data_.distance);
        //---
        vcamToolTF.position = new Vector3( windowParams.centerGroundPos.x, vcamTF.position.y, windowParams.centerGroundPos.z );
        vcamTF.LookAt( vcamToolTF );
        vcamTF.Rotate( Vector3.right, data_.pitchDegree, Space.Self );
        //---
        var lens = vcam_.m_Lens;
        lens.FieldOfView = data_.fov;
        vcam_.m_Lens = lens;
        
    }




}

