using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class Main : MonoBehaviour {

    public InputField srcInputField;
    public InputField outputInputField;
    public InputField log;
    public Text version;
    public Button HTMLButton;

    private string logFile;
    private PRPLDocCompiler compiler;

    void Awake() {
        string[] args = System.Environment.GetCommandLineArgs();
        /*       
         *        check if started from commnad line with arguments for source and output directories.
         *        -srcdir=<source path and filename>
         *        -outdir=<output path and filename> can be same as source, since filename/type is different.
         *        
        /**/
        foreach (string arg in args) {

            LogMessage("Launch ARG: " + arg);
            if (arg.ToLower().StartsWith("-srcdir=")) {
 //             srcInputField.text = arg.Substring(8, arg.Length-8);
                PlayerPrefs.SetString("srcDir", arg.Substring(8, arg.Length - 8)); // input arguments override stored value
            }
            if (arg.ToLower().StartsWith("-outdir=")) {
//                outputInputField.text = arg.Substring(8, arg.Length - 8);
                PlayerPrefs.SetString("outDir", arg.Substring(8, arg.Length - 8)); // input arguments override stored value
            }

        }

          logFile =  "PRPLDocCompiler_Data/output_log.txt";  // location of Unity3D output_log.txt on Windows-only machines
          version.text = "GMK.01";                           // Display at bottom of pane to indicate version.
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

        // set view buttons active.
        HTMLButton.interactable = true;
    }

    public void OnViewHTMLClicked()
    {
        if (compiler != null && compiler.resultHTMLFile != null)
        {
            Application.OpenURL(compiler.resultHTMLFile);
        }
    }

    public void OnViewLogClicked()
    {
        if (File.Exists(logFile)) {
            logFile = Path.GetFullPath(logFile);
            Application.OpenURL(logFile);
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
