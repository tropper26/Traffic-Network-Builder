using System.Xml;

namespace OpenDriveXMLGenerator
{
    public class XODRConnection : XODRBase
    {
        public XODRConnection(XmlElement element) : base(element) { }
    }

    public static class XODRConnectionExtentions
    {
        public static XODRLaneLink AddLaneLinkElement(this XODRConnection parent, int from, int to)
        {
            var laneLink = new XODRLaneLink(parent.OwnerDocument.CreateElement("laneLink"));
            laneLink.SetAttribute("from", from.ToString());
            laneLink.SetAttribute("to", to.ToString());
            parent.AppendChild(laneLink.XmlElement);

            return laneLink;
        }
    }

    public class XODRLaneLink : XODRBase
    {
        public XODRLaneLink(XmlElement element) : base(element) { }
    }
}