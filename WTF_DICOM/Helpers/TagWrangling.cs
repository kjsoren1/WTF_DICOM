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

        public static string SequenceRepresentativeString(DicomSequence seq, DicomTag dicomTag)
        {
            string toReturn = "{";
            foreach (DicomDataset dicomDataset in seq)
            {
                string name = "";
                try
                {
                    name = dicomDataset.GetString(dicomTag);
                    toReturn += name;
                    if (!dicomDataset.Equals(seq.Last()))
                    {
                        toReturn += ", ";
                    }
                } catch (Exception ex)
                {
                }
            }
            toReturn += "}";
            return toReturn;
        }

        public static string GetDisplayValueForSequence(DicomSequence seq, DicomTag dicomTag)
        {
            string value = "";
            string tagName = "";
            string nameInsideItem = "";

            if (dicomTag.Equals(DicomTag.BeamSequence))
            {
                nameInsideItem = Helpers.TagWrangling.SequenceRepresentativeString(seq, DicomTag.BeamName);
            }
            else if (dicomTag.Equals(DicomTag.DoseReferenceSequence))
            {
                nameInsideItem = Helpers.TagWrangling.SequenceRepresentativeString(seq, DicomTag.DoseReferenceDescription);
            }
            else if (dicomTag.Equals(DicomTag.StructureSetROISequence))
            {
                nameInsideItem = Helpers.TagWrangling.SequenceRepresentativeString(seq, DicomTag.ROIName);
            }
            else if (dicomTag.Equals(DicomTag.ReferencedDoseSequence))
            {
                nameInsideItem = Helpers.TagWrangling.SequenceRepresentativeString(seq, DicomTag.ReferencedSOPInstanceUID);
            }
            else if (dicomTag.Equals(DicomTag.ReferencedStructureSetSequence))
            {
                nameInsideItem = Helpers.TagWrangling.SequenceRepresentativeString(seq, DicomTag.ReferencedSOPInstanceUID);
            }
            else
            {
                DicomDataset dicomDataset = seq.ElementAt(0);
                foreach (DicomItem dicomItem in dicomDataset)
                {
                    tagName = dicomItem.Tag.DictionaryEntry.Name;
                    StringComparison comparison = StringComparison.OrdinalIgnoreCase;
                    if (tagName.Contains("Name", comparison))
                    {
                        string tempName = dicomDataset.GetString(dicomItem.Tag);
                        if (!tempName.Equals("")) nameInsideItem = tempName;
                    }
                }
            }


            if (Helpers.TagWrangling.IsReferencedSequence(dicomTag))
            {
                value = "Referenced Sequence of " + seq.Count() + " items: " + nameInsideItem;
            }
            else
            {
                value = "Sequence of " + seq.Count() + " items: " + nameInsideItem;
            }
            return value;
        }
    }
}
