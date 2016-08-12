using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

public static class SerializationHelper<T> where T : class, new()
{
    public static void SaveToXML(string FileName, T objectToSave)
    {
        using (TextWriter tw = new StreamWriter(FileName))
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            xs.Serialize(tw, objectToSave);
        }
    }
	
	
    public static T LoadFromXML(string FileName)
    {
        T DataLoad = new T();

        using (TextReader tr = new StreamReader(FileName))
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            DataLoad = xs.Deserialize(tr) as T;
        }

        return DataLoad;
    }
}

