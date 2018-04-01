using UnityEngine;
using System.IO;
using System;

public class HandleTextFile {

    public static void writeTextToFile(string textName, string data) {
        TextWriter tw = new StreamWriter(Application.persistentDataPath + @"/" + textName + ".sav");
        tw.Write(data);
        tw.Close();
    }
    
    public static string readFromTextFile(string textName) {
        string data = "";
        //Read the text from directly from the test.txt file
        try {
            StreamReader reader = new StreamReader(Application.persistentDataPath + @"/" + textName + ".sav");
            data = reader.ReadToEnd();
            reader.Close();
        } catch (Exception e) {
            return "";
        }

        return data;
    }

}