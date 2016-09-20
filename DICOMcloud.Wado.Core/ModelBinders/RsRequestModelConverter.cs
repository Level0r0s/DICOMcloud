﻿
using DICOMcloud.Wado.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using DICOMcloud.Pacs;

namespace DICOMcloud.Wado.Core
{
    public class RsRequestModelConverter<T> where T :class
    {
        public RsRequestModelConverter ( )
        { }

        public bool TryParse ( HttpRequestMessage request, ModelBindingContext bindingContext, out T result )
        {
            var query = request.RequestUri.ParseQueryString ( ) ;        
            result = null ;

            if ( typeof(T) == typeof(IWadoRsStudiesRequest) )
            {
                WadoRSStudiesRequest wadoReq = new WadoRSStudiesRequest ( ) ;

                FillStudyParams ( bindingContext.ValueProvider, wadoReq ) ;
                
                wadoReq.QueryLevel = ObjectQueryLevel.Study ;
                
                result = wadoReq as T;
            }

            if ( typeof(T) == typeof(IWadoRsSeriesRequest) )
            {
                WadoRSSeriesRequest wadoReq = new WadoRSSeriesRequest ( ) ;

                FillSeriesParams ( bindingContext.ValueProvider, wadoReq ) ;
             
                wadoReq.QueryLevel = ObjectQueryLevel.Series ;
                
                result = wadoReq as T;
            }

            if ( typeof(T) == typeof(IWadoRSInstanceRequest) )
            {
                WadoRSInstanceRequest wadoReq = new WadoRSInstanceRequest ( ) ;

                FillInstanceParams ( bindingContext.ValueProvider, wadoReq ) ;

                wadoReq.QueryLevel = ObjectQueryLevel.Instance ;
                
                result = wadoReq as T;
            }

            if ( typeof(T) == typeof(IWadoRSFramesRequest) )
            {
                WadoRSFramesRequest wadoReq = new WadoRSFramesRequest ( ) ;

                FillIFramesParams ( bindingContext.ValueProvider, wadoReq) ;

                wadoReq.QueryLevel = ObjectQueryLevel.Instance ;

                result = wadoReq as T;
            }

            if ( null != result)
            { 
                WadoRsRequestBase reqBase = result as WadoRsRequestBase ;

                reqBase.AcceptHeader        = request.Headers.Accept;
                reqBase.AcceptCharsetHeader = request.Headers.AcceptCharset;
                reqBase.QueryLevel          = ObjectQueryLevel.Instance ;
                
                return true ;
            }
            else
            { 
                return false ;
            }
        }

        private int[] ParseFrames(string frames)
        {
            if ( !string.IsNullOrEmpty (frames) )
            { 
                return frames.Split(',').Select(Int32.Parse).ToArray();
            }

            return null ;
        }

        private WadoBurnAnnotation ParseAnnotation ( string annotationString)
        {
            WadoBurnAnnotation annotation = WadoBurnAnnotation.None ;

            if ( !string.IsNullOrWhiteSpace ( annotationString ) )
            { 
            string[] parts = annotationString.Trim().Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries) ;
         
            foreach(string part in parts)
            {
                WadoBurnAnnotation tempAnn ; 
               
                if ( Enum.TryParse<WadoBurnAnnotation>(part.Trim(), true, out tempAnn ) )
                { 
                    annotation |= tempAnn ;
                }
            }
            }

            return annotation ;
        }

        private int? GetIntValue ( string stringValue )
        {
            if ( string.IsNullOrWhiteSpace(stringValue))
            {
            return null ;
            }
            else
            { 
            int parsedVal ;

            if ( int.TryParse (stringValue.Trim(), out parsedVal))
            { 
                return parsedVal ;
            }
            else
            { 
                return null ;
            }
            }
        }
   
        private void FillStudyParams ( IValueProvider valueProvider, IWadoRsStudiesRequest result )
        { 
            result.StudyInstanceUID = valueProvider.GetValue ("StudyInstanceUID").RawValue as string  ;
        }

        private void FillSeriesParams ( IValueProvider valueProvider, IWadoRsSeriesRequest result )
        { 
            FillStudyParams ( valueProvider, result ) ;

            result.SeriesInstanceUID = valueProvider.GetValue ("SeriesInstanceUID").RawValue as string  ;
        }

        private void FillInstanceParams ( IValueProvider valueProvider, IWadoRSInstanceRequest result )
        { 
            FillSeriesParams ( valueProvider, result ) ;

            result.SOPInstanceUID = valueProvider.GetValue ("SOPInstanceUID").RawValue as string  ;
        }

        private void FillIFramesParams ( IValueProvider valueProvider, IWadoRSFramesRequest result )
        { 
            FillInstanceParams ( valueProvider, result ) ;

            result.Frames = ParseFrames ( valueProvider.GetValue ( "FrameList" ).RawValue as string ) ;
        }
   }

   //public abstract class WadoRequestKeys
   //{
   //   private WadoRequestKeys (){} 

   //   public const string RequestType           = "requestType" ;
   //   public const string StudyUID              = "studyUID" ;
   //   public const string SeriesUID             = "seriesUID" ;
   //   public const string ObjectUID             = "objectUID" ;
   //   public const string ContentType           = "contentType" ;
   //   public const string Charset               = "charset" ;
   //   public const string Anonymize             = "anonymize" ;
   //   public const string Annotation            = "annotation" ;
   //   public const string Rows                  = "rows" ;
   //   public const string Columns               = "columns" ;
   //   public const string Region                = "region" ;
   //   public const string WindowWidth           = "windowWidth" ;
   //   public const string WindowCenter          = "windowCenter" ;
   //   public const string FrameNumber           = "frameNumber" ;
   //   public const string ImageQuality          = "imageQuality" ;
   //   public const string PresentationUID       = "presentationUID" ;
   //   public const string PresentationSeriesUID = "presentationSeriesUID" ;
   //   public const string TransferSyntax        = "transferSyntax" ;
      
   //}
}
