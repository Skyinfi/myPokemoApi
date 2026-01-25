# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MyPokemoApi is an ASP.NET Core 10.0 Web API built with .NET 10.0. It is a minimal API project that uses:
- **Microsoft.AspNetCore.OpenApi** (10.0.1) - for OpenAPI/Swagger documentation
- **Npgsql.EntityFrameworkCore.PostgreSQL** (10.0.0) - for PostgreSQL database integration via Entity Framework Core

## Development Commands

### Building the Project
```bash
dotnet build
```

### Running the Application
```bash
dotnet run
```

The API runs on `http://localhost:5176` by default (see `MyPokemoApi.http`).

### Running in Watch Mode (Development)
```bash
dotnet watch
```

### Restoring Dependencies
```bash
dotnet restore
```

### Cleaning Build Artifacts
```bash
dotnet clean
```

### Directory Structure

```
MyPokemoApi/
├── Controllers/            # 处理 HTTP 请求的控制器
├── Models/                 # 核心实体类（数据库表对应的类）
│   ├── DTOs/               # 数据传输对象（用于 API 输入输出，隔离数据库模型）
│   └── Entities/           # 原始数据库实体
├── Data/                   # 数据库访问层
│   ├── AppDbContext.cs     # EF Core 上下文
│   └── Migrations/         # 数据库迁移脚本（自动生成）
├── Services/               # 业务逻辑层（编写复杂的逻辑，避免 Controller 过重）
├── Interfaces/             # 接口定义（用于依赖注入）
├── Middlewares/            # 自定义中间件（如全局异常处理、日志）
├── Properties/             # 项目配置文件（如 launchSettings.json）
├── appsettings.json        # 配置文件（数据库连接、密钥等）
├── appsettings.Development.json
└── Program.cs              # 应用入口：配置服务、中间件和路由
```

## Architecture

### Minimal API Pattern
This project uses ASP.NET Core's minimal API pattern where endpoints are defined directly in `Program.cs` using `app.MapGet()`, `app.MapPost()`, etc. There are no traditional Controller classes.

### Application Structure
- **Program.cs** - Contains all application setup, middleware configuration, and endpoint definitions
- **appsettings.json** - Application configuration (non-environment-specific)
- **appsettings.Development.json** - Development-specific overrides (e.g., enhanced logging)

### Current Endpoint
The project includes a sample `/weatherforecast` endpoint that returns dummy weather data.

### Database Integration
The project references Npgsql.EntityFrameworkCore.PostgreSQL for PostgreSQL connectivity, though Entity Framework Core is not yet configured in `Program.cs`. When adding database functionality:
1. Add DbContext to DI container in Program.cs
2. Configure PostgreSQL connection string in appsettings.json
3. Create migrations using `dotnet ef migrations add`
4. Apply migrations using `dotnet ef database update`

### OpenAPI Documentation
In development mode, OpenAPI/Swagger documentation is available via `app.MapOpenApi()`.

## Configuration Notes
- The project uses .NET 10.0 SDK
- Implicit usings and nullable reference types are enabled
- HTTPS redirection is enabled in the request pipeline
