using AspireDevDatabasePerBranch;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .AddDatabaseWithBranchNameSuffix("database");
    // This will create a different database for each branch, avoiding conflicts.

builder.Build().Run();