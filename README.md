# OCR_Invoice_Recognition

Optical Character Recognition using Microsoft Recognize Text API C# in Cognitive Service. Optical Character Recognition (OCR) is the technology that converts printed texts into a digital text format. It is the conversion of typed images, printed texts or handwrites into machine-encoded texts. This project is to read receipt date, receipt number and total purchase of the receipt and to write a .csv file.
You can follow this link to see Computer Vision API version -v2.0 https://westcentralus.dev.cognitive.microsoft.com/docs/services/5adf991815e1060e6355ad44/operations/587f2c6a154055056008f200 and you can follow this link for more detail about reading both printed and handwritten text in images: https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/

Prerequisites:
If you do not already have a Microsoft account, go to https://signup.live.com and create one. If you don't have an Azure subscription, create a free account before you begin. You must have a subscription key for Computer Vision. Create a Cognitive Services account to subscribe Computer Vision and get your key. You must have Visual Studio 2015 or later. Use your subscription key, your uribase and your file path in your local computer to save your .csv file. Run the program.

Input requirements:

Supported image formats: JPEG, PNG and BMP.

Image file size must be less than 4MB.

Image dimensions must be at least 50 x 50, at most 4200 x 4200.

Change the following variables in Program.cs before running:

        const string subscriptionKey = "<SUBSCRIPTION_KEY>";

        const string uriBase = "https://LOCATION.api.cognitive.microsoft.com/vision/v2.0/recognizeText";         //example https://northeurope.api.cognitive.microsoft.com/vision/v2.0/recognizeText

        const string outputfilepath = "<OUTPUT_PATH>";
        
        
      
