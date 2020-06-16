using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using WordpressApi.DAL.Models;

namespace WordpressApi.Service.Services
{
    public static class XsdValidation
    {
        private static string xsdEmailEvent = @"<?xml version='1.0'?> 
                    <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'> 
                     <xs:element name='email_event'> 
                      <xs:complexType> 
                       <xs:sequence> 
                        <xs:element name='application_name' type='xs:string'/> 
                        <xs:element name='event_id' type='xs:string'/>                        
                       </xs:sequence> 
                      </xs:complexType> 
                     </xs:element> 
                    </xs:schema>";
        private static string xsdPatchUser = @"<?xml version='1.0'?> 
                   <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                    <xs:element name='patch_user'> 
                     <xs:complexType> 
                      <xs:sequence>
                        <xs:element name='application_name' type='xs:string'/> 
                        <xs:element name='name' type='xs:string'/> 
                        <xs:element name='uuid' type='xs:string'/> 
                        <xs:element name='email' type='xs:string'/> 
                        <xs:element name='street' type='xs:string'/> 
                        <xs:element name='municipal' type='xs:string'/> 
                        <xs:element name='postalCode' type='xs:string'/> 
                        <xs:element name='vat' type='xs:string'/> 
                      </xs:sequence> 
                     </xs:complexType> 
                    </xs:element> 
                   </xs:schema>";
        private static string xsdAddUser = @"<?xml version='1.0'?> 
                   <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                    <xs:element name='add_user'> 
                     <xs:complexType> 
                      <xs:sequence> 
                        <xs:element name='application_name' type='xs:string'/>
                        <xs:element name='name' type='xs:string'/> 
                        <xs:element name='uuid' type='xs:string'/> 
                        <xs:element name='email' type='xs:string'/> 
                        <xs:element name='street' type='xs:string'/> 
                        <xs:element name='municipal' type='xs:string'/> 
                        <xs:element name='postalCode' type='xs:string'/> 
                        <xs:element name='vat' type='xs:string'/> 
                      </xs:sequence> 
                     </xs:complexType> 
                    </xs:element> 
                   </xs:schema>";
        private static string xsdRequestInvoice = @"<?xml version='1.0'?> 
                   <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                    <xs:element name='email_invoice'> 
                     <xs:complexType> 
                      <xs:sequence> 
                        <xs:element name='application_name' type='xs:string'/>                        
                        <xs:element name='event_id' type='xs:string'/>                        
                        <xs:element name='uuid' type='xs:string'/>                         
                      </xs:sequence> 
                     </xs:complexType> 
                    </xs:element> 
                   </xs:schema>";
        private static string xsdLog = @"<?xml version='1.0'?> 
                   <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                    <xs:element name='log'> 
                     <xs:complexType> 
                      <xs:sequence> 
                        <xs:element name='application_name' type='xs:string'/>                        
                        <xs:element name='timestamp' type='xs:string'/>                        
                        <xs:element name='message' type='xs:string'/>                         
                      </xs:sequence> 
                     </xs:complexType> 
                    </xs:element> 
                   </xs:schema>";
        private static string xsdError = @"<?xml version='1.0'?>
                        < xs:schema xmlns:xs = 'http://www.w3.org/2001/XMLSchema' > 
                            < xs:element name = 'error' >  
                                < xs:complexType >   
                                    < xs:sequence >    
                                        < xs:element name = 'application_name' type = 'xs:string' />       
                                        < xs:element name = 'timestamp' type = 'xs:string' />          
                                        < xs:element name = 'message' type = 'xs:string' />             
                                    </ xs:sequence >              
                                </ xs:complexType >               
                            </ xs:element >
                        </ xs:schema >"; 

        public static string XmlStringValidation(string objectThatNeedsValidation)
        {
            //XML validation with XSD
            //Select the xsd file
            XDocument xDoc = XDocument.Parse(objectThatNeedsValidation);
            string xsdData;
            string rootname = xDoc.Root.Name.ToString();
            if (rootname == "email_event")
            {
                xsdData = xsdEmailEvent;
            }
            else if (rootname == "log")
            {
                xsdData = xsdLog;
            }
            else if (rootname == "add_user")
            {
                xsdData = xsdAddUser;
            }
            else if (rootname == "patch_user")
            {
                xsdData = xsdPatchUser;
            }
            else if (rootname == "error")
            {
                xsdData = xsdError;
            }
            else
            {
                return null;
            }

            if (xsdData == "") {
                return null;
            }

            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", XmlReader.Create(new StringReader(xsdData)));

            //Validation of XML
            //var xDoc = XDocument.Parse(xml);
            bool errors = false;
            xDoc.Validate(schemas, (o, e) =>
            {
                errors = true;
            });

            //Return null when validation has errors
            if (errors)
            {
                return null;
            }
            else
            {
                return rootname;
            }
        }

        public static string XmlObjectValidation(IXsdValidation objectThatNeedsValidation)
        {

            //Make an XML from the object
            XmlSerializer xmlSerializer = new XmlSerializer(objectThatNeedsValidation.GetType());
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            string xml;
            var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };
            var stringBuilder = new StringBuilder();
            using (var sww = new ExtendedStringWriter(stringBuilder, Encoding.UTF8))
            {
                using (XmlWriter writer = XmlWriter.Create(sww, settings))
                {
                    xmlSerializer.Serialize(writer, objectThatNeedsValidation, ns);
                    xml = sww.ToString();
                }
            }

            //XML validation with XSD

            //Select the xsd file
            string xsdData = "";
            if (typeof(PatchUserFromFrontend).IsInstanceOfType(objectThatNeedsValidation))
            {
                xsdData = xsdPatchUser;

            }
            else if (typeof(AddUserFromFrontend).IsInstanceOfType(objectThatNeedsValidation))
            {
                xsdData = xsdAddUser;
            }
            else if (typeof(RequestInvoiceFromFrontend).IsInstanceOfType(objectThatNeedsValidation))
            {
                xsdData = xsdRequestInvoice;
            }
            else if (typeof(Log).IsInstanceOfType(objectThatNeedsValidation))
            {
                xsdData = xsdLog;
            }
            else if (typeof(CustomError).IsInstanceOfType(objectThatNeedsValidation))
            {
                xsdData = xsdError;
            }
            else {
                return null;
            }

            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", XmlReader.Create(new StringReader(xsdData)));

            //Validation of XML
            var xDoc = XDocument.Parse(xml);
            bool errors = false;
            xDoc.Validate(schemas, (o, e) =>
            {
                errors = true;
            });

            //Return null when validation has errors
            if (errors)
            {
                return null;
            }
            else
            {
                return xml;
            }
        }
    }
}
