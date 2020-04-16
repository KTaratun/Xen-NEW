using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class Act
{
    [XmlAttribute("Name")]
    public string m_name;
    [XmlElement("Energy")]
    public string m_energy;
    [XmlElement("DMG")]
    public int m_damage;
    [XmlElement("RNG")]
    public int m_range;
    [XmlElement("RAD")]
    public int m_radius;
    [XmlElement("Description")]
    public string m_effect; // Description

    // Start is called before the first frame update
}
