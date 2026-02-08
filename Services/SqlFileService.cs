using System.Reflection;

namespace MyPokemoApi.Services;

public interface ISqlFileService
{
    Task<string> ReadSqlFileAsync(string fileName);
    string GetSqlFilePath(string fileName);
}

public class SqlFileService : ISqlFileService
{
    private readonly string _sqlDirectory;

    public SqlFileService()
    {
        // 获取当前程序集的位置
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        
        // 构建SQL文件目录路径
        _sqlDirectory = Path.Combine(assemblyDirectory ?? "", "Sql");
        
        // 如果在开发环境中，可能需要向上查找到项目根目录
        if (!Directory.Exists(_sqlDirectory))
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            _sqlDirectory = Path.Combine(currentDirectory, "Sql");
        }
    }

    public async Task<string> ReadSqlFileAsync(string fileName)
    {
        var filePath = GetSqlFilePath(fileName);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"SQL file not found: {filePath}");
        }
        
        return await File.ReadAllTextAsync(filePath);
    }

    public string GetSqlFilePath(string fileName)
    {
        // 确保文件名有.sql扩展名
        if (!fileName.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".sql";
        }
        
        return Path.Combine(_sqlDirectory, fileName);
    }
}