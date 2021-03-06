﻿using System;
using fo = Dicom;
using DICOMcloud.Pacs.Commands;
using DICOMcloud.Core.Storage;

namespace DICOMcloud.Pacs
{

    public class ObjectRetrieveResult : DicomOperationResult
    {
        public ObjectRetrieveResult ( IStorageLocation location, string transfer ) 
        {
            Location       = location ;
            TransferSyntax = transfer ;
        }

        public IStorageLocation Location { get; set; }
        public string TransferSyntax { get; set; }
    }
}