using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FellowOakDicom;

namespace WTF_DICOM
{
    [Serializable]
    public class DisplayTemplate
    {
        //public List<DicomTag> TagColumnsToDisplay { get; set; } = new(); // doesn't serialize properly
        public List<Tuple<ushort, ushort>> GroupsAndElements {  get; set; } = new List<Tuple<ushort, ushort>>();
        public List<MainWindowViewModel.NonTagColumnTypes> NonTagColumnsToDisplay { get; set; } = new();


        public static List<Tuple<ushort, ushort>> GetGroupsElementsFromTags(List<DicomTag> tagColumnsToDisplay)
        {
            List<Tuple<ushort, ushort>> groupsAndElements = new();
            foreach(DicomTag tag in tagColumnsToDisplay)
            {
                ushort group = tag.Group;
                ushort element = tag.Element;
                Tuple<ushort, ushort> geTuple = new Tuple<ushort, ushort>(group, element);
                groupsAndElements.Add(geTuple);
            }
            return groupsAndElements;
        }

        public static List<DicomTag> GetTagsFromGroupsAndElements(List<Tuple<ushort, ushort>> groupsAndElements)
        {
            List<DicomTag> tagColumnsToDisplay = new();
            foreach(var geTuple in groupsAndElements)
            {
                DicomTag tag = new DicomTag(geTuple.Item1, geTuple.Item2);
                tagColumnsToDisplay.Add(tag);
            }
            return tagColumnsToDisplay;
        }
    }
}
