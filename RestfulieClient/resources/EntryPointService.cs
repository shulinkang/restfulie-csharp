﻿using System;
using System.Dynamic;
using System.Net;
using RestfulieClient.service;
using System.Text;
using System.IO;

namespace RestfulieClient.resources
{
    public class EntryPointService : IRemoteResourceService
    {
        private string entryPointURI = "";
        private string contentType = "";
        private string accepts = "";

        private RestfulieHttpVerbDiscovery httpVerbDiscovery = new RestfulieHttpVerbDiscovery();

        public EntryPointService(string uri)
        {
            this.entryPointURI = uri;
        }

        public dynamic As(string contentType)
        {
            this.contentType = contentType;
            this.accepts = contentType;
            return this;
        }

        public dynamic Get()
        {
            if (string.IsNullOrEmpty(this.entryPointURI))
                throw new ArgumentNullException("There is no uri defined. Use the At() method for to define the uri.");
            return this.FromXml(this.entryPointURI);
        }


        public dynamic Create(string content)
        {
            return InvokeRemoteUri(this.entryPointURI, "post", content);
        }

        private dynamic FromXml(string uri)
        {
            dynamic response = this.GetResourceFromWeb(uri);
            //todo - criar um enum para MediaType
            if (response.ContentType.Equals("application/xml"))
            {
                return new DynamicXmlResource(response, this);
            }
            else
            {
                throw new InvalidOperationException("unsupported media type {0}", response.ContentType);
            }
        }

        public object Execute(string uri, string transitionName)
        {
            string httpVerb = httpVerbDiscovery.GetHttpVerbByTransitionName(transitionName);
            return InvokeRemoteUri(uri, httpVerb);
        }

        public object GetResourceFromWeb(string uri)
        {
            return this.InvokeRemoteUri(uri, "get");
        }


        private object InvokeRemoteUri(string uri, string httpVerb, string content = "")
        {
            Uri requestUri = new Uri(this.entryPointURI);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            try
            {
                request.Method = httpVerb;
                if (!accepts.Equals(""))
                {
                    request.Accept = accepts;
                    request.ContentType = contentType;
                }
                if (!content.Equals(""))
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(content);
                    request.ContentLength = byteArray.Length;
                    Stream bodyStream = request.GetRequestStream();
                    bodyStream.Write(byteArray, 0, byteArray.Length);    
                    bodyStream.Close();
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return HttpRemoteResponseFactory.GetRemoteResponse(response);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("An error occurred while connecting to the resource in url {0} with message {1}.", uri, ex.Message), ex);
            }
        }

    }
}
