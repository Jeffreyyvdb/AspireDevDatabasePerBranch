# Database Per Branch for .NET Aspire

This repository demonstrates a solution for managing separate local development PostgreSQL databases per git branch in .NET Aspire applications. This approach solves common development challenges when working with multiple branches that have different database migrations or test data.

## Problem Statement

When developing applications with .NET Aspire and PostgreSQL, developers often face these challenges:

- Different branches may have different database migrations
- Test data added in one branch might conflict with another branch
- Switching between branches requires database resets or migrations

## Local Development Workflow

In a typical local development environment, we often:

- Automatically apply database migrations on startup
- Seed development data for testing
- Work on multiple features in different branches simultaneously
- Review pull requests locally before merging

This workflow can lead to conflicts when:

1. You're working on a feature branch with new migrations
2. You need to review another PR that has different migrations
3. Your local database schema doesn't match the branch you're switching to
4. Development data you've added for testing conflicts with another branch's schema

For example, if you're working on a feature that adds a new table and you've added test data, switching to review another PR that modifies that same table structure can cause conflicts. Without separate databases per branch, you'd need to:

- Reset your database
- Re-run migrations
- Re-seed test data
- Or maintain separate database instances manually

## Solution

This solution provides a .NET Aspire extension that automatically creates a separate PostgreSQL database for each git branch. Key features:

- Automatically detects the current git branch
- Creates a unique database name by appending the branch name
- Persists database state per branch using Docker volumes
- Sanitizes branch names to ensure valid database names
- Maintains separate database states for different branches

## Implementation

The solution consists of two main components:

1. A custom extension method `AddDatabaseWithBranchNameSuffix` for PostgreSQL resources
2. Integration with .NET Aspire's resource model

### Usage Example

```csharp
var builder = DistributedApplication.CreateBuilder(args);

builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .AddDatabaseWithBranchNameSuffix("database");
```

This will create a database named `database-{branch-name}` where `{branch-name}` is your current git branch name (sanitized for database compatibility).

## Features

- **Automatic Branch Detection**: Uses git to detect the current branch name
- **Name Sanitization**: Converts branch names to valid database names
- **Data Persistence**: Uses Docker volumes to persist database state per branch

## Benefits

- **Isolation**: Each branch has its own database state
- **Persistence**: Database state is preserved between branch switches
- **No Conflicts**: Different branches can have different schemas and data
- **Easy Testing**: Test data can be added to specific branches without affecting others
- **Development Efficiency**: No need to reset or migrate databases when switching branches
