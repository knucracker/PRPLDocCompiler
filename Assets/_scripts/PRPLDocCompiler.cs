using System;
using UnityEngine;
using System.Collections.Generic;

public class PRPLDocCompiler {

    private Main main;
    private int cmdCount;
    private string currentSection;
    private string[] docIndexLines;
    private OrderedDictionaryG<string, List<Tuple<int, string>>> sectionDict;
    
    public string resultFile;

    public void MakePRPLDocs(Main main, string srcDir, string outputDir) {
        this.main = main;
        resultFile = null;
        cmdCount = 0;
        currentSection = null;
        sectionDict = new OrderedDictionaryG<string, List<Tuple<int, string>>>();
        System.Text.StringBuilder sbcommands = new System.Text.StringBuilder();

        srcDir = srcDir.Trim();
        outputDir = outputDir.Trim();

        if (srcDir != "" && !srcDir.EndsWith("/") && !srcDir.EndsWith("\\")) {
            srcDir = srcDir + "/";
        }
        if (outputDir != "" && !outputDir.EndsWith("/") && !outputDir.EndsWith("\\")) {
            outputDir = outputDir + "/";
        }

        try {
            docIndexLines = System.IO.File.ReadAllLines(srcDir + "prpldocindex.txt");
            foreach (string i in docIndexLines) {
                ProcessDocFile(srcDir, i.Trim(), sbcommands);
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(@"
<html>
<head>
<style type='text/css'>

    html {font-family: Monaco, Consolas, 'Andale Mono', 'DejaVu Sans Mono', monospace;}
	a {text-decoration: none;}
	a:link {color: green;}
	a:visited {color: green;}
	a:hover {color: blue;}
	a:active {color: black;}	
    code {
        font-size: 90%;
        line-height: 1.2em;
        font-family: Monaco, Consolas, 'Andale Mono', 'DejaVu Sans Mono', monospace;
        white-space: pre;
        white-space: pre-wrap;
        white-space: -moz-pre-wrap;
        white-space: -o-pre-wrap;
        display: block;
        color: #555555;
		background: #f4f4f4;
	}
	.command_syntax { padding: 1px 5px; font-size: 110%; font-weight:bold; background-color: #bfc7cf; color: #000000; width:850px; }
	.command_body{ padding: 1px 1px 50px 15px; }
	.example{ padding: 0px 0px 5px 0px; }
	.description{ padding: 0px 0px 5px 0px; }
	.index {border-spacing:20px;}
	.index tr{vertical-align:top;}
	.section {width: 400px; background-color:#d0d0d0; border-spacing:1px;}
    .section th{ font-size: 120%;}
	.section td{ padding: 0px 0px 0px 4px;}
	.section tr:nth-child(odd) {background-color: #f7f7f7;}
	.section tr:nth-child(even) {background-color: #ffffff;}
	.sectionHead{background-color:#e0e7ef !important;}
	.backtotop{font-size:60%; padding:0; float:right}
</style >
</head >
");

            GenerateDocSections(sb);
            sb.Append("<br/>");
            sb.Append(sbcommands);

            sb.Append(@"</html>");
            System.IO.File.WriteAllText(outputDir + "PRPLDocs.html", sb.ToString());
            resultFile = outputDir + "PRPLDocs.html";
            main.LogMessage("COMPLETE: HTML file created: " + resultFile);
        } catch (Exception e) {
            main.LogMessage("ERROR: " + e.Message);
            main.LogMessage(e.StackTrace);
        }
    }

    private void ProcessDocFile(string srcDir, string sect, System.Text.StringBuilder sbcommands) {
        currentSection = sect;
        string f = srcDir + sect + ".txt";
        main.LogMessage("Processing File: " + f);
        if (System.IO.File.Exists(f)) {
            string[] lines = System.IO.File.ReadAllLines(f);
            for (int i = 0; i < lines.Length; i++) {
                string l = lines[i].TrimStart();
                if (l.StartsWith("=CMD")) {
                    i = ProcessDocCMD(sbcommands, lines, i);
                }
            }
        } else {
            main.LogMessage("File Not Found: " + f);
        }
    }

    private void GenerateDocSections(System.Text.StringBuilder sb) {
        sb.Append("<table id='index' class='index'>\r\n");

        bool open = false;
        for (int i = 0; i < docIndexLines.Length; i++) {
            if (i % 2 == 0) {
                sb.Append("<tr>\r\n");
                open = true;
            }

            sb.Append("<td>\r\n");
            if (!sectionDict.ContainsKey(docIndexLines[i].Trim())) {
                sb.Append("&nbsp;");
            } else {
                GenerateDocSection(sb, docIndexLines[i].Trim());
            }
            sb.Append("</td>\r\n");

            if (i % 2 == 1) {
                sb.Append("</tr>\r\n");
                open = false;
            }
        }
        if (open) {
            sb.Append("</tr>\r\n");
            open = false;
        }
        /*
        sb.Append("<tr>\r\n");
        sb.Append("<td>\r\n");
        GenerateDocSection(sb, "Vars and Functions");
        sb.Append("</td>\r\n");

        sb.Append("<td>\r\n");
        GenerateDocSection(sb, "Logic");
        sb.Append("</td>\r\n");

        sb.Append("</tr>\r\n");
        */

        sb.Append("</table>");

    }

    private void GenerateDocSection(System.Text.StringBuilder sb, string section) {
        int cols = 2;
        sb.Append("<table class='section'>\r\n");
        sb.Append("<thead>\r\n");
        sb.Append("<th colspan='" + cols.ToString() +"'>" + section + "</th>\r\n");
        sb.Append("</thead>\r\n");
        List<Tuple<int, string>> list = sectionDict[section];
        int cc = 0;
        bool open = false;
        foreach (Tuple<int, string> tup in list) {
            if (cc % cols == 0) {
                sb.Append("<tr>\r\n");
                open = true;
            }

            string cs;
            if (cc == list.Count - 1) {
                cs = "colspan='" + (cols - (cc % cols)).ToString() + "'";
            } else {
                cs = "";
            }
            sb.Append("<td " + cs + " ><a href='#cmd" + tup.Item1.ToString() + "'>" + tup.Item2 + "</a></td>\r\n");

            if (cc % cols == cols-1) {
                sb.Append("</tr>\r\n");
                open = false;
            }
            cc++;
        }
        if (open) {
            sb.Append("</tr>\r\n");
            open = false;
        }
        sb.Append("</table>\r\n");
    }


    private int ProcessDocCMD(System.Text.StringBuilder sb, string[] lines, int pos) {
        sb.Append(@"
<div class='command'>
");
        string startingSection = currentSection;
        int i;
        for (i = pos; i < lines.Length; i++) {
            string l = lines[i].TrimStart();
            if (l.StartsWith("=ENDCMD")) {
                break;
            } else
            if (l.StartsWith("=CMDCLASS")) {
                ProcessDocCMDCLASS(sb, l);
            } else
            if (l.StartsWith("=COMMAND")) {
                ProcessDocCOMMAND(sb, l);
            } else
            if (l.StartsWith("=DESC")) {
                i = ProcessDocDESC(sb, lines, i);
            } else
            if (l.StartsWith("=EX")) {
                i = ProcessDocEX(sb, lines, i);
            }
        }
        currentSection = startingSection;

        sb.Append("</div>\r\n");
        sb.Append("</div>\r\n");

        return i;
    }

    private void ProcessDocCMDCLASS(System.Text.StringBuilder sb, string l) {
        string sect = l.Substring("=CMDCLASS".Length).Trim();
        if (sect != "") {
            currentSection = sect;
        }
        //sb.Append("<a name='"+sect+"'></a>\r\n");
    }

    private void ProcessDocCOMMAND(System.Text.StringBuilder sb, string l) {
        string cmd = l.Substring("=COMMAND".Length).Trim();
        string[] splitcmd = cmd.Split(' ', '(');
        List<Tuple<int, string>> section = null;
        if (sectionDict.ContainsKey(currentSection)) {
            section = sectionDict[currentSection];
        } else {
            section = new List<Tuple<int, string>>();
            sectionDict[currentSection] = section;
        }
        section.Add(new Tuple<int, string>(cmdCount, splitcmd[0]));
        sb.Append("<div id='cmd" + cmdCount.ToString() + "' class='command_syntax'>\r\n");
        sb.Append(cmd);
        sb.Append("<span class='backtotop'><a href='#index'>[TOP]</a></span>");
        sb.Append("</div>\r\n");
        cmdCount++;
    }

    private int ProcessDocDESC(System.Text.StringBuilder sb, string[] lines, int pos) {
        sb.Append("<div class='command_body'>\r\n");
        sb.Append("<div class='description'>\r\n");
        sb.Append("<b>Description</b>\r\n");
        sb.Append("<div>\r\n");
        int i;
        for (i = pos + 1; i < lines.Length; i++) {
            if (lines[i].Trim() == "=ENDDESC") {
                break;
            } else {
                //sb.Append(lines[i].TrimStart().Substring(2) + " ");
                sb.Append(lines[i].TrimEnd() + " ");
            }
        }
        sb.Append("</div>\r\n");
        sb.Append("</div>\r\n");

        sb.Append("<div>\r\n");
        sb.Append("<b>Examples</b>\r\n");
        sb.Append("</div>\r\n");
        return i;
    }

    private int ProcessDocEX(System.Text.StringBuilder sb, string[] lines, int pos) {
        sb.Append("<div class='example'>\r\n");
        sb.Append("<code>");
        int i;
        for (i = pos + 1; i < lines.Length; i++) {
            if (lines[i].Trim() == "=ENDEX") {
                break;
            } else {
                //sb.Append(lines[i].TrimStart().Substring(2) + "\r\n");
                sb.Append(lines[i].TrimEnd() + "\r\n");
            }
        }
        sb.Append("</code>\r\n");
        sb.Append("</div>\r\n");
        return i;
    }


}
