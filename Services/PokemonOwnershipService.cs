using Microsoft.EntityFrameworkCore;
using MyPokemoApi.Data;
using MyPokemoApi.Models.DTOs;
using MyPokemoApi.Models.Entities;
using System.Data;
using Microsoft.Data.SqlClient;

namespace MyPokemoApi.Services;

public class PokemonOwnershipService : IPokemonOwnershipService
{
    private readonly ApplicationDbContext _context;
    private readonly ISqlFileService _sqlFileService;

    public PokemonOwnershipService(ApplicationDbContext context, ISqlFileService sqlFileService)
    {
        _context = context;
        _sqlFileService = sqlFileService;
    }

    public async Task<UserPokemonDto> CatchPokemonAsync(string userId, int pokemonId, string? nickname = null)
    {
        // 检查用户是否已经拥有这个Pokemon
        var existingUserPokemon = await _context.UserPokemons
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PokemonId == pokemonId);

        if (existingUserPokemon != null)
        {
            throw new InvalidOperationException("User already owns this Pokemon");
        }

        // 检查Pokemon是否存在
        var pokemon = await _context.Pokemons.FindAsync(pokemonId);
        if (pokemon == null)
        {
            throw new ArgumentException("Pokemon not found", nameof(pokemonId));
        }

        // 创建新的UserPokemon记录
        var userPokemon = new UserPokemon
        {
            UserId = userId,
            PokemonId = pokemonId,
            CaughtAt = DateTime.UtcNow,
            Nickname = nickname,
            IsFavorite = false,
            Level = 1,
            Experience = 0,
            ExperienceToNextLevel = 100,
            Health = 100,
            MaxHealth = 100,
            BattlesWon = 0,
            BattlesLost = 0
        };

        _context.UserPokemons.Add(userPokemon);
        await _context.SaveChangesAsync();

