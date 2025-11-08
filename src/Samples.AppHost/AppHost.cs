//YouTube video that cover this sample: https://youtu.be/JgV241C-vdk

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AspireBlazorDemo>("aspireblazordemo");

builder.Build().Run();