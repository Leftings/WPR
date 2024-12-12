using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Employee.Swagger;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var parameter in operation.Parameters)
        {
            if (parameter.Schema.Type == "string" && parameter.Schema.Format == "binary")
            {
                Console.WriteLine("X");
                parameter.Description = "Upload file (binary)";
            }
        }
    }
}
