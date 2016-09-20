﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Core.Storage;
using fo = Dicom;
using Dicom.Imaging ;
using Dicom.Imaging.Codec;

namespace DICOMcloud.Dicom.Media
{
    public class NativeMediaWriter : DicomMediaWriterBase
    {
        public NativeMediaWriter ( ) : base ( ) {}
         
        public NativeMediaWriter ( IMediaStorageService mediaStorage ) : base ( mediaStorage ) 
        {
        }

        public override string MediaType 
        { 
            get 
            {
                return MimeMediaTypes.DICOM ;
            }
        }

        protected override bool StoreMultiFrames
        {
            get
            {
                return false ;
            }
        }

        protected override fo.DicomDataset GetMediaDataset ( fo.DicomDataset data, DicomMediaProperties mediaInfo  )
        {
            if ( mediaInfo.MediaType != MediaType )
            {
                throw new InvalidOperationException ( string.Format ( "Invalid media type. Supported media type is:{0} and provided media type is:{1}",
                                                      MediaType, mediaInfo.MediaType ) ) ;
            }

            if ( !string.IsNullOrWhiteSpace ( mediaInfo.TransferSyntax ) )
            {
                return data.Clone ( fo.DicomTransferSyntax.Parse ( mediaInfo.TransferSyntax ) ) ;
            }

            return base.GetMediaDataset ( data, mediaInfo );
        }

        protected override void Upload( fo.DicomDataset dicomDataset, int frame, IStorageLocation location )
        {
            fo.DicomFile df = new fo.DicomFile ( dicomDataset ) ;


            using (Stream stream = new MemoryStream())
            {
                df.Save(stream);
                stream.Position = 0;

                location.Upload(stream);
            }
        }
    }
}
