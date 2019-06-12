using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

public class Response
{
    public string language { get; set; }
    public float textAngle { get; set; }
    public string orientation { get; set; }
    public List<regions> regions { get; set; }

}
public class regions
{

    public string boundingBox { get; set; }
    public List<lines> lines { get; set; }
}

public class lines
{
    public string boundingBox { get; set; }
    public List<words> words { get; set; }
}

public class words
{
    public string boundingBox { get; set; }
    public string text { get; set; }
}
class RecognizeText
{
    [JsonProperty(PropertyName = "status")]
    public string Status { get; set; }
    [JsonProperty(PropertyName = "recognitionResult")]
    public Region RecognitionResult { get; set; }
}

public class Region
{
    [JsonProperty(PropertyName = "lines")]
    public Line[] Lines { get; set; }
}

public class Line
{
    [JsonProperty(PropertyName = "boundingBox")]
    public int[] BoundingBox { get; set; }

    [JsonProperty(PropertyName = "text")]
    public string Text { get; set; }

    [JsonProperty(PropertyName = "words")]
    public Word[] Words { get; set; }
}

public class Word
{
    [JsonProperty(PropertyName = "boundingBox")]
    public int[] BoundingBox { get; set; }

    [JsonProperty(PropertyName = "text")]
    public string Text { get; set; }

    [JsonProperty(PropertyName = "Confidence")]
    public string Confidence { get; set; }
}



namespace OCR_FirstCase
{
    static class Program
    {

        const string subscriptionKey = "27064a36be37483aba4fdca19ecf14af";

        const string uriBase = "https://northeurope.api.cognitive.microsoft.com/vision/v2.0/recognizeText";         //example https://northeurope.api.cognitive.microsoft.com/vision/v2.0/recognizeText

        const string outputfilepath = "C:/Users/t-daalka/Desktop/OCR/test_data/";        //example C:/Users/t-daalka/Desktop/OCR/



