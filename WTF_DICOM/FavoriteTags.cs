using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FellowOakDicom;

namespace WTF_DICOM
{
    [Serializable]
    public class FavoriteTags
    {
        public List<Tuple<ushort, ushort>> GroupsAndElements {  get; set; } = new List<Tuple<ushort, ushort>>();


    }
}
