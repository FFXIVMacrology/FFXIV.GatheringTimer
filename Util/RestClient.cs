using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GatheringTimer.Util
{
    /// <summary>
    /// Restful Request Enum
    /// </summary>
    public enum HttpVerb
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    /// <summary>
    /// Restful Api Method
    /// </summary>
    class RestClient
    {
        //request url
        public string EndPoint { get; set; }
        //request method(enum HttpVerb)
        public HttpVerb Method { get; set; }
        //content type(application/json or text/xml)
        public string ContentType { get; set; }
        //post data(json)
        public string PostData { get; set; }

        /// <summary>
        /// struct RestClient(no param@endpoint)
        /// </summary>
        public RestClient()
        {
            EndPoint = "";
            Method = HttpVerb.GET;
            ContentType = "application/json";
            PostData = "";
        }

        /// <summary>
        /// default GET method
        /// </summary>
        /// <param name="endpoint"></param>
        public RestClient(string endpoint)
        {
            EndPoint = endpoint;
            Method = HttpVerb.GET;
            ContentType = "application/json";
            PostData = "";
        }

        /// <summary>
        /// any method with no data
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="method"></param>
        public RestClient(string endpoint, HttpVerb method)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = "application/json";
            PostData = "";
        }

        /// <summary>
        /// any method with data
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="method"></param>
        /// <param name="postData"></param>
        public RestClient(string endpoint, HttpVerb method, string postData)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = "application/json";
            PostData = postData;
        }

        /// <summary>
        /// Request with "" Param
        /// </summary>
        /// <returns></returns>
        public string MakeRequest()
        {
            return MakeRequest("");
        }

        /// <summary>
        /// Request need param
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string MakeRequest(string parameters)
        {
            var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);

            request.Method = Method.ToString();
            request.ContentLength = 0;
            request.ContentType = ContentType;

            //Is POST or PUT request And PostData not Empty
            if (!string.IsNullOrEmpty(PostData) && (HttpVerb.POST == Method || Method == HttpVerb.PUT))
            {
                //Encoding UTF-8
                var bytes = Encoding.GetEncoding("UTF-8").GetBytes(PostData);
                request.ContentLength = bytes.Length;

                using (var writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }

                // grab the response
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (var reader = new StreamReader(responseStream))
                        {
                            responseValue = reader.ReadToEnd();
                        }
                }

                return responseValue;
            }
        }

        /// <summary>
        /// RequestAsync with "" Param
        /// </summary>
        /// <returns></returns>
        public async Task<string> MakeRequestAsync()
        {
            return await MakeRequestAsync("");
        }

        /// <summary>
        /// RequestAsync need param
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<string> MakeRequestAsync(string parameters)
        {
            Logger.Debug("HttpWebRequest-'" + EndPoint + parameters + "'");

            var responseValue = string.Empty;
            await Task.Run(() =>
            {
                var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);

                request.Method = Method.ToString();
                request.ContentLength = 0;
                request.ContentType = ContentType;

                //Is POST or PUT request And PostData not Empty
                if (!string.IsNullOrEmpty(PostData) && (HttpVerb.POST == Method || Method == HttpVerb.PUT))
                {
                    //Encoding UTF-8
                    var bytes = Encoding.GetEncoding("UTF-8").GetBytes(PostData);
                    request.ContentLength = bytes.Length;

                    using (var writeStream = request.GetRequestStream())
                    {
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                }
                try
                {
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {

                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                            Logger.Error("", new ApplicationException(message));
                        }

                        // grab the response
                        using (var responseStream = response.GetResponseStream())
                        {
                            if (responseStream != null)
                                using (var reader = new StreamReader(responseStream))
                                {
                                    responseValue = reader.ReadToEnd();
                                }
                        }


                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse response = (HttpWebResponse)ex.Response;
                    if (!(response is null) && response.StatusCode != HttpStatusCode.NotFound)
                    {
                        Logger.Warning(":\nCan not GetResponse-'" + EndPoint + parameters + "'", ex);
                    }  
                }
            });
            return responseValue;
        }


    }


}