        static void Main()
        {
            // Get the path and filename to process from the user.
            Console.WriteLine("Analyze an image:");
            Console.Write(
                "Enter the path to the image you wish to analyze: ");
            string imageFilePath = Console.ReadLine();

            if (File.Exists(imageFilePath))
            {
                // Call the REST API method.
                Console.WriteLine("\nWait a moment for the results to appear.\n");
                MakeAnalysisRequest(imageFilePath).Wait();
            }
            else
            {
                Console.WriteLine("\nInvalid file path");
            }
            Console.WriteLine("\nPress Enter to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// Gets the analysis of the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file to analyze.</param>
        /// 

        static async Task MakeAnalysisRequest(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                // Request parameters. A third optional parameter is "details".
                // The Analyze Image method returns information about the following
                // visual features:
                // Categories:  categorizes image content according to a
                //              taxonomy defined in documentation.
                // Description: describes the image content with a complete
                //              sentence in supported languages.
                // Color:       determines the accent color, dominant color, 
                //              and whether an image is black & white.
                string requestParameters = String.Format("?mode=Printed");

                // Assemble the URI for the REST API method.
                string uri = uriBase + requestParameters;

                HttpResponseMessage response;

                // Read the contents of the specified local image
                // into a byte array.
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Asynchronously call the REST API method.
                    response = await client.PostAsync(uri, content);
                }
                string operationLocation = null;

                // The response contains the URI to retrieve the result of the process.
                if (response.IsSuccessStatusCode)
                {
                    operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                }

                string contentString;
                int s = 0;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    response = await client.GetAsync(operationLocation);
                    contentString = await response.Content.ReadAsStringAsync();
                    ++s;
                }
                while (s < 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1);


                string json = response.Content.ReadAsStringAsync().Result;
                json = json.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                RecognizeText ocrOutput = JsonConvert.DeserializeObject<RecognizeText>(json);

                // Asynchronously get the JSON response.
                //string contentString = await response.Content.ReadAsStringAsync();
                //string contentString_parsed = JToken.Parse(contentString).ToString()
                List<Word> textvalues = new List<Word>();
                List<Line> linevalues = new List<Line>();
                List<string> result = new List<string>();

                foreach (Line sline in ocrOutput.RecognitionResult.Lines)
                {
                    int[] lvalues = sline.BoundingBox;
                    linevalues.Add(new Line { Text = sline.Text, BoundingBox = lvalues });

                    foreach (Word sword in sline.Words)
                    {
                        int[] wvalues = sword.BoundingBox;
                        textvalues.Add(new Word { Text = sword.Text, BoundingBox = wvalues });
                    }
                }

                var csv = new StringBuilder();
                
                string fileName = imageFilePath;
                string csvname = Path.GetFileNameWithoutExtension(fileName);
                string filePath = @outputfilepath + "result_" + csvname + ".csv";

                //TARIH

                var date = new System.Text.StringBuilder();
                bool success = false;
                string date_pattern = @"(\d{2})[-.\/](\d{2})[-.\/](\d{4})";
                for (int i = 0; i < textvalues.Count - 1; i++)
                {
                    date.Append(textvalues[i + 1].Text.ToString());
                    Match m1 = Regex.Match(date.ToString(), date_pattern, RegexOptions.IgnoreCase);
                    if (m1.Success)
                    {
                        success = true;
                        var date_key = "TARIH";
                        var dateLine = string.Format("{0},{1}", date_key, m1.Value);
                        csv.AppendLine(dateLine);
                        break;
                    }

                }
                if (success == false)
                {
                    var date_key = "TARIH";
                    var dateLine = string.Format("{0},{1}", date_key, "Not Found");
                    csv.AppendLine(dateLine);
                }


                //FIS NO

                bool IsDigitsOnly(string str)
                {
                    foreach (char c in str)
                    {
                        if (c < '0' || c > '9')
                            return false;
                    }
                    return true;
                }

                var receipt_number = new List<string>();
                bool afterFis = false;
                for (int i = 0; i < textvalues.Count-1; i++)
                {
                    if (textvalues[i].Text == "Fis" || textvalues[i].Text == "FIS" || textvalues[i].Text == "FiS" || textvalues[i].Text == "Fi$")
                    {
                        afterFis = true;
                    }
                    if (afterFis && (textvalues[i].Text == "NO" || textvalues[i].Text == "NO:"))
                    {
                        afterFis = false;
                        if (IsDigitsOnly(textvalues[i + 1].Text))
                        {
                            receipt_number.Add(textvalues[i + 1].Text);
                        }
                        else if (IsDigitsOnly(textvalues[i + 2].Text))
                        {
                            receipt_number.Add(textvalues[i + 2].Text);
                        }
                        else
                        {
                            string fis_no = "FIS NO";
                            string csv2 = string.Format("{0}, {1}\n ", fis_no, "Not found");
                            csv.Append(csv2);
                            break;
                        }

                        string fisno = "FIS NO";
                        string csv3 = string.Format("{0}, {1}\n ", fisno, receipt_number[0]);
                        csv.Append(csv3);

                    }
                }


                var newtoplam = new System.Text.StringBuilder();
                bool success2 = false;
                string total_pattern = @"\d+[,]\d{2}";
                for (int i = 0; i < textvalues.Count - 1; i++)
                {
                    if (textvalues[i].Text == "TOPLAM" && textvalues[i - 1].Text != "ARA")
                        
                    {
                        for (int k = 0; k < 5; i++)
                        {
                            newtoplam.Append(textvalues[i + 1].Text.ToString());
                            Match m2 = Regex.Match(newtoplam.ToString(), total_pattern, RegexOptions.IgnoreCase);
                            if (m2.Success)
                            {
                                success2 = true;
                                var total_key = "TOPLAM";
                                var newValue = m2.Value.Replace(",", ".");
                                var totalLine = string.Format("{0}, {1} ", total_key, newValue);
                                
                                csv.AppendLine(totalLine);
                                break;
                            }
                        }
                        break;
                    }

                }
                if (success2 == false)
                {
                    var total_key = "TOPLAM";
                    var totalLine = string.Format("{0},{1} ", total_key, "Not Found");

                    csv.AppendLine(totalLine);

                }


                //TOP

                var newtop = new System.Text.StringBuilder();
                bool success3 = false;
                string top_pattern = @"\d+[,]\d{2}";
                for (int i = 0; i < textvalues.Count - 1; i++)
                {
                    if (textvalues[i].Text == "TOP" && textvalues[i - 1].Text != "ARA")
                    {
                        for (int k = 0; k < 5; i++)
                        {
                            newtop.Append(textvalues[i + 1].Text.ToString());
                            Match m3 = Regex.Match(newtop.ToString(), top_pattern, RegexOptions.IgnoreCase);
                            if (m3.Success)
                            {
                                success3 = true;
                                var top_key = "TOP";
                                var replaced_value = m3.Value.Replace(",", ".");
                                var totalLine = string.Format("{0}, {1} ", top_key, replaced_value);
                                csv.AppendLine(totalLine);
                                break;
                            }
                        }
                        break;
                    }

                }
                if (success3 == false)
                {
                    var top_key = "TOP";
                    var topLine = string.Format("{0},{1} ", top_key, "Not Found");

                    csv.AppendLine(topLine);

                }

                //TUTAR

                var newtutar = new System.Text.StringBuilder();
                bool success4 = false;
                string tutar_pattern = @"\d+[,]\d{2}";
                for (int i = 0; i < textvalues.Count - 1; i++)
                {
                    if (textvalues[i].Text == "TUTAR" )
                    {
                        for (int k = 0; k < 5; i++)
                        {
                            newtutar.Append(textvalues[i + 1].Text.ToString());
                            Match m4 = Regex.Match(newtop.ToString(), tutar_pattern, RegexOptions.IgnoreCase);
                            if (m4.Success)
                            {
                                success4 = true;
                                var tutar_key = "TUTAR";
                                var replaced_value = m4.Value.Replace(",", ".");
                                var tutarLine = string.Format("{0}, {1} ", tutar_key, replaced_value);
                                csv.AppendLine(tutarLine);
                                break;
                            }
                        }
                        break;
                    }

                }
                if (success4 == false)
                {
                    var tutar_key = "TUTAR";
                    var tutarLine = string.Format("{0},{1} ", tutar_key, "Not Found");

                    csv.AppendLine(tutarLine);

                }

                File.AppendAllText(filePath, csv.ToString());




                // Display the JSON response.
                Console.WriteLine("\nResponse:\n\n{0}\n",
                    csv.ToString());
                Console.WriteLine("\nResponse:\n\n{0}\n",JToken.Parse(contentString).ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            // Open a read-only file stream for the specified file.
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the file's contents into a byte array.
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}



