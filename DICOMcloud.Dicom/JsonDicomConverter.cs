﻿using foDicom = Dicom;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Dicom
{
    public interface IJsonDicomConverter : IDicomConverter<string>
    { }

    public class JsonDicomConverter : IJsonDicomConverter
    {
        private int _minValueIndex;

        public JsonDicomConverter()
        {
            IncludeEmptyElements = false;
        }

        public bool IncludeEmptyElements
        {
            get
            {
                return (_minValueIndex == -1);
            }
            set
            {
                _minValueIndex = (value ? -1 : 0);
            }
        }

        public string Convert(foDicom.DicomDataset ds)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();

                ConvertChildren(ds, writer);

                writer.WriteEndObject();
            }

            return sb.ToString();
        }

        private void ConvertChildren(foDicom.DicomDataset ds, JsonWriter writer)
        {
            //WriteDicomAttribute ( ds, ds[DicomTags.FileMetaInformationVersion], writer ) ;
            //WriteDicomAttribute ( ds, ds[DicomTags.MediaStorageSopClassUid], writer ) ;
            //WriteDicomAttribute ( ds, ds[DicomTags.MediaStorageSopInstanceUid], writer ) ;
            //WriteDicomAttribute ( ds, ds[DicomTags.TransferSyntaxUid], writer ) ;
            //WriteDicomAttribute ( ds, ds[DicomTags.ImplementationClassUid], writer ) ;
            //WriteDicomAttribute ( ds, ds[DicomTags.ImplementationVersionName], writer ) ;

            foreach (var element in ds )
            {
                //TODO:
                //WriterService.WriteElement (element,writer);
                WriteDicomAttribute(ds, element, writer);
            }
        }

        private void WriteDicomAttribute
        (
            foDicom.DicomDataset ds,
            foDicom.DicomItem element,
            JsonWriter writer
        )
        {
            foDicom.DicomVR dicomVr = element.ValueRepresentation;

            writer.WritePropertyName(((uint)element.Tag).ToString ("X8", null));//TODO: zaid, pass proper hex format
            //writer.WritePropertyName(element.Tag.HexString, false);
            //writer.WritePropertyName(element.Tag.Group.ToString("D4") + element.Tag.Element.ToString("D4"), false);
            writer.WriteStartObject();


            writer.WritePropertyName("temp");
            writer.WriteValue(element.Tag.DictionaryEntry.Name);

            writer.WritePropertyName("vr");
            writer.WriteValue(element.ValueRepresentation.Name);


            if (element is foDicom.DicomSequence)
            {
                ConvertSequence((foDicom.DicomSequence) element, writer);
            }
            else if (dicomVr.Equals(foDicom.DicomVR.PN))
            {

                writer.WritePropertyName(JsonConstants.ValueField);
                writer.WriteStartArray();
                writer.WriteStartObject();
                writer.WritePropertyName(JsonConstants.Alphabetic);
                writer.WriteValue(element.ToString().TrimEnd()); //TODO: not sure if PN need to be trimmed
                writer.WriteEndObject();
                writer.WriteEndArray();
            }
            else if (dicomVr.Equals(foDicom.DicomVR.OB) || dicomVr.Equals(foDicom.DicomVR.OD) ||
                      dicomVr.Equals(foDicom.DicomVR.OF) || dicomVr.Equals(foDicom.DicomVR.OW) ||
                      dicomVr.Equals(foDicom.DicomVR.UN)) //TODO inline bulk
            {
                if (element.Tag == foDicom.DicomTag.PixelData)
                { }
                else
                {
                    var dicomElement = (foDicom.DicomElement)element ;
                    byte[] data = (byte[])dicomElement.Buffer.Data;
                    WriteStringValue(writer, System.Convert.ToBase64String(data));
                }
            }
            //else if ( dicomVr.Equals (foDicom.DicomVR.PNvr) ) //TODO bulk reference
            //{

            //}
            else
            {
                ConvertValue((foDicom.DicomElement) element, writer);
            }

            if (element.Tag.IsPrivate)
            {
                //TODO:
                //writer.WriteAttributeString ("privateCreator", ds[DicomTags.privatecreatro. ) ;                        
            }

            writer.WriteEndObject();
        }


        private void ConvertSequence(foDicom.DicomSequence element, JsonWriter writer)
        {
            for (int index = 0; index < element.Items.Count; index++)
            {
                StringBuilder sqBuilder = new StringBuilder();
                StringWriter sw = new StringWriter(sqBuilder);

                using (JsonWriter sqWriter = new JsonTextWriter(sw))
                {
                    sqWriter.Formatting = Formatting.Indented;

                    sqWriter.WriteStartArray();

                    var item = element.Items[index];
                    sqWriter.WriteStartObject();
                    if (null != item)
                    {
                        ConvertChildren(item, sqWriter);
                    }
                    sqWriter.WriteEndObject();
                    sqWriter.WriteEndArray();

                }

                WriteSequence(writer, sqBuilder.ToString());
            }
        }

        private void WriteSequence(JsonWriter writer, string data)
        {
            writer.WritePropertyName(JsonConstants.ValueField);
            writer.WriteRawValue(data);
        }

        private void WriteStringValue(JsonWriter writer, string data)
        {
            writer.WritePropertyName(JsonConstants.ValueField);
            writer.WriteStartArray();
            writer.WriteValue(data);
            writer.WriteEndArray();

        }

        private void WriteNumberValue(JsonWriter writer, string data)
        {
            writer.WritePropertyName(JsonConstants.ValueField);
            writer.WriteStartArray();
            writer.WriteValue(data); //TODO: handle numbers to be with no ""
            writer.WriteEndArray();
        }

        private void ConvertValue(foDicom.DicomElement element, JsonWriter writer)
        {
            if (_numberBasedVrs.Contains(element.ValueRepresentation.Name))
            {
                WriteNumberValue(writer, element.ToString().TrimEnd());
            }
            else
            {
                //TODO: NOT ALL VRS CAN BE TRIMMED, CHECK THE STANDARD     
                WriteStringValue(writer, element.ToString().TrimEnd());
            }
        }

        private static List<string> _numberBasedVrs = new List<string>();
        private const string QuoutedStringFormat = "\"{0}\"";
        private const string QuoutedKeyValueStringFormat = "\"{0}\":\"{1}\"";
        private const string QuoutedKeyValueArrayFormat = "\"Value\":[\"{0}\"]";
        private const string SequenceValueFormatted = "\"Value\":[{\"{0}\"}]";
        private const string NumberValueFormatted = "\"Value\":[{1}]";

        private abstract class JsonConstants
        {
            public const string ValueField = "Value";
            public const string Alphabetic = "Alphabetic";
        }
    }
}
