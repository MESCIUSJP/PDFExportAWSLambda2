using Amazon;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using GrapeCity.Documents.Pdf;
using GrapeCity.Documents.Text;
using System.Drawing;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PDFExportAWSLambda2;

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        APIGatewayProxyResponse response;

        try
        {
            // �N�G����������擾
            string? queryString;
            input.QueryStringParameters.TryGetValue("name", out queryString);

            // �h�L�������g�ɒǉ�����e�L�X�g
            string Message = string.IsNullOrEmpty(queryString)
                ? "Hello, World!!"
                : $"Hello, {queryString}!!";

            //GcPdfDocument.SetLicenseKey("���i�ł܂��̓g���C�A���ł̃��C�Z���X�L�[��ݒ�");

            GcPdfDocument doc = new GcPdfDocument();
            GcPdfGraphics g = doc.NewPage().Graphics;

            g.DrawString(Message,
                new TextFormat() { Font = StandardFonts.Helvetica, FontSize = 12 },
                new PointF(72, 72));

            using (var ms = new MemoryStream())
            {
                doc.Save(ms, false);

                // S3�ɃA�b�v���[�h
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
                Body = "�t�@�C�����ۑ�����܂����B",
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
