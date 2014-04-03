using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;


namespace OpcodeStarter.Models
{
    // This file class takes care of all serialization and writing to disk
    public class File
    {
        private string fileName;
        
        public File(string name)
        {
            fileName = name;
        }
        // Serializes a generic object to XML and writes it to disk
        public bool WriteObjectToFile <T> (List<T> node)
        {
            var emptyNs = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));

            using (TextWriter writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, node, emptyNs);
            }  

            return true;
        }

        // Grabs xml from disk and deserializes it into a valid object
        public bool GetObjectsFromFile <T> (ref List<T> nodes)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(List<T>));
            try
            {
                TextReader reader = new StreamReader(fileName);
                nodes = (List<T>)deserializer.Deserialize(reader);
                reader.Close();
            }
            catch { }

            return false;
        }
    }
}