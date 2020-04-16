using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("Actions")]
public class ActionContainerScript
{
    [XmlArray("White")]
    [XmlArrayItem("Action")]
    public List<Act> m_whiteActions = new List<Act>();
    [XmlArray("Green")]
    [XmlArrayItem("Action")]
    public List<Act> m_greenActions = new List<Act>();
    [XmlArray("Blue")]
    [XmlArrayItem("Action")]
    public List<Act> m_blueActions = new List<Act>();
    [XmlArray("Red")]
    [XmlArrayItem("Action")]
    public List<Act> m_redActions = new List<Act>();


    public static ActionContainerScript Load()
    {
        TextAsset xml = Resources.Load<TextAsset>("Actions");

        XmlSerializer serializer = new XmlSerializer(typeof(ActionContainerScript));

        StringReader reader = new StringReader(xml.text);
       
        ActionContainerScript aC = serializer.Deserialize(reader) as ActionContainerScript;
        
        reader.Close();
        
        return aC;
    }
}
