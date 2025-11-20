using System.Collections.ObjectModel;

using FellowOakDicom;

using WTF_DICOM.Models;

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

        // NOTE: C:\Users\Kelly\source\repos\DicomTestFiles\SlicerRtData\eclipse-11-lung-contours-on-same-slices 
        // contains private tags with all ValueRepresentations so this check is not a guarantee
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

        public static bool ContainsReferencedSOPInstanceUID(WTFDicomItem dicomItem)
        {
            if (dicomItem == null) return false;
            if (dicomItem.Tag.Equals(DicomTag.ReferencedSOPInstanceUID)) return true;
            if (!dicomItem.IsSequence) return false;

            foreach (DicomDataset dicomDataset in dicomItem.MyDicomSequence)
            {
                return ContainsReferencedSOPInstanceUID(dicomDataset);
            }

            return false;
        }

        public static bool ContainsReferencedSOPInstanceUID(DicomDataset dicomDataset)
        {
            if (dicomDataset == null) return false;

            foreach (var dicomItem in dicomDataset)
            {
                DicomTag dicomTag = dicomItem.Tag;
                if (dicomTag.Equals(DicomTag.ReferencedSOPInstanceUID))
                {
                    return true;
                }
                else if (IsSequence(dicomTag))
                {
                    try
                    {
                        DicomSequence seq = dicomDataset.GetSequence(dicomTag);
                        foreach (DicomDataset dataset in seq)
                        {
                            if (ContainsReferencedSOPInstanceUID(dataset))
                            {
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        // NOTE: C:\Users\Kelly\source\repos\DicomTestFiles\SlicerRtData\eclipse-11-lung-contours-on-same-slices 
                        // contains private tags with all ValueRepresentations so the IsSequence() check is not a guarantee
                        // this try-catch guards against the case where it was not actually a sequence
                        return false;
                    }
                }
            }

            return false;
        }

        public static List<DicomItem> GetAllReferencedSOPInstanceUID(WTFDicomItem dicomItem)
        {
            List<DicomItem> references = new List<DicomItem>();
            foreach (DicomDataset dicomDataset in dicomItem.MyDicomSequence)
            {
                GetAllReferencedSOPInstanceUID(dicomDataset, references);
            }
            return references;
        }

        public static List<DicomItem> GetAllReferencedSOPInstanceUID(DicomFileCommon dicomFileCommon)
        {
            List<DicomItem> references = new List<DicomItem>();
            GetAllReferencedSOPInstanceUID(dicomFileCommon.OpenedFile.Dataset, references);
            return references;
        }

        public static void GetAllReferencedSOPInstanceUID(DicomDataset dicomDataset, List<DicomItem> referencedSOPInstanceUIDItems)
        {
            if (dicomDataset == null) return;
            if (referencedSOPInstanceUIDItems == null)
            {
                referencedSOPInstanceUIDItems = new List<DicomItem>();
            }

            foreach (var dicomItem in dicomDataset)
            {
                DicomTag dicomTag = dicomItem.Tag;
                if (dicomTag.Equals(DicomTag.ReferencedSOPInstanceUID))
                {
                    referencedSOPInstanceUIDItems.Add(dicomItem);
                }
                else if (IsSequence(dicomItem.Tag))
                {
                    DicomSequence seq = dicomDataset.GetSequence(dicomTag);
                    foreach (DicomDataset innerDataset in seq)
                    {
                        GetAllReferencedSOPInstanceUID(innerDataset, referencedSOPInstanceUIDItems);
                    }
                }
            }
        }

        public static ObservableCollection<ReferencedSOPInstanceUIDInfo> GetReferencedSOPInstanceUIDs(WTFDicomItem tag, ObservableCollection<DicomFileCommon> dicomFiles)
        {
            if (tag == null) return new ObservableCollection<ReferencedSOPInstanceUIDInfo>();

            List<DicomItem> referenceDicomItems = GetAllReferencedSOPInstanceUID(tag);
            return GetReferencedSOPInstanceUIDs(referenceDicomItems, dicomFiles);
        }

        public static ObservableCollection<ReferencedSOPInstanceUIDInfo> GetReferencedSOPInstanceUIDs(DicomFileCommon dicomFileCommon, ObservableCollection<DicomFileCommon> dicomFiles)
        {
            if (dicomFileCommon == null) return new ObservableCollection<ReferencedSOPInstanceUIDInfo>();

            List<DicomItem> referenceDicomItems = GetAllReferencedSOPInstanceUID(dicomFileCommon);
            return GetReferencedSOPInstanceUIDs(referenceDicomItems, dicomFiles);
        }


        public static ObservableCollection<ReferencedSOPInstanceUIDInfo> GetReferencedSOPInstanceUIDs(List<DicomItem> referenceDicomItems, ObservableCollection<DicomFileCommon> dicomFiles)
        {               
            ObservableCollection<ReferencedSOPInstanceUIDInfo> referencedFiles = new ObservableCollection<ReferencedSOPInstanceUIDInfo>();
            foreach (DicomItem item in referenceDicomItems)
            {
                string referencedSOPInstanceUID = "";
                if (item is DicomElement element)
                {
                    referencedSOPInstanceUID = element.Get<string>();
                }

                bool found = false;
                // try to find file in our list of known files
                foreach (DicomFileCommon dicomFileCommon in dicomFiles)
                {
                    if (referencedSOPInstanceUID.Equals(dicomFileCommon.SOPInstanceUID))
                    {
                        ReferencedSOPInstanceUIDInfo referencedFile =
                            new ReferencedSOPInstanceUIDInfo(referencedSOPInstanceUID, dicomFileCommon.DicomFileName, dicomFileCommon.Modality);
                        referencedFiles.Add(referencedFile);
                        found = true;
                        break;
                    }
                    foreach (DicomFileCommon relatedFile in dicomFileCommon.RelatedDicomFiles)
                    {
                        if (referencedSOPInstanceUID.Equals(relatedFile.SOPInstanceUID))
                        {
                            ReferencedSOPInstanceUIDInfo referencedFile =
                                new ReferencedSOPInstanceUIDInfo(referencedSOPInstanceUID, relatedFile.DicomFileName, relatedFile.Modality);
                            referencedFiles.Add(referencedFile);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    ReferencedSOPInstanceUIDInfo referencedFile =
                        new ReferencedSOPInstanceUIDInfo(referencedSOPInstanceUID, "no file found", "unknown");
                    referencedFiles.Add(referencedFile);
                }
            }
            return referencedFiles;
        }


        public static int SelectReferencedSOPInstanceUIDs(DicomFileCommon dicomFileCommon, ObservableCollection<DicomFileCommon> dicomFiles)
        {
            if (dicomFileCommon == null) return -1;

            List<DicomItem> referenceDicomItems = GetAllReferencedSOPInstanceUID(dicomFileCommon);
            return SelectReferencedSOPInstanceUIDs(referenceDicomItems, dicomFiles);
        }

        public static int SelectReferencedSOPInstanceUIDs(List<DicomItem> referenceDicomItems, ObservableCollection<DicomFileCommon> dicomFiles)
        {      
            int nReferencedAndNotFound = 0;
            foreach (DicomItem item in referenceDicomItems)
            {
                string referencedSOPInstanceUID = "";
                if (item is DicomElement element)
                {
                    referencedSOPInstanceUID = element.Get<string>();
                }

                bool found = false;
                // try to find file in our list of known files
                foreach (DicomFileCommon dicomFileCommon in dicomFiles)
                {
                    if (referencedSOPInstanceUID.Equals(dicomFileCommon.SOPInstanceUID))
                    {
                        dicomFileCommon.Selected = true;
                        found = true;
                        break;
                    }
                    foreach (DicomFileCommon relatedFile in dicomFileCommon.RelatedDicomFiles)
                    {
                        if (referencedSOPInstanceUID.Equals(relatedFile.SOPInstanceUID))
                        {
                            relatedFile.Selected = true;
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    nReferencedAndNotFound++;
                }
            }
            return nReferencedAndNotFound;
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
                }
                catch (Exception ex)
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
