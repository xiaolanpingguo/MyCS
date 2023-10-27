using System;
using System.Collections.Generic;
using System.IO;
using Lockstep.Util;
using UnityEngine;
using Debug = Lockstep.Logging.Debug;
using Lockstep.Logging;
#if UNITY_5_3_OR_NEWER
using UnityEditor;
using Lockstep.Game;

#endif

namespace Lockstep.CodeGenerator {


    public class CodeGenHelper {
        public class CodeGenInfos {
            public GenInfo[] GenInfos;
        }

        public static void Gen(string[] args){
            if (args == null || args.Length == 0) {
                args = new[] {"../Config/CodeGenerator/Config.json"};
            }

            if (args.Length > 0) {
                foreach (var path in args) {
                    Debug.Log(path);
                    CopyFilesByConfig(Path.Combine(Define.BaseDirectory, path));
                }
            }
            else {
                Debug.Log("Need config path");
            }
        }

        static void CopyFilesByConfig(string configPath){
            var allTxt = File.ReadAllText(configPath);
            var config = JsonUtil.ToObject<CodeGenInfos>(allTxt);
            var prefix = Define.BaseDirectory;
            foreach (var genInfo in config.GenInfos) {
                GenCode(genInfo);
            }
        }

        static void GenCode(GenInfo info){
            EditorBaseCodeGenerator gener = null;
            if (info == null || string.IsNullOrEmpty(info.GenerateFileName)) return;
            var path = Path.Combine(Define.BaseDirectory, info.TypeHandlerConfigPath);
            Debug.Log(path);
            var allTxt = File.ReadAllText(path);
            var config = JsonUtil.ToObject<FileHandlerInfo>(allTxt);
            info.FileHandlerInfo = config;
            gener = new EditorBaseCodeGenerator(info) { };
            gener.HideGenerateCodes();
            gener.BuildProject();
            gener.GenerateCodeNodeData(true);
        }
    }
#if !UNITY_5_3_OR_NEWER
    internal class Program {
        public static void Main(string[] args){
            CodeGenHelper.Gen(args);
        }
    }
#else

    public static class EditorCodeGen {
        [MenuItem("LPEngine/CodeGen")]
        static void CodeGen(){
            Lockstep.Logging.Logger.OnMessage += OnLog;
            var config = Resources.Load<CodeGenConfig>("CodeGenerator/CodeGenConfig");
            Define.RelPath = config.relPath;
            var path = Define.BaseDirectory;
            CodeGenHelper.Gen(config.args.Split(';'));
        }

        private static void OnLog(object sender, LogEventArgs args)
        {
            switch (args.LogSeverity)
            {
                case LogSeverity.Info:
                    UnityEngine.Debug.Log(args.Message);
                    break;
                case LogSeverity.Warn:
                    UnityEngine.Debug.LogWarning(args.Message);
                    break;
                case LogSeverity.Error:
                    UnityEngine.Debug.LogError(args.Message);
                    break;
                case LogSeverity.Exception:
                    UnityEngine.Debug.LogError(args.Message);
                    break;
            }
        }
    }
#endif
}