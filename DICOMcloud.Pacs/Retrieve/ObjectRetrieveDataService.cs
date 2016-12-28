﻿using DICOMcloud.Core.Storage;
using DICOMcloud.Dicom.Data;
using DICOMcloud.Dicom.Media;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using fo = Dicom ;
using Dicom.Imaging.Codec ;

namespace DICOMcloud.Pacs
{
    public class ObjectRetrieveDataService : IObjectRetrieveDataService
    {
        public virtual IMediaStorageService     StorageService     { get; protected set; }
        public virtual IDicomMediaWriterFactory MediaWriterFactory { get; protected set ; }
        public virtual IDicomMediaIdFactory     MediaFactory       { get; protected set ; }
        public virtual string AnyTransferSyntaxValue               { get; set; }

        public ObjectRetrieveDataService 
        ( 
            IMediaStorageService mediaStorage, 
            IDicomMediaWriterFactory mediaWriterFactory, 
            IDicomMediaIdFactory mediaFactory 
        )
        {
            AnyTransferSyntaxValue = "*" ;
            
            StorageService     = mediaStorage ;
            MediaWriterFactory = mediaWriterFactory ;
            MediaFactory       = mediaFactory ;
        }
        
        public virtual IStorageLocation RetrieveSopInstance ( IObjectId query, DicomMediaProperties mediaInfo ) 
        {
            return StorageService.GetLocation ( MediaFactory.Create (query, mediaInfo ) ) ;
        }
        
        public virtual IEnumerable<IStorageLocation> RetrieveSopInstances ( IObjectId query, DicomMediaProperties mediaInfo ) 
        {
            return StorageService.EnumerateLocation ( MediaFactory.Create ( query, mediaInfo )) ;
        }

        public virtual IEnumerable<ObjectRetrieveResult> FindSopInstances
        ( 
            IObjectId query, 
            string mediaType, 
            IEnumerable<string> transferSyntaxes, 
            string defaultTransfer
        ) 
        {
            foreach ( var transfer in transferSyntaxes )
            {
                string instanceTransfer = (transfer == AnyTransferSyntaxValue) ? defaultTransfer : transfer ;

                var    mediaProperties = new DicomMediaProperties ( mediaType, instanceTransfer ) ;
                var    mediaID         = MediaFactory.Create      ( query, mediaProperties ) ;
                var    found           = false ;
                
                foreach ( IStorageLocation location in StorageService.EnumerateLocation ( mediaID ) )
                {
                    found = true ;

                    yield return new ObjectRetrieveResult ( location, transfer ) ;
                }
                
                if (found)
                {
                    break ;
                }
            }
        }

        public virtual IEnumerable<ObjectRetrieveResult> GetTransformedSopInstances 
        ( 
            IObjectId query, 
            string fromMediaType, 
            string fromTransferSyntax, 
            string toMediaType, 
            string toTransferSyntax 
        ) 
        {
            var fromMediaProp = new DicomMediaProperties ( fromMediaType, fromTransferSyntax ) ;
            var fromMediaID   = MediaFactory.Create      ( query, fromMediaProp ) ;
            var frameList     = ( null != query.Frame ) ? new int[] { query.Frame.Value } : null ;
            
             
            if ( StorageService.Exists ( fromMediaID ) ) 
            {
                foreach ( IStorageLocation location in StorageService.EnumerateLocation ( fromMediaID ) )
                {
                    fo.DicomFile defaultFile = fo.DicomFile.Open ( location.GetReadStream ( ) ) ;

                    foreach ( var transformedLocation in  TransformDataset ( defaultFile.Dataset, toMediaType, toTransferSyntax, frameList ) )
                    {
                        yield return new ObjectRetrieveResult ( transformedLocation, toTransferSyntax ) ; 
                    }
                }
            }
        }

        public virtual IEnumerable<IStorageLocation> TransformDataset 
        ( 
            fo.DicomDataset dataset, 
            string mediaType, 
            string instanceTransfer, 
            int[] frameList = null 
        ) 
        {
            var mediaProperties  = new DicomMediaProperties ( mediaType, instanceTransfer ) ;
            var writerParams     = new DicomMediaWriterParameters ( ) { Dataset = dataset, MediaInfo = mediaProperties } ;
            var locationProvider = new MemoryStorageProvider ( ) ;
            

            if ( null == frameList )
            {
                return MediaWriterFactory.GetMediaWriter ( mediaType ).CreateMedia ( writerParams, locationProvider ) ;
            }
            else
            {
                return MediaWriterFactory.GetMediaWriter ( mediaType ).CreateMedia ( writerParams, locationProvider, frameList ) ;
            }
            
        }

        public virtual fo.DicomDataset RetrieveDicomDataset ( IObjectId objectId, DicomMediaProperties mediainfo )
        {
            IStorageLocation location    ;
            fo.DicomFile defaultFile ;


            location    = RetrieveSopInstance ( objectId, mediainfo ) ;

            if ( location == null )
            {
                return null ;
            }

            defaultFile = fo.DicomFile.Open ( location.GetReadStream ( ) ) ;

            return defaultFile.Dataset ;

        }

        public virtual bool ObjetInstanceExist ( IObjectId objectId, string mediaType, string transferSyntax )
        {
            var mediaProperties = new DicomMediaProperties ( mediaType, transferSyntax ) ;
            var mediaID         = MediaFactory.Create      ( objectId, mediaProperties ) ;
            
                
            return StorageService.Exists (  mediaID ) ;
        }
    }
}
