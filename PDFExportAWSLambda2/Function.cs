using Amazon;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using GrapeCity.Documents.Pdf;
using GrapeCity.Documents.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PDFExportAWSLambda2
{
    public class Function
    {
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            APIGatewayProxyResponse response;

            try
            {
                // クエリ文字列を取得
                string queryString;
                input.QueryStringParameters.TryGetValue("name", out queryString);

                // ドキュメントに追加するテキスト
                string Message = string.IsNullOrEmpty(queryString)
                    ? "Hello, World!!"
                    : $"Hello, {queryString}!!";

                //GcPdfDocument.SetLicenseKey("");

                GcPdfDocument doc = new GcPdfDocument();
                GcPdfGraphics g = doc.NewPage().Graphics;

                g.DrawString(Message,
                    new TextFormat() { Font = StandardFonts.Helvetica, FontSize = 12 },
                    new PointF(72, 72));


                using (var ms = new MemoryStream())
                {
                    doc.Save(ms, false);

                    // S3にアップロード
                    AmazonS3Client client = new AmazonS3Client(RegionEndpoint.APNortheast1);
                    var request = new PutObjectRequest
                    {
                        BucketName = "diodocs-export",
                        Key = "Result.pdf",
                        InputStream = ms
                    };

                    await client.PutObjectAsync(request);
                }

                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = "ファイルが保存されました。",
                    Headers = new Dictionary<string, string> {
                        { "Content-Type", "text/plain; charset=utf-8" }
                    }
                };
            }
            catch (Exception e)
            {
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Body = e.Message,
                    Headers = new Dictionary<string, string> {
                        { "Content-Type", "text/plain" }
                    }
                };
            }

            return response;
        }
    }
}
