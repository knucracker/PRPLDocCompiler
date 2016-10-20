using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    public InputField srcInputField;
    public InputField outputInputField;
    public InputField log;

    private PRPLDocCompiler compiler;

    void Awake() {
        string[] args = System.Environment.GetCommandLineArgs();

        foreach(string arg in args) {
            LogMessage("Launch ARG: " + arg);
            if (arg.ToLower().StartsWith("-srcdir=")) {
                srcInputField.text = arg.Substring(8, arg.Length-8);
            }
            if (arg.ToLower().StartsWith("-outdir=")) {
                outputInputField.text = arg.Substring(8, arg.Length - 8);
            }
        }
    }

    void Start() {
        if (PlayerPrefs.HasKey("srcDir")) {
            srcInputField.text = PlayerPrefs.GetString("srcDir");
        }
        if (PlayerPrefs.HasKey("outDir")) {
            outputInputField.text = PlayerPrefs.GetString("outDir");
        }
    }

    public void OnCompileClicked() {
        PlayerPrefs.SetString("srcDir", srcInputField.text);
        PlayerPrefs.SetString("outDir", outputInputField.text);

        if (compiler == null) compiler = new PRPLDocCompiler();
        ClearLog();
        compiler.MakePRPLDocs(this, srcInputField.text, outputInputField.text);
    }

    public void OnViewHTMLClicked() {
        if (compiler != null && compiler.resultFile != null) {
            Application.OpenURL(compiler.resultFile);
        }
    }

    public void OnClearLogClicked() {
        ClearLog();
    }

    public void LogMessage(string message) {
        log.text += message + "\n";
    }

    private void ClearLog() {
        log.text = "";
    }

}
