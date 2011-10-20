using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;


namespace Haukcode.CloudFiler
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                NameValueCollection settings = new NameValueCollection();

                List<string> indexedArgs = new List<string>();
                string command = null;

                foreach (var arg in args)
                {
                    if (arg.StartsWith("--"))
                    {
                        if (arg.Contains("="))
                        {
                            // Key/Value pair
                            int equalIndex = arg.IndexOf('=');
                            string key = arg.Substring(2, equalIndex - 2);
                            string value = arg.Substring(equalIndex + 1);

                            settings.Add(key.ToLower(), value);
                            continue;
                        }
                    }
                    else
                    {
                        if (command == null)
                            command = arg.ToUpper();
                        else
                            indexedArgs.Add(arg);
                    }
                }

                if (string.IsNullOrEmpty(command))
                    throw new ArgumentException("Missing command");

                if (string.IsNullOrEmpty(settings["accesskey"]))
                    throw new ArgumentException("Missing AccessKey");
                if (string.IsNullOrEmpty(settings["accesssecret"]))
                    throw new ArgumentException("Missing AccessSecret");

                if (string.IsNullOrEmpty(settings["server"]))
                    settings.Add("server", "s3.amazonaws.com");

                string endpoint = settings["server"];
                if (endpoint.EndsWith("/"))
                    endpoint = endpoint.Substring(0, endpoint.Length - 1);

                if (!string.IsNullOrEmpty(settings["bucket"]))
                    endpoint += "/" + settings["bucket"];

                if (command == "PUT")
                {
                    if (indexedArgs.Count < 1)
                        throw new ArgumentException("Missing filename");

                    var upload = new SprightlySoftAWS.S3.Upload();

                    foreach (var localFile in indexedArgs)
                    {
                        string fileName = System.IO.Path.GetFileName(localFile);

                        var requestURL = upload.BuildS3RequestURL(true, endpoint, string.Empty, fileName, string.Empty);

                        var headers = new Dictionary<string, string>();
                        headers.Add("x-amz-date", DateTime.UtcNow.ToString("r"));

                        var authValue = upload.GetS3AuthorizationValue(requestURL, "PUT", headers, settings["accesskey"], settings["accesssecret"]);
                        headers.Add("Authorization", authValue);

                        if (!upload.UploadFile(requestURL, "PUT", headers, localFile))
                            throw new Exception(string.Format("Failed to upload file: {0}   Error: {1}",
                                localFile, upload.ErrorDescription));
                    }

                    return 0;
                }
                else if (command == "DELETE")
                {
                    if (indexedArgs.Count < 1)
                        throw new ArgumentException("Missing filename");

                    var delete = new SprightlySoftAWS.REST();

                    foreach (var remoteFilename in indexedArgs)
                    {
                        var requestURL = delete.BuildS3RequestURL(true, endpoint, string.Empty, remoteFilename, string.Empty);

                        var headers = new Dictionary<string, string>();
                        headers.Add("x-amz-date", DateTime.UtcNow.ToString("r"));

                        var authValue = delete.GetS3AuthorizationValue(requestURL, "DELETE", headers, settings["accesskey"], settings["accesssecret"]);
                        headers.Add("Authorization", authValue);

                        if (!delete.MakeRequest(requestURL, "PUT", headers, remoteFilename))
                            throw new Exception(string.Format("Failed to delete file: {0}   Error: {1}",
                                remoteFilename, delete.ErrorDescription));
                    }

                    return 0;
                }
                else if (command == "GET")
                {
                    if (indexedArgs.Count < 1)
                        throw new ArgumentException("Missing filename");

                    var download = new SprightlySoftAWS.S3.Download();

                    foreach (var remoteFilename in indexedArgs)
                    {
                        var requestURL = download.BuildS3RequestURL(true, endpoint, string.Empty, remoteFilename, string.Empty);

                        var headers = new Dictionary<string, string>();
                        headers.Add("x-amz-date", DateTime.UtcNow.ToString("r"));

                        var authValue = download.GetS3AuthorizationValue(requestURL, "GET", headers, settings["accesskey"], settings["accesssecret"]);
                        headers.Add("Authorization", authValue);

                        string localFilename = System.IO.Path.GetFullPath(remoteFilename);

                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(localFilename));

                        if (!download.DownloadFile(requestURL, "GET", headers, localFilename, false))
                            throw new Exception(string.Format("Failed to download file: {0}   Error: {1}",
                                remoteFilename, download.ErrorDescription));
                    }

                    return 0;
                }
                else
                    Console.WriteLine("Unknown command: " + command);

                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return 255;
            }
        }
    }
}
