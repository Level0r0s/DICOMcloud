﻿using fo = Dicom ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;


namespace DICOMcloud.Dicom
{
    
    public interface IXmlDicomConverter : IDicomConverter<string>
    {}

    public class XmlDicomConverter : IXmlDicomConverter
    {
        protected XmlDicomWriterService WriterService {get; set; }
        public XmlDicomConverter()
        {
            WriterService = new XmlDicomWriterService ( ) ;
        }

        public string Convert ( fo.DicomDataset ds )
        {
            StringBuilder sb = new StringBuilder ( ) ;
            XmlWriter writer = XmlTextWriter.Create (sb);
            
            WriteHeaders(writer);

            writer.WriteStartElement("NativeDicomModel") ;
            
            ConvertChildren ( ds, writer ) ;
            
            writer.WriteEndElement ( ) ;

            writer.Close ( ) ;

            return sb.ToString ( ) ;
        }

        private void WriteHeaders(XmlWriter writer)
        {
            writer.WriteStartDocument ();
        }

        private void ConvertChildren ( fo.DicomDataset ds, XmlWriter writer ) 
        {
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.FileMetaInformationVersion], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.MediaStorageSopClassUid], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.MediaStorageSopInstanceUID], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.TransferSyntaxUid], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.ImplementationClassUid], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.ImplementationVersionName], writer ) ;

            foreach ( var element in ds.OfType<fo.DicomElement>().Where ( n=>n.Count > 0 ) )
            {
                //TODO:
                //WriterService.WriteElement (element,writer);
                WriteDicomAttribute ( ds, element, writer ) ;
            }
        }

        private void WriteDicomAttribute 
        ( 
            fo.DicomDataset ds, 
            fo.DicomItem element, 
            XmlWriter writer 
        )
        {
            if (null == element) { return; }

            fo.DicomVR dicomVr = element.ValueRepresentation;

            writer.WriteStartElement("DiocomAttribute");

            writer.WriteAttributeString("keyword", element.Tag.DictionaryEntry.Name);
            writer.WriteAttributeString("tag", element.Tag.Group.ToString("D4") + element.Tag.Element.ToString("D4"));
            writer.WriteAttributeString("vr", element.ValueRepresentation.Name);

            //VR should at least support a switch!
            if (dicomVr.Name == fo.DicomVR.SQ.Name)
            {
                ConvertSequence((fo.DicomSequence)element, writer);
            }
            else
            {
                ConvertElement( ds, (fo.DicomElement) element, writer, dicomVr ) ;
            }

            if (element.Tag.IsPrivate)
            {
                //TODO:
                //writer.WriteAttributeString ("privateCreator", ds[fo.DicomTag.privatecreatro. ) ;                        
            }

            writer.WriteEndElement();
        }

        private void ConvertElement(fo.DicomDataset ds, fo.DicomElement element, XmlWriter writer, fo.DicomVR dicomVr)
        {
            //Element value can be:
            // 1. PN
            // 2. Binary
            // 3. Value

            if ( dicomVr.Equals (fo.DicomVR.PN) )
            {
                for (int index = 0; index < element.Count; index++)
                {
                    writer.WriteStartElement ( "PersonName");
                    WriteNumberAttrib(writer, index) ;

                    writer.WriteElementString("Alphabetic", GetTrimmedString ( element.Get<string> ( ) ) ) ; //TODO: >>>Include Table 10.2-1 “Person Name Components Macro”
                    writer.WriteEndElement ( ) ;
//TODO:
//>>Ideographic
//>>>Include Table 10.2-1 “Person Name Components Macro”
//A group of name components that are represented in phonetic characters (see thedefinition for the Value Representation of PN in PS3.5).
//0-1
//O
//>>Phonetic
//>>>Include Table 10.2-1 “Person Name Components Macro”

                }
            }
            else if ( IsBinary ( dicomVr ) )
            {
                //TODO: Add BulkData element support


                if ( element.Tag.Element == fo.DicomTag.PixelData )
                { 
                
                }
                else
                { 
                    byte[] data = element.Buffer.Data;
                    writer.WriteBase64 ( data, 0, data.Length ) ;
                }
            }
            else 
            {
                ConvertValue(ds, element, writer);
            }
        }

        private static void ConvertValue( fo.DicomDataset ds, fo.DicomElement element, XmlWriter writer )
        {
            fo.DicomVR dicomVr = element.ValueRepresentation ;


            for ( int index = 0; index < element.Count; index++ )
            {
                writer.WriteStartElement ( "Value");
                WriteNumberAttrib(writer, index);
                    
                if ( dicomVr.Equals(fo.DicomVR.AT))
                {
                    writer.WriteString ( GetTrimmedString ( ds.Get<string> ( element.Tag, index, string.Empty ) ) ); //TODO: check standard
                }
                else
                {
                    writer.WriteString ( GetTrimmedString ( ds.Get<string> ( element.Tag, index, string.Empty ) ) ); 
                }

                writer.WriteEndElement ( );
            }
        }

        private static void WriteNumberAttrib(XmlWriter writer, int index)
        {
            writer.WriteAttributeString("number", (index + 1).ToString());
        }

        private void ConvertSequence(fo.DicomSequence element, XmlWriter writer )
        {
            for ( int index = 0; index < element.Items.Count; index++ )
            {
                var item = element.Items [ index ] ;

                writer.WriteStartElement ( "Item");
                WriteNumberAttrib(writer, index);
                
                ConvertChildren(item, writer);

                writer.WriteEndElement () ;
            }
        }

        private bool IsBinary ( fo.DicomVR dicomVr ) 
        {
            return dicomVr.Equals (fo.DicomVR.OB) || dicomVr.Equals(fo.DicomVR.OD) ||
                      dicomVr.Equals (fo.DicomVR.OF) || dicomVr.Equals(fo.DicomVR.OW) ||
                      dicomVr.Equals (fo.DicomVR.UN ) ;
        }

        //trimming the padding the only allowed raw value transformation in XML
        //part 19 A.1.1
        private static string GetTrimmedString ( string value )
        {
            return value.TrimEnd (PADDING) ;
        }

        private static char[] PADDING = new char[] {'\0',' '};
    }
}
