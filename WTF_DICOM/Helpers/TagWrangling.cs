using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FellowOakDicom;

namespace WTF_DICOM.Helpers
{
    public class TagWrangling
    {
        public static List<Tuple<ushort, ushort>> GetGroupsElementsFromTags(List<DicomTag> tagList)
        {
            List<Tuple<ushort, ushort>> groupsAndElements = new();
            foreach (DicomTag tag in tagList)
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
            List<DicomTag> tagList = new();
            foreach (var geTuple in groupsAndElements)
            {
                DicomTag tag = new DicomTag(geTuple.Item1, geTuple.Item2);
                tagList.Add(tag);
            }
            return tagList;
        }

        public static bool valueRepresentationContains(DicomTag? dicomTag, FellowOakDicom.DicomVR valueRepresentation)
        {
            bool isVR = false;

            FellowOakDicom.DicomVR[] valueRepresentations = new DicomVR[1] { FellowOakDicom.DicomVR.NONE };
            if (dicomTag != null)
            {
                valueRepresentations = dicomTag.DictionaryEntry.ValueRepresentations;
                foreach (var vr in valueRepresentations)
                {
                    isVR = isVR || (vr == valueRepresentation);
                }
            }
            return isVR;
        }
        
        public static bool IsSequence(DicomTag dicomTag)
        {
            return Helpers.TagWrangling.valueRepresentationContains(dicomTag, FellowOakDicom.DicomVR.SQ);
        }
        public static bool IsReferencedSequence(DicomTag dicomTag)
        {
            if (dicomTag == null) return false;
            if (dicomTag.DictionaryEntry == null) return false;
            
            string tagInWords = dicomTag.DictionaryEntry.Name;
            if (tagInWords == null) return false;
            return tagInWords.Contains("Referenced"); // hack for now
        }
    }
}