        return MapToDto(userPokemon, pokemon);
    }

    public async Task<PagedResult<UserPokemonDto>> GetUserPokemonAsync(string userId, PokemonQueryParameters parameters)
    {
        // 使用原生SQL查询获取用户Pokemon列表
        return await GetUserPokemonWithSqlAsync(userId, parameters);
    }

    // 使用原生SQL查询的方法
    private async Task<PagedResult<UserPokemonDto>> GetUserPokemonWithSqlAsync(string userId, PokemonQueryParameters parameters)
    {
        // 方法1: 使用FromSqlRaw执行原生SQL
        var sql = @"
            SELECT up.*, p.Name, p.Height, p.Weight, p.[Order], p.Sprites
            FROM UserPokemons up
            INNER JOIN Pokemons p ON up.PokemonId = p.Id
            WHERE up.UserId = {0}
                AND (@FavoriteOnly = 0 OR up.IsFavorite = 1)
                AND (@MinLevel IS NULL OR up.Level >= @MinLevel)
                AND (@MaxLevel IS NULL OR up.Level <= @MaxLevel)
            ORDER BY up.CaughtAt DESC
            OFFSET {1} ROWS
            FETCH NEXT {2} ROWS ONLY";

        var offset = (parameters.Page - 1) * parameters.PageSize;
        
        // 使用Entity Framework的FromSqlRaw方法
        var query = _context.UserPokemons
            .Include(up => up.Pokemon)
            .Where(up => up.UserId == userId);

        // 应用过滤器
        if (parameters.FavoriteOnly.HasValue && parameters.FavoriteOnly.Value)
        {
            query = query.Where(up => up.IsFavorite);
        }

        if (parameters.MinLevel.HasValue)
        {
            query = query.Where(up => up.Level >= parameters.MinLevel.Value);
        }

        if (parameters.MaxLevel.HasValue)
        {
            query = query.Where(up => up.Level <= parameters.MaxLevel.Value);
        }

        var totalCount = await query.CountAsync();
        
        // 使用原生SQL进行复杂排序
        var sortedQuery = parameters.SortBy?.ToLower() switch
        {
            "level" => query.OrderBy(up => up.Level),
            "experience" => query.OrderBy(up => up.Experience),
            "name" => query.OrderBy(up => up.Pokemon.Name),
            _ => query.OrderByDescending(up => up.CaughtAt)
        };

        if (parameters.SortOrder?.ToLower() == "desc")
        {
            sortedQuery = parameters.SortBy?.ToLower() switch
            {
                "level" => query.OrderByDescending(up => up.Level),
                "experience" => query.OrderByDescending(up => up.Experience),
                "name" => query.OrderByDescending(up => up.Pokemon.Name),
                _ => query.OrderByDescending(up => up.CaughtAt)
            };
        }

        var items = await sortedQuery
            .Skip(offset)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(up => MapToDto(up, up.Pokemon)).ToList();
        var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);

        return new PagedResult<UserPokemonDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<bool> ReleasePokemonAsync(string userId, int pokemonId)
    {
        var userPokemon = await _context.UserPokemons
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PokemonId == pokemonId);

        if (userPokemon == null)
        {
            return false;
        }

        _context.UserPokemons.Remove(userPokemon);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserPokemonDto> UpdatePokemonAsync(string userId, int pokemonId, UpdateUserPokemonDto updateDto)
    {
        var userPokemon = await _context.UserPokemons
            .Include(up => up.Pokemon)
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PokemonId == pokemonId);

        if (userPokemon == null)
        {
            throw new ArgumentException("User does not own this Pokemon");
        }

        if (updateDto.Nickname != null)
        {
            userPokemon.Nickname = updateDto.Nickname;
        }

        if (updateDto.IsFavorite.HasValue)
        {
            userPokemon.IsFavorite = updateDto.IsFavorite.Value;
        }

        await _context.SaveChangesAsync();
        return MapToDto(userPokemon, userPokemon.Pokemon);
    }

    public async Task<bool> HasPokemonAsync(string userId, int pokemonId)
    {
        return await _context.UserPokemons
            .AnyAsync(up => up.UserId == userId && up.PokemonId == pokemonId);
    }

    public async Task<int> GetPokemonCountAsync(string userId)
    {
        return await _context.UserPokemons
            .CountAsync(up => up.UserId == userId);
    }

    // 新增：使用SQL查询获取用户Pokemon统计信息
    public async Task<UserPokemonStatsDto> GetUserPokemonStatsAsync(string userId)
    {
        // 方法1: 使用ExecuteSqlRaw执行原生SQL并返回标量值
        var totalPokemonParam = new SqlParameter("@TotalPokemon", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var favoritePokemonParam = new SqlParameter("@FavoritePokemon", SqlDbType.Int) { Direction = ParameterDirection.Output };
        
        // 方法2: 使用FromSqlRaw查询并映射到临时实体
        var statsQuery = @"
            SELECT 
                COUNT(*) as TotalPokemon,
                COUNT(CASE WHEN IsFavorite = 1 THEN 1 END) as FavoritePokemon,
                ISNULL(AVG(CAST(Level as FLOAT)), 0) as AverageLevel,
                ISNULL(MAX(Level), 0) as HighestLevel,
                ISNULL(MIN(Level), 0) as LowestLevel,
                SUM(BattlesWon) as TotalBattlesWon,
                SUM(BattlesLost) as TotalBattlesLost,
                COUNT(CASE WHEN Health = 0 THEN 1 END) as FaintedPokemon,
                COUNT(CASE WHEN Level = 100 THEN 1 END) as MaxLevelPokemon
            FROM UserPokemons 
            WHERE UserId = {0}";

        // 使用原生SQL查询
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = statsQuery.Replace("{0}", "@UserId");
        command.Parameters.Add(new SqlParameter("@UserId", userId));
        
        await _context.Database.OpenConnectionAsync();
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new UserPokemonStatsDto
            {
                TotalPokemon = reader.GetInt32("TotalPokemon"),
                FavoritePokemon = reader.GetInt32("FavoritePokemon"),
                AverageLevel = reader.GetDouble("AverageLevel"),
                HighestLevel = reader.GetInt32("HighestLevel"),
                LowestLevel = reader.GetInt32("LowestLevel"),
                TotalBattlesWon = reader.GetInt32("TotalBattlesWon"),
                TotalBattlesLost = reader.GetInt32("TotalBattlesLost"),
                FaintedPokemon = reader.GetInt32("FaintedPokemon"),
                MaxLevelPokemon = reader.GetInt32("MaxLevelPokemon")
            };
        }
        
        return new UserPokemonStatsDto();
    }

    // 新增：使用SQL执行批量操作的示例
    public async Task<bool> BulkHealPokemonAsync(string userId)
    {
        var sql = @"
            UPDATE UserPokemons 
            SET Health = MaxHealth 
            WHERE UserId = @UserId AND Health < MaxHealth";
        
        var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, 
            new SqlParameter("@UserId", userId));
        
        return rowsAffected > 0;
    }

    // 新增：使用存储过程的示例
    public async Task<int> BulkLevelUpPokemonAsync(string userId, int levelsToAdd = 1)
    {
        // 方式1: 如果SQL文件包含存储过程定义，需要先创建存储过程
        // 然后再调用它
        
        // 首先确保存储过程存在（通常在数据库迁移中完成）
        var createProcedureSql = await File.ReadAllTextAsync("Sql/BulkLevelUpPokemon.sql");
        
        // 执行创建存储过程的SQL（如果不存在的话）
        try
        {
            await _context.Database.ExecuteSqlRawAsync(createProcedureSql);
        }
        catch (SqlException ex) when (ex.Message.Contains("already exists"))
        {
            // 存储过程已存在，忽略错误
        }
        
        // 然后调用存储过程
        var sql = "EXEC BulkLevelUpUserPokemon @UserId, @LevelsToAdd, @MaxLevel";
        
        var parameters = new[]
        {
            new SqlParameter("@UserId", userId),
            new SqlParameter("@LevelsToAdd", levelsToAdd),
            new SqlParameter("@MaxLevel", 100)
        };

        // 执行存储过程并获取返回值
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);
        
        await _context.Database.OpenConnectionAsync();
        
        var result = await command.ExecuteScalarAsync();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    // 新增：直接使用SQL文件执行查询的示例
    public async Task<int> BulkLevelUpPokemonWithFileAsync(string userId, int levelsToAdd = 1)
    {
        // 方式2: 使用SQL文件服务读取SQL文件
        var sql = await _sqlFileService.ReadSqlFileAsync("BulkLevelUpQuery");
        
        var parameters = new[]
        {
            new SqlParameter("@UserId", userId),
            new SqlParameter("@LevelsToAdd", levelsToAdd),
            new SqlParameter("@MaxLevel", 100)
        };

        // 分割SQL语句（因为文件包含多个语句）
        var sqlStatements = sql.Split(new[] { "-- 返回受影响的行数" }, StringSplitOptions.RemoveEmptyEntries);
        
        // 执行更新语句
        var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sqlStatements[0], parameters);
        
        return rowsAffected;
    }

    public async Task<TrainingResultDto> TrainPokemonAsync(string userId, int pokemonId, TrainingRequestDto request)
    {
        var userPokemon = await _context.UserPokemons
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PokemonId == pokemonId);

        if (userPokemon == null)
        {
            throw new ArgumentException("User does not own this Pokemon");
        }

        var previousLevel = userPokemon.Level;
        var levelUp = false;

        if (request.TrainingType.ToLower() == "experience")
        {
            userPokemon.Experience += request.Amount;
            
            // 检查是否升级
            while (userPokemon.Experience >= userPokemon.ExperienceToNextLevel && userPokemon.Level < 100)
            {
                userPokemon.Experience -= userPokemon.ExperienceToNextLevel;
                userPokemon.Level++;
                userPokemon.MaxHealth += 10; // 每级增加10点最大生命值
                userPokemon.Health = userPokemon.MaxHealth; // 升级时恢复满血
                userPokemon.ExperienceToNextLevel = CalculateExperienceToNextLevel(userPokemon.Level);
                levelUp = true;
            }
        }
        else if (request.TrainingType.ToLower() == "health")
        {
            userPokemon.Health = Math.Min(userPokemon.Health + request.Amount, userPokemon.MaxHealth);
        }

        await _context.SaveChangesAsync();

        return new TrainingResultDto
        {
            LevelUp = levelUp,
            PreviousLevel = previousLevel,
            NewLevel = userPokemon.Level,
            ExperienceGained = request.TrainingType.ToLower() == "experience" ? request.Amount : 0,
            NewExperience = userPokemon.Experience,
            NewExperienceToNextLevel = userPokemon.ExperienceToNextLevel
        };
    }

    public async Task<BattleResultDto> BattlePokemonAsync(string userId, int pokemonId, BattleRequestDto request)
    {
        var userPokemon = await _context.UserPokemons
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PokemonId == pokemonId);

        if (userPokemon == null)
        {
            throw new ArgumentException("User does not own this Pokemon");
        }

        if (userPokemon.Health <= 0)
        {
            throw new InvalidOperationException("Pokemon must have health to battle");
        }

        // 简单的战斗逻辑
        var random = new Random();
        var battleResult = random.Next(0, 3) switch
        {
            0 => "won",
            1 => "lost",
            _ => "draw"
        };

        var experienceGained = 0;
        var healthLost = random.Next(10, 30);
        var levelUp = false;

        if (battleResult == "won")
        {
            userPokemon.BattlesWon++;
            experienceGained = random.Next(50, 100);
            userPokemon.Experience += experienceGained;
            
            // 检查升级
            if (userPokemon.Experience >= userPokemon.ExperienceToNextLevel && userPokemon.Level < 100)
            {
                userPokemon.Experience -= userPokemon.ExperienceToNextLevel;
                userPokemon.Level++;
                userPokemon.MaxHealth += 10;
                userPokemon.ExperienceToNextLevel = CalculateExperienceToNextLevel(userPokemon.Level);
                levelUp = true;
            }
        }
        else if (battleResult == "lost")
        {
            userPokemon.BattlesLost++;
            healthLost = Math.Min(healthLost, userPokemon.Health);
        }

        userPokemon.Health = Math.Max(0, userPokemon.Health - healthLost);
        userPokemon.LastBattleAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new BattleResultDto
        {
            BattleResult = battleResult,
            ExperienceGained = experienceGained,
            HealthLost = healthLost,
            LevelUp = levelUp,
            BattleStats = new BattleStatsDto
            {
                BattlesWon = userPokemon.BattlesWon,
                BattlesLost = userPokemon.BattlesLost,
                LastBattleAt = userPokemon.LastBattleAt ?? DateTime.UtcNow
            }
        };
    }

    public async Task<HealResultDto> HealPokemonAsync(string userId, int pokemonId)
    {
        var userPokemon = await _context.UserPokemons
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PokemonId == pokemonId);

        if (userPokemon == null)
        {
            throw new ArgumentException("User does not own this Pokemon");
        }

        var previousHealth = userPokemon.Health;
        userPokemon.Health = userPokemon.MaxHealth;
        
        await _context.SaveChangesAsync();

        return new HealResultDto
        {
            PreviousHealth = previousHealth,
            NewHealth = userPokemon.Health,
            FullyHealed = true
        };
    }

    public async Task<UserPokemonDto> LevelUpPokemonAsync(string userId, int pokemonId)
    {
        var userPokemon = await _context.UserPokemons
            .Include(up => up.Pokemon)
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PokemonId == pokemonId);

        if (userPokemon == null)
        {
            throw new ArgumentException("User does not own this Pokemon");
        }

        if (userPokemon.Level >= 100)
        {
            throw new InvalidOperationException("Pokemon is already at maximum level");
        }

        userPokemon.Level++;
        userPokemon.MaxHealth += 10;
        userPokemon.Health = userPokemon.MaxHealth;
        userPokemon.Experience = 0;
        userPokemon.ExperienceToNextLevel = CalculateExperienceToNextLevel(userPokemon.Level);

        await _context.SaveChangesAsync();
        return MapToDto(userPokemon, userPokemon.Pokemon);
    }

    private static UserPokemonDto MapToDto(UserPokemon userPokemon, Pokemon pokemon)
    {
        return new UserPokemonDto
        {
            UserId = userPokemon.UserId,
            PokemonId = userPokemon.PokemonId,
            CaughtAt = userPokemon.CaughtAt,
            Nickname = userPokemon.Nickname,
            IsFavorite = userPokemon.IsFavorite,
            Level = userPokemon.Level,
            Experience = userPokemon.Experience,
            ExperienceToNextLevel = userPokemon.ExperienceToNextLevel,
            Health = userPokemon.Health,
            MaxHealth = userPokemon.MaxHealth,
            BattlesWon = userPokemon.BattlesWon,
            BattlesLost = userPokemon.BattlesLost,
            LastBattleAt = userPokemon.LastBattleAt,
            Pokemon = new PokemonDto
            {
                Id = pokemon.Id,
                Name = pokemon.Name,
                Height = pokemon.Height,
                Weight = pokemon.Weight,
                Order = pokemon.Order,
                Sprites = new PokemonSpritesDto
                {
                    FrontDefault = pokemon.Sprites.FrontDefault,
                    FrontShiny = pokemon.Sprites.FrontShiny
                }
            }
        };
    }

    private static int CalculateExperienceToNextLevel(int level)
    {
        // 简单的经验值计算公式
        return level * 100;
    }
}