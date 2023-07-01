using System.Text;
using System.Collections.Generic;
using DateTime = System.DateTime;
using UnityEngine;
using UnityEngine.UI;
using ProjectMax.CSharp;
using ProjectMax.Unity;

public class LoggingModule : MonoBehaviour
{
    [SerializeField]
    private bool usable = false;

    private void Start(){
        Singleton<LoggingModule>.SetInstance(this, true, Init);
    }

    private void Init(){
        Application.logMessageReceived += OnReceivedLog;
        // Application.logMessageReceivedThreaded += OnReceivedLog;
        if(usable){
            _consolePanel.SetActive(false);
            _consoleButton.SetActive(true);
            Help();
        }
        else{
            _consolePanel.SetActive(false);
            _consoleButton.SetActive(false);
        }
    }

    private void Update(){
        CheckInputEnter();
    }

    private struct Log
    {
        public readonly string condition;
        public readonly string stackTrace;
        public readonly LogType type;

        public Log(string condition, string stackTrace, LogType type){
            this.condition = condition;
            this.stackTrace = stackTrace;
            this.type = type;
        }
    }

    private Dictionary<Log, List<DateTime>> _logDict = new Dictionary<Log, List<DateTime>>();

    private void OnReceivedLog(string condition, string stackTrace, LogType type){
        Log log = new Log(condition, stackTrace, type);
        if(_logDict.TryGetValue(log, out List<DateTime> info)){
            info.Add(DateTime.Now);
        }
        else{
            info = new List<DateTime>();
            info.Add(DateTime.Now);
            _logDict.Add(log, info);
        }
        UpdateConsole();
    }

    [SerializeField]
    private Text _outputText = null;

    private bool _details = false;
    private bool _collapse = true;

    private void UpdateConsole(){
        List<PrintData> prt = new List<PrintData>();
        if(_collapse){
            foreach(var key in _logDict.Keys){
                var list = _logDict[key];
                list.Sort();
                prt.Add(new PrintData{log = key, time = list[list.Count - 1]});
            }
        }
        else{
            foreach(var pair in _logDict){
                foreach(DateTime dt in pair.Value){
                    prt.Add(new PrintData{log = pair.Key, time = dt});
                }
            }
        }
        prt.Sort((x, y) => x.time.CompareTo(y.time));
        StringBuilder sb = new StringBuilder();
        foreach(var p in prt){
            string c = "";
            switch(p.log.type){
                case LogType.Error:
                    c = "red";
                    break;
                case LogType.Assert:
                    c = "orange";
                    break;
                case LogType.Warning:
                    c = "yellow";
                    break;
                case LogType.Log:
                    c = "black";
                    break;
                case LogType.Exception:
                    c = "cyan";
                    break;
            }
            string time = p.time.ToString("HH:mm:ss.fff");
            string info = $"[{time}] {p.log.condition}";
            if(_collapse){
                info = $"[{time}] [{_logDict[p.log].Count}] {p.log.condition}";
            }
            if(_details){
                sb.AppendLine(info.ToRichText(c: c, b: true));
                sb.AppendLine(p.log.stackTrace.ToRichText(c: c));
            }
            else{
                sb.AppendLine(info.ToRichText(c: c));
            }
        }
        _outputText.text = sb.ToString();
    }

    private class PrintData
    {
        public Log log;
        public DateTime time;
    }

    [SerializeField]
    private InputField _inputText = null;

    private void CheckInputEnter(){
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)){
            if(_inputText){
                if(_inputText.text.IsNullOrEmpty()){
                    // Help();
                }
                else{
                    Debug.Log($"console {_inputText.text}");
                    Command(_inputText.text);
                    _inputText.text = "";
                }
            }
        }
    }

    [SerializeField]
    private GameObject _consolePanel = null;

    [SerializeField]
    private GameObject _consoleButton = null;

    private void Command(string str){
        if(str.Contains("-h")){
            Help();
        }
        if(str.Contains("-l")){
            _collapse = !_collapse;
            UpdateConsole();
        }
        // if(str.Contains("-d")){
        //     _details = !_details;
        //     UpdateConsole();
        // }
        if(str.Contains("-c")){
            _logDict = new Dictionary<Log, List<DateTime>>();
            UpdateConsole();
        }
        if(str.Contains("-x")){
            _consolePanel.SetActive(false);
            _consoleButton.SetActive(true);
        }
    }

    private void Help(){
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("command list");
        sb.AppendLine("---------------------");
        sb.AppendLine("enter -h to show help");
        sb.AppendLine("enter -l to collapse/uncollapse");
        // sb.AppendLine("enter -d to enable/disable details");
        sb.AppendLine("enter -c to clear console");
        sb.AppendLine("enter -x to hide console");
        sb.AppendLine("---------------------");
        Debug.Log(sb.ToString().Trim());
    }

    public void OnViewClick(bool view = true){
        _consolePanel.SetActive(view);
        _consoleButton.SetActive(!view);
    }
}
