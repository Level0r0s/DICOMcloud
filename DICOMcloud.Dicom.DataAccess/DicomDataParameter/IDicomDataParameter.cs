﻿using fo = Dicom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Dicom.DataAccess
{
    public interface IDicomDataParameter
    {
        uint        KeyTag            { get; set ; }
        fo.DicomVR  VR                { get; set ; }
        bool        AllowExtraElement { get; set ; }
        IList<uint> SupportedTags     { get ; }

        bool                  IsSupported     ( fo.DicomItem element );
        void                  SetElement      ( fo.DicomItem element ) ;
        string[]              GetValues       ( ) ;
        List<PersonNameData>  GetPNValues     ( ) ;
        IDicomDataParameter   CreateParameter ( ) ;
        IList<fo.DicomItem> Elements { get; set ; }
    
    }
}
