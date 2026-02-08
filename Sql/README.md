# 在PokemonOwnershipService中使用SQL

本文档展示了在PokemonOwnershipService中使用原生SQL查询的几种方法。

## 1. SQL文件目录结构

```
MyPokemoApi/
├── Sql/
│   ├── GetUserPokemon.sql          # 获取用户Pokemon列表查询
│   ├── GetUserPokemonStats.sql     # 获取用户Pokemon统计信息
│   ├── BulkLevelUpPokemon.sql      # 存储过程定义
│   ├── BulkLevelUpQuery.sql        # 直接查询语句
│   └── README.md                   # 本文档
├── Services/
│   ├── PokemonOwnershipService.cs  # 主要服务
│   └── SqlFileService.cs           # SQL文件管理服务
└── Program.cs                      # 服务注册
```

## 2. SQL文件查找机制

### 2.1 SqlFileService的路径解析
```csharp
public class SqlFileService : ISqlFileService
{
    private readonly string _sqlDirectory;

    public SqlFileService()
    {
        // 方法1: 基于程序集位置
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        _sqlDirectory = Path.Combine(assemblyDirectory ?? "", "Sql");
        
        // 方法2: 如果程序集路径不存在，使用当前工作目录
        if (!Directory.Exists(_sqlDirectory))
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            _sqlDirectory = Path.Combine(currentDirectory, "Sql");
        }
    }
}
```

### 2.2 路径解析优先级
1. **程序集目录/Sql** - 生产环境中的路径
2. **当前工作目录/Sql** - 开发环境中的路径

### 2.3 文件名处理
```csharp
public string GetSqlFilePath(string fileName)
{
    // 自动添加.sql扩展名
    if (!fileName.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
    {
        fileName += ".sql";
    }
    
    return Path.Combine(_sqlDirectory, fileName);
}
```

## 3. 使用方式对比

### 3.1 直接硬编码路径（不推荐）
```csharp
// 问题：路径硬编码，不灵活
var sql = await File.ReadAllTextAsync("Sql/GetUserPokemon.sql");
```

### 3.2 使用SqlFileService（推荐）
```csharp
// 优点：路径管理统一，错误处理完善
var sql = await _sqlFileService.ReadSqlFileAsync("GetUserPokemon");
```

## 4. 实际使用示例

### 4.1 存储过程方式
```csharp
public async Task<int> BulkLevelUpPokemonAsync(string userId, int levelsToAdd = 1)
{
    // 1. 读取存储过程定义文件
    var createProcedureSql = await _sqlFileService.ReadSqlFileAsync("BulkLevelUpPokemon");
    
    // 2. 创建存储过程（如果不存在）
    await _context.Database.ExecuteSqlRawAsync(createProcedureSql);
    
    // 3. 调用存储过程
    var sql = "EXEC BulkLevelUpUserPokemon @UserId, @LevelsToAdd, @MaxLevel";
    // ... 执行逻辑
}
```

### 4.2 直接查询方式
```csharp
public async Task<int> BulkLevelUpPokemonWithFileAsync(string userId, int levelsToAdd = 1)
{
    // 1. 读取查询文件
    var sql = await _sqlFileService.ReadSqlFileAsync("BulkLevelUpQuery");
    
    // 2. 直接执行查询
    var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, parameters);
    return rowsAffected;
}
```

## 5. 部署注意事项

### 5.1 确保SQL文件被复制到输出目录
在`.csproj`文件中添加：
```xml
<ItemGroup>
  <Content Include="Sql\**\*.sql">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

### 5.2 Docker部署
在Dockerfile中确保复制SQL文件：
```dockerfile
COPY Sql/ ./Sql/
```

## 6. 错误处理

### 6.1 文件不存在
```csharp
public async Task<string> ReadSqlFileAsync(string fileName)
{
    var filePath = GetSqlFilePath(fileName);
    
    if (!File.Exists(filePath))
    {
        throw new FileNotFoundException($"SQL file not found: {filePath}");
    }
    
    return await File.ReadAllTextAsync(filePath);
}
```

### 6.2 SQL执行错误
```csharp
try
{
    var sql = await _sqlFileService.ReadSqlFileAsync("BulkLevelUpPokemon");
    await _context.Database.ExecuteSqlRawAsync(sql);
}
catch (FileNotFoundException ex)
{
    // SQL文件不存在
    throw new InvalidOperationException("Required SQL file is missing", ex);
}
catch (SqlException ex)
{
    // SQL执行错误
    throw new InvalidOperationException("Database operation failed", ex);
}
```

## 7. 最佳实践总结

1. **使用专门的SQL文件服务**来管理文件路径
2. **统一的错误处理**机制
3. **自动文件扩展名处理**
4. **支持开发和生产环境**的不同路径
5. **确保SQL文件正确部署**到目标环境