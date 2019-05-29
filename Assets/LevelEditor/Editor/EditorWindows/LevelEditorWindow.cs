﻿/*
 * @Author: fasthro
 * @Date: 2019-05-17 14:28:56
 * @Description: 关卡编辑窗口
 */

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace LevelEditor
{
    public class LevelEditorWindow : EditorWindow
    {
        // window
        public static LevelEditorWindow Inst;

        // 开启编辑模式
        public bool editorEnabled = false;

        // 网格模版尺寸
        private Vector2Int m_templateGridSize;
        public Vector2Int templateGridSize { get { return m_templateGridSize; } set { m_templateGridSize = value; } }

        // 当前画格子所在区域索引
        private int m_area = 1;
        public int area { get { return m_area; } set { if (value > 0) m_area = value; } }

        // 当前选择材料类型
        public MaterialType m_materialType;
        public MaterialType materialType { get { return m_materialType; } }

        // 场景窗口-主窗口
        private SceneWindow m_sceneWindow;
        public SceneWindow sceneWindow { get { return m_sceneWindow; } }

        // 模型预览
        private Vector2 m_modelViewScrollPosition;
        private int m_modelViewHorizontalCounter;
        private int m_modelViewColumn;
        // 预览分类操作记录
        private Dictionary<string, bool> classRecords;
        // 预览二级分组Id
        private string m_secondaryGroupId;
        // 当前选择的资源组
        private ResGroup m_resGroup;
        // 当前选择资源组中的资源对象
        private ResObject m_resObject;

        // content
        private GUIContent m_content;
        // GUI color
        private Color m_guiColor;


        [MenuItem("LevelEditor/Open Level Editor Window")]
        public static void OpenLevelEditorWindow()
        {
            Inst = GetWindow<LevelEditorWindow>(false, "Level Editor");
            Inst.titleContent.text = "Level Editor";
        }

        private void Initialize()
        {
            if (Inst == null) OpenLevelEditorWindow();

            // 关卡
            Environment.Inst.Initialize();

            // 资源
            ResManager.Inst.Initialize();

            // 场景界面
            m_sceneWindow = new SceneWindow();
            m_sceneWindow.Initialize();
            // 切换工具事件
            m_sceneWindow.switchToolsEventHandler += OnSwitchToolsEventHandler;

            // TODO
            m_templateGridSize = new Vector2Int(40, 40);

            // 资源预览
            classRecords = new Dictionary<string, bool>();
            m_secondaryGroupId = "";
            m_resGroup = null;
            m_resObject = null;
        }

        /// <summary>
        /// 保存场景
        /// </summary>
        private void SaveScene()
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Utils.GetLevelScenePath(Environment.Inst.levelName));
        }

        /// <summary>
        /// 导出场景配置
        /// </summary>
        private void ExportXml()
        {
            SaveScene();
            Environment.Inst.ExportXml();
        }

        /// <summary>
        /// 检查场景是否符合规则
        /// </summary>
        private bool CheckSceneRuleLegal()
        {
            if (GameObject.Find(typeof(TemplateGrid).Name) == null
                || GameObject.Find(typeof(Environment).Name) == null)
                return false;
            return true;
        }

        /// <summary>
        /// 校正网格模版尺寸
        /// </summary>
        public void CorrectSize(Vector2Int size)
        {
            // 网格规模不能小于2 * 2
            if (size.x < 2) size.x = 2;
            if (size.y < 2) size.y = 2;

            // 网格规模必须是 2 的整数倍
            if (size.x % 2 != 0) size.x += 1;
            if (size.y % 2 != 0) size.y += 1;
        }

        void OnEnable()
        {
            if (!CheckSceneRuleLegal()) return;

            Initialize();

            // 场景GUI事件
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;

            // 打开新场景事件
            EditorSceneManager.sceneOpened += OnSceneOpened;

            // 将资源预览缓存设置为可以同时保存屏幕上所有可见预览的大小
            AssetPreview.SetPreviewTextureCacheSize(1000);
        }

        void OnDestroy()
        {
            Inst = null;

            // 场景主界面
            m_sceneWindow.CloseWindow();
            m_sceneWindow = null;

            // 事件关闭
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
        }

        /// <summary>
        /// 场景打开事件
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            Close();
            if (CheckSceneRuleLegal()) OpenLevelEditorWindow();
        }

        void OnGUI()
        {
            if (Inst == null) return;
            if (Application.isPlaying) return;

            if (!CheckSceneRuleLegal())
            {
                editorEnabled = false;
                sceneWindow.CloseWindow();

                if (GUILayout.Button("Open Create Level Window", GUILayout.Height(30)))
                {
                    CreateWindow.Initialize();
                    Inst.Close();
                }
                return;
            }

            EditorGUILayout.BeginVertical("box");

            // 编辑模式开关按钮
            editorEnabled = GUILayout.Toggle(editorEnabled, "Enable Editor", "Button", GUILayout.Height(30));

            // 场景主界面
            if (editorEnabled) sceneWindow.ShowWindow();
            else sceneWindow.CloseWindow();

            // 保存场景按钮
            EditorGUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Save Scene", GUILayout.Height(30))) SaveScene();

            // 生成场景配置按钮
            if (GUILayout.Button("Export Xml", GUILayout.Height(30))) ExportXml();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // 网格规模尺寸设置
            EditorGUILayout.BeginVertical("box");
            m_templateGridSize = EditorGUILayout.Vector2IntField("Template Grid Size", m_templateGridSize);
            // 校正网格模版尺寸
            CorrectSize(m_templateGridSize);
            // 设置网格模版尺寸
            sceneWindow.templateGrid.width = m_templateGridSize.x;
            sceneWindow.templateGrid.lenght = m_templateGridSize.y;

            EditorGUILayout.EndVertical();

            // 网格高度设置
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Template Grid Height: " + sceneWindow.templateGrid.height.ToString());
            EditorGUILayout.EndVertical();

            // 材料分组按钮
            EditorGUILayout.BeginHorizontal();
            FieldInfo[] fields = typeof(MaterialType).GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                var fieldName = fields[i].Name;
                if (!fieldName.Equals("value__"))
                {
                    var mt = (MaterialType)fields[i].GetValue(fields[i]);
                    if (GUILayout.Toggle(materialType == mt, m_content = new GUIContent(mt.ToString()), "Button", GUILayout.Height(30)))
                    {
                        m_materialType = mt;
                        m_resGroup = ResManager.Inst.GetResGroupByName(mt.ToString());
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // 资源库预览界面
            DrawModelView();

            // 场景重绘
            SceneView.RepaintAll();
        }

        /// <summary>
        /// 画资源库物体预览展示
        /// </summary>
        private void DrawModelView()
        {
            if (m_resGroup == null) return;

            // 二级分组
            if (m_resGroup.haveGroup)
            {
                EditorGUILayout.BeginHorizontal();
                List<ResGroup> groups = m_resGroup.GetGroups();
                for (int i = 0; i < groups.Count; i++)
                {
                    if (GUILayout.Toggle(m_secondaryGroupId == groups[i].id, m_content = new GUIContent(groups[i].GetGroupName()), "Button", GUILayout.Height(20)))
                    {
                        m_secondaryGroupId = groups[i].id;
                        m_resGroup = groups[i];
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                m_secondaryGroupId = "";
            }

            // view 列表
            m_modelViewScrollPosition = EditorGUILayout.BeginScrollView(m_modelViewScrollPosition);

            m_modelViewHorizontalCounter = 0;

            m_modelViewColumn = (int)(Inst.position.width / 200f);
            if (m_modelViewColumn <= 1)
            {
                m_modelViewColumn = 1;
            }

            EditorGUILayout.BeginVertical();

            List<ResClass> classs = m_resGroup.GetClasss();
            for (int i = 0; i < classs.Count; i++)
            {
                var _class = classs[i];

                EditorGUILayout.BeginVertical();
                if (!classRecords.ContainsKey(_class.id))
                {
                    classRecords[_class.id] = true;
                }
                classRecords[_class.id] = GUILayout.Toggle(classRecords[_class.id], m_content = new GUIContent(_class.GetClassName()), "Box", GUILayout.Width(Inst.position.width - 35), GUILayout.Height(20));

                if (classRecords[_class.id])
                {
                    EditorGUILayout.BeginHorizontal();

                    List<ResObject> resObjects = _class.GetResObjects();
                    for (int k = 0; k < resObjects.Count; k++)
                    {
                        var resObject = resObjects[k];

                        EditorGUILayout.BeginVertical();

                        // 模型预览
                        Texture2D previewImage = AssetPreview.GetAssetPreview(resObject.prefab);
                        m_content = new GUIContent(previewImage);
                        // 选中状态
                        bool selected = false;
                        if (m_resObject != null)
                        {
                            if (m_resObject.id == resObject.id)
                            {
                                selected = true;
                            }
                        }

                        bool isSelected = GUILayout.Toggle(selected, m_content, GUI.skin.button);
                        if (isSelected && editorEnabled)
                        {
                            if ((m_resObject != null && m_resObject.id != resObject.id) || m_resObject == null)
                            {
                                // 切换到笔刷工具
                                sceneWindow.SwitchTools(SceneToolsType.Brush);
                                sceneWindow.brushTools.Bind(resObject);
                                sceneWindow.brushWindow.Initialize();
                            }
                            m_resObject = resObject;
                        }

                        EditorGUILayout.BeginHorizontal("Box");
                        EditorGUILayout.LabelField(resObject.GetFileNameWithoutExtension());
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();

                        m_modelViewHorizontalCounter++;
                        if (m_modelViewHorizontalCounter == m_modelViewColumn)
                        {
                            m_modelViewHorizontalCounter = 0;
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                        }

                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        // 设置视图选中资源
        public void SetViewSelected(ResObject resObject)
        {
            m_resObject = resObject;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (!editorEnabled) return;

            // 渲染场景主界面
            sceneWindow.OnSceneGUI(sceneView);
        }

        // 切换工具事件
        void OnSwitchToolsEventHandler(SceneToolsType stt)
        {
            // 笔刷绑定资源
            if (stt == SceneToolsType.Brush)
            {
                sceneWindow.brushTools.Bind(m_resObject);
                sceneWindow.brushWindow.Initialize();
            }
        }
    }
}
